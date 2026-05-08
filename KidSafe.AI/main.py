"""
KidSafe AI Moderation Service
==============================
To plug in your own model, implement BaseClassifier and set MODEL_BACKEND
in config.py (or env var MODEL_BACKEND).

Current backends:
  - "stub"       → always returns safe (no dependencies, for testing)
  - "hf_pipeline"→ HuggingFace pipeline (default until your model is ready)
  - "custom"     → YOUR MODEL — fill in CustomClassifier.predict()
"""

import logging
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from contextlib import asynccontextmanager
from classifier import load_classifier, BaseClassifier
from models import AnalyzeRequest, AnalyzeResponse, ConfigResponse
from config import settings

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s"
)
logger = logging.getLogger("kidsafe.ai")

# ── app state ─────────────────────────────────────────────────────────────────

classifier: BaseClassifier | None = None


@asynccontextmanager
async def lifespan(app: FastAPI):
    global classifier
    logger.info(f"Loading model backend: '{settings.model_backend}'")
    try:
        classifier = load_classifier(settings.model_backend)
        logger.info("Model loaded ✓")
    except Exception as e:
        logger.error(f"Model load failed: {e}")
        classifier = None
    yield
    logger.info("Shutting down.")


app = FastAPI(
    title="KidSafe AI Moderation",
    version="1.0.0",
    description="Toxicity classification: safe | flagged | blocked",
    lifespan=lifespan,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

# ── routes ────────────────────────────────────────────────────────────────────

@app.post("/analyze", response_model=AnalyzeResponse)
async def analyze(req: AnalyzeRequest) -> AnalyzeResponse:
    text = req.message.strip()
    if not text:
        return AnalyzeResponse(label="Safe", score=0.0)

    if classifier is None:
        raise HTTPException(status_code=503, detail="Model not loaded")

    raw_score: float = await classifier.predict(text)   # 0.0 = clean, 1.0 = toxic

    # SDD §4.1 labels: Safe | Watch | Review
    if raw_score < settings.flagged_threshold:
        label = "Safe"
    elif raw_score >= settings.blocked_threshold:
        label = "Review"
    else:
        label = "Watch"

    return AnalyzeResponse(label=label, score=round(raw_score, 4))


@app.get("/health")
async def health():
    return {
        "status": "ok",
        "model_backend": settings.model_backend,
        "model_loaded": classifier is not None,
    }


@app.get("/config", response_model=ConfigResponse)
async def get_config() -> ConfigResponse:
    return ConfigResponse(
        flagged_threshold=settings.flagged_threshold,
        blocked_threshold=settings.blocked_threshold,
        model_backend=settings.model_backend,
    )


@app.patch("/config", response_model=ConfigResponse)
async def update_config(
    flagged_threshold: float | None = None,
    blocked_threshold: float | None = None,
) -> ConfigResponse:
    ft = flagged_threshold if flagged_threshold is not None else settings.flagged_threshold
    bt = blocked_threshold if blocked_threshold is not None else settings.blocked_threshold

    if not (0.0 < ft < bt <= 1.0):
        raise HTTPException(
            status_code=400,
            detail=f"Invalid thresholds: flagged={ft}, blocked={bt}. Must satisfy 0 < flagged < blocked ≤ 1"
        )

    settings.flagged_threshold = ft
    settings.blocked_threshold = bt
    logger.info(f"Thresholds updated → flagged={ft}, blocked={bt}")

    return ConfigResponse(
        flagged_threshold=ft,
        blocked_threshold=bt,
        model_backend=settings.model_backend,
    )
