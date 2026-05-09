from pydantic import BaseModel, Field


class AnalyzeRequest(BaseModel):
    message: str = Field(..., min_length=1, max_length=2000)


class AnalyzeResponse(BaseModel):
    label: str   # Safe | Watch | Review  (SDD §4.1)
    score: float = Field(..., ge=0.0, le=1.0)


class ConfigResponse(BaseModel):
    flagged_threshold: float
    blocked_threshold: float
    model_backend: str
