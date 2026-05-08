"""
Runtime configuration — override any value via environment variable.

Examples:
  MODEL_BACKEND=custom          → use your own model
  FLAGGED_THRESHOLD=0.4
  BLOCKED_THRESHOLD=0.75
"""

from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8")

    # ── model ─────────────────────────────────────────────────
    # Options: "stub" | "hf_pipeline" | "custom"
    model_backend: str = "hf_pipeline"

    # HuggingFace pipeline model name (only used when backend = hf_pipeline)
    hf_model_name: str = "unitary/toxic-bert"
    hf_max_length: int = 128

    # ── thresholds (mutable at runtime via PATCH /config) ─────
    flagged_threshold: float = 0.5
    blocked_threshold: float = 0.8


settings = Settings()
