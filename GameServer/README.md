# 🎮 Tilevania Game Server

**Project:** Tilevania Backend Server  
**Stack:** Node.js + Express + MongoDB  
**Status:** PHASE 4 Complete ✅

---

## 📋 Overview

Tilevania Server is a Node.js/Express backend for a 2D platformer game built with Unity. It handles player authentication, game progress tracking, achievements, leaderboards, and game sessions.

---

## 🚀 Quick Start

### Prerequisites
- Node.js v18+
- MongoDB running locally or MongoDB Atlas
- npm or yarn

### Installation

```bash
# Clone/navigate to project
cd GameServer

# Install dependencies
npm install

# Setup environment
cp .env.example .env
# Edit .env with your MongoDB URI

# Seed default data (levels & achievements)
npm run seed

# Start server
npm start
```

**Server runs on:** `http://localhost:3000`

---

## 📊 Database Models

### 1. **User**
- Authentication & profile management
- Fields: username, email, passwordHash, profileImage, etc.

### 2. **GameProfile**
- Player game statistics
- Fields: totalScore, coins, enemies defeated, deaths, playtime, lives, etc.

### 3. **Level**
- Game levels configuration
- Fields: levelNumber, name, difficulty, maxCoins, maxEnemies, etc.

### 4. **GameSession**
- Individual game play sessions
- Fields: userId, levelId, score, coins, enemies, deaths, status, etc.

### 5. **LevelProgress**
- Player progress on each level
- Fields: userId, levelId, bestScore, bestTime, coins, enemies, completions, etc.

### 6. **Achievement**
- Achievement definitions
- Fields: name, description, condition, points, rarity, etc.

### 7. **PlayerAchievement**
- User achievement unlocks
- Fields: userId, achievementId, unlockedAt, progress

### 8. **Leaderboard**
- Global rankings by period (ALLTIME, WEEKLY, DAILY)
- Fields: userId, rank, totalScore, username, period, etc.

---

## 🔌 API Endpoints

### Authentication
```
POST   /api/auth/register          - Register new user
POST   /api/auth/login             - Login user
```

### Users
```
GET    /api/users/:userId          - Get user profile
PUT    /api/users/:userId          - Update user profile
DELETE /api/users/:userId          - Delete user account
```

### Levels
```
GET    /api/levels                 - Get all levels
GET    /api/levels/:levelId        - Get level by ID
POST   /api/levels                 - Create level (admin)
PUT    /api/levels/:levelId        - Update level (admin)
```

### Game Profile
```
GET    /api/gameProfile/:userId             - Get game profile
PUT    /api/gameProfile/:userId             - Update game profile
POST   /api/gameProfile/:userId/score       - Add score
POST   /api/gameProfile/:userId/coins       - Add coins
POST   /api/gameProfile/:userId/death       - Increment deaths
PUT    /api/gameProfile/:userId/lives       - Update lives
POST   /api/gameProfile/:userId/playtime    - Add playtime
```

### Level Progress
```
GET    /api/levelProgress/:userId                      - Get all level progress
GET    /api/levelProgress/:userId/:levelId             - Get specific level progress
PUT    /api/levelProgress/:userId/:levelId             - Update level progress
POST   /api/levelProgress/:userId/:levelId/complete    - Complete level
```

### Game Sessions
```
POST   /api/sessions                      - Start new session
PUT    /api/sessions/:sessionId           - Update session (during gameplay)
POST   /api/sessions/:sessionId/end       - End session
GET    /api/sessions/:userId/history      - Get session history
```

### Achievements
```
GET    /api/achievements                          - Get all achievements
GET    /api/achievements/:userId/unlocked         - Get user achievements
POST   /api/achievements/:userId/unlock/:achievementId - Unlock achievement
```

### Leaderboard
```
GET    /api/leaderboard                    - Get global leaderboard
GET    /api/leaderboard/weekly             - Get weekly leaderboard
GET    /api/leaderboard/daily              - Get daily leaderboard
GET    /api/leaderboard/rank/:userId       - Get player rank
```

### Health Check
```
GET    /api/health                - Server health check
```

---

## 🔐 Authentication

All protected endpoints require JWT token in Authorization header:

```bash
Authorization: Bearer <token>
```

Token expires in 7 days.

---

## 📦 Available Scripts

```bash
npm start          # Start production server
npm run dev        # Start with nodemon (development)
npm run seed       # Seed default levels and achievements
npm run leaderboard # Calculate leaderboards
npm test           # Run tests
```

---

## 🎮 Game Flow Example

### 1. Register & Login
```bash
# Register
curl -X POST http://localhost:3000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "player1",
    "email": "player@example.com",
    "password": "password123"
  }'

# Login
curl -X POST http://localhost:3000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "player@example.com",
    "password": "password123"
  }'
```

### 2. Get Levels
```bash
curl http://localhost:3000/api/levels
```

### 3. Start Game Session
```bash
curl -X POST http://localhost:3000/api/sessions \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "<userId>",
    "levelId": "<levelId>"
  }'
```

### 4. End Game Session
```bash
curl -X POST http://localhost:3000/api/sessions/<sessionId>/end \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "COMPLETED",
    "finalScore": 500,
    "coinsCollected": 45,
    "enemiesDefeated": 8,
    "deathCount": 1,
    "livesRemaining": 2
  }'
```

### 5. Check Stats
```bash
curl http://localhost:3000/api/gameProfile/<userId> \
  -H "Authorization: Bearer <token>"
```

---

## 🏗️ Project Structure

```
GameServer/
├── src/
│   ├── config/
│   │   └── database.js              # MongoDB connection
│   ├── models/
│   │   ├── User.js
│   │   ├── GameProfile.js
│   │   ├── Level.js
│   │   ├── GameSession.js
│   │   ├── LevelProgress.js
│   │   ├── Achievement.js
│   │   ├── PlayerAchievement.js
│   │   ├── Leaderboard.js
│   │   └── index.js
│   ├── controllers/
│   │   ├── authController.js
│   │   ├── userController.js
│   │   ├── levelController.js
│   │   ├── gameProfileController.js
│   │   ├── levelProgressController.js
│   │   ├── sessionController.js
│   │   ├── achievementController.js
│   │   └── leaderboardController.js
│   ├── routes/
│   │   ├── auth.js
│   │   ├── users.js
│   │   ├── levels.js
│   │   ├── gameProfile.js
│   │   ├── levelProgress.js
│   │   ├── sessions.js
│   │   ├── achievements.js
│   │   └── leaderboard.js
│   ├── middleware/
│   │   └── auth.js                  # JWT authentication
│   ├── utils/
│   │   ├── jwt.js                   # JWT helpers
│   │   ├── validators.js            # Input validation
│   │   ├── seedDB.js                # Database seeding
│   │   ├── calculateLeaderboard.js  # Leaderboard calculation
│   │   └── index.js
│   └── app.js                       # Express app setup
├── server.js                        # Entry point
├── .env                             # Environment variables
├── .env.example                     # Example env file
├── package.json
└── README.md
```

---

## 🎯 Completed Phases

✅ **PHASE 1** - Setup & Foundation  
✅ **PHASE 2** - Authentication & User Management  
✅ **PHASE 3** - Game Core API  
✅ **PHASE 4** - Advanced Features (Achievements, Leaderboards)  
⏳ **PHASE 5** - Testing & Optimization  
⏳ **PHASE 6** - Deployment & Monitoring  

---

## 📝 Default Data

### Levels
1. **Level 1: Castle Entrance** (EASY) - 50 coins, 10 enemies
2. **Level 2: Dark Forest** (NORMAL) - 75 coins, 15 enemies
3. **Level 3: Dragon Lair** (HARD) - 100 coins, 20 enemies

### Achievements
- First Step - Defeat first enemy
- Coin Collector - Collect 100 coins
- Fortune Hunter - Collect 500 coins
- Monster Slayer - Defeat 100 enemies
- Score Master - Reach 1000 points
- Score Legend - Reach 5000 points
- Dedicated Player - Play 1 hour
- Obsessed Gamer - Play 24 hours

---

## 🛠️ Tech Stack

- **Framework:** Express.js
- **Database:** MongoDB + Mongoose
- **Authentication:** JWT (jsonwebtoken)
- **Password Hashing:** bcryptjs
- **Validation:** Joi
- **Environment:** dotenv
- **CORS:** cors package

---

## 📋 Notes

- All timestamps are in UTC
- Passwords are hashed with bcrypt (salt rounds: 10)
- JWT tokens expire after 7 days
- Protected routes require valid JWT token
- Some endpoints marked for admin use only (to be implemented)

---

## 🚀 Next Steps

- [ ] PHASE 5: Add comprehensive testing
- [ ] PHASE 6: Deploy to production (Heroku, AWS, etc.)
- [ ] Setup CI/CD pipeline
- [ ] Add API documentation with Swagger
- [ ] Setup monitoring and logging
- [ ] Database backup strategy

---

**Last Updated:** December 12, 2025  
**Version:** 1.0.0-alpha
