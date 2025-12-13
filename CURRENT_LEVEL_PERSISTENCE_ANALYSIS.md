# Phân Tích: Lưu và Khôi Phục Level Hiện Tại

## Mục Tiêu
Kiểm tra xem hệ thống có đảm bảo: **Nếu chơi qua Level 1 và đang ở Level 2, thì dù có thoát game, đăng xuất, xoá và tải lại, khi đăng nhập lại sẽ ở Level 2**.

## Luồng Hoạt Động Hiện Tại

### 1. Khi Hoàn Thành Level 1 và Chuyển Sang Level 2

**File: `LevelExit.cs` (dòng 287-291)**
```csharp
// Update currentLevel in GameProfile to the next level
if (LevelProgressManager.Instance != null && !string.IsNullOrEmpty(nextLevelId))
{
    yield return LevelProgressManager.Instance.UpdateCurrentLevel(nextLevelId);
}
```

✅ **Khi hoàn thành Level 1:**
- `GameSession.EndSession("COMPLETED")` gọi `CompleteLevel()` để lưu tiến độ
- `LevelExit.LoadNextLevel()` cập nhật `currentLevel` lên Level 2 TRƯỚC KHI load scene mới
- Client gửi PUT request: `PUT /api/gameProfile/:userId` với body `{ currentLevel: "level2Id" }`

### 2. Khi Bắt Đầu Chơi Level 2

**File: `GameSession.cs` (dòng 602-606)**
```csharp
// Update currentLevel in GameProfile when starting a level
if (LevelProgressManager.Instance != null)
{
    yield return LevelProgressManager.Instance.UpdateCurrentLevel(levelId);
}
```

✅ **Khi bắt đầu Level 2:**
- `GameSession.StartSession()` cập nhật `currentLevel` lên Level 2
- Đảm bảo `currentLevel` luôn được cập nhật khi vào level

### 3. Khi Đăng Nhập Lại

**File: `MainMenuManager.cs` (dòng 145-160)**
```csharp
// First, try to get current level from GameProfile (saved progress)
LevelProgressManager.LevelData currentLevelData = null;
yield return LevelProgressManager.Instance.GetCurrentLevel(data => currentLevelData = data);

LevelProgressManager.LevelData levelToLoad = currentLevelData;

// If no current level saved, get next level to play (highest completed + 1, or Level 1)
if (levelToLoad == null)
{
    Debug.Log("[MainMenuManager] No current level saved, determining next level to play...");
    yield return LevelProgressManager.Instance.GetNextLevelToPlay(data => levelToLoad = data);
}
else
{
    Debug.Log($"[MainMenuManager] Found saved current level: {levelToLoad.levelName} (Level {levelToLoad.levelNumber})");
}
```

✅ **Khi đăng nhập:**
- `MainMenuManager.LoadNextLevelCoroutine()` gọi `GetCurrentLevel()` để lấy level đã lưu
- Nếu có `currentLevel` trong GameProfile → Load level đó
- Nếu không có → Tính toán level tiếp theo dựa trên level đã hoàn thành

## Server-Side Implementation

### Database Schema
**File: `GameServer/src/models/GameProfile.js` (dòng 51-55)**
```javascript
currentLevel: {
  type: mongoose.Schema.Types.ObjectId,
  ref: 'Level',
  default: null, // null means no current level (start from Level 1)
},
```

✅ Server có field `currentLevel` với type ObjectId reference đến Level

### API Endpoints

**GET Game Profile:**
- **Route:** `GET /api/gameProfile/:userId`
- **Controller:** `gameProfileController.getGameProfile`
- **Response:** Trả về GameProfile với `currentLevel` đã được populate (dòng 10)
```javascript
.populate('currentLevel', 'levelName sceneName levelNumber isUnlocked requiredScoreToUnlock');
```

✅ Server trả về `currentLevel` với đầy đủ thông tin level

**PUT Game Profile:**
- **Route:** `PUT /api/gameProfile/:userId`
- **Controller:** `gameProfileController.updateGameProfile`
- **Body:** `{ currentLevel: "levelIdString" }`
- **Implementation:** Sử dụng `findOneAndUpdate` với `updateData` trực tiếp

⚠️ **POTENTIAL ISSUE:** 
- Mongoose sẽ tự động convert string ID thành ObjectId nếu string hợp lệ
- Nhưng cần đảm bảo levelId được gửi từ client là valid ObjectId string

## Client-Side Implementation

### Update Current Level
**File: `LevelProgressManager.cs` (dòng 446-471)**
```csharp
public IEnumerator UpdateCurrentLevel(string levelId, System.Action<bool> onResult = null)
{
    var updateData = new { currentLevel = levelId };
    var json = JsonUtility.ToJson(updateData);
    
    yield return APIClient.Put(APIConfig.GameProfile(userId), json, r => apiResult = r, ...);
}
```

✅ Client gửi `currentLevel` dưới dạng string ID

### Get Current Level
**File: `LevelProgressManager.cs` (dòng 476-516)**
```csharp
public IEnumerator GetCurrentLevel(Action<LevelData> onResult)
{
    yield return APIClient.Get(APIConfig.GameProfile(userId), r => apiResult = r, ...);
    
    var profile = JsonUtility.FromJson<GameProfileResponse>(apiResult.data);
    if (profile?.gameProfile?.currentLevel != null)
    {
        var levelData = GetLevelData(profile.gameProfile.currentLevel._id);
        onResult?.Invoke(levelData);
    }
}
```

✅ Client parse response và lấy level data từ `currentLevel._id`

## Kết Luận

### ✅ Những Điểm Đã Hoạt Động Đúng:

1. **Server có field `currentLevel`** trong GameProfile model
2. **Server có API GET** để lấy GameProfile với `currentLevel` đã populate
3. **Server có API PUT** để cập nhật `currentLevel`
4. **Client cập nhật `currentLevel`** khi:
   - Hoàn thành level và chuyển sang level tiếp theo (LevelExit)
   - Bắt đầu chơi một level (GameSession.StartSession)
5. **Client lấy `currentLevel`** khi đăng nhập và load level đó

### ⚠️ Những Điểm Cần Kiểm Tra:

1. **ObjectId Conversion:**
   - Server sử dụng `findOneAndUpdate` với `updateData` trực tiếp
   - Mongoose sẽ tự convert string → ObjectId nếu string hợp lệ
   - **Cần test:** Đảm bảo levelId từ client là valid MongoDB ObjectId string

2. **Error Handling:**
   - Nếu `UpdateCurrentLevel()` fail, có log warning nhưng không block game flow
   - **Cần test:** Xem có ảnh hưởng gì không nếu update fail

3. **Edge Cases:**
   - Nếu player thoát game ngay sau khi complete level nhưng trước khi `UpdateCurrentLevel()` hoàn thành?
   - Nếu network error khi update `currentLevel`?
   - **Cần test:** Các trường hợp này

## Khuyến Nghị

### 1. Thêm Validation trên Server
```javascript
// Trong updateGameProfile controller
if (updateData.currentLevel) {
  if (!mongoose.Types.ObjectId.isValid(updateData.currentLevel)) {
    return res.status(400).json({
      error: 'Validation Failed',
      message: 'currentLevel must be a valid ObjectId',
    });
  }
}
```

### 2. Thêm Retry Logic trên Client
- Nếu `UpdateCurrentLevel()` fail, có thể retry một vài lần
- Hoặc queue lại để retry sau

### 3. Test Cases Cần Kiểm Tra:
- [ ] Complete Level 1 → Check currentLevel = Level 2
- [ ] Thoát game ngay sau khi complete → Login lại → Check currentLevel
- [ ] Xoá app và tải lại → Login → Check currentLevel
- [ ] Đăng xuất và đăng nhập lại → Check currentLevel
- [ ] Network error khi update → Check fallback behavior

## Tóm Tắt

**Câu Trả Lời:** ✅ **CÓ**, hệ thống đã được thiết kế để hỗ trợ tính năng này. Tuy nhiên, cần **test thực tế** để đảm bảo:
1. ObjectId conversion hoạt động đúng
2. Error handling đầy đủ
3. Edge cases được xử lý tốt

**Luồng hoạt động:**
1. Complete Level 1 → `currentLevel` được update lên Level 2
2. Start Level 2 → `currentLevel` được update lại (redundant nhưng an toàn)
3. Login → `currentLevel` được lấy từ server và load Level 2

