using System.Collections;
using UnityEngine;

// Một Drone kẻ địch: có máu, bị bắn thì nổ và cho điểm, và biết bắn đạn
public class Drone : MonoBehaviour
{
    [Header("Chỉ số")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private int scoreValue = 30;

    [Header("Bắn đạn")]
    // Kéo Prefab EnemyBullet vào đây
    [SerializeField] private GameObject enemyBulletPrefab;
    // Đạn xuất hiện cách tâm Drone một đoạn về phía dưới
    [SerializeField] private float muzzleOffset = 0.8f;

    [Header("Hiệu ứng khi bị phá")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField, Range(0f, 1f)] private float explosionVolume = 0.6f;

    [Header("Rơi quà")]
    // Kéo Prefab PowerUp_Weapon vào đây (để trống nếu không muốn rơi quà)
    [SerializeField] private GameObject powerUpPrefab;
    // Xác suất rơi quà khi bị hạ (0.1 = 10%)
    [SerializeField, Range(0f, 1f)] private float dropChance = 0.1f;
    // Quà khiên: hiếm hơn quà vũ khí
    [SerializeField] private GameObject shieldPrefab;
    [SerializeField, Range(0f, 1f)] private float shieldDropChance = 0.05f;
    private int health;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private ScoreManager scoreManager;

    private void Awake()
    {
        health = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        // Prefab không kéo thả được ScoreManager trong Scene nên tự tìm một lần
        scoreManager = FindFirstObjectByType<ScoreManager>();
    }

    // WaveManager gọi để tăng máu cho Drone ở level cao
    public void AddBonusHealth(int bonus)
    {
        health += bonus;
    }

    // Bullet gọi khi bắn trúng
    public void TakeDamage(int damage)
    {
        // Đã chết rồi thì bỏ qua (tránh hai viên đạn cùng giết một Drone, cộng điểm hai lần)
        if (isDead)
        {
            return;
        }

        health -= damage;

        if (health <= 0)
        {
            Die();
        }
        else
        {
            // Còn sống thì chớp đỏ để người chơi biết đã trúng
            StartCoroutine(HitFlash());
        }
    }

    private IEnumerator HitFlash()
    {
        spriteRenderer.color = new Color(1f, 0.4f, 0.4f);
        yield return new WaitForSeconds(0.08f);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        isDead = true;

        if (scoreManager != null)
        {
            scoreManager.AddScore(scoreValue);
        }

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, Camera.main.transform.position, explosionVolume);
        }
        // Có một xác suất rơi quà tại chỗ Drone bị hạ
        if (powerUpPrefab != null && Random.value < dropChance)
        {
            Instantiate(powerUpPrefab, transform.position, Quaternion.identity);
        }

                // Quà khiên rơi lệch sang bên một chút để không chồng lên quà vũ khí
        if (shieldPrefab != null && Random.value < shieldDropChance)
        {
            Instantiate(shieldPrefab, transform.position + Vector3.right * 0.8f, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    // Formation gọi để Drone này bắn một viên đạn xuống
    public void Shoot()
    {
        if (isDead || enemyBulletPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = transform.position + Vector3.down * muzzleOffset;
        Instantiate(enemyBulletPrefab, spawnPosition, Quaternion.identity);
    }
}