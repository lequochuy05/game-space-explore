using UnityEngine;

// Điều khiển thiên thạch: rơi xuống, xoay, tự hủy khi ra khỏi màn hình
public class Asteroid : MonoBehaviour
{
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
    private void Start()
    {
        // Chọn ngẫu nhiên tốc độ rơi và tốc độ xoay
        fallSpeed = Random.Range(minFallSpeed, maxFallSpeed);
        spinSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);

        // Góc xoay ban đầu ngẫu nhiên (0 đến 360 độ quanh trục Z)
        transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        // Kích thước ngẫu nhiên
        float scale = Random.Range(minScale, maxScale);
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
        Destroy(gameObject);
        return true;
    }
}