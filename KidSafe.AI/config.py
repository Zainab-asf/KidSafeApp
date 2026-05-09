"""
Runtime configuration — override any value via environment variable or .env file.

Examples:
  MODEL_BACKEND=custom           (default — uses trained BiLSTM Keras model)
  MODEL_BACKEND=stub             (testing — no ML, always returns Safe)
  FLAGGED_THRESHOLD=0.5
  BLOCKED_THRESHOLD=0.8
"""

from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8")

    # Model backend: "custom" (KidSafe Keras model) | "stub" (testing)
    model_backend: str = "custom"

    # Thresholds — mutable at runtime via PATCH /config
    # score < flagged              → Safe
    # flagged ≤ score < blocked   → Watch  (message masked)
    # score ≥ blocked             → Review (message blocked)
    flagged_threshold: float = 0.5
    blocked_threshold: float = 0.8


settings = Settings()
