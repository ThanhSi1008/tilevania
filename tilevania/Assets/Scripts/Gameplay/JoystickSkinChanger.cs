using UnityEngine;
using UnityEngine.UI;

public class JoystickSkinChanger : MonoBehaviour
{
    [Header("1. THÀNH PHẦN CẦN THIẾT")]
    [Tooltip("Kéo chính cái Joystick (Background) vào đây")]
    public Image backgroundToChange;

    [Tooltip("Kéo cái Handle (Cục tròn nhỏ) vào đây")]
    public RectTransform handleObject;

    [Header("2. KHO ẢNH (Kéo 5 ảnh vào)")]
    public Sprite defaultSprite; // Ảnh tối màu (lúc thả tay)
    public Sprite upSprite;      // Ảnh sáng hướng LÊN
    public Sprite downSprite;    // Ảnh sáng hướng XUỐNG
    public Sprite leftSprite;    // Ảnh sáng hướng TRÁI
    public Sprite rightSprite;   // Ảnh sáng hướng PHẢI

    [Header("3. CÀI ĐẶT")]
    [Tooltip("Kéo xa bao nhiêu thì mới đổi hình? (Nên để 20-30)")]
    public float threshold = 20f;

    void Update()
    {
        // Lấy vị trí của cục Handle
        Vector2 pos = handleObject.anchoredPosition;

        // --- TRƯỜNG HỢP 1: THẢ TAY HOẶC KÉO NHẸ ---
        if (pos.magnitude < threshold)
        {
            if (backgroundToChange.sprite != defaultSprite)
            {
                backgroundToChange.sprite = defaultSprite;
            }
            return; // Dừng lại, không làm gì thêm
        }

        // --- TRƯỜNG HỢP 2: ĐANG KÉO MẠNH ---
        // So sánh xem đang kéo theo chiều Ngang (X) hay Dọc (Y) nhiều hơn
        if (Mathf.Abs(pos.x) > Mathf.Abs(pos.y))
        {
            // Kéo theo chiều Ngang
            if (pos.x > 0)
                backgroundToChange.sprite = rightSprite; // Phải
            else
                backgroundToChange.sprite = leftSprite;  // Trái
        }
        else
        {
            // Kéo theo chiều Dọc
            if (pos.y > 0)
                backgroundToChange.sprite = upSprite;    // Lên
            else
                backgroundToChange.sprite = downSprite;  // Xuống
        }
    }
}