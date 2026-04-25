# 🎬 EXECUTION GUIDE - Commands to Run Now

**Time to Execute**: 30 minutes  
**Outcome**: Fully functional app with database migration  

---

## ✅ Step 1: Verify Build (5 minutes)

### Already Done ✅
The build was already verified successful:
```
Build Status: ✅ SUCCESS
Errors: 0
Warnings: 0
```

### Quick Verify
```powershell
# Navigate to solution
cd "D:\FYP\Cursor Code\KidSafeApp"

# Build again to confirm
dotnet build

# Expected output:
# Build succeeded! 0 error(s), 0 warning(s)
```

---

## 📦 Step 2: Create & Apply Database Migration (10 minutes)

### Command 1: Navigate to Backend
```powershell
cd "D:\FYP\Cursor Code\KidSafeApp\KidSafeApp.Backend"
```

### Command 2: Create Migration
```powershell
dotnet ef migrations add AddMessageIsReadProperty
```

**Expected Output:**
```
To undo this action, use 'dotnet ef migrations remove'
```

### Command 3: Update Database
```powershell
dotnet ef database update
```

**Expected Output:**
```
Done.
```

### Troubleshooting
If migration fails:
```powershell
# Check database connection
dotnet ef dbcontext info

# If connection string wrong, update appsettings.json:
# "Chat": "Server=YOUR_SERVER;Database=KidSafeApp;Integrated Security=true;Encrypt=false;"

# Then try update again
dotnet ef database update
```

---

## 🚀 Step 3: Start Backend (5 minutes)

### Command: Run Backend
```powershell
# Make sure you're in backend directory
cd "D:\FYP\Cursor Code\KidSafeApp\KidSafeApp.Backend"

# Run the backend
dotnet run
```

**Expected Output:**
```
Building...
Built successfully!
Listening on http://localhost:5000
Listening on https://localhost:5001
```

### Keep This Terminal Open
The backend should keep running. Open another terminal for next steps.

---

## 🌐 Step 4: Start Frontend (5 minutes)

### In New Terminal/PowerShell Window:

```powershell
# Navigate to frontend
cd "D:\FYP\Cursor Code\KidSafeApp\KidSafeApp"

# Run the frontend
dotnet run
```

**Expected Output:**
```
Building...
Built successfully!
Hosting environment: Development
Now listening on: https://localhost:7000
```

### Or Open in Browser
Once running, open browser to:
```
https://localhost:7000
```

---

## 🧪 Step 5: Test Messages (10 minutes)

### Test Scenario 1: Send Message
1. Login as User A (Child role)
2. Navigate to Chat
3. Select User B (Parent role)
4. Type message: "Hello, this is a test"
5. Click "Send"

**Expected Result:**
- ✅ Message disappears from input
- ✅ Message appears in chat list
- ✅ Green "Message sent!" notification appears
- ✅ Success notification auto-dismisses after 3 seconds

### Test Scenario 2: Receive Message
1. In separate browser/incognito:
   - Login as User B (Parent)
   - Go to Chat
2. In first browser (User A):
   - Send message to User B
3. In second browser (User B):
   - See message appear automatically (real-time via SignalR)

**Expected Result:**
- ✅ Message appears in real-time
- ✅ No page refresh needed
- ✅ Message displays with timestamp

### Test Scenario 3: Error Handling
1. Send message with empty content
2. Click Send

**Expected Result:**
- ✅ Red error notification appears
- ✅ Message: "Invalid recipient or message content."
- ✅ Input not cleared (can retry)

### Test Scenario 4: Network Error Simulation
1. Open browser DevTools (F12)
2. Go to Network tab
3. Throttle to "Offline"
4. Try to send message
5. See error
6. Go back to "No throttling"

**Expected Result:**
- ✅ Red error notification: "Connection error. Please check your network."
- ✅ On recovery, can send successfully
- ✅ Message eventually sent (if user retries)

### Test Scenario 5: Loading States
1. Send a message
2. Watch button during send

**Expected Result:**
- ✅ Button shows "Sending..." with spinner
- ✅ Input disabled during send
- ✅ Button disabled during send
- ✅ All re-enabled after send completes

### Test Scenario 6: Empty Chat State
1. Create new chat with user
2. Before loading messages

**Expected Result:**
- ✅ Loading spinner shows briefly
- ✅ "No messages yet. Start a conversation!" message appears

---

## ✅ Step 6: Verification Checklist

Run through this checklist to confirm everything works:

```
MESSAGES:
[ ] Send message successfully
[ ] Receive message in real-time
[ ] Message persists (refresh page, message still there)
[ ] Message history loads on chat open
[ ] Empty chat shows "No messages yet"

ERROR HANDLING:
[ ] Error notification appears on error
[ ] Error has clear message (not technical)
[ ] Error auto-dismisses after 5 seconds
[ ] Can manually dismiss error
[ ] Multiple errors stack (up to 5)

LOADING STATES:
[ ] Loading spinner shows while loading
[ ] Send button shows "Sending..." during send
[ ] Input disabled during send
[ ] Button disabled until message typed

SUCCESS FEEDBACK:
[ ] Success notification on message send
[ ] Green color for success
[ ] "Message sent!" message clear
[ ] Auto-dismisses after 3 seconds

REAL-TIME:
[ ] Receive messages without refresh
[ ] User online status updates immediately
[ ] No page reloads needed
[ ] SignalR connection stable
```

---

## 🐛 Troubleshooting Commands

### If Build Fails:
```powershell
# Clean build
dotnet clean
dotnet build
```

### If Migration Fails:
```powershell
# Check EF Core tools
dotnet ef --version

# Update if needed
dotnet tool update --global dotnet-ef

# List existing migrations
dotnet ef migrations list

# See pending migrations
dotnet ef migrations pending
```

### If Backend Won't Start:
```powershell
# Check port is available
netstat -ano | findstr :5001

# If port in use, kill process
taskkill /PID <PID> /F

# Or specify different port in launchSettings.json
```

### If Database Connection Fails:
```powershell
# Check connection string
cat appsettings.json | findstr "Chat"

# Update connection string if needed
# Look for "Chat": "Server=...;Database=KidSafeApp;..."

# Test connection
dotnet ef dbcontext info
```

### If Messages Not Showing:
```powershell
# Check browser console (F12)
# Look for JavaScript errors

# Check backend logs
# Look for SQL errors

# Verify API response
# Network tab → api/messages → check status code
```

---

## 📊 What to Look For

### Browser Console (F12 → Console)
Should see:
- ✅ No red errors
- ✅ Some info logs (optional)
- ⚠️ Some warnings are ok (browser specific)

### Network Tab (F12 → Network)
Should see:
- ✅ POST /api/messages → 200 OK
- ✅ GET /api/messages/{userId} → 200 OK
- ✅ WebSocket connection for SignalR
- ✅ No 400, 401, 500 errors

### Backend Console (where `dotnet run` running)
Should see:
- ✅ Messages sent logged
- ✅ Database queries executed
- ✅ No error stack traces
- ✅ SignalR connections tracked

---

## 🎯 Success Indicators

When everything works, you should see:

✅ **Backend**: "Listening on https://localhost:5001"  
✅ **Frontend**: "Now listening on: https://localhost:7000"  
✅ **Browser**: Chat page loads with user list  
✅ **Messages**: Can send and receive  
✅ **Notifications**: Error/success toasts appear  
✅ **Real-time**: SignalR updates work  
✅ **Database**: Messages persist  

---

## 🚀 After Verification

Once all tests pass:

### Option A: Keep Running
```powershell
# Keep both running and continue testing
# Then proceed to Phase 3
```

### Option B: Stop & Save
```powershell
# In backend terminal: Ctrl+C
# In frontend terminal: Ctrl+C

# All changes are saved to database
# Code changes are in git
```

---

## 📝 Next Phase Commands

When ready for Phase 3 (Service Abstraction):

```powershell
# Fresh build
cd "D:\FYP\Cursor Code\KidSafeApp"
dotnet clean
dotnet build

# Then start Phase 3 implementation
```

---

## ⏱️ Timeline

| Task | Time | Status |
|------|------|--------|
| Build verification | 5 min | ✅ Done |
| Create migration | 3 min | 👈 Next |
| Apply migration | 2 min | 👈 Next |
| Start backend | 2 min | 👈 Next |
| Start frontend | 2 min | 👈 Next |
| Test scenarios | 10 min | 👈 Next |
| Verification | 5 min | 👈 Next |
| **TOTAL** | **30 min** | 👈 Next |

---

## ✨ What You'll Have After

✅ Fully functional message system  
✅ Database with messages persisted  
✅ Real-time SignalR updates  
✅ Professional error handling  
✅ Professional notifications  
✅ Verified working end-to-end  
✅ Ready for next phase  

---

## 📞 Common Issues & Fixes

| Issue | Fix |
|-------|-----|
| Migration already exists | Check migrations folder, apply oldest first |
| Port 5001 in use | Change port in launchSettings.json |
| Blank chat list | Check token validity, check API response |
| Messages not persisting | Verify migration ran, check database |
| No real-time updates | Check SignalR connection in Network tab |
| Toast not showing | Verify ErrorAlert in MainLayout |
| Build fails | `dotnet clean` then `dotnet build` |

---

## 🎊 Ready?

You have everything you need. Just execute the commands in order:

1. ✅ Build verified
2. → Create migration
3. → Apply migration  
4. → Start backend
5. → Start frontend
6. → Test scenarios
7. → Verify all working
8. → Ready for Phase 3!

**Let's go! 🚀**

