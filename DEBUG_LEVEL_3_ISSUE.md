# Debug: Vấn Đề Luôn Load Level 3

## Vấn Đề
Dù đang ở đâu cũng bị xuất hiện ở Level 3 khi đăng nhập lại.

## Logic Hiện Tại

### 1. Khi Đăng Nhập và Click Play
**File:** `MainMenuManager.cs` → `LoadNextLevelCoroutine()`

```csharp
// Step 1: Lấy currentLevel từ GameProfile
GetCurrentLevel() → currentLevelData

// Step 2: Nếu currentLevelData == null
GetNextLevelToPlay() → levelToLoad
  → GetHighestCompletedLevelNumber() → highestCompleted
  → nextLevelNumber = highestCompleted + 1
  → GetLevelDataByNumber(nextLevelNumber)
```

### 2. Khi Complete Level
**File:** `LevelExit.cs` → `LoadNextLevel()`

```csharp
// Complete Level 1 → Update currentLevel to Level 2
UpdateCurrentLevel(nextLevelId) // nextLevelId = Level 2
```

### 3. Khi Start Level
**File:** `GameSession.cs` → `StartSession()`

```csharp
// Start Level 2 → Update currentLevel to Level 2
UpdateCurrentLevel(levelId) // levelId = Level 2
```

## Các Trường Hợp Có Thể Xảy Ra

### Trường Hợp 1: currentLevel luôn null
- **Nguyên nhân:** `GetCurrentLevel()` không parse đúng response từ server
- **Kết quả:** Luôn fallback sang `GetNextLevelToPlay()`
- **Nếu user đã complete Level 1 và Level 2:**
  - `highestCompleted = 2`
  - `nextLevelNumber = 2 + 1 = 3`
  - → Load Level 3 ❌

### Trường Hợp 2: currentLevel bị update sai
- **Nguyên nhân:** Khi complete Level 2, `currentLevel` được update lên Level 3
- **Kết quả:** Khi đăng nhập lại, load Level 3 (đúng nếu đã complete Level 2)
- **Nhưng nếu user chỉ complete Level 1:**
  - `currentLevel` nên là Level 2
  - Nhưng nếu bị update lên Level 3 → Load Level 3 ❌

### Trường Hợp 3: GetNextLevelToPlay tính sai
- **Nguyên nhân:** Logic tính `nextLevelNumber = highestCompleted + 1` không đúng
- **Kết quả:** Luôn trả về level cao hơn 1 level so với highest completed
- **Ví dụ:**
  - Complete Level 1 → `highestCompleted = 1` → `nextLevelNumber = 2` ✅
  - Complete Level 1 và Level 2 → `highestCompleted = 2` → `nextLevelNumber = 3` ❌ (nên là Level 2 nếu đang chơi Level 2)

## Debug Steps

### 1. Kiểm Tra Database
```javascript
// Kết nối MongoDB Atlas
// Kiểm tra GameProfile collection
db.gameprofiles.findOne({ userId: ObjectId("...") })

// Kiểm tra:
// - currentLevel có được set không?
// - currentLevel có đúng levelId không?
// - currentLevel có null không?
```

### 2. Kiểm Tra Logs
Khi chạy game, xem logs:
- `[LevelProgress] GetCurrentLevel:` - Xem response từ server
- `[LevelProgress] GetNextLevelToPlay:` - Xem highestCompleted và nextLevelNumber
- `[MainMenuManager] LoadNextLevelCoroutine:` - Xem level nào được load

### 3. Kiểm Tra Response Format
Server trả về:
```json
{
  "gameProfile": {
    "currentLevel": {
      "_id": "...",
      "levelName": "...",
      "levelNumber": 2,
      ...
    }
  }
}
```

Client parse:
```csharp
GameProfileResponse {
  GameProfileData gameProfile {
    CurrentLevelData currentLevel {
      string _id;
      int levelNumber;
    }
  }
}
```

## Giải Pháp Đề Xuất

### 1. Sửa Logic GetNextLevelToPlay
**Vấn đề:** Logic hiện tại tính `nextLevelNumber = highestCompleted + 1` không đúng trong mọi trường hợp.

**Giải pháp:** 
- Nếu `currentLevel` là null, thì load level tiếp theo dựa trên highest completed
- Nhưng nếu user đang chơi một level (chưa complete), thì `currentLevel` nên được set và load level đó

### 2. Đảm Bảo currentLevel được Update Đúng
- Khi bắt đầu chơi level → Update `currentLevel` = level đó
- Khi complete level → Update `currentLevel` = level tiếp theo (nếu muốn) HOẶC giữ nguyên level hiện tại (nếu muốn user có thể replay)

### 3. Thêm Validation
- Kiểm tra xem `currentLevel` có hợp lệ không (level có tồn tại không, level có unlocked không)
- Nếu không hợp lệ, fallback sang logic khác

## Test Cases

1. **Test 1: User mới, chưa chơi level nào**
   - `currentLevel` = null
   - `highestCompleted` = 0
   - → Nên load Level 1 ✅

2. **Test 2: User đã complete Level 1, đang chơi Level 2**
   - `currentLevel` = Level 2
   - `highestCompleted` = 1
   - → Nên load Level 2 ✅

3. **Test 3: User đã complete Level 1 và Level 2**
   - `currentLevel` = Level 3 (nếu update sau khi complete Level 2)
   - `highestCompleted` = 2
   - → Nên load Level 3 ✅

4. **Test 4: User đã complete Level 1 và Level 2, nhưng currentLevel = null**
   - `currentLevel` = null
   - `highestCompleted` = 2
   - → Hiện tại: Load Level 3 ❌
   - → Nên: Load Level 2 (level tiếp theo chưa complete) HOẶC Level 3 (nếu muốn tiếp tục)

## Next Steps

1. ✅ Đã thêm debug logging chi tiết
2. ⏳ Cần test với database thực tế
3. ⏳ Cần xem logs để xác định vấn đề chính xác
4. ⏳ Cần sửa logic dựa trên kết quả debug

