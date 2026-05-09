"""
KidSafe Classifier — uses the trained BiLSTM/GRU Keras model.
=============================================================
Wired to:
  model/cyberbully_detector_cpu.h5  (85 MB, CPU-optimised)
  model/tokenizer.pkl               (7.4 MB, fitted Keras Tokenizer)
  model/config.pkl                  (max_words, max_len, embedding_dim)

predict(text) → float [0.0 = safe … 1.0 = toxic]
"""

import asyncio
import logging
import os
import pickle
from abc import ABC, abstractmethod

import numpy as np
import tensorflow as tf
from tensorflow.keras.preprocessing.sequence import pad_sequences

logger = logging.getLogger("kidsafe.classifier")

# Path to the trained_models folder (sibling of this file)
_BASE_DIR  = os.path.dirname(os.path.abspath(__file__))
MODEL_DIR  = os.path.join(_BASE_DIR, "model")


# ── base contract ─────────────────────────────────────────────────────────────

class BaseClassifier(ABC):
    @abstractmethod
    async def predict(self, text: str) -> float:
        """Return toxicity probability in [0, 1]. 0 = safe, 1 = toxic."""
        ...


# ── stub — always safe (no ML deps, useful for unit tests) ───────────────────

class StubClassifier(BaseClassifier):
    async def predict(self, text: str) -> float:
        logger.debug("StubClassifier → 0.0")
        return 0.0


# ── KidSafe Keras model ───────────────────────────────────────────────────────

class KidSafeClassifier(BaseClassifier):
    """
    Bidirectional LSTM/GRU cyberbullying detector.

    Model files required in model/:
      cyberbully_detector_cpu.h5  — CPU-optimised Keras model
      tokenizer.pkl               — fitted Keras Tokenizer
      config.pkl                  — {'max_words':…, 'max_len':…, 'embedding_dim':…}
    """

    def __init__(self, model_dir: str = MODEL_DIR):
        logger.info(f"Loading KidSafe model from: {model_dir}")

        # ── tokenizer ────────────────────────────────────────────
        tok_path = os.path.join(model_dir, "tokenizer.pkl")
        with open(tok_path, "rb") as f:
            self._tokenizer = pickle.load(f)
        logger.info("  ✓ Tokenizer loaded")

        # ── config ───────────────────────────────────────────────
        cfg_path = os.path.join(model_dir, "config.pkl")
        with open(cfg_path, "rb") as f:
            cfg = pickle.load(f)
        self._max_len = cfg.get("max_len", 100)
        logger.info(f"  ✓ Config loaded (max_len={self._max_len})")

        # ── model ────────────────────────────────────────────────
        model_path = os.path.join(model_dir, "cyberbully_detector_cpu.h5")
        if not os.path.exists(model_path):
            raise FileNotFoundError(
                f"Model file not found: {model_path}"
            )
        self._model = tf.keras.models.load_model(model_path)
        logger.info(f"  ✓ Model loaded from {os.path.basename(model_path)}")

    # ── internal helpers ─────────────────────────────────────────

    def _preprocess(self, text: str) -> np.ndarray:
        """Tokenise + pad a single message → (1, max_len) array."""
        seqs   = self._tokenizer.texts_to_sequences([text])
        padded = pad_sequences(seqs, maxlen=self._max_len, padding="post", truncating="post")
        return padded

    def _infer(self, text: str) -> float:
        """Blocking inference — runs inside a thread-pool executor."""
        padded = self._preprocess(text)
        prob   = float(self._model.predict(padded, verbose=0)[0][0])
        return round(prob, 4)

    # ── public API ───────────────────────────────────────────────

    async def predict(self, text: str) -> float:
        """Non-blocking: offloads Keras inference to a thread executor."""
        loop = asyncio.get_event_loop()
        return await loop.run_in_executor(None, self._infer, text)


# ── factory ───────────────────────────────────────────────────────────────────

def load_classifier(backend: str) -> BaseClassifier:
    """Instantiate the classifier for the given backend name (called once at startup)."""
    match backend.lower():
        case "stub":
            return StubClassifier()
        case "custom" | "kidsafe":
            return KidSafeClassifier()
        case _:
            raise ValueError(
                f"Unknown model backend '{backend}'. "
                "Choose from: stub | custom"
            )
