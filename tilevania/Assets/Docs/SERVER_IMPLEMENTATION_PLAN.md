# Tilevania Server - Implementation Plan

## Tổng Quan Dự Án
**Project:** Tilevania Backend Server  
**Stack:** Node.js + Express + MongoDB  
**Thời gian ước tính:** 4-6 tuần  
**Mục tiêu:** Xây dựng backend để lưu trữ dữ liệu trò chơi, quản lý người chơi, và hỗ trợ multiplayer features

---

## Timeline Tổng Quát

```
Phase 1: Setup & Foundation          [Week 1]           ███░░░░░░
Phase 2: Authentication & Users      [Week 2]           ░███░░░░░
Phase 3: Game Core API               [Week 2-3]         ░░███░░░░
Phase 4: Advanced Features           [Week 4]           ░░░░███░░
Phase 5: Testing & Optimization      [Week 5-6]         ░░░░░░███
Phase 6: Deployment & Monitoring     [Week 6]           ░░░░░░░██
```

---

## PHASE 1: Setup & Foundation (Week 1 - 3-4 ngày)

### 📋 Mục tiêu
- Thiết lập dự án Node.js/Express
- Kết nối MongoDB
- Thiết lập cấu trúc project chuẩn
- Cấu hình environment variables
- Setup database models cơ bản

### ✅ Tasks

#### 1.1 Project Initialization (1 ngày)
- [ ] `npm init` - tạo package.json
- [ ] Cài đặt dependencies cơ bản:
  - express
  - mongoose
  - dotenv
  - cors
  - body-parser
  - nodemon (dev)
  - babel (nếu dùng ES6)
- [ ] Tạo `.env` file với biến môi trường:
  - PORT
  - MONGODB_URI
  - NODE_ENV
  - JWT_SECRET
- [ ] Tạo `.gitignore`

#### 1.2 Project Structure (1 ngày)
```
server/
├── src/
│   ├── config/
│   │   └── database.js
│   ├── models/
│   │   ├── User.js
│   │   ├── GameProfile.js
│   │   ├── Level.js
│   │   └── index.js
│   ├── routes/
│   │   └── index.js
│   ├── controllers/
│   │   └── index.js
│   ├── middleware/
│   │   └── auth.js
│   ├── utils/
│   │   └── validators.js
│   └── app.js
├── .env
├── .env.example
├── .gitignore
├── package.json
└── server.js
```

- [ ] Tạo tất cả các thư mục cần thiết
- [ ] Tạo file `server.js` (entry point)
- [ ] Tạo file `app.js` (express configuration)

#### 1.3 Database Connection (1 ngày)
- [ ] Tạo `config/database.js`:
  - Kết nối MongoDB Mongoose
  - Xử lý connection errors
  - Logging connection status
- [ ] Test kết nối đến MongoDB
- [ ] Tạo database dump file (nếu cần)

#### 1.4 Base Models (0.5 ngày)
- [ ] Tạo `User` schema (cơ bản)
- [ ] Tạo `GameProfile` schema
- [ ] Tạo `Level` schema
- [ ] Thêm timestamps và indexes cơ bản

### 📊 Acceptance Criteria
- [x] Server chạy được trên `http://localhost:3000`
- [x] Kết nối MongoDB thành công
- [x] Có thể khởi chạy với `npm start` và `npm run dev`
- [x] Không có console errors

### 🛠️ Tech Stack
- Node.js (v18+)
- Express.js
- Mongoose
- MongoDB Atlas hoặc Local MongoDB

### 📌 Dependencies
- Không có dependencies từ phase khác

---

## PHASE 2: Authentication & User Management (Week 2 - 5-6 ngày)

### 📋 Mục tiêu
- Xây dựng hệ thống đăng ký/đăng nhập
- Tạo JWT authentication
- Quản lý tài khoản người chơi
- Bảo mật password

### ✅ Tasks

#### 2.1 User Schema & Validation (1.5 ngày)
- [ ] Hoàn thiện `User` model:
  - username (unique, required, 3-20 chars)
  - email (unique, required, valid email)
  - passwordHash (required)
  - createdAt, updatedAt
  - isActive (default: true)
  - lastLoginAt
  - profileImage
- [ ] Tạo password hashing:
  - Cài bcryptjs
  - Thêm pre-save hook để hash password
  - Tạo method `comparePassword()`
- [ ] Tạo validators trong `utils/validators.js`

#### 2.2 JWT Authentication (1.5 ngày)
- [ ] Cài `jsonwebtoken`
- [ ] Tạo `utils/jwt.js`:
  - `generateToken(userId)` - tạo token
  - `verifyToken(token)` - kiểm tra token
  - `decodeToken(token)` - decode token
- [ ] Tạo middleware `middleware/auth.js`:
  - `authenticateToken()` - verify JWT
  - `authorizeUser()` - kiểm tra owner
- [ ] Tạo refresh token mechanism (optional)

#### 2.3 Authentication Routes (2 ngày)
- [ ] Tạo `routes/auth.js`:
  - `POST /api/auth/register` - Đăng ký
    - Validate input
    - Check duplicate email/username
    - Hash password
    - Create user
    - Return token
  - `POST /api/auth/login` - Đăng nhập
    - Validate email/password
    - Compare password
    - Generate token
    - Update lastLoginAt
  - `POST /api/auth/logout` - Đăng xuất (optional)
  - `POST /api/auth/refresh` - Refresh token
- [ ] Tạo error handling cho authentication

#### 2.4 User Management Routes (1 ngày)
- [ ] Tạo `routes/users.js`:
  - `GET /api/users/:userId` - Lấy thông tin user
  - `PUT /api/users/:userId` - Cập nhật profile (username, profileImage, email)
  - `DELETE /api/users/:userId` - Xóa tài khoản
- [ ] Thêm authentication middleware vào routes

### 📊 Acceptance Criteria
- [x] Có thể đăng ký tài khoản mới
- [x] Có thể đăng nhập với email/username
- [x] JWT token được tạo và có thể xác thực
- [x] Password được hash, không lưu plain text
- [x] Protected routes yêu cầu token
- [x] Test đăng ký/đăng nhập với Postman/Thunder Client

### 🛠️ Tech Stack
- bcryptjs
- jsonwebtoken
- joi (validation)

### 📌 Dependencies
- Phase 1 (Setup) ✅

---

## PHASE 3: Game Core API (Week 2-3 - 5-6 ngày)

### 📋 Mục tiêu
- Xây dựng API lưu trữ tiến độ trò chơi
- Quản lý levels
- Theo dõi game sessions
- Cập nhật điểm và thống kê

### ✅ Tasks

#### 3.1 Game Models & Schemas (1 ngày)
- [ ] Hoàn thiện `GameProfile` model:
  - userId, totalScore, totalCoinsCollected
  - totalEnemiesDefeated, totalDeaths, totalPlayTime
  - currentLives, highestScoreAchieved
  - updatedAt
- [ ] Hoàn thiện `Level` model:
  - levelNumber, levelName, description
  - difficulty, maxCoins, maxEnemies
  - sceneName, isUnlocked
- [ ] Tạo `LevelProgress` model
- [ ] Tạo `GameSession` model
- [ ] Tạo indexes cho performance

#### 3.2 Level Management API (1.5 ngày)
- [ ] Tạo `controllers/levelController.js`:
  - `getAllLevels()` - Lấy tất cả levels
  - `getLevelById()` - Lấy level cụ thể
  - `createLevel()` - Tạo level (admin)
  - `updateLevel()` - Cập nhật level (admin)
- [ ] Tạo `routes/levels.js`
- [ ] Seed data 3 levels mặc định
- [ ] Test GET levels

#### 3.3 Game Progress API (2 ngày)
- [ ] Tạo `controllers/gameProfileController.js`:
  - `getGameProfile(userId)` - Lấy thông tin game
  - `updateGameProfile(userId, data)` - Cập nhật tiến độ chung
  - `addScore(userId, points)` - Thêm điểm
  - `addCoins(userId, coins)` - Thêm coin
  - `incrementDeathCount(userId)` - Tăng số lần chết
  - `updateCurrentLives(userId, lives)` - Cập nhật mạng
- [ ] Tạo `routes/gameProfile.js`
- [ ] Thêm auth middleware
- [ ] Test cập nhật score/coins

#### 3.4 Level Progress API (1.5 ngày)
- [ ] Tạo `controllers/levelProgressController.js`:
  - `getLevelProgress(userId, levelId)` - Lấy tiến độ level
  - `updateLevelProgress(userId, levelId, data)` - Cập nhật
  - `completedLevel(userId, levelId)` - Hoàn thành level
- [ ] Tạo `routes/levelProgress.js`
- [ ] Tính toán best score, best time, coin collected
- [ ] Test cập nhật level progress

#### 3.5 Game Session API (1 ngày)
- [ ] Tạo `controllers/sessionController.js`:
  - `startSession(userId, levelId)` - Bắt đầu phiên
  - `updateSession(sessionId, data)` - Cập nhật realtime
  - `endSession(sessionId, data)` - Kết thúc phiên
  - `getSessionHistory(userId)` - Lịch sử phiên
- [ ] Tạo `routes/sessions.js`
- [ ] Tính toán duration, status
- [ ] Test game session flow

### 📊 Acceptance Criteria
- [x] Có thể tạo/cập nhật game profile
- [x] Có thể lấy danh sách levels
- [x] Có thể lưu tiến độ từng level
- [x] Có thể tạo/kết thúc game session
- [x] Score, coins, deaths được cập nhật chính xác
- [x] Test complete game flow (start → update → end session)

### 🛠️ Tech Stack
- Mongoose (models)
- MongoDB aggregation (nếu cần)

### 📌 Dependencies
- Phase 1 (Setup) ✅
- Phase 2 (Auth) ✅

---

## PHASE 4: Advanced Features (Week 4 - 4-5 ngày)

### 📋 Mục tiêu
- Xây dựng hệ thống achievements
- Tạo leaderboard
- Thêm social features
- Analytics & statistics

### ✅ Tasks

#### 4.1 Achievement System (2 ngày)
- [ ] Tạo `Achievement` model:
  - name, description, icon
  - condition, points, rarity
- [ ] Tạo `PlayerAchievement` model
- [ ] Tạo `controllers/achievementController.js`:
  - `getAllAchievements()`
  - `getPlayerAchievements(userId)`
  - `unlockAchievement(userId, achievementId)`
  - `checkAchievements(userId)` - auto-unlock logic
- [ ] Tạo `routes/achievements.js`
- [ ] Seed 10-15 achievements cơ bản
- [ ] Tạo logic check achievements:
  - "First Kill" - tiêu diệt kẻ địch đầu tiên
  - "Coin Collector" - thu thập 100 coins
  - "No Death" - hoàn thành level không chết
  - Etc.

#### 4.2 Leaderboard System (1.5 ngày)
- [ ] Tạo `Leaderboard` model
- [ ] Tạo `controllers/leaderboardController.js`:
  - `getGlobalLeaderboard(limit)` - top players
  - `getWeeklyLeaderboard(limit)` - top week
  - `getDailyLeaderboard(limit)` - top ngày
  - `getPlayerRank(userId, period)` - rank của player
  - `calculateLeaderboard()` - tính toán (cron job)
- [ ] Tạo `routes/leaderboard.js`
- [ ] Cài `node-cron` cho scheduled tasks
- [ ] Tạo cron job cập nhật leaderboard hàng giờ

#### 4.3 Player Statistics (1 ngày)
- [ ] Tạo endpoints thống kê:
  - `GET /api/stats/user/:userId` - Stats cá nhân
  - `GET /api/stats/global` - Stats chung
  - `GET /api/stats/levels` - Statistics by level
- [ ] Tính toán:
  - Win rate, average score
  - Favorite level, best performance
  - Play time statistics

#### 4.4 Social Features (0.5 ngày)
- [ ] Tạo endpoints (optional):
  - Follow/unfollow user
  - View other player profiles
  - Compare statistics

### 📊 Acceptance Criteria
- [x] Có thể unlock achievements
- [x] Leaderboard tính toán chính xác
- [x] Cron job chạy định kỳ
- [x] Có thể view rank của player
- [x] Statistics API trả về data chính xác

### 🛠️ Tech Stack
- node-cron (scheduled tasks)
- Aggregation pipeline (MongoDB)

### 📌 Dependencies
- Phase 1-3 ✅

---

## PHASE 5: Testing & Optimization (Week 5 - 3-4 ngày)

### 📋 Mục tiêu
- Unit & Integration tests
- API documentation
- Performance optimization
- Security hardening

### ✅ Tasks

#### 5.1 Testing (2 ngày)
- [ ] Cài jest & supertest
- [ ] Viết unit tests cho models:
  - User creation, password hashing
  - GameProfile updates
  - Level progress calculation
- [ ] Viết integration tests cho API:
  - Auth flow (register → login)
  - Game flow (start → update → end session)
  - Achievement unlock
  - Leaderboard updates
- [ ] Test coverage minimum 70%
- [ ] Setup CI/CD pipeline (GitHub Actions)

#### 5.2 API Documentation (1 ngày)
- [ ] Cài Swagger (express-jsdoc hoặc swagger-ui)
- [ ] Document tất cả endpoints:
  - Request/response format
  - Error codes
  - Authentication required
- [ ] Tạo Postman collection
- [ ] Tạo API documentation markdown

#### 5.3 Performance Optimization (1 ngày)
- [ ] Thêm caching:
  - Redis for leaderboard
  - Cache frequently accessed data
- [ ] Optimize database queries:
  - Thêm indexes
  - Use projections để giảm data
  - Use aggregation pipelines
- [ ] Load testing với k6 hoặc Artillery
- [ ] Monitor performance metrics

#### 5.4 Security Hardening (0.5 ngày)
- [ ] Cài helmet.js (security headers)
- [ ] Thêm rate limiting (express-rate-limit)
- [ ] Validate & sanitize input
- [ ] Setup CORS properly
- [ ] Security audit

### 📊 Acceptance Criteria
- [x] Test coverage >= 70%
- [x] Tất cả tests pass
- [x] API documentation hoàn chỉnh
- [x] Response time < 200ms
- [x] Không có security vulnerabilities

### 🛠️ Tech Stack
- Jest, Supertest
- Swagger/OpenAPI
- Redis (optional)
- Helmet.js
- express-rate-limit

### 📌 Dependencies
- Phase 1-4 ✅

---

## PHASE 6: Deployment & Monitoring (Week 6 - 2-3 ngày)

### 📋 Mục tiêu
- Deploy server lên production
- Setup monitoring & logging
- Database backup
- CI/CD pipeline

### ✅ Tasks

#### 6.1 Server Deployment (1.5 ngày)
- [ ] Chọn hosting:
  - Heroku, Railway, Render (easy)
  - AWS EC2, DigitalOcean (VPS)
  - Docker + Kubernetes (advanced)
- [ ] Setup environment variables production
- [ ] Setup MongoDB Atlas (production database)
- [ ] Deploy application
- [ ] Setup domain & SSL certificate
- [ ] Test production endpoints

#### 6.2 CI/CD Pipeline (1 ngày)
- [ ] GitHub Actions workflow:
  - Run tests on push
  - Build Docker image (optional)
  - Deploy on merge to main
- [ ] Setup automatic testing trước deploy
- [ ] Rollback strategy

#### 6.3 Monitoring & Logging (0.5 ngày)
- [ ] Cài Winston hoặc Morgan (logging)
- [ ] Setup error tracking (Sentry)
- [ ] Monitor server uptime
- [ ] Setup alerts cho errors

#### 6.4 Database Backup & Scaling (0.5 ngày)
- [ ] Setup MongoDB backup (Atlas automatic backup)
- [ ] Disaster recovery plan
- [ ] Database scaling strategy
- [ ] Data retention policy

### 📊 Acceptance Criteria
- [x] Server chạy on production
- [x] HTTPS enabled
- [x] Logging & monitoring active
- [x] CI/CD pipeline hoạt động
- [x] Database backup đang chạy
- [x] Uptime > 99%

### 🛠️ Tech Stack
- Docker (optional)
- GitHub Actions
- Winston/Morgan (logging)
- Sentry (error tracking)
- MongoDB Atlas
- Hosting service

### 📌 Dependencies
- Phase 1-5 ✅

---

## 🎯 Key Checkpoints

| Checkpoint | Target | Status |
|-----------|--------|--------|
| Server chạy locally | End of Phase 1 | ⬜ |
| Auth working | End of Phase 2 | ⬜ |
| Game data saving | End of Phase 3 | ⬜ |
| Leaderboard working | End of Phase 4 | ⬜ |
| Tests passing (70%+) | End of Phase 5 | ⬜ |
| Deploy to production | End of Phase 6 | ⬜ |

---

## 📋 Checklist Hàng Tuần

### Week 1 (Phase 1)
- [ ] Project initialized
- [ ] MongoDB connected
- [ ] Base models created
- [ ] Can run `npm start`

### Week 2 (Phase 2 + Phase 3)
- [ ] User registration working
- [ ] User login with JWT
- [ ] Levels API done
- [ ] Game profile API done

### Week 3 (Phase 3 continued)
- [ ] Level progress API
- [ ] Game session API
- [ ] Test game flow end-to-end
- [ ] Fix any bugs found

### Week 4 (Phase 4)
- [ ] Achievement system
- [ ] Leaderboard system
- [ ] Statistics API
- [ ] Seed production data

### Week 5 (Phase 5)
- [ ] Unit tests written
- [ ] Integration tests done
- [ ] API documentation complete
- [ ] Performance optimizations

### Week 6 (Phase 6)
- [ ] Deploy to production
- [ ] Setup monitoring
- [ ] Database backup configured
- [ ] Go live!

---

## 🚀 Quick Start Commands

```bash
# Phase 1
npm init -y
npm install express mongoose dotenv cors body-parser
npm install --save-dev nodemon

# Phase 2
npm install bcryptjs jsonwebtoken joi

# Phase 4
npm install node-cron

# Phase 5
npm install --save-dev jest supertest
npm install swagger-ui-express swagger-jsdoc

# Running
npm start          # Production
npm run dev        # Development
npm test           # Test
```

---

## 📞 Communication Points

- **Daily**: Brief standup (nếu có team)
- **End of each phase**: Review & feedback
- **End of week**: Progress report
- **Issues tracking**: GitHub Issues hoặc Jira

---

## ❓ FAQ & Troubleshooting

### Q: Phải làm phase nào trước?
**A:** Phải tuần tự từ Phase 1 → 6. Không thể skip phase.

### Q: Có thể parallelize không?
**A:** Phase 2 & 3 có thể partial overlap (end of week 2).

### Q: Nếu delay?
**A:** Prioritize Phase 1-3 (core features). Phase 4-5 có thể delay.

### Q: Cần bao nhiêu người?
**A:** 1-2 người có thể làm, nhưng 1 person/phase tốt hơn.

---

## 📚 Reference & Resources

- [Express.js Guide](https://expressjs.com/)
- [MongoDB Mongoose](https://mongoosejs.com/)
- [JWT.io](https://jwt.io/)
- [Node.js Best Practices](https://github.com/goldbergyoni/nodebestpractices)
- [REST API Design Guidelines](https://restfulapi.net/)

---

**Last Updated:** December 2025  
**Next Review:** After Phase 1 completion
