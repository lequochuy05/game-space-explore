using UnityEngine;

// Điều khiển viên đạn: bay theo một hướng, tự hủy khi ra khỏi màn hình,
// phá Asteroid và gây sát thương cho Drone, Boss
public class Bullet : MonoBehaviour
{
    // Tốc độ bay của đạn (unit / giây)
    [SerializeField] private float speed = 12f;

    // Khoảng cách thêm ngoài mép màn hình trước khi hủy đạn
    [SerializeField] private float destroyMargin = 1f;

    // Điểm nhận được khi bắn nổ một asteroid
    [SerializeField] private int asteroidScore = 20;

    // Sát thương gây ra cho Drone và Boss mỗi viên
    [SerializeField] private int damage = 1;

    // Hướng bay, mặc định là thẳng lên
    private Vector3 direction = Vector3.up;

    // Vùng cho phép (ra ngoài là hủy)
    private float minX, maxX, minY, maxY;

    private ScoreManager scoreManager;

    // Đạn đã trúng gì đó chưa (tránh một viên đạn xử lý hai lần)
    private bool hasHit = false;

    // PlayerShooting gọi hàm này ngay sau khi tạo đạn để đặt hướng bay
    public void SetDirection(Vector3 newDirection)
    {
        direction = newDirection.normalized;

        // Xoay hình đạn theo hướng bay (hình gốc quay theo hướng lên)
        transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
    }

    private void Start()
    {
        Camera cam = Camera.main;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        minX = cam.transform.position.x - halfWidth - destroyMargin;
        maxX = cam.transform.position.x + halfWidth + destroyMargin;
        minY = cam.transform.position.y - halfHeight - destroyMargin;
        maxY = cam.transform.position.y + halfHeight + destroyMargin;

        // Tự tìm ScoreManager trong Scene một lần khi đạn xuất hiện
        scoreManager = FindFirstObjectByType<ScoreManager>();
    }

    private void Update()
    {
        // Bay theo hướng đã đặt (hệ tọa độ thế giới)
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Đạn chéo có thể thoát ra mép trái, phải nên kiểm tra cả bốn phía
        Vector3 pos = transform.position;
        if (pos.x < minX || pos.x > maxX || pos.y < minY || pos.y > maxY)
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
        if (other.CompareTag("Untagged") || other.gameObject.layer == LayerMask.NameToLayer("PowerUp"))
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
                drone.TakeDamage(damage);
                Destroy(gameObject);
            }
            else if (boss != null)
            {
                hasHit = true;
                boss.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}