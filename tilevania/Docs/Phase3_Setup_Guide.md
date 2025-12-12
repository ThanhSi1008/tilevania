# 🎮 Phase 3 Setup Guide - Level Progress & Achievements

**Date:** December 2025  
**Phase:** Level Progress & Achievements Integration  
**Status:** ✅ Implementation Complete - Setup Required

---

## 📋 Tổng Quan

Tài liệu này hướng dẫn chi tiết cách setup Phase 3 (Level Progress & Achievements) vào project Unity của bạn. Phase 3 bao gồm:

1. **Level Progress Tracking** - Theo dõi tiến độ hoàn thành level, best score, coins, enemies
2. **Achievement System** - Hệ thống thành tựu với notification popup
3. **Achievement List UI** - Giao diện hiển thị danh sách achievements

### ✅ Đã Hoàn Thành (Code)

- ✅ `LevelProgressManager.cs` - Quản lý level progress và mapping levelId
- ✅ `AchievementManager.cs` - Quản lý achievements và notifications
- ✅ `AchievementNotification.cs` - UI popup khi unlock achievement
- ✅ `AchievementListUI.cs` - UI danh sách achievements
- ✅ `AchievementListUIItem.cs` - Item trong danh sách
- ✅ `GameSession.cs` - Đã tích hợp complete level và refresh achievements
- ✅ `APIConfig.cs` - Đã thêm endpoints cho level progress và achievements

### ⚠️ Cần Setup (Unity Editor)

- ⚠️ Tạo GameObject managers trong AuthScene
- ⚠️ Tạo prefab AchievementNotification
- ⚠️ Tạo prefab AchievementListUIItem
- ⚠️ Setup AchievementListUI panel trong AuthScene
- ⚠️ Kiểm tra level mapping (levelNumber và levelName)

---

## 🏗️ Cấu Trúc Files Đã Tạo

```
Assets/Scripts/
├── Managers/
│   ├── LevelProgressManager.cs      ✅ NEW
│   └── AchievementManager.cs        ✅ NEW
├── UI/
│   ├── AchievementNotification.cs   ✅ NEW
│   ├── AchievementListUI.cs         ✅ NEW
│   └── AchievementListUIItem.cs     ✅ NEW
├── Gameplay/
│   └── GameSession.cs               ✅ MODIFIED (thêm complete level logic)
└── Network/
    └── APIConfig.cs                  ✅ MODIFIED (thêm endpoints)
```

---

## 📝 Bước 1: Setup Managers trong AuthScene

### 1.1 Tạo GameObject "_Systems" (nếu chưa có)

1. Mở **AuthScene**
2. Tạo GameObject mới tên `_Systems` (hoặc dùng GameObject hiện có chứa AuthManager)
3. Đảm bảo GameObject này sẽ **DontDestroyOnLoad** (scripts tự xử lý)

### 1.2 Thêm LevelProgressManager

1. Chọn GameObject `_Systems`
2. Add Component → `LevelProgressManager`
3. **Inspector Settings:**
   - `Levels Cache TTL Seconds`: `300` (cache 5 phút, mặc định)

**Lưu ý:** Script tự động DontDestroyOnLoad và singleton, không cần setup thêm.

### 1.3 Thêm AchievementManager

1. Chọn GameObject `_Systems`
2. Add Component → `AchievementManager`
3. **Inspector Settings:**
   - `Achievement Notification Prefab`: **Để trống tạm** (sẽ setup ở bước 2)
   - `Notification Parent`: **Để trống tạm** (sẽ setup ở bước 2)

**Lưu ý:** Script tự động DontDestroyOnLoad và singleton.

### 1.4 Kiểm Tra Các Managers Khác

Đảm bảo các managers sau đã có trong `_Systems`:
- ✅ `AuthManager` (Phase 1)
- ✅ `SessionManager` (Phase 1)
- ✅ `LevelProgressManager` (vừa thêm)
- ✅ `AchievementManager` (vừa thêm)

---

## 📝 Bước 2: Tạo Achievement Notification Prefab

### 2.1 Tạo Prefab Structure

1. Trong **Project** window, tạo folder `Assets/Prefabs/UI/` (nếu chưa có)
2. Tạo GameObject mới trong Hierarchy (tạm thời, sẽ convert thành prefab)
3. Đặt tên: `AchievementNotification`

### 2.2 Setup UI Components

**Hierarchy Structure:**
```
AchievementNotification (GameObject)
├── Background (Image) - Optional, để làm nền
├── TitleText (TextMeshProUGUI)
└── DescriptionText (TextMeshProUGUI)
```

**Chi Tiết Setup:**

1. **AchievementNotification (Root)**
   - Add Component → `Canvas Group`
   - Canvas Group Settings:
     - `Alpha`: `1`
     - `Interactable`: `false`
     - `Blocks Raycasts`: `false`
   - Add Component → `AchievementNotification` script
   - **Inspector Settings:**
     - `Title Text`: Kéo `TitleText` vào
     - `Description Text`: Kéo `DescriptionText` vào
     - `Lifetime Seconds`: `3` (thời gian hiển thị)
     - `Canvas Group`: Kéo component CanvasGroup vào

2. **Background (Image)** - Optional
   - Add Component → `Image`
   - Color: Đen với alpha 0.8 (hoặc màu bạn muốn)
   - RectTransform: Stretch to fill parent

3. **TitleText (TextMeshProUGUI)**
   - Text: `"Achievement Unlocked!"` (placeholder)
   - Font Size: `24-32`
   - Alignment: Center
   - Color: Vàng hoặc màu nổi bật

4. **DescriptionText (TextMeshProUGUI)**
   - Text: `"Description here"` (placeholder)
   - Font Size: `18-22`
   - Alignment: Center
   - Color: Trắng

### 2.3 Layout & Positioning

**RectTransform Settings:**
- **AchievementNotification:**
  - Anchor: Top-Center
  - Pos Y: `-100` (cách top 100px)
  - Width: `400-600`
  - Height: `150-200`

**Layout Group (Optional):**
- Add `Vertical Layout Group` vào root để tự động sắp xếp text

### 2.4 Convert to Prefab

1. Kéo GameObject `AchievementNotification` từ Hierarchy vào `Assets/Prefabs/UI/`
2. Xóa GameObject trong Hierarchy (prefab đã được tạo)
3. **Lưu ý:** Prefab này sẽ được spawn động, không cần đặt trong scene

### 2.5 Gán Prefab vào AchievementManager

1. Chọn `_Systems` → `AchievementManager`
2. Kéo prefab `AchievementNotification` vào field `Achievement Notification Prefab`
3. **Notification Parent:** Để trống tạm (sẽ setup ở bước 4)

---

## 📝 Bước 3: Tạo Achievement List UI Item Prefab

### 3.1 Tạo Prefab Structure

1. Tạo GameObject mới trong Hierarchy: `AchievementListItem`
2. **Hierarchy Structure:**
```
AchievementListItem (GameObject)
├── Background (Image) - Optional
├── TitleText (TextMeshProUGUI)
├── DescriptionText (TextMeshProUGUI)
├── PointsText (TextMeshProUGUI)
└── UnlockedBadge (GameObject)
    └── BadgeText (TextMeshProUGUI) - hoặc Icon (Image)
```

### 3.2 Setup UI Components

1. **AchievementListItem (Root)**
   - Add Component → `AchievementListUIItem` script
   - **Inspector Settings:**
     - `Title Text`: Kéo `TitleText` vào
     - `Description Text`: Kéo `DescriptionText` vào
     - `Points Text`: Kéo `PointsText` vào
     - `Unlocked Badge`: Kéo `UnlockedBadge` vào

2. **TitleText (TextMeshProUGUI)**
   - Text: `"Achievement Name"`
   - Font Size: `20-24`
   - Alignment: Left

3. **DescriptionText (TextMeshProUGUI)**
   - Text: `"Description"`
   - Font Size: `14-16`
   - Alignment: Left
   - Color: Xám nhạt

4. **PointsText (TextMeshProUGUI)**
   - Text: `"+100 pts"`
   - Font Size: `16-18`
   - Alignment: Right
   - Color: Vàng

5. **UnlockedBadge (GameObject)**
   - Add Component → `Image` (hoặc chỉ dùng Text)
   - **UnlockedBadge/TextMeshProUGUI:**
     - Text: `"✓ UNLOCKED"` hoặc icon
     - Font Size: `14`
     - Color: Xanh lá
   - **Mặc định:** Set `SetActive(false)` (chỉ hiện khi unlocked)

### 3.3 Layout

**RectTransform:**
- Width: `600-800`
- Height: `80-100`

**Layout Group:**
- Add `Horizontal Layout Group` hoặc dùng manual layout
- Spacing: `10-20`

### 3.4 Convert to Prefab

1. Kéo `AchievementListItem` vào `Assets/Prefabs/UI/`
2. Xóa GameObject trong Hierarchy

---

## 📝 Bước 4: Setup Achievement List UI trong AuthScene

### 4.1 Tạo Achievement Panel

1. Mở **AuthScene**
2. Trong Canvas, tạo Panel mới: `AchievementPanel`
3. **Panel Settings:**
   - Background: Màu đen với alpha 0.9
   - RectTransform: Stretch to fill (full screen)

### 4.2 Tạo ScrollView

**Cách 1: Dùng Unity UI Menu (Khuyến nghị)**
1. Right-click vào `AchievementPanel` trong Hierarchy
2. Chọn **UI → Scroll View**
3. Unity sẽ tự động tạo:
```
AchievementScrollView (GameObject với Scroll Rect component)
├── Viewport (GameObject)
│   └── Content (GameObject) ← Đây là contentParent
└── Scrollbar Vertical (GameObject) - Optional
```

**Cách 2: Tạo thủ công**
1. Trong `AchievementPanel`, tạo GameObject: `AchievementScrollView`
2. Add Component → `Scroll Rect`
3. Tạo GameObject con: `Viewport`
   - Add Component → `Image` (màu trắng, alpha 0)
   - Add Component → `Mask` (để clip content)
4. Trong `Viewport`, tạo GameObject con: `Content`
   - Đây là nơi sẽ spawn achievement items
5. **Scroll Rect Settings:**
   - `Content`: Kéo `Content` GameObject vào
   - `Viewport`: Kéo `Viewport` GameObject vào
   - `Vertical`: Tick ✅
   - `Horizontal`: Bỏ tick ❌

### 4.3 Setup AchievementListUI Script

#### 4.3.1 Tạo Empty State Text

**Cấu trúc Hierarchy:**
```
AchievementPanel
├── AchievementScrollView (Scroll Rect)
│   ├── Viewport
│   │   └── Content ← Content Parent
│   ├── Scrollbar Vertical
│   └── Scrollbar Horizontal
└── EmptyStateText (TextMeshProUGUI) ← Tạo ở đây
```

**Cách tạo:**
1. Right-click vào `AchievementPanel` trong Hierarchy
2. Chọn **UI → Text - TextMeshPro**
3. Đổi tên thành `EmptyStateText`
4. **TextMeshPro Settings:**
   - Text: `"No achievements available."`
   - Font Size: `18-24`
   - Alignment: Center (cả horizontal và vertical)
   - Color: Xám nhạt (ví dụ: #808080)
   - RectTransform:
     - Anchor: Middle-Center
     - Width: `400-600`
     - Height: `50-80`
     - Pos X: `0`, Pos Y: `0` (giữa màn hình)
5. **Mặc định:** Set `SetActive(false)` (sẽ tự động hiện khi list rỗng)

#### 4.3.2 Setup Content Layout (Quan trọng - Tránh items chồng lên nhau)

**Vấn đề:** Nếu không có Layout Group, achievement items sẽ spawn chồng lên nhau ở vị trí (0,0).

**Giải pháp:** Thêm Vertical Layout Group vào Content

1. Chọn GameObject `Content` (trong Viewport của ScrollView)
2. Add Component → `Vertical Layout Group`
3. **Vertical Layout Group Settings:**
   - `Child Alignment`: Upper Center (hoặc Upper Left)
   - `Child Control Size`: 
     - ✅ Tick `Width` (items sẽ có cùng width với Content)
     - ✅ Tick `Height` (items giữ height của prefab)
   - `Child Force Expand`:
     - ❌ Bỏ tick `Width` (không stretch width)
     - ❌ Bỏ tick `Height` (không stretch height)
   - `Spacing`: `10-20` (khoảng cách giữa các items)
   - `Padding`: 
     - Left: `10`, Right: `10`
     - Top: `10`, Bottom: `10`

4. **Thêm Content Size Fitter (Optional nhưng khuyến nghị):**
   - Chọn `Content` GameObject
   - Add Component → `Content Size Fitter`
   - `Vertical Fit`: `Preferred Size` (tự động resize theo số items)
   - `Horizontal Fit`: `Unconstrained` (giữ width của Content)

**Kết quả:** Achievement items sẽ tự động xếp theo chiều dọc với khoảng cách đều nhau.

#### 4.3.3 Gán Script AchievementListUI

1. Chọn `AchievementPanel` (root của panel)
2. Add Component → `AchievementListUI`
3. **Inspector Settings:**
   - `Content Parent`: Kéo `Content` GameObject (trong Viewport của ScrollView) vào
   - `Item Prefab`: Kéo prefab `AchievementListItem` (đã tạo ở Bước 3) vào
   - `Empty State Text`: Kéo `EmptyStateText` (vừa tạo ở trên) vào

### 4.4 Tạo Button Để Mở/Đóng Panel

#### 4.4.1 Tạo Show Achievements Button

1. Tạo Button: `ShowAchievementsButton` (có thể đặt trong MainMenuPanel)
   - Right-click `MainMenuPanel` → **UI → Button - TextMeshPro**
   - Đổi tên thành `ShowAchievementsButton`
   - Text: `"Achievements"` hoặc `"View Achievements"`

2. **Setup OnClick Event:**
   - Chọn Button `ShowAchievementsButton`
   - Trong Inspector, tìm section **Button (Script)**
   - Tìm **OnClick()** section (ở cuối component)
   - Click nút **+** (Add) để thêm event mới
   - **Bước 1:** Kéo `AchievementPanel` từ Hierarchy vào ô trống (không có text)
   - **Bước 2:** Click dropdown bên cạnh (hiện "No Function")
   - **Bước 3:** Chọn: **GameObject → SetActive(bool)**
   - **Bước 4:** Tick checkbox ✅ (để set `true` = hiện panel)

**Hình ảnh mô tả:**
```
OnClick()
└── [0] AchievementPanel (GameObject)
    └── GameObject → SetActive(bool) ✅ [checked]
```

#### 4.4.2 Tạo Close Achievements Button

1. Tạo Button: `CloseAchievementsButton` (trong AchievementPanel)
   - Right-click `AchievementPanel` → **UI → Button - TextMeshPro**
   - Đổi tên thành `CloseAchievementsButton`
   - Text: `"Close"` hoặc `"X"`
   - Đặt ở góc trên phải của panel

2. **Setup OnClick Event:**
   - Chọn Button `CloseAchievementsButton`
   - Trong Inspector, tìm **OnClick()** section
   - Click nút **+** (Add) để thêm event mới
   - **Bước 1:** Kéo `AchievementPanel` từ Hierarchy vào ô trống
   - **Bước 2:** Click dropdown → Chọn: **GameObject → SetActive(bool)**
   - **Bước 3:** Bỏ tick checkbox ❌ (để set `false` = ẩn panel)

**Hình ảnh mô tả:**
```
OnClick()
└── [0] AchievementPanel (GameObject)
    └── GameObject → SetActive(bool) ❌ [unchecked]
```

**Lưu ý:** Nếu không thấy method `SetActive(bool)` trong dropdown:
- Đảm bảo đã kéo đúng GameObject (không phải component)
- Thử click vào dropdown và scroll xuống tìm "GameObject"
- Method `SetActive` luôn có sẵn cho mọi GameObject

### 4.5 Setup Notification Parent (Quan trọng - Để hiển thị notification)

Notification sẽ hiện khi player unlock achievement mới (sau khi complete level).

#### 4.5.1 Tạo Notification Parent

**Cách 1: Dùng Canvas Root (Khuyến nghị)**
1. Tìm GameObject `Canvas` trong Hierarchy
2. Đảm bảo Canvas có **Canvas Component** với:
   - `Render Mode`: Screen Space - Overlay (hoặc Screen Space - Camera)
   - `Sort Order`: Đặt số cao (ví dụ: 100) để luôn ở trên cùng

**Cách 2: Tạo NotificationParent riêng**
1. Right-click `Canvas` → **Create Empty**
2. Đổi tên thành `NotificationParent`
3. **RectTransform:**
   - Anchor: Stretch to fill (phủ toàn bộ Canvas)
   - Left: `0`, Right: `0`, Top: `0`, Bottom: `0`
   - Pos X: `0`, Pos Y: `0`
4. **Đảm bảo nằm trên cùng:**
   - Trong Hierarchy, kéo `NotificationParent` xuống cuối cùng (hoặc set Order in Layer cao)

#### 4.5.2 Gán vào AchievementManager

1. Chọn GameObject `_Systems` (có AchievementManager component)
2. Trong Inspector, tìm component `AchievementManager`
3. **Inspector Settings:**
   - `Achievement Notification Prefab`: Kéo prefab `AchievementNotification` (đã tạo ở Bước 2) vào
   - `Notification Parent`: 
     - **Cách 1:** Kéo `Canvas` root vào (nếu dùng Canvas root)
     - **Cách 2:** Kéo `NotificationParent` GameObject vào (nếu tạo riêng)

#### 4.5.3 Kiểm Tra Notification Prefab

Đảm bảo prefab `AchievementNotification` đã được setup đúng:
1. Mở prefab `AchievementNotification` trong Project
2. Kiểm tra có component `AchievementNotification` script
3. Kiểm tra có `CanvasGroup` component
4. Kiểm tra có `TitleText` và `DescriptionText` (TextMeshProUGUI)
5. Trong script `AchievementNotification`:
   - `Title Text`: Đã gán TextMeshProUGUI
   - `Description Text`: Đã gán TextMeshProUGUI
   - `Canvas Group`: Đã gán CanvasGroup component
   - `Lifetime Seconds`: `3` (thời gian hiển thị)

#### 4.5.4 Khi Nào Notification Hiện? (Quan Trọng!)

**Flow hiện tại:**

1. **Khi đang chơi game (Level 1, 2, 3):**
   - ❌ **KHÔNG có notification** trong lúc chơi
   - Player collect coins, kill enemies, tăng score → Chỉ track local, chưa check achievements

2. **Khi complete level (chạm LevelExit):**
   - ✅ **Notification SẼ HIỆN** sau khi complete level
   - Flow chi tiết:
     ```
     Player chạm LevelExit
     ↓
     LevelExit.LoadNextLevel() được gọi
     ↓
     GameSession.EndSession("COMPLETED") được gọi
     ↓
     Server sync session và update GameProfile (score, coins, enemies, etc.)
     ↓
     LevelProgressManager.CompleteLevel() → Sync level progress
     ↓
     AchievementManager.RefreshUnlocked(true) → Check achievements mới
     ↓
     Nếu có achievement mới unlock → ShowNotification() → Notification hiện!
     ↓
     Đợi 2 giây để player thấy notification
     ↓
     Load level tiếp theo (hoặc về MainMenu)
     ```

3. **Notification hiện ở đâu:**
   - Hiện ở **top-center** của màn hình (theo setup trong prefab)
   - Hiện trong **2-3 giây** (lifetimeSeconds = 3)
   - Tự động fade out và destroy

**Ví dụ cụ thể:**
- Player chơi Level 1, collect 50 coins → **KHÔNG có notification** (vì chưa complete)
- Player chạm LevelExit → Complete Level 1
- Server check: Player đã collect 50 coins → Chưa đạt "Coin Collector 100" → **KHÔNG có notification**
- Player chơi Level 2, collect thêm 60 coins → Tổng = 110 coins
- Player chạm LevelExit → Complete Level 2
- Server check: Player có 110 coins → Đạt "Coin Collector 100" → **Notification hiện!** 🎉
- Notification hiện 3 giây → Load Level 3

**Lưu ý:**
- Notification chỉ hiện **SAU KHI COMPLETE LEVEL**, không hiện trong lúc chơi
- Nếu muốn notification hiện real-time (ngay khi đạt điều kiện), cần modify code để check achievements sau mỗi action (coin, kill enemy, etc.)

**Để test notification:**
1. Chơi game và complete level
2. Nếu đạt điều kiện achievement → Notification sẽ hiện ngay sau khi complete
3. Nếu không đạt → Không có notification (bình thường)

### 4.6 Hook Refresh Logic với AchievementPanelController

Script `AchievementPanelController.cs` đã được tạo sẵn để quản lý việc mở/đóng panel và refresh achievements từ server.

#### 4.6.1 Add Component AchievementPanelController

1. Chọn GameObject `AchievementPanel` (root của panel)
2. Add Component → `AchievementPanelController`
3. **Inspector Settings:**
   - `Achievement Panel`: Kéo chính `AchievementPanel` GameObject vào (hoặc để trống, script sẽ tự tìm)
   - `Achievement List UI`: Kéo component `AchievementListUI` (đã add ở bước 4.3.3) vào
   - `Main Menu Panel`: **Kéo `MainMenuPanel` GameObject vào** (để ẩn khi mở achievements)
   - `Loading Overlay`: (Optional) Kéo GameObject loading overlay nếu có
   - `Refresh On Show`: ✅ Tick (tự động refresh khi mở panel)
   - `Hide On Start`: ✅ Tick (ẩn panel khi game start)
   - `Hide Main Menu On Show`: ✅ Tick (ẩn MainMenuPanel khi mở AchievementPanel)

#### 4.6.2 Update Button OnClick Events

**Cách 1: Dùng AchievementPanelController (Khuyến nghị)**

1. **ShowAchievementsButton:**
   - Chọn Button `ShowAchievementsButton`
   - Trong Inspector, tìm **OnClick()** section
   - Xóa event cũ (nếu có) hoặc thêm event mới
   - **Bước 1:** Kéo GameObject có `AchievementPanelController` (thường là `AchievementPanel`) vào
   - **Bước 2:** Click dropdown → Chọn: **AchievementPanelController → OnShowPanel()**
   
   **Kết quả:**
   ```
   OnClick()
   └── [0] AchievementPanel (AchievementPanelController)
       └── AchievementPanelController → OnShowPanel()
   ```

2. **CloseAchievementsButton:**
   - Chọn Button `CloseAchievementsButton`
   - Trong Inspector, tìm **OnClick()** section
   - **Bước 1:** Kéo GameObject có `AchievementPanelController` vào
   - **Bước 2:** Click dropdown → Chọn: **AchievementPanelController → OnClosePanel()**
   
   **Kết quả:**
   ```
   OnClick()
   └── [0] AchievementPanel (AchievementPanelController)
       └── AchievementPanelController → OnClosePanel()
   ```

**Cách 2: Dùng GameObject.SetActive (Đơn giản, không refresh)**

Nếu không muốn refresh từ server mỗi lần mở, có thể dùng cách cũ:
- ShowAchievementsButton → `GameObject.SetActive(true)`
- CloseAchievementsButton → `GameObject.SetActive(false)`

**Lưu ý:** Với cách này, achievements sẽ chỉ hiển thị data đã cache, không fetch mới từ server.

#### 4.6.3 Tạo Refresh Button (Optional)

Nếu muốn có nút Refresh riêng trong AchievementPanel:

1. Tạo Button: `RefreshAchievementsButton` (trong AchievementPanel)
   - Right-click `AchievementPanel` → **UI → Button - TextMeshPro**
   - Text: `"Refresh"` hoặc icon refresh
   - Đặt ở góc trên (cạnh Close button)

2. **Setup OnClick:**
   - Chọn Button `RefreshAchievementsButton`
   - OnClick() → Kéo `AchievementPanel` vào
   - Dropdown → **AchievementPanelController → OnRefreshAchievements()**

#### 4.6.4 Tạo Loading Overlay (Optional)

Nếu muốn hiển thị loading khi refresh:

1. Trong `AchievementPanel`, tạo GameObject: `LoadingOverlay`
   - Right-click `AchievementPanel` → **UI → Panel**
   - Đổi tên thành `LoadingOverlay`
   - Background: Màu đen với alpha 0.5
   - RectTransform: Stretch to fill (phủ toàn bộ panel)

2. Trong `LoadingOverlay`, tạo TextMeshPro: `LoadingText`
   - Text: `"Loading achievements..."` hoặc spinner icon
   - Alignment: Center
   - Font Size: `18-24`

3. **Mặc định:** Set `LoadingOverlay.SetActive(false)`

4. **Gán vào AchievementPanelController:**
   - Chọn `AchievementPanel` → `AchievementPanelController`
   - Kéo `LoadingOverlay` vào field `Loading Overlay`

**Kết quả:** Khi mở panel hoặc refresh, loading overlay sẽ tự động hiện/ẩn.

#### 4.6.5 Flow Hoạt Động

Khi player click "Show Achievements":
1. `OnShowPanel()` được gọi
2. Panel được hiện (`SetActive(true)`)
3. Nếu `Refresh On Show` = true:
   - Loading overlay hiện (nếu có)
   - Gọi `AchievementManager.RefreshAll()` → Fetch achievements và unlocked từ server
   - Gọi `AchievementListUI.RefreshList()` → Update UI với data mới
   - Loading overlay ẩn
4. Player thấy danh sách achievements mới nhất

**Lợi ích:**
- ✅ Luôn có data mới nhất từ server
- ✅ Tự động refresh khi mở panel
- ✅ Có thể refresh thủ công bằng nút Refresh
- ✅ Loading indicator (optional) để UX tốt hơn

---

## 📝 Bước 5: Kiểm Tra Level Mapping

### 5.1 Server Level Configuration

Đảm bảo server có levels với cấu trúc:
```json
{
  "_id": "level_object_id",
  "levelName": "Level 1",  // Phải trùng với scene name
  "levelNumber": 1          // Phải trùng với build index
}
```

### 5.2 Unity Build Settings

1. Mở **File → Build Settings**
2. Kiểm tra scene order:
   - Scene 0: `AuthScene` (hoặc MainMenu)
   - Scene 1: `Level 1` ← levelNumber = 1
   - Scene 2: `Level 2` ← levelNumber = 2
   - Scene 3: `Level 3` ← levelNumber = 3

### 5.3 Scene Names

Đảm bảo scene names trong Unity trùng với `levelName` trên server:
- Scene name: `"Level 1"` → Server `levelName: "Level 1"`
- Scene name: `"Level 2"` → Server `levelName: "Level 2"`
- Scene name: `"Level 3"` → Server `levelName: "Level 3"`

### 5.4 Mapping Logic (Tự Động)

`LevelProgressManager` sẽ tự động map theo thứ tự ưu tiên:
1. **levelNumber == buildIndex** (ưu tiên nhất)
2. **levelName == scene.name** (fallback)
3. **buildIndex as string** (fallback cuối)

---

## 📝 Bước 6: Kiểm Tra GameSession Integration

### 6.1 GameSession Đã Tích Hợp

`GameSession.cs` đã được modify để:
- ✅ Resolve levelId từ `LevelProgressManager` khi start session
- ✅ Complete level progress khi session end với status "COMPLETED"
- ✅ Refresh achievements và show notifications sau khi complete level

### 6.2 Flow Tự Động

Khi player hoàn thành level:
1. `LevelExit` trigger → `GameSession.EndSession("COMPLETED")`
2. `GameSession` gọi `LevelProgressManager.CompleteLevel()` → Sync lên server
3. `GameSession` gọi `AchievementManager.RefreshUnlocked(true)` → Check và show notifications
4. Server tự động update GameProfile và check achievements

**Không cần thêm code**, chỉ cần đảm bảo managers đã được setup.

---

## 🧪 Testing Checklist

### ✅ Basic Flow Test

- [ ] **Start Game:**
  - [ ] Load AuthScene → Login thành công
  - [ ] MainMenu hiển thị username
  - [ ] Click Play → Load Level 1

- [ ] **Gameplay:**
  - [ ] Collect coin → Coin count tăng
  - [ ] Kill enemy → Enemy count tăng
  - [ ] Score tăng khi collect coin

- [ ] **Complete Level:**
  - [ ] Đi đến LevelExit → Level complete
  - [ ] Check Console: `"[LevelProgress] CompleteLevel success"`
  - [ ] Check Console: `"[GameSession] Session ended successfully"`
  - [ ] Check Console: `"[Achievement] Refresh unlocked"`

- [ ] **Achievement Notification:**
  - [ ] Nếu đạt achievement → Popup hiển thị
  - [ ] Popup tự động fade out sau 3 giây

- [ ] **Achievement List:**
  - [ ] Mở Achievement Panel
  - [ ] Danh sách achievements hiển thị
  - [ ] Achievements đã unlock có badge "UNLOCKED"
  - [ ] Points hiển thị đúng

### ✅ Server Verification

- [ ] **Check MongoDB:**
  - [ ] Collection `levelprogressions`: Có record mới với `isCompleted: true`
  - [ ] `bestScore`, `coinsCollected`, `enemiesDefeated` đúng
  - [ ] Collection `playerachievements`: Có achievement mới unlock (nếu đạt điều kiện)
  - [ ] Collection `gameprofiles`: `totalScore`, `totalCoinsCollected` tăng

### ✅ Edge Cases

- [ ] **Offline Mode:**
  - [ ] Tắt internet → Complete level → Không crash
  - [ ] Bật lại internet → Chơi lại → Sync lại

- [ ] **Multiple Levels:**
  - [ ] Complete Level 1 → Check progress
  - [ ] Complete Level 2 → Check progress khác với Level 1
  - [ ] Complete Level 3 → Check progress

- [ ] **Level Mapping:**
  - [ ] Nếu server không có level → Fallback to buildIndex
  - [ ] Console warning: `"Falling back to scene index as levelId"`

---

## 🔧 Troubleshooting

### ❌ Achievement Notification Không Hiện

**Nguyên nhân:**
- Prefab chưa được gán vào AchievementManager
- NotificationParent chưa được set
- Achievement chưa đạt điều kiện unlock

**Giải pháp:**
1. Check `_Systems` → `AchievementManager` → `Achievement Notification Prefab` đã gán chưa
2. Check `Notification Parent` đã gán chưa
3. Check Console logs: `"[Achievement] Refresh unlocked"`
4. Check server GameProfile stats có đạt điều kiện achievement chưa

### ❌ Level Progress Không Sync

**Nguyên nhân:**
- LevelId không được resolve đúng
- Server không có level tương ứng
- Network error

**Giải pháp:**
1. Check Console: `"[LevelProgress] Cached X levels"` khi start game
2. Check Console: `"[LevelProgress] CompleteLevel success"` khi complete
3. Verify server có level với `levelNumber` hoặc `levelName` trùng
4. Check Network tab trong Unity Editor → Xem API response

### ❌ Achievement List Rỗng

**Nguyên nhân:**
- Achievements chưa được fetch
- ContentParent chưa được gán
- ItemPrefab chưa được gán

**Giải pháp:**
1. Check `AchievementListUI` → `Content Parent` đã gán chưa
2. Check `Item Prefab` đã gán chưa
3. Gọi `AchievementManager.Instance.RefreshAll()` trước khi mở panel
4. Check Console: `"[Achievement] Fetch achievements"` có log không

### ❌ Level Mapping Sai

**Nguyên nhân:**
- Server levelNumber không trùng buildIndex
- Server levelName không trùng scene name

**Giải pháp:**
1. Check server levels collection: `levelNumber` và `levelName`
2. Check Unity Build Settings: Scene order và names
3. Check Console: `"[LevelProgress] Cached X levels"` → Xem mapping
4. Fallback sẽ dùng buildIndex nếu không match

---

## 📊 API Endpoints Sử Dụng

### Level Progress
- `GET /api/levels` - Fetch danh sách levels (LevelProgressManager)
- `POST /api/levelProgress/:userId/:levelId/complete` - Complete level (GameSession)

### Achievements
- `GET /api/achievements` - Fetch tất cả achievements (AchievementManager)
- `GET /api/achievements/:userId/unlocked` - Fetch achievements đã unlock (AchievementManager)

### Sessions (Phase 2)
- `POST /api/sessions` - Start session (GameSession)
- `PUT /api/sessions/:sessionId` - Update session (GameSession)
- `POST /api/sessions/:sessionId/end` - End session (GameSession)

---

## 📁 Files Reference

### Scripts Đã Tạo
- `Assets/Scripts/Managers/LevelProgressManager.cs`
- `Assets/Scripts/Managers/AchievementManager.cs`
- `Assets/Scripts/UI/AchievementNotification.cs`
- `Assets/Scripts/UI/AchievementListUI.cs`
- `Assets/Scripts/UI/AchievementListUIItem.cs`

### Scripts Đã Modify
- `Assets/Scripts/Gameplay/GameSession.cs` (thêm complete level logic)
- `Assets/Scripts/Network/APIConfig.cs` (thêm endpoints)

### Prefabs Cần Tạo
- `Assets/Prefabs/UI/AchievementNotification.prefab`
- `Assets/Prefabs/UI/AchievementListItem.prefab`

---

## ✅ Hoàn Thành Setup

Sau khi hoàn thành tất cả các bước trên:

1. ✅ Managers đã được setup trong AuthScene
2. ✅ Prefabs đã được tạo và gán
3. ✅ UI panels đã được setup
4. ✅ Level mapping đã được verify
5. ✅ Testing checklist đã pass

**Phase 3 đã sẵn sàng!** 🎉

---

## 🚀 Next Steps (Phase 4)

Sau khi Phase 3 hoạt động tốt, có thể tiếp tục Phase 4:
- Leaderboard UI
- Player rank display
- Weekly/Daily leaderboards
- Player profile comparison

---

**Document Version:** 1.0  
**Last Updated:** December 2025  
**Status:** ✅ Ready for Implementation

