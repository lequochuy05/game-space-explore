using UnityEngine;
using UnityEngine.UI;

// Quản lý số mạng của người chơi và hiển thị bằng các biểu tượng trên UI
public class LivesManager : MonoBehaviour
{
    // Kéo các biểu tượng mạng (Life1..Life5) vào đây.
    // Số biểu tượng chính là số mạng tối đa
    [SerializeField] private Image[] lifeIcons;

    // Số mạng lúc bắt đầu chơi
    [SerializeField] private int startLives = 3;

    private int lives;

    // Cho script khác ĐỌC số mạng hiện tại
    public int Lives => lives;

    private void Awake()
    {
        // Không cho vượt quá số biểu tượng đang có
        lives = Mathf.Min(startLives, lifeIcons.Length);
        UpdateIcons();
    }

    // PlayerCollision gọi khi tàu bị bắn trúng
    public void LoseLife()
    {
        lives = Mathf.Max(lives - 1, 0);
        UpdateIcons();
    }

    // Quà Extra Life gọi. Trả về false nếu đã đầy mạng (không cộng được)
    public bool AddLife()
    {
        if (lives >= lifeIcons.Length)
        {
            return false;
        }

        lives++;
        UpdateIcons();
        return true;
    }

    // Hiện đúng số biểu tượng bằng số mạng, ẩn phần còn lại
    private void UpdateIcons()
    {
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            lifeIcons[i].gameObject.SetActive(i < lives);
        }
    }
}