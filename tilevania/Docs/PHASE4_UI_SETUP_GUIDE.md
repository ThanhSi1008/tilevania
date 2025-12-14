# 🎨 PHASE 4: HƯỚNG DẪN SETUP UI TRONG UNITY EDITOR

**Document Version:** 2.0  
**Date Created:** December 13, 2025  
**Last Updated:** December 13, 2025  
**Phase:** Phase 4 - Leaderboards & Multiplayer Features  
**Status:** ✅ Scripts Completed - Ready for UI Setup

---

## 🎯 QUICK STATUS

### ✅ Scripts Status: **HOÀN THÀNH 100%**

Tất cả scripts Phase 4 đã được tạo và hoàn chỉnh:

- ✅ **LeaderboardManager.cs** - Manager cho leaderboard data
- ✅ **LeaderboardUI.cs** - UI controller cho leaderboard screen
- ✅ **LeaderboardEntryUI.cs** - Component cho mỗi entry
- ✅ **PlayerProfileUI.cs** - UI để xem player profile
- ✅ **MainMenuManager.cs** - Đã cập nhật với Phase 4 features
- ✅ **HUDManager.cs** - Đã cập nhật với Phase 4 features
- ✅ **APIConfig.cs** - Đã thêm leaderboard endpoints

**Compile Status:** ✅ No errors, no warnings

**Next Step:** Setup UI trong Unity Editor và assign references

---

## 📋 Mục Lục

1. [Tổng Quan](#tổng-quan)
2. [Chuẩn Bị](#chuẩn-bị)
3. [Scene Structure - Chi Tiết](#scene-structure---chi-tiết)
4. [Main Menu UI - Setup Từng Bước](#main-menu-ui---setup-từng-bước)
5. [Leaderboard Screen UI - Setup Từng Bước](#leaderboard-screen-ui---setup-từng-bước)
6. [Player Profile UI - Setup Từng Bước](#player-profile-ui---setup-từng-bước)
7. [HUD Updates - Setup Từng Bước](#hud-updates---setup-từng-bước)
8. [Kết Nối Scripts với UI - Chi Tiết](#kết-nối-scripts-với-ui---chi-tiết)
9. [Testing UI trong Editor](#testing-ui-trong-editor)

---

## 🎯 Tổng Quan

Phase 4 yêu cầu tạo các UI components sau:

1. **Main Menu** - Hiển thị rank và stats của player
2. **Leaderboard Screen** - Hiển thị bảng xếp hạng (All-time, Weekly, Daily)
3. **Player Profile UI** - Xem profile của player khác
4. **HUD Updates** - Cập nhật real-time stats trong game

---

## 🛠️ Chuẩn Bị

### Yêu Cầu
- ✅ Unity 2022 LTS hoặc mới hơn
- ✅ TextMeshPro đã được import
- ✅ UI Canvas system đã setup
- ✅ Các scripts từ Phase 1-3 đã hoàn thành

### Packages Cần Thiết
- TextMeshPro (đã có sẵn)
- Unity UI (built-in)

---

## 📐 Scene Structure - Chi Tiết

### Bước 1: Mở Scene Main Menu

1. **Mở Unity Editor**
2. **Mở Scene Main Menu** (hoặc scene chứa main menu)
   - Thường là scene đầu tiên trong Build Settings
   - Hoặc scene có tên "MainMenu", "AuthScene", etc.

### Bước 2: Kiểm Tra Canvas Hiện Tại

1. **Tìm Canvas trong Hierarchy:**
   - Nếu đã có Canvas → Sử dụng Canvas đó
   - Nếu chưa có → Tạo mới

2. **Nếu cần tạo Canvas mới:**
   - Right-click trong Hierarchy → **UI → Canvas**
   - Đặt tên: `Canvas (Main)`
   - Unity sẽ tự động tạo EventSystem

### Bước 3: Setup Canvas Settings

1. **Chọn Canvas trong Hierarchy**

2. **Trong Inspector, tìm Canvas component:**
   - **Render Mode:** Screen Space - Overlay (mặc định)
   - **Pixel Perfect:** ✅ (tùy chọn, để UI sắc nét hơn)

3. **Tìm Canvas Scaler component:**
   - **UI Scale Mode:** Scale With Screen Size
   - **Reference Resolution:**
     - X: **1920**
     - Y: **1080**
   - **Match:** **0.5** (Width/Height)
   - **Reference Pixels Per Unit:** 100

4. **Tìm Graphic Raycaster component:**
   - Giữ nguyên mặc định

### Bước 4: Tạo Panel Structure

1. **Tạo MainMenuPanel:**
   - Right-click Canvas → **UI → Panel**
   - Đặt tên: `MainMenuPanel`
   - **Rect Transform:**
     - Anchor: **Stretch-Stretch** (click vào anchor preset ở góc trên trái)
     - Left, Right, Top, Bottom: **0** (full screen)

2. **Tạo LeaderboardPanel:**
   - Right-click Canvas → **UI → Panel**
   - Đặt tên: `LeaderboardPanel`
   - **Rect Transform:** Stretch-Stretch, Left/Right/Top/Bottom = 0
   - **Set Active = false** (click checkbox ở Inspector)

3. **Tạo PlayerProfilePanel:**
   - Right-click Canvas → **UI → Panel**
   - Đặt tên: `PlayerProfilePanel`
   - **Rect Transform:** Stretch-Stretch, Left/Right/Top/Bottom = 0
   - **Set Active = false**

### Bước 5: Hierarchy Structure Cuối Cùng

Sau khi setup, Hierarchy sẽ trông như sau:

```
Canvas (Main)
├── EventSystem
├── MainMenuPanel
│   ├── PlayerStatsContainer
│   └── ButtonContainer
├── LeaderboardPanel
│   ├── HeaderContainer
│   ├── TabContainer
│   └── LeaderboardScrollView
└── PlayerProfilePanel
    ├── ProfileHeader
    └── StatsContainer
```

---

## 🏠 Main Menu UI - Setup Từng Bước

### Bước 1: Setup MainMenuPanel Background

1. **Chọn MainMenuPanel trong Hierarchy**

2. **Trong Inspector, tìm Image component:**
   - **Color:** Chọn màu nền (ví dụ: Dark Blue với Alpha = 255)
   - Hoặc **Source Image:** Kéo sprite background vào (nếu có)

### Bước 2: Tạo PlayerStatsContainer

1. **Tạo Empty GameObject:**
   - Right-click MainMenuPanel → **Create Empty**
   - Đặt tên: `PlayerStatsContainer`

2. **Setup Rect Transform:**
   - **Anchor:** Top-Center
   - **Position Y:** -100 (cách top 100px)
   - **Width:** 600
   - **Height:** 400

3. **Thêm Vertical Layout Group:**
   - Add Component → **Layout → Vertical Layout Group**
   - **Spacing:** 20
   - **Child Alignment:** Middle Center
   - **Child Force Expand:**
     - ✅ Width
     - ❌ Height
   - **Child Control Size:**
     - ✅ Width
     - ❌ Height

### Bước 3: Tạo Player Name Text

1. **Tạo TextMeshPro:**
   - Right-click PlayerStatsContainer → **UI → Text - TextMeshPro**
   - Unity sẽ hỏi Import TMP Essentials → Click **Import**
   - Đặt tên: `PlayerNameText`

2. **Setup TextMeshPro:**
   - **Text:** "Welcome, Player"
   - **Font Size:** 36
   - **Alignment:** Center (cả Horizontal và Vertical)
   - **Color:** White
   - **Rect Transform:**
     - Width: 500
     - Height: 50

### Bước 4: Tạo Rank Display

1. **Tạo Container:**
   - Right-click PlayerStatsContainer → **Create Empty**
   - Đặt tên: `RankDisplayContainer`

2. **Thêm Horizontal Layout Group:**
   - Add Component → **Layout → Horizontal Layout Group**
   - **Spacing:** 10
   - **Child Alignment:** Middle Center

3. **Tạo Rank Label:**
   - Right-click RankDisplayContainer → **UI → Text - TextMeshPro**
   - Đặt tên: `RankLabelText`
   - **Text:** "Global Rank:"
   - **Font Size:** 24
   - **Color:** Light Gray (RGB: 200, 200, 200)
   - **Width:** 150, **Height:** 40

4. **Tạo Rank Value:**
   - Right-click RankDisplayContainer → **UI → Text - TextMeshPro**
   - Đặt tên: `RankValueText`
   - **Text:** "#1"
   - **Font Size:** 32
   - **Font Style:** Bold
   - **Color:** Yellow/Gold (RGB: 255, 215, 0)
   - **Width:** 100, **Height:** 40

### Bước 5: Tạo Total Score Display

1. **Tạo Container:**
   - Right-click PlayerStatsContainer → **Create Empty**
   - Đặt tên: `TotalScoreContainer`

2. **Thêm Horizontal Layout Group** (giống RankDisplayContainer)

3. **Tạo Score Label:**
   - Right-click TotalScoreContainer → **UI → Text - TextMeshPro**
   - Đặt tên: `ScoreLabelText`
   - **Text:** "Total Score:"
   - **Font Size:** 20
   - **Color:** Light Gray
   - **Width:** 150, **Height:** 35

4. **Tạo Score Value:**
   - Right-click TotalScoreContainer → **UI → Text - TextMeshPro**
   - Đặt tên: `ScoreValueText`
   - **Text:** "0"
   - **Font Size:** 28
   - **Color:** White
   - **Width:** 200, **Height:** 35

### Bước 6: Tạo Total Coins Display

1. **Lặp lại Bước 5** nhưng với:
   - Container: `CoinsContainer`
   - Label: `CoinsLabelText` - Text: "Total Coins:"
   - Value: `CoinsValueText` - Text: "0"

### Bước 7: Tạo Achievements Display

1. **Lặp lại Bước 5** nhưng với:
   - Container: `AchievementsContainer`
   - Label: `AchievementsLabelText` - Text: "Achievements:"
   - Value: `AchievementsValueText` - Text: "0/8"

### Bước 8: Tạo Button Container

1. **Tạo Empty GameObject:**
   - Right-click MainMenuPanel → **Create Empty**
   - Đặt tên: `ButtonContainer`

2. **Setup Rect Transform:**
   - **Anchor:** Bottom-Center
   - **Position Y:** 100 (cách bottom 100px)
   - **Width:** 600
   - **Height:** 300

3. **Thêm Vertical Layout Group:**
   - **Spacing:** 15
   - **Child Alignment:** Middle Center
   - **Child Force Expand:** ✅ Width, ❌ Height

### Bước 9: Tạo Navigation Buttons

1. **Tạo Play Button:**
   - Right-click ButtonContainer → **UI → Button - TextMeshPro**
   - Đặt tên: `PlayButton`
   - **Text (Button):** "PLAY"
   - **Font Size:** 32
   - **Width:** 300, **Height:** 60
   - **Colors:**
     - Normal: Green (RGB: 0, 200, 0)
     - Highlighted: Light Green (RGB: 100, 255, 100)

2. **Tạo Leaderboard Button:**
   - Right-click ButtonContainer → **UI → Button - TextMeshPro**
   - Đặt tên: `LeaderboardButton`
   - **Text:** "LEADERBOARD"
   - **Font Size:** 28
   - **Width:** 300, **Height:** 50

3. **Tạo Achievements Button:**
   - Right-click ButtonContainer → **UI → Button - TextMeshPro**
   - Đặt tên: `AchievementsButton`
   - **Text:** "ACHIEVEMENTS"
   - **Font Size:** 28
   - **Width:** 300, **Height:** 50

4. **Tạo Logout Button:**
   - Right-click ButtonContainer → **UI → Button - TextMeshPro**
   - Đặt tên: `LogoutButton`
   - **Text:** "LOGOUT"
   - **Font Size:** 24
   - **Width:** 200, **Height:** 40
   - **Colors:**
     - Normal: Red (RGB: 200, 0, 0)
     - Highlighted: Light Red (RGB: 255, 100, 100)

---

## 🏆 Leaderboard Screen UI - Setup Từng Bước

### Bước 1: Setup LeaderboardPanel Background

1. **Chọn LeaderboardPanel trong Hierarchy**

2. **Image component:**
   - **Color:** Dark background (ví dụ: RGB: 30, 30, 30, Alpha: 255)

### Bước 2: Tạo Header Container

1. **Tạo Empty GameObject:**
   - Right-click LeaderboardPanel → **Create Empty**
   - Đặt tên: `HeaderContainer`

2. **Setup Rect Transform:**
   - **Anchor:** Top-Stretch
   - **Top:** 0, **Height:** 80
   - **Left:** 0, **Right:** 0

3. **Thêm Horizontal Layout Group:**
   - **Spacing:** 20
   - **Child Alignment:** Middle
   - **Padding:** Left/Right = 20

### Bước 3: Tạo Header Elements

1. **Tạo Back Button:**
   - Right-click HeaderContainer → **UI → Button - TextMeshPro**
   - Đặt tên: `BackButton`
   - **Text:** "← BACK"
   - **Font Size:** 24
   - **Width:** 120, **Height:** 50
   - **Rect Transform:** Set Left anchor để button ở góc trái

2. **Tạo Title Text:**
   - Right-click HeaderContainer → **UI → Text - TextMeshPro**
   - Đặt tên: `TitleText`
   - **Text:** "LEADERBOARD"
   - **Font Size:** 48
   - **Font Style:** Bold
   - **Alignment:** Center
   - **Color:** White
   - **Flexible Width:** ✅ (để chiếm không gian giữa)

3. **Tạo Refresh Button:**
   - Right-click HeaderContainer → **UI → Button - TextMeshPro**
   - Đặt tên: `RefreshButton`
   - **Text:** "🔄 REFRESH"
   - **Font Size:** 24
   - **Width:** 120, **Height:** 50
   - **Rect Transform:** Set Right anchor để button ở góc phải

### Bước 4: Tạo Tab Container

1. **Tạo Empty GameObject:**
   - Right-click LeaderboardPanel → **Create Empty**
   - Đặt tên: `TabContainer`

2. **Setup Rect Transform:**
   - **Anchor:** Top-Stretch
   - **Top:** 80, **Height:** 60
   - **Left:** 0, **Right:** 0

3. **Thêm Horizontal Layout Group:**
   - **Spacing:** 10
   - **Child Alignment:** Middle Center
   - **Padding:** Left/Right = 20

4. **Thêm Toggle Group:**
   - Add Component → **UI → Toggle Group**
   - Đặt tên: `LeaderboardTabGroup`

### Bước 5: Tạo Tab Buttons (Convert to Toggle)

1. **Tạo All-Time Tab:**
   - Right-click TabContainer → **UI → Button - TextMeshPro**
   - Đặt tên: `AllTimeTab`
   - **Text:** "ALL-TIME"
   - **Font Size:** 24
   - **Width:** 200, **Height:** 50

2. **Convert Button to Toggle:**
   - **Xóa Button component**
   - Add Component → **UI → Toggle**
   - **Toggle Group:** Kéo `LeaderboardTabGroup` vào
   - **Is On:** ✅ (default selected)
   - **Graphic:** Kéo Text element vào

3. **Lặp lại cho WeeklyTab và DailyTab:**
   - `WeeklyTab` - Text: "WEEKLY"
   - `DailyTab` - Text: "DAILY"
   - **Is On:** ❌ (không selected mặc định)

### Bước 6: Tạo Leaderboard Scroll View

1. **Tạo Scroll View:**
   - Right-click LeaderboardPanel → **UI → Scroll View**
   - Đặt tên: `LeaderboardScrollView`

2. **Setup Rect Transform:**
   - **Anchor:** Stretch-Stretch
   - **Top:** 140 (dưới header và tabs)
   - **Left:** 20, **Right:** 20
   - **Bottom:** 20

3. **Setup Scroll Rect component:**
   - **Movement Type:** Elastic
   - **Scroll Sensitivity:** 20
   - **Viewport:** Kéo Viewport child vào
   - **Content:** Kéo Content child vào
   - **Horizontal:** ❌ (chỉ scroll dọc)
   - **Vertical:** ✅

4. **Setup Viewport:**
   - Chọn `Viewport` child
   - **Mask component:** ✅ Enabled
   - **Show Mask Graphic:** ❌

5. **Setup Content:**
   - Chọn `Content` child trong Viewport
   - **Rect Transform:**
     - **Anchor:** Top-Stretch
     - **Top:** 0
     - **Left:** 0, **Right:** 0
     - **Height:** 0 (sẽ tự động expand)
   - **Add Vertical Layout Group:**
     - **Spacing:** 5
     - **Child Alignment:** Upper Center
     - **Child Force Expand:** ✅ Width, ❌ Height
     - **Child Control Size:** ✅ Height
   - **Add Content Size Fitter:**
     - **Vertical Fit:** Preferred Size

### Bước 7: Tạo Leaderboard Entry Prefab

1. **Tạo Entry Item trong Scene:**
   - Right-click Content → **UI → Panel**
   - Đặt tên: `LeaderboardEntryItem`

2. **Setup Entry Item:**
   - **Rect Transform:**
     - **Height:** 80
     - **Width:** Stretch (Left/Right = 0)
   - **Image Color:** Semi-transparent (RGB: 50, 50, 50, Alpha: 200)

3. **Thêm Horizontal Layout Group:**
   - **Spacing:** 20
   - **Padding:** Left/Right = 15
   - **Child Alignment:** Middle Center
   - **Child Force Expand:** ✅ Height

4. **Tạo Rank Text:**
   - Right-click LeaderboardEntryItem → **UI → Text - TextMeshPro**
   - Đặt tên: `RankText`
   - **Text:** "#1"
   - **Font Size:** 32
   - **Font Style:** Bold
   - **Alignment:** Center
   - **Color:** Yellow
   - **Width:** 80, **Height:** 60

5. **Tạo Player Info Container:**
   - Right-click LeaderboardEntryItem → **Create Empty**
   - Đặt tên: `PlayerInfoContainer`
   - **Width:** 400, **Height:** 60
   - **Add Horizontal Layout Group:**
     - **Spacing:** 10
     - **Child Alignment:** Middle Left

6. **Tạo Player Name Text:**
   - Right-click PlayerInfoContainer → **UI → Text - TextMeshPro**
   - Đặt tên: `PlayerNameText`
   - **Text:** "PlayerName"
   - **Font Size:** 28
   - **Alignment:** Left
   - **Color:** White
   - **Flexible Width:** ✅

7. **Tạo Score Text:**
   - Right-click LeaderboardEntryItem → **UI → Text - TextMeshPro**
   - Đặt tên: `ScoreText`
   - **Text:** "1,234,567"
   - **Font Size:** 28
   - **Alignment:** Right
   - **Color:** White
   - **Width:** 200, **Height:** 60

8. **Tạo Highlight Background:**
   - Right-click LeaderboardEntryItem → **UI → Image**
   - Đặt tên: `HighlightBackground`
   - **Color:** Yellow với Alpha = 50 (RGB: 255, 255, 0, Alpha: 128)
   - **Move to top** trong Hierarchy (để render sau, làm background)
   - **Set Active = false** (chỉ bật khi là current player)

9. **Add LeaderboardEntryUI Script:**
   - Add Component → Tìm `LeaderboardEntryUI`
   - Script sẽ tự động attach

10. **Assign References trong Inspector:**
    - **Rank Text:** Kéo `RankText` vào
    - **Player Name Text:** Kéo `PlayerNameText` vào
    - **Score Text:** Kéo `ScoreText` vào
    - **Highlight Background:** Kéo `HighlightBackground` vào

11. **Save as Prefab:**
    - Kéo `LeaderboardEntryItem` từ Hierarchy vào `Assets/Prefabs/` folder
    - Đặt tên: `LeaderboardEntryPrefab`
    - **Xóa** `LeaderboardEntryItem` khỏi Hierarchy (chỉ giữ prefab)

12. **Add Button Component (Optional - để click vào entry):**
    - Mở prefab `LeaderboardEntryPrefab`
    - Add Component → **UI → Button**
    - **Transition:** None (hoặc Color Tint nếu muốn)
    - Script `LeaderboardEntryUI` đã có method `OnEntryClicked()`

---

## 👤 Player Profile UI - Setup Từng Bước

### Bước 1: Setup PlayerProfilePanel Background

1. **Chọn PlayerProfilePanel trong Hierarchy**
2. **Image component:** Dark background

### Bước 2: Tạo Profile Header

1. **Tạo Empty GameObject:**
   - Right-click PlayerProfilePanel → **Create Empty**
   - Đặt tên: `ProfileHeader`

2. **Setup Rect Transform:**
   - **Anchor:** Top-Stretch
   - **Top:** 0, **Height:** 120
   - **Left:** 0, **Right:** 0

3. **Thêm Horizontal Layout Group:**
   - **Spacing:** 20
   - **Padding:** All = 20

4. **Tạo Back Button:**
   - Right-click ProfileHeader → **UI → Button - TextMeshPro**
   - Đặt tên: `BackButton`
   - **Text:** "← BACK"
   - **Width:** 100, **Height:** 50

5. **Tạo Player Name Text:**
   - Right-click ProfileHeader → **UI → Text - TextMeshPro**
   - Đặt tên: `PlayerNameText`
   - **Text:** "Player Name"
   - **Font Size:** 42
   - **Font Style:** Bold
   - **Alignment:** Center
   - **Flexible Width:** ✅

6. **Tạo Player Rank Text:**
   - Right-click ProfileHeader → **UI → Text - TextMeshPro**
   - Đặt tên: `PlayerRankText`
   - **Text:** "Rank: #1"
   - **Font Size:** 28
   - **Color:** Gold
   - **Width:** 150, **Height:** 50

### Bước 3: Tạo Stats Scroll View

1. **Tạo Scroll View:**
   - Right-click PlayerProfilePanel → **UI → Scroll View**
   - Đặt tên: `StatsContainer`

2. **Setup Rect Transform:**
   - **Anchor:** Stretch-Stretch
   - **Top:** 120 (dưới header)
   - **Left:** 20, **Right:** 20
   - **Bottom:** 20

3. **Setup Scroll Rect:** (giống LeaderboardScrollView)

4. **Setup Content:**
   - Chọn `Content` child
   - **Add Vertical Layout Group:**
     - **Spacing:** 30
     - **Padding:** All = 20
     - **Child Alignment:** Upper Center
   - **Add Content Size Fitter:**
     - **Vertical Fit:** Preferred Size

### Bước 4: Tạo Stats Sections

1. **Tạo Total Score Section:**
   - Right-click Content → **Create Empty**
   - Đặt tên: `TotalScoreSection`
   - **Add Horizontal Layout Group:**
     - **Spacing:** 20
     - **Child Alignment:** Middle Left

2. **Tạo Label và Value:**
   - Right-click TotalScoreSection → **UI → Text - TextMeshPro**
   - Đặt tên: `LabelText`
   - **Text:** "Total Score:"
   - **Font Size:** 24
   - **Color:** Gray
   - **Width:** 200, **Height:** 40

   - Right-click TotalScoreSection → **UI → Text - TextMeshPro**
   - Đặt tên: `ValueText`
   - **Text:** "0"
   - **Font Size:** 32
   - **Color:** White
   - **Width:** 300, **Height:** 40

3. **Lặp lại cho các sections khác:**
   - `TotalCoinsSection` - Label: "Total Coins:", Value: "0"
   - `EnemiesDefeatedSection` - Label: "Enemies Defeated:", Value: "0"
   - `DeathsSection` - Label: "Total Deaths:", Value: "0"
   - `PlaytimeSection` - Label: "Playtime:", Value: "00:00:00"
   - `CurrentLevelSection` - Label: "Current Level:", Value: "Not started"

### Bước 5: Tạo Achievements Section

1. **Tạo Container:**
   - Right-click Content → **Create Empty**
   - Đặt tên: `AchievementsSection`

2. **Add Vertical Layout Group:**
   - **Spacing:** 10
   - **Child Alignment:** Upper Left

3. **Tạo Label:**
   - Right-click AchievementsSection → **UI → Text - TextMeshPro**
   - Đặt tên: `AchievementsLabelText`
   - **Text:** "Achievements:"
   - **Font Size:** 24
   - **Color:** Gray

4. **Tạo Achievement List Container:**
   - Right-click AchievementsSection → **Create Empty**
   - Đặt tên: `AchievementList`
   - **Add Vertical Layout Group:**
     - **Spacing:** 10
   - **Add Content Size Fitter:**
     - **Vertical Fit:** Preferred Size

5. **Sử dụng AchievementItemPrefab có sẵn:**
   - Nếu đã có prefab từ Phase 3 → Sử dụng prefab đó
   - Nếu chưa có → Tạo mới tương tự như Phase 3

---

## 🎮 HUD Updates - Setup Từng Bước

### Bước 1: Tìm HUD Canvas trong Game Scene

1. **Mở Game Scene** (ví dụ: "Level 1")

2. **Tìm Canvas:**
   - Thường có tên "HUDCanvas" hoặc "Canvas"
   - Nếu chưa có → Tạo mới (UI → Canvas)

### Bước 2: Tạo Rank Display Container

1. **Tạo Empty GameObject:**
   - Right-click HUDCanvas → **Create Empty**
   - Đặt tên: `RankDisplayContainer`

2. **Setup Rect Transform:**
   - **Anchor:** Top-Right
   - **Position X:** -100 (cách right 100px)
   - **Position Y:** -50 (cách top 50px)
   - **Width:** 150, **Height:** 50

3. **Add Horizontal Layout Group:**
   - **Spacing:** 5
   - **Child Alignment:** Middle Center

4. **Tạo Rank Label:**
   - Right-click RankDisplayContainer → **UI → Text - TextMeshPro**
   - Đặt tên: `RankLabelText`
   - **Text:** "Rank:"
   - **Font Size:** 20
   - **Color:** White
   - **Width:** 60, **Height:** 30

5. **Tạo Rank Value:**
   - Right-click RankDisplayContainer → **UI → Text - TextMeshPro**
   - Đặt tên: `RankValueText`
   - **Text:** "#1"
   - **Font Size:** 24
   - **Font Style:** Bold
   - **Color:** Gold
   - **Width:** 80, **Height:** 30

### Bước 3: Tạo Connection Status Indicator

1. **Tạo Empty GameObject:**
   - Right-click HUDCanvas → **Create Empty**
   - Đặt tên: `ConnectionStatus`

2. **Setup Rect Transform:**
   - **Anchor:** Top-Left
   - **Position X:** 50 (cách left 50px)
   - **Position Y:** -30 (cách top 30px)
   - **Width:** 150, **Height:** 30

3. **Add Horizontal Layout Group:**
   - **Spacing:** 5

4. **Tạo Status Icon:**
   - Right-click ConnectionStatus → **UI → Image**
   - Đặt tên: `StatusIcon`
   - **Width:** 20, **Height:** 20
   - **Color:** Green (RGB: 0, 255, 0)
   - **Shape:** Circle (hoặc dùng sprite circle)

5. **Tạo Status Text:**
   - Right-click ConnectionStatus → **UI → Text - TextMeshPro**
   - Đặt tên: `StatusText`
   - **Text:** "Online"
   - **Font Size:** 16
   - **Color:** Green
   - **Width:** 100, **Height:** 20

---

## 🔗 Kết Nối Scripts với UI - Chi Tiết

### Bước 1: Kết Nối MainMenuManager với UI

1. **Chọn MainMenuPanel trong Hierarchy**

2. **Add Component:**
   - Add Component → Tìm `MainMenuManager`
   - Hoặc kéo script `MainMenuManager.cs` từ Project vào Inspector

3. **Assign UI References trong Inspector:**

   **Player Stats UI:**
   - **Username Text:** Kéo `PlayerNameText` (hoặc `usernameText` nếu đã có) vào
   - **Rank Value Text:** Kéo `RankValueText` vào
   - **Total Score Text:** Kéo `ScoreValueText` vào
   - **Total Coins Text:** Kéo `CoinsValueText` vào
   - **Achievements Text:** Kéo `AchievementsValueText` vào

   **Navigation Buttons:**
   - **Play Button:** Kéo `PlayButton` vào
   - **Leaderboard Button:** Kéo `LeaderboardButton` vào
   - **Achievements Button:** Kéo `AchievementsButton` vào
   - **Logout Button:** Kéo `LogoutButton` vào (hoặc button logout có sẵn)

   **Panel References:**
   - **Leaderboard Panel:** Kéo `LeaderboardPanel` từ Hierarchy vào
   - **Achievements Panel:** Kéo achievements panel (nếu có) vào
   - **Login Panel:** Kéo login panel (nếu có) vào
   - **Register Panel:** Kéo register panel (nếu có) vào
   - **Main Menu Panel:** Kéo chính `MainMenuPanel` vào

   **Other References:**
   - **Logged In Panel:** Kéo panel hiển thị khi đã login (nếu có)
   - **Logged Out Panel:** Kéo panel hiển thị khi chưa login (nếu có)

4. **Kiểm tra:**
   - Tất cả fields có SerializeField đều được assign
   - Không có field nào bị null (trừ optional fields)

### Bước 2: Kết Nối LeaderboardUI với UI

1. **Chọn LeaderboardPanel trong Hierarchy**

2. **Add Component:**
   - Add Component → `LeaderboardUI`

3. **Assign UI References:**

   **UI References:**
   - **Back Button:** Kéo `BackButton` từ HeaderContainer vào
   - **Refresh Button:** Kéo `RefreshButton` từ HeaderContainer vào
   - **All Time Tab:** Kéo `AllTimeTab` từ TabContainer vào
   - **Weekly Tab:** Kéo `WeeklyTab` từ TabContainer vào
   - **Daily Tab:** Kéo `DailyTab` từ TabContainer vào
   - **Leaderboard Content:** Kéo `Content` child của LeaderboardScrollView vào
   - **Leaderboard Entry Prefab:** Kéo `LeaderboardEntryPrefab` từ Assets/Prefabs vào

   **Optional References:**
   - **Loading Text:** Tạo TextMeshPro hiển thị "Loading..." (nếu muốn)
   - **Empty Text:** Tạo TextMeshPro hiển thị "No entries" (nếu muốn)

4. **Kiểm tra:**
   - Tất cả required fields đã được assign
   - Prefab đã được assign đúng

### Bước 3: Kết Nối PlayerProfileUI với UI

1. **Chọn PlayerProfilePanel trong Hierarchy**

2. **Add Component:**
   - Add Component → `PlayerProfileUI`

3. **Assign UI References:**

   **UI References:**
   - **Back Button:** Kéo `BackButton` từ ProfileHeader vào
   - **Player Name Text:** Kéo `PlayerNameText` từ ProfileHeader vào
   - **Player Rank Text:** Kéo `PlayerRankText` từ ProfileHeader vào
   - **Total Score Text:** Kéo `ValueText` từ TotalScoreSection vào
   - **Total Coins Text:** Kéo `ValueText` từ TotalCoinsSection vào
   - **Enemies Defeated Text:** Kéo `ValueText` từ EnemiesDefeatedSection vào
   - **Deaths Text:** Kéo `ValueText` từ DeathsSection vào
   - **Playtime Text:** Kéo `ValueText` từ PlaytimeSection vào
   - **Current Level Text:** Kéo `ValueText` từ CurrentLevelSection vào
   - **Achievements Container:** Kéo `AchievementList` vào
   - **Achievement Item Prefab:** Kéo achievement item prefab vào

   **Optional:**
   - **Loading Text:** Tạo TextMeshPro "Loading..." (nếu muốn)
   - **Error Text:** Tạo TextMeshPro hiển thị error (nếu muốn)

4. **Lưu ý:**
   - Mỗi section cần có đúng tên để dễ tìm
   - Hoặc assign trực tiếp từng ValueText vào đúng field

### Bước 4: Kết Nối HUDManager với UI

1. **Tìm HUDManager trong Game Scene:**
   - Có thể đã có sẵn từ Phase 1-3
   - Hoặc tạo mới GameObject và add component `HUDManager`

2. **Assign UI References:**

   **Existing References:**
   - **Username Text:** Kéo username text có sẵn vào (nếu có)

   **Phase 4 References:**
   - **Rank Label Text:** Kéo `RankLabelText` từ RankDisplayContainer vào
   - **Rank Value Text:** Kéo `RankValueText` từ RankDisplayContainer vào
   - **Connection Status Indicator:** Kéo `ConnectionStatus` GameObject vào
   - **Connection Status Text:** Kéo `StatusText` vào

3. **Kiểm tra:**
   - Tất cả fields đã được assign

### Bước 5: Tạo LeaderboardManager GameObject

1. **Tạo GameObject:**
   - Right-click trong Hierarchy (bất kỳ scene nào) → **Create Empty**
   - Đặt tên: `LeaderboardManager`

2. **Add Component:**
   - Add Component → `LeaderboardManager`

3. **Setup:**
   - Script sẽ tự động tạo Instance
   - **Cache Duration:** 300 (5 phút) - có thể điều chỉnh

4. **DontDestroyOnLoad:**
   - Script đã tự động setup DontDestroyOnLoad
   - GameObject sẽ persist qua các scene

### Bước 6: Verify All Connections

1. **Check MainMenuManager:**
   - Mở Inspector của MainMenuPanel
   - Kiểm tra tất cả fields không bị null (trừ optional)

2. **Check LeaderboardUI:**
   - Mở Inspector của LeaderboardPanel
   - Kiểm tra tất cả required fields

3. **Check PlayerProfileUI:**
   - Mở Inspector của PlayerProfilePanel
   - Kiểm tra tất cả fields

4. **Check HUDManager:**
   - Tìm HUDManager trong game scene
   - Kiểm tra tất cả fields

5. **Test trong Play Mode:**
   - Nhấn Play
   - Kiểm tra console không có null reference errors
   - Test navigation giữa các panels

---

## ✅ Testing UI trong Editor

### Bước 1: Test Main Menu

1. **Play Mode Testing:**
   - Nhấn **Play** trong Unity Editor
   - Kiểm tra MainMenuPanel hiển thị
   - Kiểm tra stats có load không (cần đăng nhập trước)
   - Test các button navigation

2. **Test Button Clicks:**
   - Click **Leaderboard Button** → LeaderboardPanel phải hiện lên
   - Click **Back** → Quay về MainMenuPanel
   - Click **Play Button** → Load game scene

### Bước 2: Test Leaderboard

1. **Test Tab Switching:**
   - Mở LeaderboardPanel
   - Click các tabs (All-time, Weekly, Daily)
   - Kiểm tra leaderboard content thay đổi

2. **Test Scroll View:**
   - Nếu có nhiều entries, test scroll
   - Kiểm tra scroll mượt mà

3. **Test Refresh:**
   - Click Refresh button
   - Kiểm tra leaderboard reload

### Bước 3: Test Profile UI

1. **Test Load Profile:**
   - Từ Leaderboard, click vào một entry
   - PlayerProfilePanel phải hiện lên
   - Kiểm tra stats hiển thị đúng

2. **Test Back Button:**
   - Click Back → Quay về Leaderboard

### Bước 4: Test HUD

1. **Test trong Game Scene:**
   - Load game scene
   - Kiểm tra Rank display ở góc trên phải
   - Kiểm tra Connection status ở góc trên trái

2. **Test Updates:**
   - Chơi game một chút
   - Kiểm tra rank có update không (sau 60 giây)

---

## 📝 Checklist Hoàn Thành

### Scene Structure
- [ ] Canvas đã được setup với Canvas Scaler
- [ ] MainMenuPanel đã tạo
- [ ] LeaderboardPanel đã tạo và set inactive
- [ ] PlayerProfilePanel đã tạo và set inactive

### Main Menu UI
- [ ] PlayerStatsContainer với tất cả stats elements
- [ ] ButtonContainer với tất cả navigation buttons
- [ ] Layout Groups đã setup đúng
- [ ] MainMenuManager script đã attach và assign đầy đủ references

### Leaderboard UI
- [ ] HeaderContainer với Back và Refresh buttons
- [ ] TabContainer với 3 tabs (All-time, Weekly, Daily)
- [ ] LeaderboardScrollView đã setup
- [ ] LeaderboardEntryPrefab đã tạo và save
- [ ] LeaderboardUI script đã attach và assign đầy đủ references

### Player Profile UI
- [ ] ProfileHeader với Back button và player info
- [ ] StatsContainer với tất cả stats sections
- [ ] AchievementList container
- [ ] PlayerProfileUI script đã attach và assign đầy đủ references

### HUD Updates
- [ ] RankDisplayContainer trong game scene
- [ ] ConnectionStatus indicator trong game scene
- [ ] HUDManager script đã assign đầy đủ references

### Scripts Integration
- [x] ✅ **MainMenuManager.cs** - Hoàn chỉnh
- [x] ✅ **LeaderboardUI.cs** - Hoàn chỉnh
- [x] ✅ **LeaderboardEntryUI.cs** - Hoàn chỉnh
- [x] ✅ **PlayerProfileUI.cs** - Hoàn chỉnh
- [x] ✅ **LeaderboardManager.cs** - Hoàn chỉnh
- [x] ✅ **HUDManager.cs** - Hoàn chỉnh
- [x] ✅ **APIConfig.cs** - Hoàn chỉnh
- [ ] Tất cả UI references đã được assign trong Inspector
- [ ] Test trong Play Mode không có errors

---

## 🎨 Design Tips

### Color Scheme
- **Gold/Yellow:** Top ranks, highlights (RGB: 255, 215, 0)
- **White:** Normal text (RGB: 255, 255, 255)
- **Gray:** Labels, secondary text (RGB: 200, 200, 200)
- **Green:** Success, achievements (RGB: 0, 255, 0)
- **Red:** Errors, logout (RGB: 200, 0, 0)
- **Blue:** Selected tabs, links (RGB: 100, 150, 255)

### Typography
- **Headers:** 42-48px, Bold
- **Subheaders:** 28-32px, Bold
- **Body Text:** 20-24px, Regular
- **Small Text:** 16-18px, Regular

### Spacing
- **Section Spacing:** 30-40px
- **Element Spacing:** 20px
- **Padding:** 15-20px

---

## 🔄 Troubleshooting

### Vấn Đề: UI không hiển thị

**Giải pháp:**
- Kiểm tra Canvas Render Mode = Screen Space - Overlay
- Kiểm tra Panel Active = true
- Kiểm tra Rect Transform anchor và position

### Vấn Đề: Script không tìm thấy UI elements

**Giải pháp:**
- Kiểm tra tên GameObject đúng
- Kiểm tra script đã được attach vào đúng GameObject
- Kiểm tra references đã được assign trong Inspector
- Kiểm tra console có null reference errors

### Vấn Đề: Layout không đúng

**Giải pháp:**
- Kiểm tra Layout Group settings
- Kiểm tra Child Force Expand
- Kiểm tra Content Size Fitter (nếu dùng Scroll View)
- Kiểm tra Rect Transform của child elements

### Vấn Đề: Button không hoạt động

**Giải pháp:**
- Kiểm tra EventSystem có trong scene
- Kiểm tra Button component có enabled
- Kiểm tra script đã add listener trong Start()
- Kiểm tra Panel không block raycasts (CanvasGroup)

---

**Document Status:** ✅ **READY FOR UI SETUP**  
**Last Updated:** December 13, 2025  
**Scripts Status:** ✅ All Phase 4 scripts completed and tested  
**Next Step:** Follow hướng dẫn trên để setup UI trong Unity Editor
