using UnityEngine;
using UnityEngine.SceneManagement;

// Quản lý tạm dừng game: nút Pause, phím ESC/P và bảng PAUSED
public class PauseManager : MonoBehaviour
{
    // Kéo object PausePanel vào ô này trong Inspector
    [SerializeField] private GameObject pausePanel;

    // Kéo object GameManager vào ô này (để biết game đã kết thúc chưa)
    [SerializeField] private GameManager gameManager;

    // Tên scene MainMenu (phải trùng chính xác với tên file scene)
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    private void Start()
    {
        // Đảm bảo luôn bắt đầu ở trạng thái không pause
        SetPaused(false);
    }

    private void Update()
    {
        // Nhấn ESC hoặc P để bật/tắt pause
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    // Gắn vào nút PAUSE
    public void TogglePause()
    {
        // Game đã kết thúc thì không cho pause
        if (!isPaused && gameManager.IsGameOver)
        {
            return;
        }

        SetPaused(!isPaused);
    }

    // Gắn vào nút RESUME
    public void ResumeGame()
    {
        SetPaused(false);
    }

    // Gắn vào nút MAIN MENU
    public void GoToMainMenu()
    {
        // Phải trả thời gian về bình thường trước khi chuyển scene
        SetPaused(false);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Bật hoặc tắt trạng thái pause
    private void SetPaused(bool paused)
    {
        isPaused = paused;

        pausePanel.SetActive(paused);

        // 0 = đóng băng thời gian, 1 = chạy bình thường
        Time.timeScale = paused ? 0f : 1f;

        // Tạm dừng toàn bộ âm thanh (nhạc nền, hiệu ứng)
        AudioListener.pause = paused;
    }

    // Khi rời scene, luôn trả về trạng thái bình thường
    // (Time.timeScale và AudioListener.pause là biến TOÀN CỤC, không tự reset khi đổi scene)
    private void OnDestroy()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}