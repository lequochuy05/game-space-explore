using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Script điều khiển các nút trong scene MainMenu
public class MainMenuController : MonoBehaviour
{
    // Kéo InstructionsPanel vào ô này trong Inspector
    [SerializeField] private GameObject instructionsPanel;

    // Tên scene Gameplay (phải trùng chính xác với tên file scene)
    [SerializeField] private string gameplaySceneName = "Gameplay";
    [SerializeField] private TextMeshProUGUI highScoreText;

    private void Start()
    {
        // Đảm bảo bảng hướng dẫn luôn ẩn khi vào menu
        instructionsPanel.SetActive(false);

        // Hiển thị điểm cao nhất (nếu đã gán ô này)
        if (highScoreText != null)
        {
            highScoreText.text = "Best Score: " + PlayerPrefs.GetInt(GameManager.HighScoreKey, 0);
        }
    }

    // Gắn vào nút PLAY
    public void PlayGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    // Gắn vào nút INSTRUCTIONS
    public void OpenInstructions()
    {
        instructionsPanel.SetActive(true);
    }

    // Gắn vào nút BACK
    public void CloseInstructions()
    {
        instructionsPanel.SetActive(false);
    }

    // Gắn vào nút EXIT
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