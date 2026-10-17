using UnityEngine;

// Ẩn con trỏ chuột khi đang chơi bằng chuột, hiện lại khi cần bấm nút
public class CursorController : MonoBehaviour
{
    // Kéo các object tương ứng từ Hierarchy vào Inspector
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PauseManager pauseManager;
    [SerializeField] private GameManager gameManager;

    // Giam con trỏ trong cửa sổ game khi đang chơi bằng chuột
    [SerializeField] private bool confineCursor = true;

    private void Update()
    {
        // Đang ở menu pause hoặc đã Game Over thì phải cho người chơi thấy con trỏ để bấm nút
        bool menuMode = pauseManager.IsPaused || gameManager.IsGameOver;

        // Chỉ ẩn con trỏ khi tàu đang bám theo chuột
        bool hide = playerController.UsingMouse && !menuMode;

        Cursor.visible = !hide;

        if (hide && confineCursor)
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // Khi rời Scene, luôn trả con trỏ về bình thường
    // (Cursor.visible và Cursor.lockState là trạng thái TOÀN CỤC, không tự reset khi đổi Scene)
    private void OnDestroy()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}