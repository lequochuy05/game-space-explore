using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Các loại đợt (wave) có trong một màn chơi
public enum WaveType
{
    DroneGrid,      // Đội hình Drone xếp lưới
    DroneV,         // Đội hình Drone xếp hình chữ V
    AsteroidField,  // Bãi thiên thạch: sống sót trong một khoảng thời gian
    Boss            // Boss Mothership
}

// Điều khiển luồng màn chơi: Level → các Wave → Level Clear → Level tiếp theo
public class WaveManager : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] private Spawner spawner;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameManager gameManager;
    // Chữ thông báo giữa màn hình (kéo object LevelUpText vào)
    [SerializeField] private TextMeshProUGUI bannerText;

    [Header("Drone (mỗi level dùng một loại, xoay vòng)")]
    [SerializeField] private GameObject[] dronePrefabs;

    [Header("Kế hoạch một màn (lặp lại ở mọi level)")]
    [SerializeField] private WaveType[] wavePlan = { WaveType.DroneGrid, WaveType.AsteroidField, WaveType.DroneV, WaveType.Boss };

    [Header("Bãi thiên thạch")]
    [SerializeField] private float asteroidFieldDuration = 25f;

    [Header("Đội hình Drone")]
    // Khoảng cách giữa tâm hai Drone: chiều ngang và chiều dọc
    [SerializeField] private float spacingX = 2.4f;
    [SerializeField] private float spacingY = 1.8f;
    // Một nửa chiều rộng của Drone (để đội hình không tràn mép màn hình)
    [SerializeField] private float droneHalfSize = 1f;
    // Độ cao của hàng Drone trên cùng khi đã vào vị trí
    [SerializeField] private float hoverY = 3.2f;
    [SerializeField] private float enterSpeed = 4f;
    // Biên độ lắc lư tối đa (tự thu nhỏ nếu đội hình quá rộng)
    [SerializeField] private float swayAmplitude = 3f;
    [SerializeField] private float swaySpeed = 1f;
    // Thời gian trung bình giữa hai lần có Drone bắn (giây)
    [SerializeField] private float shotInterval = 1.2f;

    [Header("Boss")]
    // Kéo Prefab Boss_Mothership vào đây
    [SerializeField] private GameObject bossPrefab;
    // Kéo object BossBar (chứa tên và thanh máu) vào đây
    [SerializeField] private GameObject bossBar;
    // Kéo object BossHealthSlider vào đây
    [SerializeField] private Slider bossHealthSlider;
    // Máu boss ở level 1, và máu cộng thêm mỗi level
    [SerializeField] private int baseBossHealth = 40;
    [SerializeField] private int healthPerLevel = 20;

    [Header("Nhịp độ")]
    [SerializeField] private float bannerTime = 1.5f;
    [SerializeField] private float pauseBetweenWaves = 1.5f;
    // Thưởng khi qua màn = levelClearBonus x số level
    [SerializeField] private int levelClearBonus = 100;

    private void Start()
    {
        bannerText.gameObject.SetActive(false);

        if (bossBar != null)
        {
            bossBar.SetActive(false);
        }

        // Bãi thiên thạch chỉ chạy khi tới wave AsteroidField
        spawner.SetRunning(false);

        StartCoroutine(GameLoop());
    }

    // Vòng lặp chính: chơi hết level này đến level khác cho đến khi Game Over
    private IEnumerator GameLoop()
    {
        // Chờ một chút để các script khác khởi tạo xong
        yield return new WaitForSeconds(0.5f);

        int level = 1;

        while (!gameManager.IsGameOver)
        {
            yield return StartCoroutine(PlayLevel(level));
            level++;
        }
    }

    // Một màn chơi: lần lượt chơi các wave trong wavePlan rồi thưởng điểm
    private IEnumerator PlayLevel(int level)
    {
        levelManager.SetLevel(level);
        yield return StartCoroutine(ShowBanner("LEVEL " + level, bannerTime));

        for (int i = 0; i < wavePlan.Length; i++)
        {
            if (gameManager.IsGameOver)
            {
                yield break;
            }

            // Boss có banner cảnh báo riêng, các wave khác hiện số thứ tự
            string message;
            float time = bannerTime;

            if (wavePlan[i] == WaveType.Boss)
            {
                message = "WARNING!\n<size=60%>MOTHERSHIP APPROACHING</size>";
                time = bannerTime + 0.5f;
            }
            else
            {
                message = "WAVE " + (i + 1) + "/" + wavePlan.Length
                        + "\n<size=60%>" + GetWaveName(wavePlan[i]) + "</size>";
            }

            yield return StartCoroutine(ShowBanner(message, time));

            yield return StartCoroutine(PlayWave(wavePlan[i], level));

            if (gameManager.IsGameOver)
            {
                yield break;
            }

            yield return new WaitForSeconds(pauseBetweenWaves);
        }

        // Game Over có thể xảy ra trong lúc nghỉ
        if (gameManager.IsGameOver)
        {
            yield break;
        }

        int bonus = levelClearBonus * level;
        scoreManager.AddScore(bonus);

        yield return StartCoroutine(ShowBanner("LEVEL " + level + " CLEAR!\n<size=60%>BONUS +" + bonus + "</size>", bannerTime + 0.5f));
    }

    // Chọn cách chơi theo loại wave
    private IEnumerator PlayWave(WaveType type, int level)
    {
        switch (type)
        {
            case WaveType.AsteroidField:
                yield return StartCoroutine(PlayAsteroidField());
                break;

            case WaveType.DroneGrid:
            case WaveType.DroneV:
                yield return StartCoroutine(PlayDroneWave(type, level));
                break;

            case WaveType.Boss:
                yield return StartCoroutine(PlayBossWave(level));
                break;
        }
    }

    // ===== WAVE: BÃI THIÊN THẠCH =====

    private IEnumerator PlayAsteroidField()
    {
        // Bật Spawner: asteroid và Debris Wave hoạt động như bản cũ
        spawner.SetRunning(true);

        float timer = 0f;
        while (timer < asteroidFieldDuration && !gameManager.IsGameOver)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        spawner.SetRunning(false);
    }

    // ===== WAVE: ĐỘI HÌNH DRONE =====

    private IEnumerator PlayDroneWave(WaveType type, int level)
    {
        if (dronePrefabs == null || dronePrefabs.Length == 0)
        {
            Debug.LogError("WaveManager chưa có Drone Prefab nào!");
            yield break;
        }

        Camera cam = Camera.main;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // Level càng cao càng nhiều Drone (có giới hạn để vừa màn hình)
        int rows = Mathf.Min(2 + (level - 1) / 2, 3);
        int cols = Mathf.Min(4 + level, 6);

        // Tính vị trí từng Drone so với tâm đội hình
        List<Vector2> offsets;
        if (type == WaveType.DroneV)
        {
            int layers = Mathf.Min(1 + (level - 1) / 2, 2);
            offsets = BuildVOffsets(layers, cols);
        }
        else
        {
            offsets = BuildGridOffsets(rows, cols);
        }

        // Tìm Drone thấp nhất và Drone xa tâm nhất
        float lowestY = 0f;
        float widestX = 0f;
        foreach (Vector2 offset in offsets)
        {
            lowestY = Mathf.Min(lowestY, offset.y);
            widestX = Mathf.Max(widestX, Mathf.Abs(offset.x));
        }

        // Bắt đầu phía trên màn hình sao cho Drone thấp nhất cũng chưa lọt vào tầm nhìn
        float startY = halfHeight + droneHalfSize + 0.5f - lowestY;

        // Biên độ lắc lư không được làm đội hình tràn mép màn hình
        float maxAmplitude = halfWidth - (widestX + droneHalfSize);
        float amplitude = Mathf.Clamp(maxAmplitude, 0f, swayAmplitude);

        // Level càng cao Drone bắn càng dày và lắc nhanh hơn (có giới hạn)
        float difficulty = levelManager.DifficultyMultiplier;
        float interval = shotInterval / difficulty;
        float speed = swaySpeed * Mathf.Min(difficulty, 1.5f);

        // Tạo object đội hình (rỗng) và gắn script Formation bằng code
        GameObject root = new GameObject("Formation");
        root.transform.position = new Vector3(0f, startY, 0f);

        Formation formation = root.AddComponent<Formation>();
        formation.Setup(hoverY, enterSpeed, amplitude, speed, interval);

        // Mỗi level dùng một loại Drone, xoay vòng nếu hết danh sách
        GameObject prefab = dronePrefabs[(level - 1) % dronePrefabs.Length];

        // Cứ 3 level thì Drone thêm 1 máu
        int bonusHealth = (level - 1) / 3;

        foreach (Vector2 offset in offsets)
        {
            // Tạo Drone làm con của đội hình rồi đặt đúng vị trí
            GameObject obj = Instantiate(prefab, root.transform);
            obj.transform.localPosition = offset;

            Drone drone = obj.GetComponent<Drone>();
            if (drone != null)
            {
                drone.AddBonusHealth(bonusHealth);
            }
        }

        // Chờ cho đến khi đội hình bị tiêu diệt hết
        while (formation != null && !gameManager.IsGameOver)
        {
            yield return null;
        }
    }

    // Xếp lưới: rows hàng, cols cột, căn giữa theo chiều ngang, hàng đầu ở y = 0
    private List<Vector2> BuildGridOffsets(int rows, int cols)
    {
        List<Vector2> offsets = new List<Vector2>();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float x = (c - (cols - 1) / 2f) * spacingX;
                float y = -r * spacingY;
                offsets.Add(new Vector2(x, y));
            }
        }

        return offsets;
    }

    // Xếp hình chữ V (mũi chữ V hướng xuống), có thể xếp chồng nhiều lớp
    private List<Vector2> BuildVOffsets(int layers, int cols)
    {
        List<Vector2> offsets = new List<Vector2>();

        float center = (cols - 1) / 2f;
        // Độ dốc của hai cánh chữ V
        float step = spacingY * 0.6f;

        for (int layer = 0; layer < layers; layer++)
        {
            for (int c = 0; c < cols; c++)
            {
                float distance = Mathf.Abs(c - center);
                float x = (c - center) * spacingX;

                // Giữa thấp nhất, hai đầu cao nhất; mỗi lớp lùi xuống một hàng
                float y = -(center - distance) * step - layer * spacingY;
                offsets.Add(new Vector2(x, y));
            }
        }

        return offsets;
    }

    // ===== WAVE: BOSS =====

    private IEnumerator PlayBossWave(int level)
    {
        if (bossPrefab == null)
        {
            Debug.LogError("WaveManager chưa có Boss Prefab!");
            yield break;
        }

        // Tạo boss ở xa ngoài màn hình; hàm Setup sẽ đặt lại đúng vị trí ngay lập tức
        GameObject obj = Instantiate(bossPrefab, new Vector3(0f, 30f, 0f), Quaternion.identity);
        Boss boss = obj.GetComponent<Boss>();

        if (boss == null)
        {
            Debug.LogError("Boss Prefab chưa gắn script Boss!");
            Destroy(obj);
            yield break;
        }

        // Level càng cao boss càng trâu
        int health = baseBossHealth + (level - 1) * healthPerLevel;
        boss.Setup(health, levelManager.DifficultyMultiplier);

        // Hiện thanh máu đầy
        if (bossBar != null)
        {
            bossBar.SetActive(true);
        }

        if (bossHealthSlider != null)
        {
            bossHealthSlider.value = 1f;
        }

        // Chờ boss bị hạ, trong lúc đó cập nhật thanh máu mỗi frame
        // (khi boss bị Destroy, biến boss trở thành null)
        while (boss != null && !gameManager.IsGameOver)
        {
            if (bossHealthSlider != null)
            {
                bossHealthSlider.value = boss.HealthPercent;
            }

            yield return null;
        }

        if (bossBar != null)
        {
            bossBar.SetActive(false);
        }
    }

    // ===== HỖ TRỢ =====

    private string GetWaveName(WaveType type)
    {
        switch (type)
        {
            case WaveType.DroneGrid: return "SENTINEL PATROL";
            case WaveType.DroneV: return "SENTINEL STRIKE";
            case WaveType.AsteroidField: return "DEBRIS FIELD";
            case WaveType.Boss: return "MOTHERSHIP";
            default: return "";
        }
    }

    // Hiện chữ thông báo trong một khoảng thời gian rồi ẩn đi
    private IEnumerator ShowBanner(string message, float duration)
    {
        bannerText.text = message;
        bannerText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        bannerText.gameObject.SetActive(false);
    }
}