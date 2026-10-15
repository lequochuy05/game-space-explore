using UnityEngine;

// Điều khiển viên đạn: bay lên, tự hủy khi ra khỏi màn hình,
// phá Asteroid và gây sát thương cho Drone
public class Bullet : MonoBehaviour
{
    // Tốc độ bay của đạn (unit / giây)
    [SerializeField] private float speed = 12f;

    // Khoảng cách thêm phía trên màn hình trước khi hủy đạn
    [SerializeField] private float destroyMargin = 1f;

    // Điểm nhận được khi bắn nổ một asteroid
    [SerializeField] private int asteroidScore = 20;

    // Sát thương gây ra cho Drone mỗi viên
    [SerializeField] private int damage = 1;

    private float topLimit;
    private ScoreManager scoreManager;

    // Đạn đã trúng gì đó chưa (tránh một viên đạn xử lý hai lần)
    private bool hasHit = false;

    private void Start()
    {
        // Tính mép trên của màn hình (cộng thêm margin)
        Camera cam = Camera.main;
        topLimit = cam.transform.position.y + cam.orthographicSize + destroyMargin;

        // Tự tìm ScoreManager trong Scene một lần khi đạn xuất hiện
        scoreManager = FindFirstObjectByType<ScoreManager>();
    }

    private void Update()
    {
        // Di chuyển lên theo trục Y của thế giới
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.World);

        // Nếu đạn bay quá mép trên thì hủy để tránh thừa object
        if (transform.position.y > topLimit)
        {
            Destroy(gameObject);
        }
    }

    // Unity tự gọi khi đạn (Trigger) chạm vào một Collider2D khác
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Đạn đã trúng rồi thì bỏ qua
        if (hasHit)
        {
            return;
        }

        if (other.CompareTag("Asteroid"))
        {
            Asteroid asteroid = other.GetComponent<Asteroid>();

            // Chỉ tính điểm nếu đây là phát bắn trúng đầu tiên vào asteroid này
            if (asteroid != null && asteroid.TryDestroyByBullet())
            {
                hasHit = true;

                if (scoreManager != null)
                {
                    scoreManager.AddScore(asteroidScore);
                }

                Destroy(gameObject);
            }
        }
        else if (other.CompareTag("Enemy"))
        {
            Drone drone = other.GetComponent<Drone>();
            Boss boss = other.GetComponent<Boss>();

            if (drone != null)
            {
                hasHit = true;

                // Drone tự xử lý máu, điểm và hiệu ứng nổ
                drone.TakeDamage(damage);

                Destroy(gameObject);
            }
            else if (boss != null)
            {
                hasHit = true;

                // Boss tự xử lý máu, đổi pha, điểm và hiệu ứng nổ
                boss.TakeDamage(damage);

                Destroy(gameObject);
            }
        }
    }
}