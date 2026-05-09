"""
Run: pytest tests/ -v
Forces StubClassifier so tests run without TensorFlow/Keras (no model file needed).
"""
import pytest
from httpx import AsyncClient, ASGITransport
from unittest.mock import patch


@pytest.fixture(scope="module")
def anyio_backend():
    return "asyncio"


@pytest.fixture(scope="module")
async def client():
    # Force stub backend so tests run without torch/transformers
    with patch("config.settings") as mock_cfg:
        mock_cfg.model_backend    = "stub"
        mock_cfg.flagged_threshold = 0.5
        mock_cfg.blocked_threshold = 0.8

        from main import app
        async with AsyncClient(
            transport=ASGITransport(app=app), base_url="http://test"
        ) as c:
            yield c


@pytest.mark.anyio
async def test_empty_message(client):
    r = await client.post("/analyze", json={"message": "   "})
    assert r.status_code == 200
    assert r.json()["label"] == "safe"


@pytest.mark.anyio
async def test_health(client):
    r = await client.get("/health")
    assert r.status_code == 200
    assert r.json()["status"] == "ok"


@pytest.mark.anyio
async def test_config_get(client):
    r = await client.get("/config")
    assert r.status_code == 200
    data = r.json()
    assert "flagged_threshold" in data
    assert "blocked_threshold" in data


@pytest.mark.anyio
async def test_config_patch_valid(client):
    r = await client.patch("/config", params={"flagged_threshold": 0.4, "blocked_threshold": 0.75})
    assert r.status_code == 200


@pytest.mark.anyio
async def test_config_patch_invalid(client):
    r = await client.patch("/config", params={"flagged_threshold": 0.9, "blocked_threshold": 0.5})
    assert r.status_code == 400
