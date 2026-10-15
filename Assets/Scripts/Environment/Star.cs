using UnityEngine;

// Điều khiển ngôi sao: rơi xuống, xoay nhẹ, cộng điểm khi Player chạm vào
public class Star : MonoBehaviour
{
    [Header("Hiệu ứng khi nhặt")]
    [SerializeField] private GameObject collectEffectPrefab;
    [SerializeField] private AudioClip collectSound;
    [SerializeField, Range(0f, 1f)] private float collectVolume = 0.6f;
    [Header("Tốc độ rơi (chọn ngẫu nhiên trong khoảng)")]
    [SerializeField] private float minFallSpeed = 1.5f;
    [SerializeField] private float maxFallSpeed = 3f;

    // Tốc độ xoay (độ / giây), giúp sao dễ nhận biết hơn
    [SerializeField] private float spinSpeed = 120f;

    // Khoảng cách thêm phía dưới màn hình trước khi hủy
    [SerializeField] private float destroyMargin = 1f;

    // Điểm nhận được khi nhặt sao
    [SerializeField] private int scoreValue = 10;

    private float fallSpeed;
    private float bottomLimit;
    private ScoreManager scoreManager;

    private void Start()
    {
        // Chọn ngẫu nhiên tốc độ rơi
        fallSpeed = Random.Range(minFallSpeed, maxFallSpeed);

        // Tính mép dưới của màn hình (trừ thêm margin)
        Camera cam = Camera.main;
        bottomLimit = cam.transform.position.y - cam.orthographicSize - destroyMargin;

        // Prefab không kéo thả được ScoreManager trong Scene,
        // nên khi sao xuất hiện ta tự tìm ScoreManager một lần
        scoreManager = FindFirstObjectByType<ScoreManager>();
    }

    private void Update()
    {
        // Rơi xuống theo trục Y của thế giới
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        // Xoay tại chỗ quanh trục Z
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

        // Ra khỏi mép dưới mà chưa ai nhặt thì hủy
        if (transform.position.y < bottomLimit)
        {
            Destroy(gameObject);
        }
    }

    // Unity tự gọi khi một Collider2D khác đi vào vùng Trigger của Star
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (scoreManager != null)
            {
                scoreManager.AddScore(scoreValue);
            }

            // Hiệu ứng và âm thanh khi nhặt sao
            if (collectEffectPrefab != null)
            {
                Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
            }

            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, Camera.main.transform.position, collectVolume);
            }

            Destroy(gameObject);
        }
    }
}