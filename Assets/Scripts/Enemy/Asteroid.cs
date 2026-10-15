using UnityEngine;

// Điều khiển thiên thạch: rơi xuống, xoay, tự hủy khi ra khỏi màn hình
public class Asteroid : MonoBehaviour
{
    [Header("Hiệu ứng khi bị phá")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField, Range(0f, 1f)] private float explosionVolume = 0.6f;
    [Header("Tốc độ rơi (chọn ngẫu nhiên trong khoảng)")]
    [SerializeField] private float minFallSpeed = 2f;
    [SerializeField] private float maxFallSpeed = 5f;

    [Header("Tốc độ xoay (độ / giây)")]
    [SerializeField] private float minSpinSpeed = -90f;
    [SerializeField] private float maxSpinSpeed = 90f;

    [Header("Kích thước ngẫu nhiên")]
    [SerializeField] private float minScale = 0.7f;
    [SerializeField] private float maxScale = 1.4f;

    // Khoảng cách thêm phía dưới màn hình trước khi hủy
    [SerializeField] private float destroyMargin = 1f;

    private float fallSpeed;
    private float spinSpeed;
    private float bottomLimit;
    private bool isHit = false;
    private bool isWaveAsteroid = false;
    private float waveScale = 1f;
        private void Start()
    {
        // Tốc độ xoay và góc xoay ban đầu vẫn ngẫu nhiên cho mọi asteroid
        spinSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);
        transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        float scale;

        if (isWaveAsteroid)
        {
            // Asteroid của đợt sóng: dùng giá trị Spawner đã đặt
            scale = waveScale;
        }
        else
        {
            // Asteroid thường: chọn ngẫu nhiên tốc độ rơi và kích thước
            fallSpeed = Random.Range(minFallSpeed, maxFallSpeed);

            // Level càng cao asteroid càng rơi nhanh
            LevelManager levelManager = FindFirstObjectByType<LevelManager>();
            if (levelManager != null)
            {
                fallSpeed *= levelManager.DifficultyMultiplier;
            }

            scale = Random.Range(minScale, maxScale);
        }

        transform.localScale = new Vector3(scale, scale, 1f);

        // Tính mép dưới của màn hình (trừ thêm margin)
        Camera cam = Camera.main;
        bottomLimit = cam.transform.position.y - cam.orthographicSize - destroyMargin;
    }

    private void Update()
    {
        // Rơi xuống theo trục Y của thế giới
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        // Xoay tại chỗ quanh trục Z
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

        // Ra khỏi mép dưới thì hủy để tránh thừa object
        if (transform.position.y < bottomLimit)
        {
            Destroy(gameObject);
        }
    }

    // Bullet gọi hàm này khi bắn trúng.
    // Trả về true nếu đây là phát bắn trúng ĐẦU TIÊN, false nếu đã bị bắn trước đó
    public bool TryDestroyByBullet()
    {
        if (isHit)
        {
            return false;
        }

        isHit = true;
        PlayDestroyEffects();
        Destroy(gameObject);
        return true;
    }
        // Tạo hiệu ứng nổ và phát tiếng nổ tại vị trí asteroid
    private void PlayDestroyEffects()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (explosionSound != null)
        {
            // PlayClipAtPoint tạo một AudioSource tạm, phát xong tự xóa,
            // nên vẫn nghe được tiếng dù asteroid đã bị Destroy
            AudioSource.PlayClipAtPoint(explosionSound, Camera.main.transform.position, explosionVolume);
        }
    }
    public void SetupForWave(float speed, float scale)
    {
        isWaveAsteroid = true;
        fallSpeed = speed;
        waveScale = scale;
    }
}