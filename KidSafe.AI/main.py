"""
KidSafe AI Moderation Service
==============================
FastAPI server wrapping the trained BiLSTM/GRU cyberbullying detector.

Model: trained_models/cyberbully_detector_cpu.h5
       (custom Keras model trained on cyberbullying datasets)

Endpoints:
  POST /analyze   → {"message": "..."} → {"label": "Safe|Watch|Review", "score": 0.0-1.0}
  GET  /health    → service + model status
  GET  /config    → current thresholds
  PATCH /config   → update thresholds at runtime

Start:
  uvicorn main:app --reload --port 8000
"""

import logging
from contextlib import asynccontextmanager

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware

from classifier import BaseClassifier, load_classifier
from config import settings
from models import AnalyzeRequest, AnalyzeResponse, ConfigResponse

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s",
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
        logger.info("Model ready ✓")
    except Exception as exc:
        logger.error(f"Model load failed: {exc}")
        classifier = None
    yield
    logger.info("Shutting down.")


app = FastAPI(
    title="KidSafe AI Moderation",
    version="2.0.0",
    description="Cyberbullying detection: Safe | Watch | Review",
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
    """
    Classify a message.

    Returns:
      label  — Safe | Watch | Review
      score  — toxicity probability [0.0, 1.0]
    """
    text = req.message.strip()
    if not text:
        return AnalyzeResponse(label="Safe", score=0.0)

    if classifier is None:
        raise HTTPException(status_code=503, detail="Model not loaded")

    score: float = await classifier.predict(text)

    # SDD §4.1 label mapping
    if score < settings.flagged_threshold:
        label = "Safe"
    elif score >= settings.blocked_threshold:
        label = "Review"
    else:
        label = "Watch"

    logger.info(f"analyze → score={score:.4f} label={label}")
    return AnalyzeResponse(label=label, score=round(score, 4))


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
            detail=f"Invalid thresholds: flagged={ft}, blocked={bt}. Must satisfy 0 < flagged < blocked ≤ 1",
        )

    settings.flagged_threshold = ft
    settings.blocked_threshold = bt
    logger.info(f"Thresholds updated → flagged={ft}, blocked={bt}")

    return ConfigResponse(
        flagged_threshold=ft,
        blocked_threshold=bt,
        model_backend=settings.model_backend,
    )
