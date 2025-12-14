# Tilevania - Database Schema Design

## Tổng Quan Game
**Tên Game:** Tilevania  
**Type:** 2D Platformer  
**Engine:** Unity  
**Công Nghệ:** C#, MongoDB, Node.js/Express  

---

## Phân Tích Dữ Liệu Cần Lưu

### 1. **Player (Người Chơi)**
Dữ liệu cơ bản của mỗi người chơi cần lưu trữ.

**Fields:**
- `userId` (ObjectId) - ID duy nhất của người chơi
- `username` (String) - Tên người chơi
- `email` (String) - Email (cho login)
- `passwordHash` (String) - Mật khẩu (hash)
- `createdAt` (DateTime) - Thời gian tạo tài khoản
- `lastLoginAt` (DateTime) - Lần đăng nhập gần nhất
- `profileImage` (String) - URL ảnh đại diện
- `isActive` (Boolean) - Trạng thái tài khoản

---

### 2. **GameProfile (Hồ Sơ Trò Chơi)**
Thông tin tiến độ và thống kê chơi game của người chơi.

**Fields:**
- `gameProfileId` (ObjectId) - ID hồ sơ game
- `userId` (ObjectId) - Reference đến Player
- `totalScore` (Integer) - Tổng điểm hiện tại
- `totalCoinsCollected` (Integer) - Tổng số coin đã thu thập
- `totalEnemiesDefeated` (Integer) - Tổng số kẻ địch đã tiêu diệt
- `totalDeaths` (Integer) - Tổng số lần chết
- `totalPlayTime` (Integer) - Tổng thời gian chơi (giây)
- `currentLives` (Integer) - Số mạng còn lại (default: 3)
- `highestScoreAchieved` (Integer) - Điểm cao nhất từng đạt được
- `lastSessionScore` (Integer) - Điểm session cuối cùng
- `updatedAt` (DateTime) - Cập nhật lần cuối

---

### 3. **Level (Cấp Độ)**
Thông tin chi tiết về các cấp độ trong game.

**Fields:**
- `levelId` (ObjectId) - ID cấp độ
- `levelNumber` (Integer) - Số thứ tự cấp độ (1, 2, 3...)
- `levelName` (String) - Tên cấp độ (vd: "Level 1: Castle Entrance")
- `description` (String) - Mô tả cấp độ
- `difficulty` (String) - Độ khó (EASY, NORMAL, HARD)
- `maxCoins` (Integer) - Số coin tối đa trong cấp độ
- `maxEnemies` (Integer) - Số kẻ địch tối đa
- `sceneName` (String) - Tên scene trong Unity (vd: "Level 1")
- `isUnlocked` (Boolean) - Cấp độ có được mở khóa hay không
- `requiredScoreToUnlock` (Integer) - Điểm cần thiết để mở cấp độ (nếu có)
- `createdAt` (DateTime)

---

### 4. **LevelProgress (Tiến Độ Cấp Độ)**
Theo dõi tiến độ của từng người chơi trên mỗi cấp độ.

**Fields:**
- `progressId` (ObjectId) - ID tiến độ
- `userId` (ObjectId) - Reference đến Player
- `levelId` (ObjectId) - Reference đến Level
- `isCompleted` (Boolean) - Cấp độ đã hoàn thành hay chưa
- `coinsCollected` (Integer) - Số coin đã thu thập ở cấp độ này
- `enemiesDefeated` (Integer) - Số kẻ địch đã tiêu diệt ở cấp độ này
- `deathCount` (Integer) - Số lần chết ở cấp độ này
- `bestScore` (Integer) - Điểm cao nhất ở cấp độ này
- `bestTime` (Integer) - Thời gian hoàn thành nhanh nhất (giây)
- `playCount` (Integer) - Số lần chơi cấp độ này
- `lastPlayedAt` (DateTime) - Lần chơi gần nhất
- `completedAt` (DateTime) - Thời gian hoàn thành lần đầu

---

### 5. **GameSession (Phiên Chơi)**
Theo dõi từng phiên chơi của người chơi.

**Fields:**
- `sessionId` (ObjectId) - ID phiên chơi
- `userId` (ObjectId) - Reference đến Player
- `levelId` (ObjectId) - Cấp độ đang chơi
- `startTime` (DateTime) - Thời gian bắt đầu
- `endTime` (DateTime) - Thời gian kết thúc
- `duration` (Integer) - Thời lượng phiên chơi (giây)
- `finalScore` (Integer) - Điểm cuối cùng
- `coinsCollected` (Integer) - Coin thu thập trong phiên này
- `enemiesDefeated` (Integer) - Kẻ địch tiêu diệt trong phiên này
- `deathCount` (Integer) - Số lần chết
- `livesRemaining` (Integer) - Số mạng còn lại khi kết thúc
- `sessionStatus` (String) - ACTIVE, COMPLETED, ABANDONED, FAILED
- `isCompleted` (Boolean) - Phiên chơi hoàn thành hay chưa

---

### 6. **Achievement (Thành Tích)**
Các thành tích/huy hiệu mà người chơi có thể đạt được.

**Fields:**
- `achievementId` (ObjectId) - ID thành tích
- `name` (String) - Tên thành tích (vd: "First Step", "Coin Collector")
- `description` (String) - Mô tả thành tích
- `icon` (String) - URL ảnh thành tích
- `condition` (String) - Điều kiện để đạt được
- `points` (Integer) - Điểm thành tích
- `rarity` (String) - Độ hiếm (COMMON, RARE, EPIC, LEGENDARY)

---

### 7. **PlayerAchievement (Thành Tích Của Người Chơi)**
Theo dõi thành tích đã đạt được của từng người chơi.

**Fields:**
- `playerAchievementId` (ObjectId) - ID
- `userId` (ObjectId) - Reference đến Player
- `achievementId` (ObjectId) - Reference đến Achievement
- `unlockedAt` (DateTime) - Thời gian đạt được thành tích
- `progress` (Integer) - Tiến độ (0-100%) nếu là thành tích tiến độ

---

### 8. **Leaderboard (Bảng Xếp Hạng)**
Bảng xếp hạng toàn cầu hoặc hàng ngày/hàng tuần.

**Fields:**
- `leaderboardId` (ObjectId) - ID bảng xếp hạng
- `userId` (ObjectId) - Reference đến Player
- `rank` (Integer) - Vị trí xếp hạng
- `totalScore` (Integer) - Tổng điểm
- `username` (String) - Tên người chơi
- `profileImage` (String) - Ảnh đại diện
- `period` (String) - ALLTIME, WEEKLY, DAILY
- `calculatedAt` (DateTime) - Thời gian tính toán
- `updatedAt` (DateTime)

---

## MongoDB Collections Tóm Tắt

```
Database: "tilevania"

Collections:
├── players (Người chơi)
├── gameProfiles (Hồ sơ trò chơi)
├── levels (Cấp độ)
├── levelProgress (Tiến độ từng cấp độ)
├── gameSessions (Phiên chơi)
├── achievements (Thành tích)
├── playerAchievements (Thành tích người chơi)
└── leaderboards (Bảng xếp hạng)
```

---

## Dữ Liệu Mặc Định Khi Game Khởi Động
1. **Levels** - Tạo 3 level mặc định (Level 1, Level 2, Level 3)
2. **Achievements** - Tạo các thành tích cơ bản
3. **Mỗi Player mới** - Tạo GameProfile và khởi tạo với giá trị mặc định

---

## API Endpoints Cần Thiết

### User Management
- `POST /api/auth/register` - Đăng ký
- `POST /api/auth/login` - Đăng nhập
- `GET /api/users/:userId` - Lấy thông tin người chơi
- `PUT /api/users/:userId` - Cập nhật thông tin

### Game Progress
- `GET /api/gameProfile/:userId` - Lấy tiến độ trò chơi
- `PUT /api/gameProfile/:userId` - Cập nhật tiến độ
- `POST /api/session` - Tạo phiên chơi mới
- `PUT /api/session/:sessionId` - Cập nhật phiên chơi

### Levels & Progress
- `GET /api/levels` - Lấy danh sách tất cả cấp độ
- `GET /api/levels/:levelId` - Lấy chi tiết cấp độ
- `GET /api/levelProgress/:userId/:levelId` - Lấy tiến độ cấp độ
- `PUT /api/levelProgress/:userId/:levelId` - Cập nhật tiến độ cấp độ

### Achievements
- `GET /api/achievements` - Lấy danh sách thành tích
- `GET /api/playerAchievements/:userId` - Lấy thành tích của người chơi
- `POST /api/playerAchievements/:userId/:achievementId` - Đạt thành tích

### Leaderboard
- `GET /api/leaderboard` - Lấy bảng xếp hạng toàn cầu
- `GET /api/leaderboard/weekly` - Bảng xếp hạng hàng tuần
- `GET /api/leaderboard/daily` - Bảng xếp hạng hàng ngày

---

## Lưu Ý Thiết Kế
1. **Indexes**: Tạo indexes trên `userId`, `levelId`, `period` để truy vấn nhanh
2. **Transactions**: Sử dụng transactions khi cập nhật nhiều collection liên quan
3. **Validation**: Xác thực dữ liệu trên server trước khi lưu
4. **Security**: Hash password, xác thực token JWT cho API
5. **Real-time Updates**: Có thể sử dụng WebSockets để cập nhật bảng xếp hạng real-time
