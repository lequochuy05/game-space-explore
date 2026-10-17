using UnityEngine;

// Các chế độ điều khiển tàu
public enum ControlMode
{
    Keyboard,   // Chỉ bàn phím (WASD / phím mũi tên)
    Mouse,      // Chỉ chuột (tàu bám theo con trỏ)
    Auto        // Tự chuyển: dùng cái nào thì theo cái đó
}

// Điều khiển di chuyển của tàu vũ trụ bằng bàn phím hoặc chuột
public class PlayerController : MonoBehaviour
{
    [Header("Bàn phím")]
    // Tốc độ di chuyển bằng phím (unit / giây)
    [SerializeField] private float moveSpeed = 8f;

    [Header("Chuột")]
    [SerializeField] private ControlMode controlMode = ControlMode.Auto;
    // Tốc độ tối đa khi bám theo chuột (unit / giây)
    [SerializeField] private float mouseFollowSpeed = 20f;
    // Chuột phải dịch ít nhất bao nhiêu pixel trong một frame mới tính là "đang di chuột"
    [SerializeField] private float mouseMoveThreshold = 2f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Camera cam;

    // Hướng di chuyển đọc từ bàn phím
    private Vector2 moveInput;

    // Vị trí tàu muốn tới khi điều khiển bằng chuột
    private Vector2 targetPosition;

    // Đang điều khiển bằng chuột hay không
    private bool usingMouse = false;
    private Vector3 lastMousePosition;

    // Giới hạn vùng di chuyển của tàu
    private float minX, maxX, minY, maxY;

    // Cho script khác (CursorController) biết đang dùng chuột hay không
    public bool UsingMouse => usingMouse;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Chạy mỗi lần script được bật (kể cả khi hồi sinh)
    private void OnEnable()
    {
        // Xóa dữ liệu cũ để tàu không bị "giật" về vị trí chuột cũ
        lastMousePosition = Input.mousePosition;
        targetPosition = rb.position;
    }

    private void Start()
    {
        cam = Camera.main;
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

    // Đọc bàn phím và chuột, rồi quyết định đang dùng thiết bị nào
    private void ReadInput()
    {
        // Bàn phím
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Giới hạn độ dài vector tối đa là 1 để đi chéo không nhanh hơn đi thẳng
        moveInput = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);

        bool keyboardActive = moveInput.sqrMagnitude > 0.01f;

        // Chuột: so sánh vị trí hiện tại với frame trước
        // (so sánh bình phương độ dài, nhanh hơn vì không phải tính căn bậc hai)
        Vector3 mousePosition = Input.mousePosition;
        bool mouseMoved = (mousePosition - lastMousePosition).sqrMagnitude
                          > mouseMoveThreshold * mouseMoveThreshold;
        lastMousePosition = mousePosition;

        switch (controlMode)
        {
            case ControlMode.Keyboard:
                usingMouse = false;
                break;

            case ControlMode.Mouse:
                usingMouse = true;
                break;

            default: // Auto
                if (keyboardActive)
                {
                    usingMouse = false;
                }
                else if (mouseMoved)
                {
                    usingMouse = true;
                }
                break;
        }

        // Nếu đang dùng chuột thì cập nhật điểm đến
        if (usingMouse)
        {
            Vector3 worldPoint = cam.ScreenToWorldPoint(mousePosition);
            targetPosition = new Vector2(worldPoint.x, worldPoint.y);
        }
    }

    // Tính vị trí mới và giữ tàu trong màn hình
    private void Move()
    {
        Vector2 newPosition;

        if (usingMouse)
        {
            // Tiến về phía con trỏ, mỗi bước đi tối đa mouseFollowSpeed * Time.fixedDeltaTime
            newPosition = Vector2.MoveTowards(rb.position, targetPosition, mouseFollowSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // Vị trí mới = vị trí hiện tại + hướng * tốc độ * thời gian
            newPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        }

        // Giới hạn tọa độ trong khoảng [min, max]
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        // Di chuyển Rigidbody2D (đúng cách cho vật có vật lý)
        rb.MovePosition(newPosition);
    }

    // Tính biên màn hình dựa vào Camera
    private void CalculateScreenBoundaries()
    {
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