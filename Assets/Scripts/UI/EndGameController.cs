using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Điều khiển màn hình EndGame: hiển thị điểm, kỷ lục và xử lý 3 nút
public class EndGameController : MonoBehaviour
{
    [Header("UI")]
    // Kéo object ScoreText vào ô này trong Inspector
    [SerializeField] private TextMeshProUGUI scoreText;
    // Kéo object HighScoreText vào ô này trong Inspector
    [SerializeField] private TextMeshProUGUI highScoreText;

    [Header("Màu chữ kỷ lục")]
    [SerializeField] private Color normalColor = new Color(0f, 0.9f, 1f);      // cyan
    [SerializeField] private Color recordColor = new Color(1f, 0.78f, 0.24f);  // vàng

    [Header("Tên scene")]
    // Phải trùng chính xác với tên file scene
    [SerializeField] private string gameplaySceneName = "Gameplay";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        ShowScores();
    }

    // Đọc điểm đã được GameManager lưu ở scene Gameplay
    private void ShowScores()
    {
        int lastScore = PlayerPrefs.GetInt(GameManager.LastScoreKey, 0);
        int highScore = PlayerPrefs.GetInt(GameManager.HighScoreKey, 0);
        bool isNewRecord = PlayerPrefs.GetInt(GameManager.NewRecordKey, 0) == 1;

        scoreText.text = "Your Score: " + lastScore;

        if (isNewRecord)
        {
            // Phá kỷ lục: báo tin vui bằng chữ vàng
            highScoreText.text = "NEW RECORD!";
            highScoreText.color = recordColor;
        }
        else
        {
            // Chưa phá: hiện kỷ lục hiện tại để người chơi có mục tiêu
            highScoreText.text = "Best: " + highScore;
            highScoreText.color = normalColor;
        }
    }

    // Gắn vào nút RETRY
    public void Retry()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    // Gắn vào nút MAIN MENU
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Gắn vào nút QUIT
    public void QuitGame()
    {
        Debug.Log("Quit Game");

        // Application.Quit() chỉ hoạt động khi đã Build game
        Application.Quit();

#if UNITY_EDITOR
        // Trong Unity Editor thì dừng chế độ Play
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}