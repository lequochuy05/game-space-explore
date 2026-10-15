using TMPro;
using UnityEngine;

// Quản lý điểm số, cập nhật chữ hiển thị và báo cho LevelManager khi điểm đổi
public class ScoreManager : MonoBehaviour
{
    // Kéo object ScoreText (TextMeshPro) vào ô này trong Inspector
    [SerializeField] private TextMeshProUGUI scoreText;

    // Kéo object LevelManager vào ô này trong Inspector
    [SerializeField] private LevelManager levelManager;

    // Điểm hiện tại, ban đầu bằng 0
    private int score = 0;

    // Cho script khác ĐỌC điểm nhưng không tự ý sửa
    public int Score => score;

    private void Start()
    {
        UpdateScoreText();
    }

    // Các script khác gọi hàm này để cộng điểm
    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreText();

        // Báo cho LevelManager kiểm tra xem đã đủ điểm lên level chưa
        if (levelManager != null)
        {
            levelManager.CheckLevelUp(score);
        }
    }

    // Cập nhật chữ trên màn hình
    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }
}