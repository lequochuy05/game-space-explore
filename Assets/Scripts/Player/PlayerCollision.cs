using System.Collections;
using UnityEngine;

// Xử lý va chạm của Player: mất mạng, hồi sinh có thời gian bất tử, hoặc Game Over
public class PlayerCollision : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private LivesManager livesManager;

    [Header("Hồi sinh")]
    // Chờ bao lâu rồi hồi sinh (giây)
    [SerializeField] private float respawnDelay = 1f;
    // Thời gian bất tử sau khi hồi sinh (giây)
    [SerializeField] private float invulnerableTime = 2.5f;
    // Tốc độ nhấp nháy (giây mỗi lần bật/tắt)
    [SerializeField] private float blinkInterval = 0.1f;

    [Header("Hiệu ứng khi tàu nổ")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField, Range(0f, 1f)] private float explosionVolume = 0.8f;

    // Tùy chọn: kéo Main Camera vào để rung màn hình. Có thể để trống
    [SerializeField] private CameraShake cameraShake;

    // Hai cờ trạng thái:
    // isDead: tàu đang "chết" (chờ hồi sinh hoặc hết mạng)
    // isInvulnerable: tàu đang bất tử sau khi hồi sinh
    private bool isDead = false;
    private bool isInvulnerable = false;

    private Vector3 startPosition;
    private Rigidbody2D rb;
    private PlayerController playerController;
    private PlayerShooting playerShooting;
    private Collider2D playerCollider;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        playerShooting = GetComponent<PlayerShooting>();
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Nhớ vị trí xuất phát để hồi sinh về đúng chỗ
        startPosition = transform.position;
    }

    // Unity tự gọi khi Collider của Player va chạm vật lý với Collider khác
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Asteroid") || collision.gameObject.CompareTag("Enemy"))
        {
            Die();
        }
    }

    // Asteroid, Drone, Boss và đạn địch đều gọi hàm này
    public void Die()
    {
        // Đang chết hoặc đang bất tử thì bỏ qua
        if (isDead || isInvulnerable)
        {
            return;
        }

        isDead = true;

        PlayExplosion();

        if (cameraShake != null)
        {
            cameraShake.Shake(0.3f, 0.25f);
        }

        // Trừ một mạng
        if (livesManager != null)
        {
            livesManager.LoseLife();
        }

        // Ẩn tàu, tắt điều khiển, bắn và va chạm
        SetShipActive(false);

        bool hasLivesLeft = livesManager != null && livesManager.Lives > 0;

        if (hasLivesLeft)
        {
            StartCoroutine(RespawnRoutine());
        }
        else
        {
            // Hết mạng: Game Over như cũ
            gameManager.GameOver();
        }
    }

    // Bật hoặc tắt tàu (điều khiển, bắn, va chạm và hình ảnh)
    private void SetShipActive(bool active)
    {
        playerController.enabled = active;
        playerShooting.enabled = active;
        playerCollider.enabled = active;
        spriteRenderer.enabled = active;
    }

    // Chờ một chút, đặt tàu về vị trí xuất phát, rồi bất tử trong ít giây
    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        // Đặt lại vị trí và xóa vận tốc cũ
        rb.position = startPosition;
        transform.position = startPosition;
        rb.linearVelocity = Vector2.zero;

        SetShipActive(true);
        isDead = false;

        yield return StartCoroutine(InvulnerableRoutine());
    }

    // Nhấp nháy trong thời gian bất tử
    private IEnumerator InvulnerableRoutine()
    {
        isInvulnerable = true;

        float timer = 0f;
        while (timer < invulnerableTime)
        {
            // Đảo trạng thái hiển thị: đang hiện thì ẩn, đang ẩn thì hiện
            spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        // Kết thúc: chắc chắn tàu hiện lên đầy đủ
        spriteRenderer.enabled = true;
        isInvulnerable = false;
    }

    // Tạo hiệu ứng nổ và phát tiếng nổ tại vị trí tàu
    private void PlayExplosion()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, Camera.main.transform.position, explosionVolume);
        }
    }
}