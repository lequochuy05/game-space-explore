using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Quản lý trạng thái Game Over, lưu điểm và chuyển sang scene EndGame
public class GameManager : MonoBehaviour
{
    // Các key dùng để lưu dữ liệu vào PlayerPrefs
    // (scene EndGame và MainMenu sẽ đọc bằng đúng các key này)
    public const string LastScoreKey = "LastScore";
    public const string HighScoreKey = "HighScore";
    public const string NewRecordKey = "NewRecord";

    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private Spawner spawner;

    // Tên scene EndGame (phải trùng chính xác với tên file scene)
    [SerializeField] private string endGameSceneName = "EndGame";

    // Thời gian chờ trước khi chuyển scene (giây)
    [SerializeField] private float gameOverDelay = 1f;

    // Đã Game Over chưa (tránh gọi Game Over nhiều lần)
    private bool isGameOver = false;

    // Cho script khác biết game đã kết thúc chưa
    public bool IsGameOver => isGameOver;

    // Các script khác (PlayerCollision) gọi hàm này khi Player chết
    public void GameOver()
    {
        // Đã Game Over rồi thì bỏ qua
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;

        // Dừng spawn asteroid và star mới
        spawner.StopSpawning();

        StartCoroutine(LoadEndGameAfterDelay());
    }

    private IEnumerator LoadEndGameAfterDelay()
    {
        // Chờ một chút để người chơi kịp nhận ra mình đã thua
        yield return new WaitForSeconds(gameOverDelay);

        SaveScores();

        SceneManager.LoadScene(endGameSceneName);
    }

    // Lưu điểm lượt chơi và cập nhật điểm cao nhất nếu phá kỷ lục
    private void SaveScores()
    {
        int finalScore = scoreManager.Score;
        int highScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        // Điểm lượt này phải CAO HƠN kỷ lục cũ mới tính là kỷ lục mới
        bool isNewRecord = finalScore > highScore;

        if (isNewRecord)
        {
            PlayerPrefs.SetInt(HighScoreKey, finalScore);
        }

        PlayerPrefs.SetInt(LastScoreKey, finalScore);

        // Lưu bool dưới dạng số: 1 = có, 0 = không
        PlayerPrefs.SetInt(NewRecordKey, isNewRecord ? 1 : 0);

        PlayerPrefs.Save();

        Debug.Log("Game Over! Điểm: " + finalScore + " | Kỷ lục: " + PlayerPrefs.GetInt(HighScoreKey, 0));
    }
}