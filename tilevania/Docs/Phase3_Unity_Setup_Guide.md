# 🎮 Phase 3 Unity Editor Setup Guide

**Date:** December 2025  
**Phase:** Level Progress & Achievements - Unity Editor Setup  
**Status:** Complete Implementation Guide

---

## 📋 Mục Lục

1. [AchievementNotification Prefab Setup](#1-achievementnotification-prefab-setup)
2. [AchievementListItem Prefab Setup](#2-achievementlistitem-prefab-setup)
3. [Level Unlock Testing](#3-level-unlock-testing)
4. [Troubleshooting](#4-troubleshooting)

---

## 1. AchievementNotification Prefab Setup

### 1.1 Mở AchievementNotification Prefab

1. Trong Unity Editor, mở **Project** window
2. Điều hướng đến: `Assets/Prefabs/UI/AchievementNotification.prefab`
3. **Double-click** để mở prefab trong **Prefab Mode**

### 1.2 Thêm AudioSource Component (Optional)

**Nếu bạn muốn dùng AudioSource thay vì PlayClipAtPoint:**

1. Chọn **root GameObject** của prefab (thường là `AchievementNotification`)
2. Trong **Inspector**, click **Add Component**
3. Tìm và thêm **Audio Source**
4. **Settings:**
   - ✅ **Play On Awake**: `false` (sẽ play từ code)
   - **Volume**: `0.6` - `0.8` (tùy chỉnh)
   - **Spatial Blend**: `0` (2D sound)
   - **Loop**: `false`

### 1.3 Gán Unlock Sound AudioClip

**Cách 1: Sử dụng AudioClip field trong AchievementNotification script**

1. Trong **Prefab Mode**, chọn root GameObject
2. Trong **Inspector**, tìm component **Achievement Notification (Script)**
3. Tìm field **Unlock Sound**
4. **Kéo thả AudioClip** từ Project window vào field này
   - Hoặc click **circle icon** bên cạnh field để chọn từ Asset picker

**Lưu ý:**
- AudioClip phải là file `.wav`, `.mp3`, hoặc `.ogg`
- Nếu chưa có sound, có thể bỏ qua (code sẽ không play sound nếu null)
- Sound sẽ tự động play khi notification xuất hiện

**Cách 2: Tạo AudioClip mới (nếu chưa có)**

1. Import sound file vào Unity:
   - Kéo file audio vào `Assets/Audio/` folder
   - Unity sẽ tự động import
2. Chọn audio file trong Project
3. Trong **Inspector**, settings:
   - **Load Type**: `Decompress On Load` (cho short sounds)
   - **Compression Format**: `PCM` (best quality) hoặc `Vorbis` (smaller size)
4. Kéo audio file vào **Unlock Sound** field trong AchievementNotification component

### 1.4 Điều Chỉnh Animation Settings

1. Trong **Achievement Notification (Script)** component, tìm các fields:
   - **Animation Duration**: `0.5` (seconds) - Thời gian animation entrance
   - **Scale Curve**: Animation curve cho scale animation
   - **Bounce Curve**: Animation curve cho bounce animation

2. **Điều chỉnh Animation Duration:**
   - Giá trị mặc định: `0.5` giây
   - Tăng lên `0.7-1.0` để animation chậm hơn
   - Giảm xuống `0.3` để animation nhanh hơn

3. **Điều chỉnh Scale Curve:**
   - Click vào **Scale Curve** field
   - **Curve Editor** sẽ mở
   - Mặc định: `EaseInOut(0, 0, 1, 1)` - smooth scale
   - Có thể tùy chỉnh để tạo hiệu ứng khác:
     - **Bounce**: Thêm keyframe ở giữa với value > 1.0
     - **Elastic**: Thêm nhiều keyframes với oscillation

4. **Điều chỉnh Bounce Curve:**
   - Tương tự Scale Curve
   - Mặc định: `EaseInOut(0, 0, 1, 1)`
   - Để tạo bounce effect, thêm keyframe ở giữa với value cao hơn

**Ví dụ Animation Curve cho Bounce Effect:**
```
Keyframe 1: time=0, value=0
Keyframe 2: time=0.3, value=1.2 (overshoot)
Keyframe 3: time=0.6, value=0.9 (bounce back)
Keyframe 4: time=1.0, value=1.0 (settle)
```

### 1.5 Save Prefab

1. Click **Overrides** button ở top-right của Prefab Mode
2. Chọn **Save All** để lưu tất cả changes
3. Hoặc click **Ctrl+S** (Windows) / **Cmd+S** (Mac)

---

## 2. AchievementListItem Prefab Setup

### 2.1 Mở AchievementListItem Prefab

1. Trong **Project** window, điều hướng đến: `Assets/Prefabs/UI/AchievementListItem.prefab`
2. **Double-click** để mở prefab trong **Prefab Mode**

### 2.2 Thêm ProgressText (TextMeshProUGUI)

**Bước 1: Tạo GameObject cho ProgressText**

1. Trong **Hierarchy** của prefab, chọn **root GameObject** (thường là `AchievementListItem`)
2. **Right-click** → **UI** → **Text - TextMeshPro**
3. Đổi tên thành `ProgressText`

**Bước 2: Setup RectTransform**

1. Chọn `ProgressText` GameObject
2. Trong **Inspector**, tìm **Rect Transform**:
   - **Anchor Presets**: Chọn vị trí phù hợp (ví dụ: bottom-left)
   - **Position**: Điều chỉnh để đặt text ở vị trí mong muốn
   - **Width**: `200-300`
   - **Height**: `30-40`

**Bước 3: Setup TextMeshProUGUI Component**

1. Trong **Inspector**, tìm **TextMeshPro - Text (UI)** component:
   - **Text**: `"0/100"` (placeholder)
   - **Font Size**: `14-16`
   - **Alignment**: `Left` hoặc `Center`
   - **Color**: Màu xám nhạt (ví dụ: `#808080`)
   - **Font Asset**: Chọn font phù hợp (LiberationSans SDF hoặc font khác)

**Bước 4: Gán vào AchievementListUIItem Script**

1. Chọn **root GameObject** của prefab
2. Trong **Inspector**, tìm **Achievement List UI Item (Script)** component
3. Tìm field **Progress Text**
4. **Kéo thả** `ProgressText` GameObject từ Hierarchy vào field này

### 2.3 Thêm ProgressBar (Slider) - Optional

**Bước 1: Tạo Slider**

1. Chọn **root GameObject** của prefab
2. **Right-click** → **UI** → **Slider**
3. Đổi tên thành `ProgressBar`

**Bước 2: Setup Slider**

1. Chọn `ProgressBar` GameObject
2. Trong **Inspector**, tìm **Rect Transform**:
   - **Anchor Presets**: Stretch horizontally, bottom
   - **Pos Y**: Đặt ở vị trí phù hợp (dưới description text)
   - **Height**: `5-10` (thin bar)
   - **Left/Right**: `10-20` (margins)

3. Trong **Slider** component:
   - **Min Value**: `0`
   - **Max Value**: `1`
   - **Whole Numbers**: `false`
   - **Value**: `0` (mặc định)

**Bước 3: Setup Slider Visuals**

1. Trong Hierarchy, mở `ProgressBar` để xem children:
   - `Background` - Background của slider
   - `Fill Area` → `Fill` - Fill bar (phần hiển thị progress)

2. **Setup Background:**
   - Chọn `Background` GameObject
   - **Image** component:
     - **Color**: Màu xám nhạt (ví dụ: `#333333` với alpha 0.5)

3. **Setup Fill:**
   - Chọn `Fill Area` → `Fill` GameObject
   - **Image** component:
     - **Color**: Màu xanh lá hoặc vàng (ví dụ: `#4CAF50`)
     - **Image Type**: `Filled`
     - **Fill Method**: `Horizontal`

**Bước 4: Gán vào AchievementListUIItem Script**

1. Chọn **root GameObject** của prefab
2. Trong **Inspector**, tìm **Achievement List UI Item (Script)** component
3. Tìm field **Progress Bar**
4. **Kéo thả** `ProgressBar` GameObject từ Hierarchy vào field này

**Bước 5: Gán Progress Bar Fill Image (Optional)**

1. Trong **Achievement List UI Item (Script)** component, tìm field **Progress Bar Fill**
2. **Kéo thả** `Fill` GameObject (trong `ProgressBar` → `Fill Area` → `Fill`) vào field này
3. Điều này cho phép code update fill amount trực tiếp

### 2.4 Layout Arrangement

**Sắp xếp các elements trong prefab:**

```
AchievementListItem (Root)
├── TitleText (TextMeshProUGUI) - Top
├── DescriptionText (TextMeshProUGUI) - Middle
├── PointsText (TextMeshProUGUI) - Right side
├── ProgressText (TextMeshProUGUI) - Bottom left (NEW)
├── ProgressBar (Slider) - Bottom (NEW, optional)
└── UnlockedBadge (GameObject)
    └── BadgeText (TextMeshProUGUI)
```

**Layout Tips:**
- Sử dụng **Vertical Layout Group** hoặc **Horizontal Layout Group** để tự động sắp xếp
- Hoặc đặt thủ công bằng cách điều chỉnh **RectTransform** positions
- Đảm bảo **ProgressText** và **ProgressBar** chỉ hiện khi achievement chưa unlock

### 2.5 Save Prefab

1. Click **Overrides** → **Save All**
2. Hoặc **Ctrl+S** / **Cmd+S**

---

## 3. Level Unlock Testing

### 3.1 Kiểm Tra Server API Response

**Bước 1: Test API Endpoint**

1. Mở **Postman** hoặc **Insomnia**
2. Gửi GET request đến: `https://tilevania.onrender.com/api/levels`
3. Kiểm tra response có chứa:
   ```json
   {
     "levels": [
       {
         "_id": "...",
         "levelNumber": 1,
         "levelName": "Level 1",
         "sceneName": "Level 1",
         "isUnlocked": true,
         "requiredScoreToUnlock": 0
       },
       {
         "_id": "...",
         "levelNumber": 2,
         "levelName": "Level 2",
         "sceneName": "Level 2",
         "isUnlocked": false,
         "requiredScoreToUnlock": 1000
       }
     ]
   }
   ```

**Bước 2: Verify Fields**

- ✅ `isUnlocked`: Boolean (true/false)
- ✅ `requiredScoreToUnlock`: Number (0 hoặc giá trị > 0)

### 3.2 Setup Test Levels trong Database

**Option 1: Sử dụng MongoDB Compass hoặc mongo shell**

```javascript
// Connect to MongoDB
use tilevania

// Update Level 1 to be unlocked by default
db.levels.updateOne(
  { levelNumber: 1 },
  { $set: { isUnlocked: true, requiredScoreToUnlock: 0 } }
)

// Update Level 2 to require 1000 score
db.levels.updateOne(
  { levelNumber: 2 },
  { $set: { isUnlocked: false, requiredScoreToUnlock: 1000 } }
)

// Update Level 3 to require 5000 score
db.levels.updateOne(
  { levelNumber: 3 },
  { $set: { isUnlocked: false, requiredScoreToUnlock: 5000 } }
)
```

**Option 2: Sử dụng API (nếu có admin endpoint)**

```bash
# Update level via API (nếu có authentication)
PUT /api/levels/:levelId
Body: {
  "isUnlocked": false,
  "requiredScoreToUnlock": 1000
}
```

### 3.3 Test trong Unity Editor

**Test Case 1: Level Unlocked (isUnlocked = true)**

1. **Setup:**
   - Level 1: `isUnlocked = true`, `requiredScoreToUnlock = 0`
2. **Test:**
   - Chơi Level 1 và complete
   - Chạm Exit Portal
   - **Expected:** Level 2 load bình thường (không có message)

**Test Case 2: Level Locked (isUnlocked = false, requiredScoreToUnlock > 0)**

1. **Setup:**
   - Level 2: `isUnlocked = false`, `requiredScoreToUnlock = 1000`
   - Player total score: `500` (chưa đủ)
2. **Test:**
   - Chơi Level 1 và complete
   - Chạm Exit Portal
   - **Expected:** 
     - Console log: `"Level 'Level 2' is locked. Requires 1000 total score. Your score: 500"`
     - Load về MainMenu (scene 0)
     - Không load Level 2

**Test Case 3: Level Unlocked by Score (isUnlocked = false, nhưng score đủ)**

1. **Setup:**
   - Level 2: `isUnlocked = false`, `requiredScoreToUnlock = 1000`
   - Player total score: `1200` (đủ)
2. **Test:**
   - Chơi Level 1 và complete
   - Chạm Exit Portal
   - **Expected:**
     - Console log: `"Next level 'Level 2' is unlocked, loading..."`
     - Level 2 load bình thường

**Test Case 4: Last Level**

1. **Setup:**
   - Complete level cuối cùng (Level 3)
2. **Test:**
   - Chạm Exit Portal
   - **Expected:**
     - Console log: `"Reached last level, returning to scene 0 (MainMenu)"`
     - Load về MainMenu

### 3.4 Debug Console Messages

Khi test, kiểm tra **Console** window trong Unity Editor:

**Success Messages:**
```
[LevelExit] Next level 'Level 2' is unlocked, loading...
[LevelExit] Scene loaded, loading overlay hidden, input re-enabled
```

**Locked Level Messages:**
```
[LevelExit] Level 'Level 2' is locked. Requires 1000 total score. Your score: 500
[LevelExit] Loading overlay hidden, input re-enabled
```

**Error Messages:**
```
[LevelExit] Cannot find level data for scene 'Level 2', loading anyway
[LevelExit] LevelProgressManager not found, loading level anyway
```

---

## 4. Troubleshooting

### 4.1 AchievementNotification Issues

**Problem: Sound không play**

**Solutions:**
1. Kiểm tra `unlockSound` field đã được gán chưa
2. Kiểm tra AudioClip import settings:
   - **Load Type**: `Decompress On Load` hoặc `Compressed In Memory`
   - **Compression Format**: `PCM` (best quality)
3. Kiểm tra AudioClip có bị disable không
4. Kiểm tra **Audio Listener** có trong scene không (thường ở Main Camera)

**Problem: Animation không chạy**

**Solutions:**
1. Kiểm tra `rectTransform` có null không (cần RectTransform component)
2. Kiểm tra `animationDuration` > 0
3. Kiểm tra GameObject có active không
4. Kiểm tra Canvas có active không

### 4.2 AchievementListItem Issues

**Problem: ProgressText không hiển thị**

**Solutions:**
1. Kiểm tra `progressText` field đã được gán trong Inspector
2. Kiểm tra ProgressText GameObject có active không
3. Kiểm tra TextMeshProUGUI component có được gán đúng không
4. Kiểm tra code: ProgressText chỉ hiện khi `unlocked = false`

**Problem: ProgressBar không update**

**Solutions:**
1. Kiểm tra `progressBar` field đã được gán
2. Kiểm tra Slider component có Min=0, Max=1
3. Kiểm tra Fill Image có Image Type = Filled
4. Kiểm tra progress value từ server (0-100)

**Problem: Progress text hiển thị sai format**

**Solutions:**
1. Kiểm tra `condition` string từ server có đúng format không
2. Kiểm tra `ExtractTargetNumber()` có parse đúng không
3. Test với các condition strings khác nhau:
   - "Collect 100 coins" → "50/100 coins"
   - "Kill 50 enemies" → "25/50 enemies"
   - "Score 1000 points" → "500/1000 points"

### 4.3 Level Unlock Issues

**Problem: Level không unlock dù đã đủ score**

**Solutions:**
1. Kiểm tra API response có `isUnlocked` và `requiredScoreToUnlock` không
2. Kiểm tra `LevelProgressManager` có cache levels đúng không
3. Kiểm tra player total score có được fetch đúng không
4. Kiểm tra console logs để debug:
   ```
   [LevelProgress] Cached X levels successfully
   [LevelExit] Next level 'Level 2' is unlocked, loading...
   ```

**Problem: Level unlock check fail**

**Solutions:**
1. Kiểm tra `LevelProgressManager.Instance` có null không
2. Kiểm tra `AuthManager.Instance.CurrentPlayer` có null không
3. Kiểm tra network connection
4. Kiểm tra API endpoint `/api/levels` có accessible không

**Problem: Level load về MainMenu dù đã unlock**

**Solutions:**
1. Kiểm tra scene build index có đúng không
2. Kiểm tra scene name có match với server `sceneName` không
3. Kiểm tra `CheckAndLoadNextLevel()` có được gọi không
4. Kiểm tra console logs để xem flow

### 4.4 Common Setup Mistakes

1. **Prefab không save:**
   - Luôn click **Save All** sau khi edit prefab
   - Hoặc dùng **Ctrl+S** / **Cmd+S**

2. **References bị mất:**
   - Khi duplicate prefab, kiểm tra lại tất cả references
   - Sử dụng **Prefab Variant** nếu cần nhiều versions

3. **UI Elements không hiển thị:**
   - Kiểm tra Canvas có active không
   - Kiểm tra RectTransform có trong viewport không
   - Kiểm tra Canvas Scaler settings

4. **Audio không play:**
   - Kiểm tra Audio Listener có trong scene
   - Kiểm tra Audio Mixer settings (nếu có)
   - Kiểm tra volume settings

---

## 5. Quick Checklist

### AchievementNotification Prefab
- [ ] AudioClip gán vào `unlockSound` field
- [ ] `animationDuration` được set (mặc định 0.5)
- [ ] Animation curves được setup (nếu cần custom)
- [ ] Prefab đã được save

### AchievementListItem Prefab
- [ ] `ProgressText` GameObject đã tạo và gán vào script
- [ ] `ProgressBar` GameObject đã tạo và gán (optional)
- [ ] `ProgressBarFill` Image đã gán (optional)
- [ ] Layout đã được sắp xếp đúng
- [ ] Prefab đã được save

### Level Unlock
- [ ] Server API `/api/levels` trả về `isUnlocked` và `requiredScoreToUnlock`
- [ ] Test levels đã được setup trong database
- [ ] Test cases đã được verify
- [ ] Console logs được check

---

## 6. Additional Resources

- **Unity UI Documentation:** https://docs.unity3d.com/Manual/UISystem.html
- **TextMeshPro Documentation:** https://docs.unity3d.com/Manual/com.unity.textmeshpro.html
- **Audio Source Documentation:** https://docs.unity3d.com/Manual/class-AudioSource.html
- **Animation Curves:** https://docs.unity3d.com/Manual/animeditor-AnimationCurves.html

---

**End of Setup Guide**

