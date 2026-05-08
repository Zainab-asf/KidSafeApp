# 🛡️ KidSafe — AI Cyberbullying Detection App

Cross-platform child-safe chat with real-time AI moderation.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  KidSafe.Frontend  (Blazor WASM, port 5001)                 │
│  Login · Register · ChildChat · ParentDashboard · Teacher   │
└────────────────┬────────────────────────────────────────────┘
                 │ HTTP + SignalR WS
┌────────────────▼────────────────────────────────────────────┐
│  KidSafe.Backend  (ASP.NET Core 8, port 5000)               │
│  AuthController · MessagesController · DashboardController  │
│  RewardsController · ChatHub (SignalR)                      │
│  SQLite (flagged messages only) · JWT auth                  │
└──────┬─────────────────────────────┬───────────────────────┘
       │ HTTP POST /analyze           │ Firebase Admin SDK
┌──────▼──────────────┐   ┌──────────▼──────────────────────┐
│  KidSafe.AI         │   │  Firebase Cloud Messaging        │
│  FastAPI port 8000  │   │  Push to parent/teacher devices  │
│  BaseClassifier     │   └─────────────────────────────────┘
│  (pluggable model)  │
└─────────────────────┘
```

---

## Quick Start (Local Dev)

### Prerequisites
- .NET 8 SDK
- Python 3.11+
- Node is NOT required

### 1 — AI Service

```bash
cd KidSafe.AI
python -m venv venv
venv\Scripts\activate        # Windows
# source venv/bin/activate   # macOS/Linux
pip install -r requirements.txt
cp .env.example .env
uvicorn main:app --reload --port 8000
```

Swagger: http://localhost:8000/docs

**To use your own model instead of TinyBERT:**
1. Open `classifier.py` → fill in `CustomClassifier`
2. Set `MODEL_BACKEND=custom` in `.env`
3. Restart

### 2 — Backend

```bash
cd KidSafe.Backend
dotnet run
```

- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- SQLite DB auto-created as `kidsafe.db` on first run

### 3 — Frontend

```bash
cd KidSafe.Frontend
dotnet run
```

App: http://localhost:5001

---

## Firebase Setup (Push Notifications)

> Skip this section if you don't need push notifications — the app works without it.

1. Go to [Firebase Console](https://console.firebase.google.com) → Create project
2. **Backend credentials:**
   - Project Settings → Service Accounts → Generate new private key
   - Save as `KidSafe.Backend/firebase-credentials.json`
   - `appsettings.json` → `Firebase:CredentialFile` already points there
3. **Frontend (Web Push):**
   - Project Settings → General → Add Web App → copy config
   - Replace `YOUR_*` in `wwwroot/js/firebase-messaging.js`
   - Replace `YOUR_*` in `wwwroot/firebase-messaging-sw.js`
   - Cloud Messaging → Web Push certificates → Generate → copy VAPID key
   - Paste into `firebase-messaging.js` → `VAPID_KEY`

---

## Docker (All Services)

```bash
# Create secrets folder
mkdir secrets
cp path/to/firebase-credentials.json secrets/

# Create .env for secrets
echo "JWT_KEY=YourSuperSecretKeyMinimum32Chars!!" > .env

docker compose up --build
```

| Service | URL |
|---------|-----|
| Frontend | http://localhost:5001 |
| Backend API | http://localhost:5000 |
| Backend Swagger | http://localhost:5000/swagger |
| AI Service | http://localhost:8000/docs |

---

## API Reference

### Auth
```
POST /auth/register   { email, password, displayName, role }
POST /auth/login      { email, password }
```
Both return `{ token, role, userId, displayName }`

### Messages
```
POST /messages/send          { receiverId, message }   → { status, maskedMessage, label, score }
GET  /messages/flagged       → [ { id, senderName, maskedMessage, label, score, timestamp } ]
```

### Dashboard
```
GET  /dashboard/stats        → { totalFlagged, totalBlocked, totalChildren, recentActivity }
```

### Rewards
```
GET  /rewards                → { points, badges: [] }
POST /rewards/fcm-token      { token }
```

### AI Service
```
POST /analyze                { message }  → { label: "safe|flagged|blocked", score: 0.0-1.0 }
GET  /health
GET  /config
PATCH /config                ?flagged_threshold=0.5&blocked_threshold=0.8
```

---

## Message Flow

```
Child types message
      │
      ▼
POST /messages/send  (JWT)
      │
      ▼
AIService → POST :8000/analyze → classifier.predict(text) → score
      │
      ├─ score < 0.5  ──► safe
      │                    ├─ +10 pts reward
      │                    ├─ SignalR → receiver (full message)
      │                    └─ 200 { status: "sent" }
      │
      ├─ 0.5–0.8  ────► flagged
      │                    ├─ store in FlaggedMessages (masked)
      │                    ├─ FCM multicast → all parents/teachers
      │                    ├─ SignalR "parents" group → dashboard alert
      │                    ├─ SignalR → receiver (masked message)
      │                    └─ 200 { status: "masked", maskedMessage }
      │
      └─ ≥ 0.8  ──────► blocked
                           ├─ store in FlaggedMessages
                           ├─ FCM multicast → all parents/teachers
                           ├─ SignalR "parents" group → dashboard alert
                           └─ 200 { status: "blocked" }  (not delivered)
```

---

## Toxicity Thresholds

| Score | Label | Behaviour |
|-------|-------|-----------|
| < 0.5 | `safe` | Deliver + award 10 pts |
| 0.5–0.8 | `flagged` | Mask + store + alert |
| ≥ 0.8 | `blocked` | Block + store + alert |

Tune at runtime (no restart): `PATCH http://localhost:8000/config?flagged_threshold=0.4&blocked_threshold=0.75`

---

## Reward System

| Points | Badge |
|--------|-------|
| 100 | 💬 Safe Chatter |
| 500 | 🌟 Kind Star |
| 1000 | 🦸 Cyber Hero |
| 2000 | 🎓 Chat Scholar |
| 5000 | 👑 Safety King |
| 10000 | 🚀 Legend |

Every safe message = +10 points. Badges auto-awarded when threshold crossed.

---

## Project Structure

```
KidSafe.slnx
├── KidSafe.Backend/
│   ├── Controllers/   Auth · Messages · Dashboard · Rewards
│   ├── Data/          AppDbContext · Entities (User, FlaggedMessage, Reward)
│   ├── Hubs/          ChatHub (typed IChatClient)
│   ├── Services/      AI · Auth · Notification
│   ├── Program.cs
│   └── appsettings.json
├── KidSafe.Frontend/
│   ├── Pages/         Splash · Login · Register · ChildChat · Rewards · ParentDashboard · TeacherPanel
│   ├── Services/      Api · AuthState · ChatHub · Fcm
│   └── wwwroot/       css/app.css · js/firebase-messaging.js · firebase-messaging-sw.js
├── KidSafe.Shared/
│   ├── DTOs/          Auth · Message · Dashboard
│   └── Interfaces/    IChatHubClient · IChatHubServer
└── KidSafe.AI/
    ├── main.py        FastAPI app
    ├── classifier.py  BaseClassifier → Stub | HFPipeline | Custom ← YOUR MODEL
    ├── config.py      Pydantic settings
    └── models.py      Request/Response schemas
```

---

## Swapping in Your Own Model

```python
# KidSafe.AI/classifier.py → CustomClassifier

def __init__(self):
    self.tokenizer = ...   # load your tokenizer
    self.model     = ...   # load your weights

async def predict(self, text: str) -> float:
    # run inference, return 0.0 (safe) → 1.0 (toxic)
    return your_model.score(text)
```

```
# .env
MODEL_BACKEND=custom
```

Restart the AI service — nothing else changes.

---

## Common Issues

| Problem | Fix |
|---------|-----|
| SignalR 403 | Check JWT token is passed as `?access_token=` query param |
| CORS error | Add frontend origin to `AllowedOrigins` in `appsettings.json` |
| FCM not sending | Check `firebase-credentials.json` exists and `Firebase:CredentialFile` path is correct |
| AI 503 | AI service not running — `uvicorn main:app --port 8000` |
| `MODEL_BACKEND=hf_pipeline` slow first load | TinyBERT downloads on first run (~60 MB) — subsequent starts use cache |
| SQLite locked | Only one backend instance can write; use PostgreSQL for multi-instance prod |
