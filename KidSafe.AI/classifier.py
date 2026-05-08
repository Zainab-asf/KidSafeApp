"""
Model abstraction layer.
========================
All classifiers implement BaseClassifier.predict(text) → float [0, 1].
0.0 = completely safe   |   1.0 = maximally toxic

To add your own model:
  1. Create a class that extends BaseClassifier
  2. Implement async predict(self, text: str) -> float
  3. Register it in load_classifier() below
  4. Set MODEL_BACKEND=custom in .env
"""

import asyncio
import logging
from abc import ABC, abstractmethod

logger = logging.getLogger("kidsafe.classifier")


# ── base ──────────────────────────────────────────────────────────────────────

class BaseClassifier(ABC):
    @abstractmethod
    async def predict(self, text: str) -> float:
        """Return toxicity probability in [0, 1]."""
        ...


# ── stub (no ML deps — always safe, useful for dev/tests) ────────────────────

class StubClassifier(BaseClassifier):
    async def predict(self, text: str) -> float:
        logger.debug("StubClassifier → 0.0")
        return 0.0


# ── HuggingFace pipeline backend ─────────────────────────────────────────────

class HFPipelineClassifier(BaseClassifier):
    def __init__(self, model_name: str, max_length: int = 128):
        from transformers import pipeline
        logger.info(f"Loading HuggingFace model: {model_name}")
        self._pipe = pipeline(
            "text-classification",
            model=model_name,
            top_k=1,
            truncation=True,
            max_length=max_length,
        )
        logger.info("HuggingFace model ready")

    async def predict(self, text: str) -> float:
        loop = asyncio.get_event_loop()
        # Run blocking inference in thread pool
        results = await loop.run_in_executor(None, self._pipe, text)
        result  = results[0][0] if isinstance(results[0], list) else results[0]
        raw_label: str = result["label"].lower()
        score: float   = result["score"]
        # toxic-bert: label is "toxic" or "non_toxic"
        is_toxic = "toxic" in raw_label and "non" not in raw_label
        return round(score if is_toxic else 0.0, 4)


# ── ✏️  YOUR MODEL — plug in here ─────────────────────────────────────────────

class CustomClassifier(BaseClassifier):
    """
    Replace this implementation with your own model.

    Contract:
      - __init__  → load your model weights / tokenizer
      - predict   → return a float in [0.0, 1.0]
                    0.0 = safe, 1.0 = toxic

    Example skeleton (PyTorch):
        def __init__(self):
            from transformers import AutoTokenizer, AutoModelForSequenceClassification
            import torch
            self.tokenizer = AutoTokenizer.from_pretrained("./my_model")
            self.model     = AutoModelForSequenceClassification.from_pretrained("./my_model")
            self.model.eval()

        async def predict(self, text: str) -> float:
            import torch
            inputs = self.tokenizer(text, return_tensors="pt", truncation=True, max_length=128)
            with torch.no_grad():
                logits = self.model(**inputs).logits
            probs = torch.softmax(logits, dim=-1)
            # Assume index 1 = toxic
            return float(probs[0][1].item())
    """

    def __init__(self):
        # TODO: load your model here
        logger.warning("CustomClassifier: no model loaded — returning 0.0 (safe) for all inputs")

    async def predict(self, text: str) -> float:
        # TODO: replace with your inference logic
        return 0.0


# ── factory ───────────────────────────────────────────────────────────────────

def load_classifier(backend: str) -> BaseClassifier:
    """
    Instantiate the classifier for the given backend name.
    Called once at startup.
    """
    match backend.lower():
        case "stub":
            return StubClassifier()
        case "hf_pipeline":
            from config import settings
            return HFPipelineClassifier(settings.hf_model_name, settings.hf_max_length)
        case "custom":
            return CustomClassifier()
        case _:
            raise ValueError(
                f"Unknown model backend '{backend}'. "
                "Choose from: stub | hf_pipeline | custom"
            )
