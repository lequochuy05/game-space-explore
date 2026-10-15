using System.Collections;
using TMPro;
using UnityEngine;

// Spawn Asteroid, Star và các đợt Debris Wave ở phía trên màn hình
public class Spawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private GameObject starPrefab;

    [Header("Thời gian spawn Asteroid (giây)")]
    [SerializeField] private float asteroidMinInterval = 0.8f;
    [SerializeField] private float asteroidMaxInterval = 1.8f;

    [Header("Thời gian spawn Star (giây)")]
    [SerializeField] private float starMinInterval = 2.5f;
    [SerializeField] private float starMaxInterval = 5f;

    [Header("Độ khó")]
    [SerializeField] private LevelManager levelManager;

    [Header("Vị trí spawn")]
    [SerializeField] private float sideMargin = 0.5f;

    [Header("Debris Wave: thời điểm")]
    // Chờ bao lâu sau khi bắt đầu thì có đợt sóng đầu tiên
    [SerializeField] private float firstWaveDelay = 15f;
    // Khoảng chờ giữa hai đợt sóng (chọn ngẫu nhiên trong khoảng này)
    [SerializeField] private float waveMinInterval = 15f;
    [SerializeField] private float waveMaxInterval = 25f;

    [Header("Debris Wave: cảnh báo")]
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private float warningTime = 1.5f;
    // Sau khi hàng asteroid đã spawn, chờ bao lâu mới cho asteroid thường quay lại
    [SerializeField] private float resumeDelay = 1.5f;

    [Header("Debris Wave: hình dạng")]
    // Khoảng cách giữa tâm hai asteroid cạnh nhau
    [SerializeField] private float waveSpacing = 1.1f;
    [SerializeField] private float waveAsteroidScale = 1f;
    [SerializeField] private float waveFallSpeed = 3.5f;
    // Chặn trần tốc độ đợt sóng để người chơi luôn kịp lách
    [SerializeField] private float waveMaxSpeedMultiplier = 1.5f;

    [Header("Debris Wave: độ rộng khe hở")]
    [SerializeField] private float startGapWidth = 3.5f;
    // Mỗi level khe hở hẹp đi bao nhiêu
    [SerializeField] private float gapShrinkPerLevel = 0.15f;
    // Khe hở hẹp nhất (phải lớn hơn bề ngang tàu)
    [SerializeField] private float minGapWidth = 2.2f;

    private float minX, maxX, spawnY;

    // Đang chuẩn bị hoặc vừa spawn đợt sóng: tạm ngừng asteroid thường
    private bool pauseAsteroids = false;
        // Spawner có đang ở đợt thiên thạch không (WaveManager bật/tắt)
    // Chỉ ảnh hưởng Asteroid và Debris Wave; Star luôn spawn
    private bool isRunning = false;

    // Lưu lại Coroutine để có thể dừng khi Game Over
    private Coroutine asteroidRoutine;
    private Coroutine starRoutine;
    private Coroutine waveRoutine;

    private void Start()
    {
        // Ẩn chữ cảnh báo lúc bắt đầu
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }

        CalculateSpawnArea();
        StartSpawning();
    }

    // Tính vùng spawn dựa vào Camera
    private void CalculateSpawnArea()
    {
        Camera cam = Camera.main;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        minX = -halfWidth + sideMargin;
        maxX = halfWidth - sideMargin;

        // Spawn phía trên mép màn hình để vật trượt vào, không hiện đột ngột
        spawnY = halfHeight + 1f;
    }

    // Bắt đầu các vòng lặp spawn chạy song song
    private void StartSpawning()
    {
        asteroidRoutine = StartCoroutine(SpawnLoop(asteroidPrefab, asteroidMinInterval, asteroidMaxInterval, true));
        starRoutine = StartCoroutine(SpawnLoop(starPrefab, starMinInterval, starMaxInterval, false));
        waveRoutine = StartCoroutine(WaveLoop());
    }

    // Dừng spawn (GameManager gọi khi Game Over)
    public void StopSpawning()
    {
        if (asteroidRoutine != null) StopCoroutine(asteroidRoutine);
        if (starRoutine != null) StopCoroutine(starRoutine);
        if (waveRoutine != null) StopCoroutine(waveRoutine);

        // Nếu Game Over đúng lúc đang cảnh báo thì tắt chữ cảnh báo
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }
    }
        public void SetRunning(bool running)
    {
        isRunning = running;
    }

    // Vòng lặp spawn: chờ ngẫu nhiên, spawn, rồi lặp lại mãi
    // isAsteroid = true: spawn dày hơn theo level và nghỉ khi có Debris Wave
    private IEnumerator SpawnLoop(GameObject prefab, float minInterval, float maxInterval, bool isAsteroid)
    {
        while (true)
        {
            // Chờ một khoảng thời gian ngẫu nhiên
            float waitTime = Random.Range(minInterval, maxInterval);

            // Level càng cao thì thời gian chờ càng ngắn (spawn càng dày)
            if (isAsteroid && levelManager != null)
            {
                waitTime /= levelManager.DifficultyMultiplier;
            }

            yield return new WaitForSeconds(waitTime);

            // Đang có Debris Wave thì asteroid thường nghỉ để không lấp khe hở
            if (isAsteroid && (pauseAsteroids || !isRunning))
            {
                continue;
            }

            SpawnObject(prefab);
        }
    }

    // Tạo một vật tại vị trí X ngẫu nhiên, Y ở phía trên màn hình
    private void SpawnObject(GameObject prefab)
    {
        Vector2 spawnPosition = new Vector2(Random.Range(minX, maxX), spawnY);
        Instantiate(prefab, spawnPosition, Quaternion.identity);
    }

    // ===== DEBRIS WAVE =====

    // Vòng lặp của các đợt sóng
        // Vòng lặp của các đợt sóng
    private IEnumerator WaveLoop()
    {
        yield return StartCoroutine(WaitWhileRunning(firstWaveDelay));

        while (true)
        {
            // Báo hiệu và dừng asteroid thường
            pauseAsteroids = true;
            yield return StartCoroutine(ShowWarning());

            // Spawn hàng asteroid
            SpawnWave();

            // Chờ hàng asteroid đi khỏi vùng spawn rồi mới cho asteroid thường quay lại
            yield return new WaitForSeconds(resumeDelay);
            pauseAsteroids = false;

            // Chờ đến đợt sóng tiếp theo (level cao thì đến thường hơn)
            float waitTime = Random.Range(waveMinInterval, waveMaxInterval);
            if (levelManager != null)
            {
                waitTime /= levelManager.DifficultyMultiplier;
            }

            yield return StartCoroutine(WaitWhileRunning(waitTime));
        }
    }

    // Chờ một khoảng thời gian, nhưng chỉ tính giờ khi đang ở đợt thiên thạch
    private IEnumerator WaitWhileRunning(float seconds)
    {
        float timer = seconds;

        while (timer > 0f)
        {
            if (isRunning)
            {
                timer -= Time.deltaTime;
            }

            yield return null;
        }
    }
    // Hiện chữ cảnh báo trong một khoảng thời gian
    private IEnumerator ShowWarning()
    {
        if (warningText != null)
        {
            warningText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(warningTime);

        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }
    }

    // Xếp một hàng asteroid ngang màn hình, chừa một khe hở ngẫu nhiên
    private void SpawnWave()
    {
        Camera cam = Camera.main;
        float halfWidth = cam.orthographicSize * cam.aspect;

        float gapWidth = GetCurrentGapWidth();

        // Chọn tâm khe hở ngẫu nhiên, nhưng không sát mép để tàu vẫn đến được
        float gapLimit = halfWidth - gapWidth / 2f - sideMargin;
        float gapCenter = Random.Range(-gapLimit, gapLimit);

        // Tốc độ cả đợt sóng (tăng theo level nhưng có chặn trần)
        float multiplier = 1f;
        if (levelManager != null)
        {
            multiplier = Mathf.Min(levelManager.DifficultyMultiplier, waveMaxSpeedMultiplier);
        }
        float speed = waveFallSpeed * multiplier;

        // Tính số asteroid xếp vừa chiều ngang màn hình và căn giữa hàng
        float totalWidth = halfWidth * 2f;
        int count = Mathf.FloorToInt(totalWidth / waveSpacing);
        float startX = -(count - 1) * waveSpacing / 2f;

        for (int i = 0; i < count; i++)
        {
            float x = startX + i * waveSpacing;

            // Vị trí nằm trong khe hở thì bỏ qua, không spawn
            if (Mathf.Abs(x - gapCenter) < gapWidth / 2f)
            {
                continue;
            }

            GameObject obj = Instantiate(asteroidPrefab, new Vector2(x, spawnY), Quaternion.identity);

            // Báo cho asteroid biết nó thuộc đợt sóng
            Asteroid asteroid = obj.GetComponent<Asteroid>();
            if (asteroid != null)
            {
                asteroid.SetupForWave(speed, waveAsteroidScale);
            }
        }
    }

    // Level càng cao khe hở càng hẹp, nhưng không hẹp hơn minGapWidth
    private float GetCurrentGapWidth()
    {
        int level = 1;
        if (levelManager != null)
        {
            level = levelManager.CurrentLevel;
        }

        float gap = startGapWidth - (level - 1) * gapShrinkPerLevel;
        return Mathf.Max(gap, minGapWidth);
    }
}