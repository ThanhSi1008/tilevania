# 🎮 Tilevania CLIENT-SERVER INTEGRATION PLAN

**Document Version:** 1.0  
**Date Created:** December 12, 2025  
**Status:** Planning Phase  
**Estimated Duration:** 3-4 weeks (Phases 1-4)

---

## 📋 Executive Summary

This document outlines a **4-phase integration plan** to connect the existing Tilevania Unity game with the Node.js/Express backend server. The goal is to:

1. ✅ Enable player authentication (register/login)
2. ✅ Sync game progress to cloud database
3. ✅ Enable multiplayer leaderboards
4. ✅ Unlock achievements during gameplay
5. ✅ Persist player stats across sessions

---

## 📊 Current State Analysis

### Client-Side (Tilevania Unity Game)
**Status:** Local gameplay functional, no server integration

**Existing Systems:**
- PlayerMovement.cs - Controls player character
- GameSession.cs - Manages lives and score (local only)
- CoinPickup.cs - Coin collection logic
- EnemyMovement.cs - Enemy AI
- LevelExit.cs - Level completion logic
- 3 Playable Levels (Level 1, 2, 3)

**Current Data Storage:**
- Lives: Local int variable
- Score: Local int variable
- Coins: Not persisted
- Level Progress: Not persisted
- Player Identity: None (anonymous)

**UI System:**
- TextMeshPro for lives and score display
- Scene-based level management

---

### Server-Side (Node.js Backend)
**Status:** ✅ Fully operational (Phases 1-4 complete)

**Existing APIs:**
- 39 REST endpoints
- MongoDB database with 8 collections
- JWT authentication
- User accounts with profiles
- Level management
- Game session tracking
- Achievement system
- Leaderboard system

**Database Ready:**
- Users collection (auth)
- GameProfiles collection (stats)
- Levels collection (level data)
- GameSessions collection (session history)
- LevelProgress collection (per-level tracking)
- Achievements collection (achievement definitions)
- PlayerAchievements collection (unlocks)
- Leaderboards collection (rankings)

---

## 🎯 Integration Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    TILEVANIA GAME (Unity)                    │
├─────────────────────────────────────────────────────────────┤
│  UI Layer (Login, Menu, HUD)                                │
│  ├─ LoginManager.cs (NEW)                                   │
│  ├─ MainMenuManager.cs (NEW)                                │
│  └─ HUDManager.cs (MODIFIED)                                │
├─────────────────────────────────────────────────────────────┤
│  Game Logic Layer                                            │
│  ├─ GameSession.cs (MODIFIED) - API integration             │
│  ├─ PlayerMovement.cs (EXISTING)                            │
│  ├─ CoinPickup.cs (MODIFIED) - Stat tracking               │
│  ├─ EnemyMovement.cs (EXISTING)                             │
│  └─ LevelExit.cs (MODIFIED) - Session completion            │
├─────────────────────────────────────────────────────────────┤
│  Network Layer                                              │
│  ├─ APIClient.cs (NEW) - HTTP client wrapper               │
│  ├─ APIConfig.cs (NEW) - Configuration                      │
│  └─ AuthManager.cs (NEW) - Token management                │
├─────────────────────────────────────────────────────────────┤
│  Data Layer                                                  │
│  ├─ PlayerData.cs (NEW) - Local cache                       │
│  └─ SessionManager.cs (NEW) - Session tracking             │
└─────────────────────────────────────────────────────────────┘
           ↓ (HTTPS/REST API)
┌─────────────────────────────────────────────────────────────┐
│               GAMESERVER (Node.js/Express)                   │
├─────────────────────────────────────────────────────────────┤
│  Authentication Routes         /api/auth/*                   │
│  User Routes                   /api/users/*                  │
│  Level Routes                  /api/levels/*                 │
│  GameSession Routes            /api/sessions/*               │
│  GameProfile Routes            /api/gameProfile/*            │
│  LevelProgress Routes          /api/levelProgress/*          │
│  Achievement Routes            /api/achievements/*           │
│  Leaderboard Routes            /api/leaderboard/*            │
├─────────────────────────────────────────────────────────────┤
│               MongoDB Database                               │
│  (users, gameprofiles, levels, sessions, etc.)             │
└─────────────────────────────────────────────────────────────┘
```

---

## 📅 Implementation Phases

### ⏳ PHASE 1: Authentication & Account Management (Week 1)
**Duration:** 3-4 days  
**Goal:** Enable player login and create authenticated sessions

#### Objectives:
1. Create APIClient.cs for HTTP communication
2. Create APIConfig.cs with server settings
3. Create AuthManager.cs for token management
4. Create LoginManager.cs UI screen
5. Implement user registration endpoint
6. Implement user login endpoint
7. Store JWT token locally
8. Create auto-login functionality
9. Handle authentication errors

#### Tasks:
```
PHASE 1.1: API Communication Layer
  ✓ APIClient.cs - Generic HTTP client for GET/POST/PUT/DELETE
  ✓ APIConfig.cs - Base URL, endpoints, configuration
  ✓ AuthManager.cs - JWT token storage and validation
  
PHASE 1.2: Authentication UI
  ✓ LoginManager.cs - Login screen with username/password input
  ✓ RegisterManager.cs - Registration screen
  ✓ Main Menu - Navigation between login/register/play
  
PHASE 1.3: Session Management
  ✓ PlayerData.cs - Cache user data locally
  ✓ SessionManager.cs - Track current user and token
  ✓ Auto-login on game start (if token valid)
```

#### Expected Outcomes:
- ✅ Login screen displays in game
- ✅ User can register with email/username
- ✅ User can login and receive JWT token
- ✅ Token persists between game sessions
- ✅ Game auto-logs in if token still valid
- ✅ Menu shows current player username

#### API Endpoints Used:
- `POST /api/auth/register` - Create new account
- `POST /api/auth/login` - Authenticate user
- `GET /api/users/:userId` - Get profile info

---

### ⏳ PHASE 2: Game Session Tracking (Week 1-2)
**Duration:** 3-4 days  
**Goal:** Sync gameplay data with server in real-time

#### Objectives:
1. Modify GameSession.cs to integrate with API
2. Track game start, end, and duration
3. Upload score and coins to server
4. Sync deaths and enemy defeats
5. Update lives on HUD from server
6. Handle offline mode gracefully
7. Retry failed uploads

#### Tasks:
```
PHASE 2.1: Session Lifecycle Integration
  ✓ OnGameStart() - Create session on server
  ✓ OnGameUpdate() - Send periodic stat updates
  ✓ OnGameEnd() - Complete session and sync final stats
  ✓ SessionManager.cs - Manage active session ID
  
PHASE 2.2: Real-time Data Sync
  ✓ Modify GameSession.cs AddToScore() to sync
  ✓ Modify GameSession.cs ProcessPlayerDeath() to sync
  ✓ Modify CoinPickup.cs to track coin count
  ✓ Sync all stats when level completes
  
PHASE 2.3: Offline Support
  ✓ Queue stats if offline (retry on reconnect)
  ✓ Show connection status indicator
  ✓ Cache last known stats locally
```

#### Expected Outcomes:
- ✅ Starting a level creates session on server
- ✅ Score/coins sync every 10 seconds or on change
- ✅ Deaths sync to server when player dies
- ✅ Completing level sends final stats to server
- ✅ Player can continue playing if offline (optional)
- ✅ Game profile on server reflects gameplay stats

#### API Endpoints Used:
- `POST /api/sessions` - Start game session
- `PUT /api/sessions/:sessionId` - Update session stats
- `POST /api/sessions/:sessionId/end` - Complete session
- `POST /api/gameProfile/:userId/score` - Add score
- `POST /api/gameProfile/:userId/coins` - Add coins
- `POST /api/gameProfile/:userId/death` - Track death

---

### ⏳ PHASE 3: Level Progress & Achievements (Week 2)
**Duration:** 3-4 days  
**Goal:** Track per-level progress and unlock achievements

#### Objectives:
1. Modify LevelExit.cs to track level completion
2. Sync best scores and coins per level
3. Sync level completion status
4. Check for achievement unlocks
5. Display achievement notifications
6. Update achievement progress
7. Show achievement list in menu

#### Tasks:
```
PHASE 3.1: Level Progress Integration
  ✓ OnLevelComplete() in LevelExit.cs - Send completion to server
  ✓ LevelProgressManager.cs - Track per-level stats
  ✓ Sync best score, best time, coins collected
  ✓ Mark level as completed on server
  ✓ Unlock next level if available
  
PHASE 3.2: Achievement System
  ✓ AchievementManager.cs - Check achievements after session
  ✓ Display achievement unlock popup
  ✓ Play achievement unlock sound/animation
  ✓ AchievementListUI.cs - Show all achievements
  ✓ Track progress toward achievements
  
PHASE 3.3: Achievement Notifications
  ✓ AchievementNotification.cs - Popup prefab
  ✓ Toast notifications for unlocks
  ✓ Achievement progress bar (e.g., "3/100 enemies")
```

#### Expected Outcomes:
- ✅ Completing level sends completion data to server
- ✅ LevelProgress shows best scores for each level
- ✅ Achievements unlock during gameplay
- ✅ Achievement popup displays when unlocked
- ✅ Achievement list shows all 8 achievements
- ✅ Progress displayed toward incomplete achievements
- ✅ Achievement points reflected in game profile

#### API Endpoints Used:
- `POST /api/levelProgress/:userId/:levelId/complete` - Mark complete
- `PUT /api/levelProgress/:userId/:levelId` - Update progress
- `GET /api/achievements` - Get all achievements
- `GET /api/achievements/:userId/unlocked` - Get user's achievements
- `POST /api/achievements/:userId/unlock/:achievementId` - Unlock

---

### ⏳ PHASE 4: Leaderboards & Multiplayer Features (Week 3)
**Duration:** 3-4 days  
**Goal:** Enable social features and competitive elements

#### Objectives:
1. Create LeaderboardUI.cs for displaying rankings
2. Fetch global leaderboard from server
3. Show player's rank on HUD
4. Display weekly and daily leaderboards
5. Enable comparing stats with other players
6. Show recent player activity
7. Implement search for specific players

#### Tasks:
```
PHASE 4.1: Leaderboard Display
  ✓ LeaderboardManager.cs - Fetch and cache leaderboards
  ✓ LeaderboardUI.cs - Display global rankings
  ✓ Weekly/Daily leaderboard tabs
  ✓ Player highlight (show current player in list)
  ✓ Refresh leaderboard data on demand
  
PHASE 4.2: Player Stats Comparison
  ✓ PlayerProfileUI.cs - Show other player profiles
  ✓ Compare stats with selected player
  ✓ Show achievements earned by player
  ✓ Show player level progress
  
PHASE 4.3: Real-time Stats
  ✓ Show current player's rank in menu
  ✓ Display player's total score
  ✓ Show total coins collected
  ✓ Display total playtime
  ✓ Show achievement count "8/8"
  
PHASE 4.4: Social Features (Optional)
  ✓ Recent players list
  ✓ Player search functionality
  ✓ Follow/friend system (if expanded)
```

#### Expected Outcomes:
- ✅ Main menu shows player's current rank
- ✅ Leaderboard screen shows global rankings
- ✅ Weekly/daily tabs show different rankings
- ✅ Player can view other player profiles
- ✅ Player rank updates after each session
- ✅ Achievement count visible in profile
- ✅ Competitive element in gameplay

#### API Endpoints Used:
- `GET /api/leaderboard` - Get ALLTIME rankings
- `GET /api/leaderboard/weekly` - Get weekly rankings
- `GET /api/leaderboard/daily` - Get daily rankings
- `GET /api/leaderboard/rank/:userId` - Get player's rank
- `GET /api/users/:userId` - Get player profile
- `GET /api/gameProfile/:userId` - Get player stats

---

## 📁 New Files to Create

### C# Scripts for Unity

```
Assets/Scripts/
├── Network/
│   ├── APIClient.cs              # HTTP client wrapper
│   ├── APIConfig.cs              # Configuration and constants
│   └── APIResponse.cs            # Response wrapper for type safety
├── Managers/
│   ├── AuthManager.cs            # JWT token management
│   ├── SessionManager.cs          # Game session tracking
│   ├── PlayerData.cs             # Local player data cache
│   ├── LevelProgressManager.cs   # Level progress tracking
│   ├── AchievementManager.cs     # Achievement system
│   └── LeaderboardManager.cs     # Leaderboard fetching
├── UI/
│   ├── LoginManager.cs           # Login screen
│   ├── RegisterManager.cs        # Register screen
│   ├── MainMenuManager.cs        # Main menu navigation
│   ├── HUDManager.cs             # HUD updates (MODIFIED)
│   ├── LeaderboardUI.cs          # Leaderboard display
│   ├── AchievementListUI.cs      # Achievement list
│   ├── PlayerProfileUI.cs        # Player profile view
│   └── AchievementNotification.cs # Unlock popup
└── Models/
    ├── UserModel.cs              # User data structure
    ├── GameSessionModel.cs       # Session data structure
    ├── AchievementModel.cs       # Achievement data structure
    └── LeaderboardModel.cs       # Leaderboard data structure

Modified Files:
├── GameSession.cs                # Add API integration
├── LevelExit.cs                  # Add level completion sync
├── CoinPickup.cs                 # Add coin tracking
├── ScenePersist.cs               # Modify for auth check
└── PlayerMovement.cs             # Optional: Add analytics
```

---

## 🔌 API Integration Summary

### Authentication Endpoints
```csharp
// Register new account
POST /api/auth/register
Request: { username, email, password }
Response: { token, user { id, username, email } }

// Login with credentials
POST /api/auth/login
Request: { email, password }
Response: { token, user { id, username, email } }
```

### Game Session Endpoints
```csharp
// Start new game session
POST /api/sessions
Request: { userId, levelId }
Response: { sessionId, startTime }

// Update session stats (periodic)
PUT /api/sessions/:sessionId
Request: { score, coins, enemiesDefeated, deaths }
Response: { success }

// End game session
POST /api/sessions/:sessionId/end
Request: { score, coins, enemiesDefeated, deaths, status }
Response: { sessionId, totalScore, duration }
```

### Game Profile Endpoints
```csharp
// Get user's overall stats
GET /api/gameProfile/:userId
Response: { totalScore, coins, enemies, deaths, playtime }

// Add score
POST /api/gameProfile/:userId/score
Request: { points }
Response: { newTotalScore }

// Add coins
POST /api/gameProfile/:userId/coins
Request: { amount }
Response: { newTotalCoins }

// Track death
POST /api/gameProfile/:userId/death
Request: { }
Response: { totalDeaths }
```

### Level Progress Endpoints
```csharp
// Get all level progress
GET /api/levelProgress/:userId
Response: [ { levelId, completed, bestScore, coins } ]

// Update level progress
PUT /api/levelProgress/:userId/:levelId
Request: { coins, enemiesDefeated, bestScore }
Response: { levelId, bestScore, coins }

// Mark level completed
POST /api/levelProgress/:userId/:levelId/complete
Request: { finalScore, coins }
Response: { levelId, completed, completedAt }
```

### Achievement Endpoints
```csharp
// Get all available achievements
GET /api/achievements
Response: [ { id, name, description, points, rarity } ]

// Get user's unlocked achievements
GET /api/achievements/:userId/unlocked
Response: [ { id, name, unlockedAt, points } ]

// Unlock achievement
POST /api/achievements/:userId/unlock/:achievementId
Request: { }
Response: { achievement, points, newTotal }
```

### Leaderboard Endpoints
```csharp
// Get global (ALLTIME) leaderboard
GET /api/leaderboard
Response: [ { rank, userId, username, totalScore } ]

// Get weekly leaderboard
GET /api/leaderboard/weekly
Response: [ { rank, userId, username, totalScore } ]

// Get player's rank
GET /api/leaderboard/rank/:userId
Response: { userId, username, rank, totalScore }
```

---

## 🛡️ Security Considerations

### Token Storage Risk (Phase 1)
- Hiện đang dự kiến lưu JWT trong `PlayerPrefs`, vốn là file XML/registry không mã hóa trên Android/PC. Token có thể bị sao chép và dùng để giả mạo người chơi.
- Nâng cấp: mã hóa chuỗi token trước khi lưu (XOR/AES đơn giản) hoặc dùng plugin bảo mật (vd. SecurePlayerPrefs). Giải mã khi đọc, xóa sạch token khi logout.

### Loading State (UI/UX Guard)
- Vấn đề: Khi mạng lag, người dùng có thể bấm nút Login nhiều lần → Unity gửi nhiều request song song và trả về nhiều token → lỗi logic.
- Nâng cấp: thêm `LoadingOverlay` (nền mờ + spinner) và chặn toàn bộ input trong lúc gọi API.
- Luồng: `ShowLoading()` → Gọi API → `HideLoading()`. Chỉ cho phép thao tác khi overlay đã tắt.

### Client Versioning (Quan trọng cho Mobile)
- Vấn đề: App cũ (chưa cập nhật) gửi payload sai khi server đã đổi logic → server crash/DB lỗi.
- Nâng cấp: trong `APIConfig.cs` thêm `const string CLIENT_VERSION = "1.0.0";` và gửi header `x-client-version` trên mọi request. Server kiểm tra version và trả lỗi yêu cầu cập nhật nếu quá cũ.

### JWT Token Management
- Store JWT token in PlayerPrefs (or secure storage)
- Include token in `Authorization: Bearer <token>` header
- Refresh token on app startup if expired (optional)
- Clear token on logout
- Validate token before making API calls

### Data Validation
- Validate user input on client side
- Server validates all data on backend
- Don't trust client-side score modifications
- Validate session ownership before updating

### HTTPS/SSL
- Use HTTPS for all API calls
- Validate SSL certificates
- Handle certificate pinning if needed

### Rate Limiting
- Implement request throttling on client
- Queue rapid stat updates
- Avoid spamming API endpoints

---

## 🧪 Testing Strategy

### Phase 1 Testing (Auth)
```
Test Cases:
✓ Register new user
✓ Login with correct credentials
✓ Login with wrong password (error)
✓ Register with duplicate email (error)
✓ Token persists in PlayerPrefs
✓ Auto-login on app restart
✓ Logout clears token
```

### Phase 2 Testing (Sessions)
```
Test Cases:
✓ Game session creates on level start
✓ Score updates sync to server
✓ Coins sync to server
✓ Death counter increments
✓ Session completes when level exits
✓ Stats persist in GameProfile
✓ Offline mode queues updates
```

### Phase 3 Testing (Progress & Achievements)
```
Test Cases:
✓ Level marked as completed
✓ Best score saved per level
✓ Achievement unlocks when condition met
✓ Achievement notification displays
✓ Points added to game profile
✓ Multiple levels show different progress
```

### Phase 4 Testing (Leaderboards)
```
Test Cases:
✓ Leaderboard displays rankings
✓ Player rank visible in menu
✓ Weekly/daily rankings differ
✓ Player profile viewable from leaderboard
✓ Rank updates after new session
```

---

## 🚀 Development Environment Setup

### Server Requirements
```
✓ Node.js server running on localhost:3000 (or cloud)
✓ MongoDB database running
✓ All API endpoints tested with Postman/Insomnia
✓ CORS enabled for localhost:* during development
```

### Client Requirements
```
✓ Unity 2022 LTS or newer
✓ TextMeshPro updated
✓ Newtonsoft.Json (JSON.NET) package for requests
✓ Rest Client package or use Unity's UnityWebRequest
```

### Configuration
```csharp
// APIConfig.cs
#if UNITY_EDITOR
public const string API_BASE_URL = "http://localhost:3000";
#else
public const string API_BASE_URL = "https://api.tilevania.com";
#endif
public const string CLIENT_VERSION = "1.0.0"; // gửi trong header x-client-version
```

---

## 📊 Estimated Timeline

| Phase | Task | Duration | Start | End |
|-------|------|----------|-------|-----|
| 1 | Auth & Login | 3-4 days | Week 1 | Week 1 |
| 2 | Session Tracking | 3-4 days | Week 1 | Week 2 |
| 3 | Progress & Achievements | 3-4 days | Week 2 | Week 2 |
| 4 | Leaderboards & Social | 3-4 days | Week 3 | Week 3 |
| Testing | Integration testing | 2-3 days | Week 3 | Week 4 |
| **Total** | **Full Integration** | **~4 weeks** | **Week 1** | **Week 4** |

---

## ✅ Success Criteria

### Phase 1 Success
- [ ] Login screen displays
- [ ] User can register and login
- [ ] JWT token stored and validated
- [ ] Auto-login works

### Phase 2 Success
- [ ] Game sessions created and tracked
- [ ] Score/coins sync in real-time
- [ ] Stats visible on server database
- [ ] Offline mode works (optional)

### Phase 3 Success
- [ ] Level progress tracked per player
- [ ] Achievements unlock during gameplay
- [ ] Notifications display on unlock
- [ ] Achievement list shows all items

### Phase 4 Success
- [ ] Leaderboard displays correctly
- [ ] Player rank visible in menu
- [ ] Weekly/daily rankings working
- [ ] Social features functional

### Overall Success
- [ ] Fully integrated client-server system
- [ ] No data loss between sessions
- [ ] Smooth multiplayer experience
- [ ] Responsive UI with no lag
- [ ] < 200ms API response times

---

## 📝 Implementation Checklist

### PHASE 1: Authentication
- [ ] Create APIClient.cs with GET/POST/PUT/DELETE methods
- [ ] Create APIConfig.cs with base URL and endpoints
- [ ] Create AuthManager.cs for token storage
- [ ] Create LoginManager.cs UI screen
- [ ] Create RegisterManager.cs UI screen
- [ ] Create SessionManager.cs for user tracking
- [ ] Create PlayerData.cs for local caching
- [ ] Implement registration endpoint
- [ ] Implement login endpoint
- [ ] Add auto-login on app start
- [ ] Test all auth flows
- [ ] Handle error cases (duplicate email, wrong password, etc.)

### PHASE 2: Game Sessions
- [ ] Modify GameSession.cs to track session ID
- [ ] Implement OnGameStart() to create session
- [ ] Implement OnGameEnd() to complete session
- [ ] Sync score updates to API
- [ ] Sync coin updates to API
- [ ] Sync death counter to API
- [ ] Implement periodic stat syncing (every 10s)
- [ ] Implement offline mode queue
- [ ] Add connection status indicator
- [ ] Test complete game flow
- [ ] Verify stats on server database

### PHASE 3: Progress & Achievements
- [ ] Modify LevelExit.cs to track completion
- [ ] Create LevelProgressManager.cs
- [ ] Implement level completion API call
- [ ] Create AchievementManager.cs
- [ ] Implement achievement checking logic
- [ ] Create AchievementNotification.cs UI
- [ ] Create AchievementListUI.cs
- [ ] Implement achievement display
- [ ] Test all achievement unlock conditions
- [ ] Test level progress persistence

### PHASE 4: Leaderboards
- [ ] Create LeaderboardManager.cs
- [ ] Create LeaderboardUI.cs
- [ ] Implement fetch global leaderboard
- [ ] Implement fetch weekly leaderboard
- [ ] Implement fetch daily leaderboard
- [ ] Create PlayerProfileUI.cs
- [ ] Show player rank in main menu
- [ ] Show player stats in HUD
- [ ] Implement player search (optional)
- [ ] Test all leaderboard views

---

## 🎯 Key Design Decisions

### Authentication Strategy
- **JWT Tokens:** Stateless authentication, can be used across multiple servers
- **Token Storage:** PlayerPrefs (simplest) or SecurePlayerPrefs (more secure)
- **Token Expiry:** 7 days (can be refreshed)

### Data Sync Strategy
- **Hybrid Approach:** Keep local copy + sync with server
- **Frequency:** Update server on events (score, death, level complete)
- **Conflict Resolution:** Server version is source of truth

### Offline Support
- **Queue System:** Queue failed requests and retry when online
- **Local Cache:** Cache last known good state locally
- **Status Indicator:** Show connection status to player

### Leaderboard Caching
- **Cache Duration:** 5 minutes (prevents excessive API calls)
- **Force Refresh:** Allow manual refresh button
- **Auto-update:** Update when player's rank changes

---

## 📚 Dependencies & Libraries

### Unity Packages
- **Newtonsoft.Json:** For JSON serialization/deserialization
- **TextMeshPro:** Already in project (for UI)
- **UnityWebRequest:** Built-in (for HTTP)

### Server Dependencies
- **Express.js:** Already installed
- **Mongoose:** Already installed
- **JWT:** Already installed
- **CORS:** Already enabled

---

## 🔄 Next Steps

### Immediate Actions (Before Phase 1)
1. ✅ Review this document and confirm approach
2. ✅ Setup development environment
3. ✅ Test all API endpoints with Postman
4. ✅ Create API documentation (Swagger)

### Phase 1 Start
1. Create APIClient.cs with HTTP methods
2. Create AuthManager.cs for token management
3. Create LoginManager.cs UI
4. Implement register and login flows
5. Test authentication end-to-end

---

## 📞 Communication Protocol

### Error Handling
All API responses should follow this format:

**Success Response (200/201):**
```json
{
  "success": true,
  "data": { /* response data */ }
}
```

**Error Response (400/401/500):**
```json
{
  "success": false,
  "error": "Error message",
  "statusCode": 400,
  "details": { /* detailed errors */ }
}
```

### Client-Side Error Handling
```csharp
try {
  var response = await APIClient.POST("/api/auth/login", data);
  if (response.success) {
    // Handle success
  } else {
    // Show error message
    ShowError(response.error);
  }
} catch (Exception ex) {
  ShowError("Network error: " + ex.Message);
}
```

---

## 📖 Documentation References

### Server Documentation
- See `GameServer/README.md` for API endpoint details
- See `GameServer/IMPLEMENTATION_SUMMARY.md` for architecture

### Phase Documentation
- Each phase has detailed task breakdown above
- Check success criteria before marking complete

---

**Document Status:** ✅ Ready for Implementation  
**Last Updated:** December 12, 2025  
**Next Review:** After Phase 1 completion
