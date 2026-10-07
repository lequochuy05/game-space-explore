using UnityEngine;

// Xử lý va chạm của Player với các vật khác
public class PlayerCollision : MonoBehaviour
{
    // Kéo object GameManager vào ô này trong Inspector
    [SerializeField] private GameManager gameManager;

    // Player đã chết chưa (tránh xử lý chết nhiều lần)
    private bool isDead = false;

    // Unity tự gọi khi Collider của Player va chạm vật lý với Collider khác
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Asteroid"))
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // Tắt điều khiển và bắn
        GetComponent<PlayerController>().enabled = false;
        GetComponent<PlayerShooting>().enabled = false;

        // Tắt va chạm và ẩn hình tàu
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;

        // Báo cho GameManager xử lý Game Over
        gameManager.GameOver();
    }
}