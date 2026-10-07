using UnityEngine;

// Điều khiển di chuyển của tàu vũ trụ
public class PlayerController : MonoBehaviour
{
    // Tốc độ di chuyển (đơn vị: unit / giây), chỉnh trong Inspector
    [SerializeField] private float moveSpeed = 6f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // Hướng di chuyển đọc từ bàn phím
    private Vector2 moveInput;

    // Giới hạn vùng di chuyển của tàu
    private float minX, maxX, minY, maxY;

    private void Awake()
    {
        // Lấy các Component cùng gắn trên Player
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        CalculateScreenBoundaries();
    }

    private void Update()
    {
        // Đọc input mỗi frame
        ReadInput();
    }

    private void FixedUpdate()
    {
        // Di chuyển vật lý chạy theo nhịp cố định
        Move();
    }

    // Đọc phím W/A/S/D hoặc các phím mũi tên
    private void ReadInput()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D hoặc ←/→ : từ -1 đến 1
        float vertical = Input.GetAxis("Vertical");     // W/S hoặc ↑/↓ : từ -1 đến 1

        moveInput = new Vector2(horizontal, vertical);

        // Giới hạn độ dài vector tối đa là 1
        // để đi chéo không nhanh hơn đi thẳng
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);
    }

    // Tính vị trí mới và giữ tàu trong màn hình
    private void Move()
    {
        // Vị trí mới = vị trí hiện tại + hướng * tốc độ * thời gian
        Vector2 newPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;

        // Giới hạn tọa độ trong khoảng [min, max]
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        // Di chuyển Rigidbody2D (đúng cách cho vật có vật lý)
        rb.MovePosition(newPosition);
    }

    // Tính biên màn hình dựa vào Camera
    private void CalculateScreenBoundaries()
    {
        Camera cam = Camera.main;

        // Camera Orthographic: Size = nửa chiều cao, rộng = cao * aspect
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // Một nửa kích thước tàu, để cả thân tàu nằm trong màn hình
        Vector2 shipExtents = spriteRenderer.bounds.extents;

        minX = -halfWidth + shipExtents.x;
        maxX = halfWidth - shipExtents.x;
        minY = -halfHeight + shipExtents.y;
        maxY = halfHeight - shipExtents.y;
    }
}