## AuthScene Setup (Phase 1 Auth)

Hướng dẫn chi tiết cấu trúc thành phần trong scene `AuthScene`, các component, và liên kết Inspector để hoàn thành Phase 1 (đăng nhập/đăng ký/auto-login).

---

### 1) Cấu trúc tổng thể (Hierarchy gợi ý)
```
AuthScene
 ├─ Canvas
 │   ├─ LoginPanel
 │   │   ├─ Title (TMP_Text)
 │   │   ├─ EmailInput (TMP_InputField)
 │   │   ├─ PasswordInput (TMP_InputField, Password)
 │   │   ├─ LoginButton (Button)
 │   │   ├─ GoRegisterButton (Button)
 │   │   └─ ErrorText (TMP_Text)
 │   ├─ RegisterPanel
 │   │   ├─ Title (TMP_Text)
 │   │   ├─ EmailInput (TMP_InputField)
 │   │   ├─ UsernameInput (TMP_InputField)
 │   │   ├─ PasswordInput (TMP_InputField, Password)
 │   │   ├─ RegisterButton (Button)
 │   │   ├─ GoLoginButton (Button)
 │   │   └─ ErrorText (TMP_Text)
 │   ├─ MainMenuPanel
 │   │   ├─ WelcomeText (TMP_Text)
 │   │   ├─ PlayButton (Button)
 │   │   └─ LogoutButton (Button)
 │   └─ LoadingOverlay
 │       └─ Spinner/Text (tuỳ chọn)
 ├─ EventSystem
 ├─ AuthRoot
 │   ├─ AuthManager (MonoBehaviour)
 │   └─ SessionManager (MonoBehaviour)
 └─ Bootstrap (GameObject rỗng)
     └─ BootstrapAuth (MonoBehaviour)
```

Canvas: UI Scale Mode = Scale With Screen Size, Reference Resolution 1920x1080, Match 0.5. LoadingOverlay phủ full màn, có CanvasGroup.

---

### 2) Scripts sử dụng
- `Assets/Scripts/Network/APIClient.cs` (static, không gắn GameObject).
- `Assets/Scripts/Network/APIConfig.cs`.
- `Assets/Scripts/Managers/AuthManager.cs`.
- `Assets/Scripts/Managers/SessionManager.cs`.
- `Assets/Scripts/Managers/AutoLogin.cs` (nếu dùng, hoặc `BootstrapAuth.cs` tương đương).
- `Assets/Scripts/UI/LoginManager.cs`.
- `Assets/Scripts/UI/RegisterManager.cs`.
- `Assets/Scripts/UI/MainMenuManager.cs`.
- `Assets/Scripts/UI/LoadingOverlay.cs` (hoặc tương tự).

(Tuỳ chọn) `ToastManager.cs` nếu cần thông báo ngắn.

---

### 3) Gán Component & Reference (Inspector)

**AuthRoot**
- `AuthManager` (đặt trên AuthRoot).
- `SessionManager`: tham chiếu tới `AuthManager` (field `authManager`).

**LoadingOverlay**
- Có `CanvasGroup`.
- Script `LoadingOverlay`: gán `canvasGroup`.
- Mặc định `SetActive(false)`.

**LoginPanel** + `LoginManager` (attach lên LoginPanel)
- Fields:
  - `emailInput` → EmailInput (TMP_InputField).
  - `passwordInput` → PasswordInput (TMP_InputField).
  - `errorText` → ErrorText (TMP_Text).
  - `loginPanel` → LoginPanel.
  - `registerPanel` → RegisterPanel.
  - `mainMenuPanel` → MainMenuPanel.
  - `loading` → LoadingOverlay.
  - `authManager` → từ AuthRoot.
  - `sessionManager` → từ AuthRoot.
  - `mainMenuManager` → component trên MainMenuPanel.
  - `loginButton` → LoginButton (Button).

**RegisterPanel** + `RegisterManager`
- `emailInput` → EmailInput.
- `usernameInput` → UsernameInput.
- `passwordInput` → PasswordInput.
- `errorText` → ErrorText.
- `registerPanel` → RegisterPanel.
- `loginPanel` → LoginPanel.
- `mainMenuPanel` → MainMenuPanel.
- `loading` → LoadingOverlay.
- `authManager` → từ AuthRoot.
- `sessionManager` → từ AuthRoot.
- `mainMenuManager` → trên MainMenuPanel.
- `registerButton` → RegisterButton.

**MainMenuPanel** + `MainMenuManager`
- `welcomeText` → WelcomeText.
- `loginPanel` → LoginPanel.
- `registerPanel` → RegisterPanel.
- `mainMenuPanel` → MainMenuPanel.
- `sessionManager` → từ AuthRoot.

**Bootstrap** + `BootstrapAuth` (hoặc `AutoLogin`)
- `loading` → LoadingOverlay.
- `sessionManager` → từ AuthRoot.
- `loginPanel` → LoginPanel.
- `registerPanel` → RegisterPanel.
- `mainMenuPanel` → MainMenuPanel.
- `mainMenuManager` → trên MainMenuPanel.

---

### 4) Button OnClick mapping
- `LoginButton` → `LoginManager.OnLoginClicked`.
- `GoRegisterButton` → `LoginManager.OnGoRegister`.
- `RegisterButton` → `RegisterManager.OnRegisterClicked`.
- `GoLoginButton` → `RegisterManager.OnGoLogin`.
- `LogoutButton` → `MainMenuManager.OnLogoutClicked`.
- `PlayButton` → `MainMenuManager.OnPlayClicked` (sau này load scene gameplay).

---

### 5) Trạng thái bật/tắt mặc định
- `LoginPanel`: Active = true.
- `RegisterPanel`: Active = false.
- `MainMenuPanel`: Active = false.
- `LoadingOverlay`: Active = false (bật trong khi gọi API).

---

### 6) Flow khởi động (Auto-login)
`BootstrapAuth.Start()` (hoặc `AutoLogin`):
1. `loading.Show()`.
2. `await sessionManager.AutoLogin()`.
   - Nếu thành công: tắt Login/Register, bật MainMenu, gọi `mainMenuManager.Show(username)`.
   - Nếu thất bại: bật LoginPanel, tắt Register/MainMenu.
3. `loading.Hide()`.

---

### 7) Kiểm thử nhanh
- Đăng nhập sai mật khẩu → hiển thị lỗi, không crash.
- Đăng ký email/username trùng → hiển thị lỗi.
- Đăng nhập đúng → chuyển MainMenu, WelcomeText hiển thị username.
- Dừng Play, chạy lại → nếu token hợp lệ sẽ vào thẳng MainMenu (auto-login).
- Logout → quay về Login, token bị xoá.

---

### 8) Lưu ý
- `APIClient` là static class, **không** kéo vào GameObject.
- Đảm bảo import TextMeshPro Essentials.
- Nếu dùng build mobile, cân nhắc secure storage cho token (hiện đang XOR + Base64). 

