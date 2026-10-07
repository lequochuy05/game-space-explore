using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Quản lý trạng thái Game Over và chuyển sang scene EndGame
public class GameManager : MonoBehaviour
{
    // Key dùng để lưu điểm vào PlayerPrefs (scene EndGame sẽ đọc bằng đúng key này)
    public const string LastScoreKey = "LastScore";

    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private Spawner spawner;

    // Tên scene EndGame (phải trùng chính xác với tên file scene)
    [SerializeField] private string endGameSceneName = "EndGame";

    // Thời gian chờ trước khi chuyển scene (giây)
    [SerializeField] private float gameOverDelay = 1f;

    // Đã Game Over chưa (tránh gọi Game Over nhiều lần)
    private bool isGameOver = false;

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

        // Lưu điểm ngay trước khi chuyển scene
        PlayerPrefs.SetInt(LastScoreKey, scoreManager.Score);
        PlayerPrefs.Save();

        Debug.Log("Game Over! Điểm đã lưu: " + scoreManager.Score);

        SceneManager.LoadScene(endGameSceneName);
    }
}