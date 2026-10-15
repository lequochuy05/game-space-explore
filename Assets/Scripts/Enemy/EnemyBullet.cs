using UnityEngine;

// Đạn của kẻ địch: bay theo một hướng, giết Player nếu chạm, tự hủy khi ra khỏi màn hình
public class EnemyBullet : MonoBehaviour
{
    // Tốc độ bay (unit / giây)
    [SerializeField] private float speed = 6f;

    // Khoảng cách thêm ngoài mép màn hình trước khi hủy
    [SerializeField] private float destroyMargin = 1f;

    // Hướng bay, mặc định là thẳng xuống
    private Vector3 direction = Vector3.down;

    // Giới hạn vùng cho phép (ra ngoài là hủy)
    private float minX, maxX, minY, maxY;

    // Drone và Boss gọi hàm này ngay sau khi tạo đạn để đặt hướng bay
    public void SetDirection(Vector3 newDirection)
    {
        direction = newDirection.normalized;

        // Xoay hình đạn theo hướng bay (hình gốc quay theo hướng xuống)
        transform.rotation = Quaternion.FromToRotation(Vector3.down, direction);
    }

    // Boss gọi hàm này để đặt tốc độ đạn
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
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
    }

    private void Update()
    {
        // Bay theo hướng đã đặt (hệ tọa độ thế giới)
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Đạn chéo có thể thoát ra mép trái, phải, nên kiểm tra cả bốn phía
        Vector3 pos = transform.position;
        if (pos.x < minX || pos.x > maxX || pos.y < minY || pos.y > maxY)
        {
            Destroy(gameObject);
        }
    }

    // Unity tự gọi khi đạn (Trigger) chạm vào một Collider2D khác
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerCollision player = other.GetComponent<PlayerCollision>();

            if (player != null)
            {
                player.Die();
            }

            Destroy(gameObject);
        }
    }
}