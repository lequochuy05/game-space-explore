using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Điều khiển màn hình EndGame: hiển thị điểm và xử lý 3 nút
public class EndGameController : MonoBehaviour
{
    // Kéo object ScoreText vào ô này trong Inspector
    [SerializeField] private TextMeshProUGUI scoreText;

    // Tên các scene (phải trùng chính xác với tên file scene)
    [SerializeField] private string gameplaySceneName = "Gameplay";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        ShowScore();
    }

    // Đọc điểm đã được GameManager lưu ở scene Gameplay
    private void ShowScore()
    {
        // Tham số thứ hai (0) là giá trị mặc định nếu chưa từng lưu điểm
        int lastScore = PlayerPrefs.GetInt(GameManager.LastScoreKey, 0);

        scoreText.text = "Your Score: " + lastScore;
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