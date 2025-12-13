# 🔧 Phase 3 Server Updates Required

**Date:** December 2025  
**Status:** Required Changes for Client Compatibility

---

## 📋 Tổng Quan

Sau khi implement các features mới trong client (Level Unlock Logic, Achievement Progress Display), server cần một số updates để đảm bảo compatibility.

---

## ✅ Đã Hoạt Động Tốt

### 1. Level Model & API
- ✅ Level model có đầy đủ fields: `isUnlocked`, `requiredScoreToUnlock`
- ✅ `GET /api/levels` trả về tất cả fields (bao gồm unlock info)
- ✅ `PUT /api/levels/:levelId` cho phép update unlock fields

### 2. GameProfile API
- ✅ `GET /api/gameProfile/:userId` trả về `totalScore`
- ✅ Response format: `{ gameProfile: {...} }` - phù hợp với client

### 3. Achievement Model
- ✅ Achievement model có `condition` field
- ✅ PlayerAchievement model có `progress` field (0-100)

---

## ❌ Cần Sửa

### Issue 1: createLevel không set isUnlocked và requiredScoreToUnlock

**File:** `GameServer/src/controllers/levelController.js`

**Vấn đề:**
- `createLevel` function không accept hoặc set `isUnlocked` và `requiredScoreToUnlock` từ request body
- Mặc định sẽ dùng giá trị từ schema (isUnlocked: false, requiredScoreToUnlock: 0)

**Fix Required:**

```javascript
// In createLevel function, line 48-76
const createLevel = async (req, res) => {
  try {
    const { 
      levelNumber, 
      levelName, 
      description, 
      difficulty, 
      maxCoins, 
      maxEnemies, 
      sceneName,
      isUnlocked,        // ADD THIS
      requiredScoreToUnlock  // ADD THIS
    } = req.body;

    // Validate required fields
    if (!levelNumber || !levelName || !sceneName || maxCoins === undefined || maxEnemies === undefined) {
      return res.status(400).json({
        error: 'Validation Failed',
        message: 'Missing required fields',
      });
    }

    // Check if level already exists
    const existingLevel = await Level.findOne({ levelNumber });
    if (existingLevel) {
      return res.status(409).json({
        error: 'Level Already Exists',
        message: 'Level with this number already exists',
      });
    }

    // Create new level
    const newLevel = new Level({
      levelNumber,
      levelName,
      description: description || '',
      difficulty: difficulty || 'NORMAL',
      maxCoins,
      maxEnemies,
      sceneName,
      isUnlocked: isUnlocked !== undefined ? isUnlocked : false,  // ADD THIS
      requiredScoreToUnlock: requiredScoreToUnlock !== undefined ? requiredScoreToUnlock : 0,  // ADD THIS
    });

    await newLevel.save();

    return res.status(201).json({
      message: 'Level created successfully',
      level: newLevel,
    });
  } catch (error) {
    console.error('Create level error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};
```

---

### Issue 2: Achievement Progress không được track cho locked achievements

**File:** `GameServer/src/controllers/achievementController.js`

**Vấn đề:**
- `checkAchievements` chỉ tạo PlayerAchievement khi achievement được unlock (progress = 100)
- Client cần progress cho cả locked achievements để hiển thị "3/100 enemies"
- `getPlayerAchievements` chỉ trả về unlocked achievements, không có progress cho locked ones

**Fix Required:**

Cần update `checkAchievements` để:
1. Calculate progress cho TẤT CẢ achievements (cả locked và unlocked)
2. Create hoặc update PlayerAchievement với progress tương ứng
3. Nếu progress >= 100, unlock achievement

**Solution 1: Update checkAchievements function**

```javascript
// Auto-check and unlock achievements
const checkAchievements = async (userId) => {
  try {
    const gameProfile = await GameProfile.findOne({ userId });
    if (!gameProfile) return;

    // List of achievements to check
    const achievements = await Achievement.find();
    const unlockedAchievements = await PlayerAchievement.find({ userId });
    const unlockedMap = new Map();
    unlockedAchievements.forEach(a => {
      unlockedMap.set(a.achievementId.toString(), a);
    });

    for (const achievement of achievements) {
      const achievementIdStr = achievement._id.toString();
      const existing = unlockedMap.get(achievementIdStr);
      const isUnlocked = existing && existing.progress >= 100;

      // Calculate current progress (0-100)
      let currentProgress = 0;
      let shouldUnlock = false;

      // Calculate progress based on condition
      switch (achievement.condition) {
        case 'FIRST_KILL':
          currentProgress = gameProfile.totalEnemiesDefeated >= 1 ? 100 : 0;
          shouldUnlock = currentProgress >= 100;
          break;
        case 'COIN_COLLECTOR_100':
          currentProgress = Math.min(100, Math.floor((gameProfile.totalCoinsCollected / 100) * 100));
          shouldUnlock = gameProfile.totalCoinsCollected >= 100;
          break;
        case 'COIN_COLLECTOR_500':
          currentProgress = Math.min(100, Math.floor((gameProfile.totalCoinsCollected / 500) * 100));
          shouldUnlock = gameProfile.totalCoinsCollected >= 500;
          break;
        case 'SCORE_MASTER_1000':
          currentProgress = Math.min(100, Math.floor((gameProfile.totalScore / 1000) * 100));
          shouldUnlock = gameProfile.totalScore >= 1000;
          break;
        case 'SCORE_MASTER_5000':
          currentProgress = Math.min(100, Math.floor((gameProfile.totalScore / 5000) * 100));
          shouldUnlock = gameProfile.totalScore >= 5000;
          break;
        case 'KILLER_100':
          currentProgress = Math.min(100, Math.floor((gameProfile.totalEnemiesDefeated / 100) * 100));
          shouldUnlock = gameProfile.totalEnemiesDefeated >= 100;
          break;
        case 'PLAYTIME_HOUR':
          currentProgress = Math.min(100, Math.floor((gameProfile.totalPlayTime / 3600) * 100));
          shouldUnlock = gameProfile.totalPlayTime >= 3600;
          break;
        case 'PLAYTIME_DAY':
          currentProgress = Math.min(100, Math.floor((gameProfile.totalPlayTime / 86400) * 100));
          shouldUnlock = gameProfile.totalPlayTime >= 86400;
          break;
        default:
          currentProgress = 0;
          break;
      }

      // Ensure progress is 100 if unlocked
      if (shouldUnlock && !isUnlocked) {
        currentProgress = 100;
      }

      // Update or create PlayerAchievement
      if (existing) {
        // Update existing
        existing.progress = currentProgress;
        if (shouldUnlock && !isUnlocked) {
          existing.unlockedAt = new Date();
          gameProfile.totalScore += achievement.points;
        }
        await existing.save();
      } else {
        // Create new PlayerAchievement (even if not unlocked)
        const playerAchievement = new PlayerAchievement({
          userId,
          achievementId: achievement._id,
          progress: currentProgress,
          unlockedAt: shouldUnlock ? new Date() : null,
        });
        await playerAchievement.save();

        if (shouldUnlock) {
          gameProfile.totalScore += achievement.points;
        }
      }
    }

    // Save updated game profile
    await gameProfile.save();
  } catch (error) {
    console.error('Check achievements error:', error);
  }
};
```

**Solution 2: Update getPlayerAchievements để trả về tất cả achievements với progress**

```javascript
// Get user achievements
const getPlayerAchievements = async (req, res) => {
  try {
    const { userId } = req.params;

    // Get all achievements
    const allAchievements = await Achievement.find().sort({ rarity: -1, createdAt: 1 });
    
    // Get player's achievement progress
    const playerAchievements = await PlayerAchievement.find({ userId })
      .populate('achievementId');
    
    // Create a map for quick lookup
    const progressMap = new Map();
    playerAchievements.forEach(pa => {
      progressMap.set(pa.achievementId._id.toString(), pa);
    });

    // Calculate progress for all achievements (including locked ones)
    const gameProfile = await GameProfile.findOne({ userId });
    if (!gameProfile) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'Game profile not found',
      });
    }

    // Build response with all achievements and their progress
    const achievementsWithProgress = allAchievements.map(achievement => {
      const existing = progressMap.get(achievement._id.toString());
      
      // If no PlayerAchievement exists, calculate progress
      if (!existing) {
        let progress = 0;
        
        switch (achievement.condition) {
          case 'FIRST_KILL':
            progress = gameProfile.totalEnemiesDefeated >= 1 ? 100 : 0;
            break;
          case 'COIN_COLLECTOR_100':
            progress = Math.min(100, Math.floor((gameProfile.totalCoinsCollected / 100) * 100));
            break;
          case 'COIN_COLLECTOR_500':
            progress = Math.min(100, Math.floor((gameProfile.totalCoinsCollected / 500) * 100));
            break;
          case 'SCORE_MASTER_1000':
            progress = Math.min(100, Math.floor((gameProfile.totalScore / 1000) * 100));
            break;
          case 'SCORE_MASTER_5000':
            progress = Math.min(100, Math.floor((gameProfile.totalScore / 5000) * 100));
            break;
          case 'KILLER_100':
            progress = Math.min(100, Math.floor((gameProfile.totalEnemiesDefeated / 100) * 100));
            break;
          case 'PLAYTIME_HOUR':
            progress = Math.min(100, Math.floor((gameProfile.totalPlayTime / 3600) * 100));
            break;
          case 'PLAYTIME_DAY':
            progress = Math.min(100, Math.floor((gameProfile.totalPlayTime / 86400) * 100));
            break;
          default:
            progress = 0;
        }

        return {
          _id: null, // No PlayerAchievement record yet
          unlockedAt: null,
          progress: progress,
          achievementId: achievement,
        };
      }

      return existing;
    });

    return res.status(200).json({
      count: achievementsWithProgress.length,
      achievements: achievementsWithProgress,
    });
  } catch (error) {
    console.error('Get player achievements error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};
```

**Recommendation:** 
- Sử dụng **Solution 1** (update checkAchievements) vì nó đảm bảo progress được track và lưu trong database
- **Solution 2** chỉ tính toán on-the-fly, không lưu vào DB

---

### Issue 3: PlayerAchievement Schema - unlockedAt có thể null

**File:** `GameServer/src/models/PlayerAchievement.js`

**Vấn đề:**
- `unlockedAt` field có `default: Date.now`, nhưng nếu achievement chưa unlock thì không nên có date
- Cần cho phép `unlockedAt` là `null` cho locked achievements

**Fix Required:**

```javascript
unlockedAt: {
  type: Date,
  default: null,  // CHANGE FROM Date.now to null
},
```

**Note:** Cần migration script để update existing records nếu có.

---

## 📝 Implementation Steps

### Step 1: Fix createLevel
1. Update `GameServer/src/controllers/levelController.js`
2. Add `isUnlocked` và `requiredScoreToUnlock` vào destructuring
3. Set values khi create level
4. Test với Postman/Insomnia

### Step 2: Fix Achievement Progress Tracking
1. Update `GameServer/src/controllers/achievementController.js`
2. Modify `checkAchievements` để track progress cho tất cả achievements
3. Update `getPlayerAchievements` nếu cần (hoặc dùng Solution 1)
4. Test với existing achievements

### Step 3: Fix PlayerAchievement Schema
1. Update `GameServer/src/models/PlayerAchievement.js`
2. Change `unlockedAt` default to `null`
3. Create migration script nếu cần
4. Test với new achievements

### Step 4: Testing
1. Test createLevel với isUnlocked và requiredScoreToUnlock
2. Test achievement progress calculation
3. Test getPlayerAchievements trả về progress cho locked achievements
4. Verify client có thể hiển thị progress đúng

---

## 🧪 Test Cases

### Test 1: Create Level with Unlock Settings
```bash
POST /api/levels
Body: {
  "levelNumber": 2,
  "levelName": "Level 2",
  "sceneName": "Level 2",
  "maxCoins": 50,
  "maxEnemies": 20,
  "isUnlocked": false,
  "requiredScoreToUnlock": 1000
}
Expected: Level created with isUnlocked=false, requiredScoreToUnlock=1000
```

### Test 2: Achievement Progress Calculation
```bash
# Setup: User has 50 coins, 25 enemies, 500 score
# Achievement: COIN_COLLECTOR_100 (requires 100 coins)

GET /api/achievements/:userId/unlocked
Expected: {
  "achievements": [
    {
      "progress": 50,  // 50/100 = 50%
      "achievementId": { "condition": "COIN_COLLECTOR_100", ... }
    }
  ]
}
```

### Test 3: Achievement Unlock with Progress
```bash
# Setup: User has 100 coins
# Call checkAchievements
# Achievement: COIN_COLLECTOR_100

Expected: PlayerAchievement created with progress=100, unlockedAt=Date
```

---

## 📋 Checklist

- [ ] Fix createLevel to accept isUnlocked and requiredScoreToUnlock
- [ ] Update checkAchievements to track progress for all achievements
- [ ] Update getPlayerAchievements to return progress for locked achievements (if using Solution 2)
- [ ] Fix PlayerAchievement schema unlockedAt default
- [ ] Test createLevel API
- [ ] Test achievement progress calculation
- [ ] Test achievement unlock flow
- [ ] Verify client can display progress correctly

---

## 🔗 Related Files

- `GameServer/src/controllers/levelController.js` - Level CRUD operations
- `GameServer/src/controllers/achievementController.js` - Achievement operations
- `GameServer/src/models/Level.js` - Level schema
- `GameServer/src/models/PlayerAchievement.js` - PlayerAchievement schema
- `GameServer/src/models/Achievement.js` - Achievement schema

---

**End of Server Updates Document**

