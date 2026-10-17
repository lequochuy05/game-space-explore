using System.Collections;
using UnityEngine;

// Boss Mothership: bay vào, di chuyển qua lại, bắn nhiều kiểu đạn,
// đổi sang pha 2 khi còn nửa máu và nổ liên hoàn khi chết
public class Boss : MonoBehaviour
{
    [Header("Điểm")]
    [SerializeField] private int scoreValue = 500;

    [Header("Di chuyển")]
    // Độ cao boss dừng lại khi đã vào màn hình
    [SerializeField] private float hoverY = 2.8f;
    [SerializeField] private float enterSpeed = 2f;
    // Tốc độ lắc lư qua lại (số càng lớn càng nhanh)
    [SerializeField] private float swaySpeed = 0.7f;
    // Pha 2 lắc nhanh gấp bao nhiêu lần
    [SerializeField] private float phase2SwayMultiplier = 1.6f;

    [Header("Đạn")]
    // Kéo Prefab EnemyBullet vào đây
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private float bulletSpeed = 6f;
    // Đạn xuất hiện cách tâm boss một đoạn về phía dưới
    [SerializeField] private float muzzleOffset = 1.2f;
    // Thời gian nghỉ giữa hai đợt tấn công (giây)
    [SerializeField] private float attackInterval = 1.6f;
    // Góc xòe của kiểu Fan (độ)
    [SerializeField] private float fanAngle = 60f;
    [SerializeField] private int fanCountPhase1 = 5;
    [SerializeField] private int fanCountPhase2 = 7;

    [Header("Pha 2 (khi còn nửa máu)")]
    // Màu boss khi nổi giận
    [SerializeField] private Color phase2Color = new Color(1f, 0.65f, 0.65f);
    // Nghỉ ngắn hơn bao nhiêu lần (0.7 = nghỉ còn 70%)
    [SerializeField] private float phase2IntervalMultiplier = 0.7f;

    [Header("Hiệu ứng")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField, Range(0f, 1f)] private float explosionVolume = 0.7f;

    [Header("Phần thưởng khi bị hạ")]
    // Kéo các Prefab quà vào đây (ví dụ PowerUp_Weapon, PowerUp_Life): tất cả đều rơi
    [SerializeField] private GameObject[] guaranteedDrops;

    // Các giá trị do WaveManager cấp qua hàm Setup
    private int maxHealth = 1;
    private int health = 1;
    private float difficulty = 1f;
    private float speedMultiplier = 1f;

    // Trạng thái
    private bool isActive = false;   // Đã vào vị trí, bắt đầu chiến đấu và bị thương được
    private bool isDead = false;
    private bool inPhase2 = false;

    // Di chuyển
    private float amplitude;
    private float swayPhase = 0f;
    private float currentSwaySpeed;

    // Vùng màn hình
    private float halfWidth;
    private float halfHeight;

    // Thành phần
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private ScoreManager scoreManager;
    private CameraShake cameraShake;
    private Transform player;

    // Cho script khác (WaveManager) đọc phần trăm máu còn lại: 1 = đầy, 0 = hết
    public float HealthPercent => maxHealth > 0 ? (float)health / maxHealth : 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer.color;
        currentSwaySpeed = swaySpeed;

        scoreManager = FindFirstObjectByType<ScoreManager>();
        cameraShake = FindFirstObjectByType<CameraShake>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    // WaveManager gọi ngay sau khi tạo boss
    public void Setup(int bossHealth, float levelDifficulty)
    {
        maxHealth = bossHealth;
        health = bossHealth;
        difficulty = levelDifficulty;

        // Đạn nhanh hơn theo level nhưng có giới hạn
        speedMultiplier = Mathf.Min(levelDifficulty, 1.5f);

        Camera cam = Camera.main;
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;

        // Biên độ lắc lư không được làm boss tràn mép màn hình
        Vector3 extents = spriteRenderer.bounds.extents;
        amplitude = Mathf.Max(halfWidth - extents.x - 0.5f, 0f);

        // Bắt đầu phía trên màn hình, chưa lọt vào tầm nhìn
        transform.position = new Vector3(0f, halfHeight + extents.y + 0.5f, 0f);
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        // Giai đoạn bay vào: chưa chiến đấu, chưa bị thương
        if (!isActive)
        {
            transform.Translate(Vector3.down * enterSpeed * Time.deltaTime, Space.World);

            if (transform.position.y <= hoverY)
            {
                isActive = true;
                StartCoroutine(AttackLoop());
            }

            return;
        }

        // Lắc lư qua lại bằng hàm sin.
        // Cộng dồn "pha" (swayPhase) thay vì nhân thẳng với Time.time,
        // để khi đổi tốc độ ở pha 2 boss không bị giật nhảy vị trí
        swayPhase += currentSwaySpeed * Time.deltaTime;
        transform.position = new Vector3(Mathf.Sin(swayPhase) * amplitude, hoverY, 0f);
    }

    // Bullet gọi khi bắn trúng
    public void TakeDamage(int damage)
    {
        // Chưa vào vị trí hoặc đã chết thì không nhận sát thương
        if (!isActive || isDead)
        {
            return;
        }

        health -= damage;

        if (health <= 0)
        {
            health = 0;
            Die();
            return;
        }

        // Còn nửa máu thì đổi sang pha 2
        if (!inPhase2 && health <= maxHealth / 2)
        {
            EnterPhase2();
        }

        StartCoroutine(HitFlash());
    }

    // Chớp đỏ ngắn để người chơi biết đã trúng
    private IEnumerator HitFlash()
    {
        spriteRenderer.color = new Color(1f, 0.25f, 0.25f);
        yield return new WaitForSeconds(0.06f);

        if (!isDead)
        {
            spriteRenderer.color = baseColor;
        }
    }

    private void EnterPhase2()
    {
        inPhase2 = true;
        currentSwaySpeed = swaySpeed * phase2SwayMultiplier;

        // Đổi màu gốc sang đỏ nhạt: báo hiệu boss nổi giận
        baseColor = phase2Color;
        spriteRenderer.color = baseColor;

        if (cameraShake != null)
        {
            cameraShake.Shake(0.4f, 0.15f);
        }
    }

    // ===== TẤN CÔNG =====

    // Vòng lặp tấn công: nghỉ một chút, rồi chọn kiểu tấn công theo thứ tự
    private IEnumerator AttackLoop()
    {
        int step = 0;

        while (!isDead)
        {
            yield return new WaitForSeconds(GetInterval());

            if (inPhase2)
            {
                // Pha 2: xoay vòng 3 kiểu
                switch (step % 3)
                {
                    case 0:
                        FireFan(fanCountPhase2);
                        break;
                    case 1:
                        yield return StartCoroutine(FireAimed(5));
                        break;
                    case 2:
                        yield return StartCoroutine(FireRain(10));
                        break;
                }
            }
            else
            {
                // Pha 1: luân phiên 2 kiểu
                if (step % 2 == 0)
                {
                    FireFan(fanCountPhase1);
                }
                else
                {
                    yield return StartCoroutine(FireAimed(3));
                }
            }

            step++;
        }
    }

    // Thời gian nghỉ: level cao nghỉ ngắn hơn, pha 2 nghỉ ngắn hơn nữa
    private float GetInterval()
    {
        float interval = attackInterval / difficulty;

        if (inPhase2)
        {
            interval *= phase2IntervalMultiplier;
        }

        return interval;
    }

    // KIỂU 1: Fan. Bắn một loạt đạn tỏa ra như cái quạt
    private void FireFan(int count)
    {
        Vector3 origin = transform.position + Vector3.down * muzzleOffset;

        for (int i = 0; i < count; i++)
        {
            // t chạy từ 0 đến 1 qua các viên đạn
            float t = count == 1 ? 0.5f : (float)i / (count - 1);

            // Góc lệch của viên này, từ -fanAngle/2 đến +fanAngle/2
            float angle = Mathf.Lerp(-fanAngle / 2f, fanAngle / 2f, t);

            // Xoay vector "thẳng xuống" đi một góc quanh trục Z
            Vector3 direction = Quaternion.Euler(0f, 0f, angle) * Vector3.down;

            SpawnBullet(origin, direction);
        }
    }

    // KIỂU 2: Aimed. Bắn nhiều viên vào vị trí tàu bạn tại thời điểm từng viên
    private IEnumerator FireAimed(int shots)
    {
        for (int i = 0; i < shots; i++)
        {
            if (isDead)
            {
                yield break;
            }

            Vector3 origin = transform.position + Vector3.down * muzzleOffset;
            Vector3 direction = Vector3.down;

            if (player != null)
            {
                // Hướng từ boss tới tàu = (đích - xuất phát), rồi chuẩn hóa độ dài về 1
                direction = (player.position - origin).normalized;
            }

            SpawnBullet(origin, direction);

            yield return new WaitForSeconds(0.25f);
        }
    }

    // KIỂU 3 (chỉ pha 2): Rain. Đạn rơi từ mép trên ở vị trí X ngẫu nhiên
    private IEnumerator FireRain(int count)
    {
        float topY = halfHeight + 0.3f;

        for (int i = 0; i < count; i++)
        {
            if (isDead)
            {
                yield break;
            }

            float x = Random.Range(-halfWidth + 1f, halfWidth - 1f);
            SpawnBullet(new Vector3(x, topY, 0f), Vector3.down);

            yield return new WaitForSeconds(0.15f);
        }
    }

    // Tạo một viên đạn tại vị trí cho trước, bay theo hướng cho trước
    private void SpawnBullet(Vector3 position, Vector3 direction)
    {
        if (enemyBulletPrefab == null)
        {
            return;
        }

        GameObject obj = Instantiate(enemyBulletPrefab, position, Quaternion.identity);
        EnemyBullet bullet = obj.GetComponent<EnemyBullet>();

        if (bullet != null)
        {
            bullet.SetDirection(direction);
            bullet.SetSpeed(bulletSpeed * speedMultiplier);
        }
    }

    // ===== CHẾT =====

    private void Die()
    {
        isDead = true;

        // Dừng toàn bộ coroutine đang chạy (AttackLoop, FireAimed, FireRain, HitFlash)
        StopAllCoroutines();

        // Tắt va chạm để không còn gây chết tàu, không nhận thêm đạn
        GetComponent<Collider2D>().enabled = false;
        spriteRenderer.color = baseColor;

        if (scoreManager != null)
        {
            scoreManager.AddScore(scoreValue);
        }

        StartCoroutine(DeathRoutine());
    }

    // Nổ liên hoàn khắp thân boss rồi biến mất
    private IEnumerator DeathRoutine()
    {
        // Xóa sạch đạn địch còn trên màn hình để người chơi không chết oan
        EnemyBullet[] bullets = FindObjectsByType<EnemyBullet>(FindObjectsSortMode.None);
        foreach (EnemyBullet bullet in bullets)
        {
            Destroy(bullet.gameObject);
        }

        if (cameraShake != null)
        {
            cameraShake.Shake(1.2f, 0.15f);
        }

        Bounds bounds = spriteRenderer.bounds;

        // 12 vụ nổ nhỏ ở vị trí ngẫu nhiên trên thân boss
        for (int i = 0; i < 12; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-bounds.extents.x, bounds.extents.x),
                Random.Range(-bounds.extents.y, bounds.extents.y),
                0f) * 0.8f;

            SpawnExplosion(transform.position + offset, 1f);

            yield return new WaitForSeconds(0.1f);
        }
        DropRewards();
        // Vụ nổ lớn cuối cùng ở tâm
        SpawnExplosion(transform.position, 3f);
        yield return new WaitForSeconds(0.3f);

        Destroy(gameObject);
    }

    // Tạo hiệu ứng nổ (kèm âm thanh) với kích thước tùy chọn
    private void SpawnExplosion(Vector3 position, float size)
    {
        if (explosionPrefab != null)
        {
            GameObject effect = Instantiate(explosionPrefab, position, Quaternion.identity);
            effect.transform.localScale = Vector3.one * size;
        }

        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, Camera.main.transform.position, explosionVolume);
        }
    }
        // Thả các phần thưởng đảm bảo tại chỗ boss chết
    private void DropRewards()
    {
        if (guaranteedDrops == null)
        {
            return;
        }

        for (int i = 0; i < guaranteedDrops.Length; i++)
        {
            if (guaranteedDrops[i] == null)
            {
                continue;
            }

            // Rải đều sang hai bên để quà không chồng lên nhau
            float x = (i - (guaranteedDrops.Length - 1) / 2f) * 2f;

            Instantiate(guaranteedDrops[i], transform.position + new Vector3(x, 0f, 0f), Quaternion.identity);
        }
    }
}