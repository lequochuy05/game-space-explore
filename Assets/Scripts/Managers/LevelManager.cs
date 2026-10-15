using System.Collections;
using TMPro;
using UnityEngine;

// Quản lý level: tính level theo điểm và tăng độ khó theo level
public class LevelManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI levelUpText;
    // Thời gian chữ LEVEL UP hiện trên màn hình (giây)
    [SerializeField] private float levelUpDisplayTime = 1.2f;

    [Header("Tham chiếu")]
    [SerializeField] private PlayerShooting playerShooting;

    [Header("Cấu hình level")]
    // Cần bao nhiêu điểm để lên 1 level
    [SerializeField] private int pointsPerLevel = 100;

    [Header("Độ khó (asteroid nhanh hơn và spawn dày hơn)")]
    // Mỗi level tăng thêm bao nhiêu (0.15 = tăng 15%)
    [SerializeField] private float difficultyStep = 0.15f;
    // Giới hạn trên để game không bất khả thi
    [SerializeField] private float maxDifficultyMultiplier = 2f;

    [Header("Tốc độ bắn của tàu (giây giữa hai phát)")]
    [SerializeField] private float startFireRate = 0.3f;
    [SerializeField] private float fireRateStep = 0.02f;
    // Nhanh nhất có thể (số càng nhỏ bắn càng nhanh)
    [SerializeField] private float minFireRate = 0.12f;

    private int currentLevel = 1;
    private Coroutine levelUpRoutine;

    // Cho script khác ĐỌC level hiện tại
    public int CurrentLevel => currentLevel;

    // Hệ số độ khó: level 1 = 1.0, level 2 = 1.15, ... và không vượt quá mức tối đa
    public float DifficultyMultiplier
    {
        get
        {
            float value = 1f + (currentLevel - 1) * difficultyStep;
            return Mathf.Min(value, maxDifficultyMultiplier);
        }
    }

    private void Start()
    {
        // Ẩn chữ LEVEL UP lúc bắt đầu
        levelUpText.gameObject.SetActive(false);

        ApplyLevel();
    }

    // ScoreManager gọi hàm này mỗi khi điểm thay đổi
    public void CheckLevelUp(int score)
    {
        // Ví dụ: 0-99 điểm → level 1, 100-199 → level 2, ...
        int newLevel = score / pointsPerLevel + 1;

        if (newLevel > currentLevel)
        {
            currentLevel = newLevel;
            ApplyLevel();
            ShowLevelUp();
        }
    }

    // Cập nhật chữ Level và tốc độ bắn theo level hiện tại
    private void ApplyLevel()
    {
        levelText.text = "Level: " + currentLevel;

        // Level càng cao bắn càng nhanh, nhưng không nhanh hơn minFireRate
        float fireRate = startFireRate - (currentLevel - 1) * fireRateStep;
        fireRate = Mathf.Max(fireRate, minFireRate);

        playerShooting.SetFireRate(fireRate);
    }

    // Hiện chữ LEVEL UP trong thời gian ngắn
    private void ShowLevelUp()
    {
        // Nếu chữ cũ đang hiện thì dừng lại trước khi hiện chữ mới
        if (levelUpRoutine != null)
        {
            StopCoroutine(levelUpRoutine);
        }

        levelUpRoutine = StartCoroutine(LevelUpRoutine());
    }

    private IEnumerator LevelUpRoutine()
    {
        levelUpText.text = "LEVEL " + currentLevel;
        levelUpText.gameObject.SetActive(true);

        yield return new WaitForSeconds(levelUpDisplayTime);

        levelUpText.gameObject.SetActive(false);
    }

    // WaveManager gọi hàm này khi bắt đầu một màn mới
    public void SetLevel(int level)
    {
        currentLevel = level;
        ApplyLevel();
    }
}