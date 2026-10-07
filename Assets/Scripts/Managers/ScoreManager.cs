using TMPro;
using UnityEngine;

// Quản lý điểm số và cập nhật chữ hiển thị trên UI
public class ScoreManager : MonoBehaviour
{
    // Kéo object ScoreText (TextMeshPro) vào ô này trong Inspector
    [SerializeField] private TextMeshProUGUI scoreText;

    // Điểm hiện tại, ban đầu bằng 0
    private int score = 0;

    // Cho script khác ĐỌC điểm nhưng không tự ý sửa (Phase 9 sẽ dùng)
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
    }

    // Cập nhật chữ trên màn hình
    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }
}