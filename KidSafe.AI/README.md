# KidSafe AI Service

FastAPI toxicity classification microservice — swappable model backend.

## Quickstart

```bash
python -m venv venv
venv\Scripts\activate          # Windows
pip install -r requirements.txt
cp .env.example .env
uvicorn main:app --reload --port 8000
```

## Plug in your own model

1. Open `classifier.py`
2. Fill in `CustomClassifier.__init__()` — load weights/tokenizer
3. Fill in `CustomClassifier.predict(text)` — return float [0, 1]
4. Set `MODEL_BACKEND=custom` in `.env`
5. Restart — nothing else changes

## API

| Method | Route | Notes |
|--------|-------|-------|
| POST | `/analyze` | `{"message": "text"}` → `{"label": "...", "score": 0.0}` |
| GET  | `/health`  | Model loaded status |
| GET  | `/config`  | Current thresholds + backend |
| PATCH| `/config`  | `?flagged_threshold=0.5&blocked_threshold=0.8` |

## Backends

| `MODEL_BACKEND` | Description |
|-----------------|-------------|
| `stub`          | Always safe — no ML, for unit tests |
| `hf_pipeline`   | `unitary/toxic-bert` via HuggingFace |
| `custom`        | **Your model** in `classifier.py` |

## Thresholds

| Score | Label |
|-------|-------|
| < 0.5 | `safe` |
| 0.5 – 0.8 | `flagged` (masked) |
| ≥ 0.8 | `blocked` |

Tune live: `PATCH /config?flagged_threshold=0.4&blocked_threshold=0.75`

## Tests
```bash
pytest tests/ -v
```
