using System.Collections;
using UnityEngine;

// Spawn Asteroid và Star tại vị trí ngẫu nhiên ở phía trên màn hình
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

    [Header("Vị trí spawn")]
    // Cách mép trái/phải một đoạn để vật không spawn sát mép
    [SerializeField] private float sideMargin = 0.5f;

    private float minX, maxX, spawnY;

    // Lưu lại Coroutine để có thể dừng khi Game Over
    private Coroutine asteroidRoutine;
    private Coroutine starRoutine;

    private void Start()
    {
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

    // Bắt đầu hai vòng lặp spawn chạy song song
    private void StartSpawning()
    {
        asteroidRoutine = StartCoroutine(SpawnLoop(asteroidPrefab, asteroidMinInterval, asteroidMaxInterval));
        starRoutine = StartCoroutine(SpawnLoop(starPrefab, starMinInterval, starMaxInterval));
    }

    // Dừng spawn (Phase 9 sẽ gọi hàm này khi Game Over)
    public void StopSpawning()
    {
        if (asteroidRoutine != null) StopCoroutine(asteroidRoutine);
        if (starRoutine != null) StopCoroutine(starRoutine);
    }

    // Vòng lặp spawn: chờ ngẫu nhiên, spawn, rồi lặp lại mãi
    private IEnumerator SpawnLoop(GameObject prefab, float minInterval, float maxInterval)
    {
        while (true)
        {
            // Chờ một khoảng thời gian ngẫu nhiên
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            SpawnObject(prefab);
        }
    }

    // Tạo một vật tại vị trí X ngẫu nhiên, Y ở phía trên màn hình
    private void SpawnObject(GameObject prefab)
    {
        Vector2 spawnPosition = new Vector2(Random.Range(minX, maxX), spawnY);
        Instantiate(prefab, spawnPosition, Quaternion.identity);
    }
}