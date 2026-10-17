using UnityEngine;

// Các loại quà
public enum PowerUpType
{
    WeaponUpgrade,  // Lên một cấp vũ khí
    ExtraLife       // Thêm một mạng
}

// Quà rơi từ Drone và Boss: rơi xuống, đập nhịp, và có tác dụng khi Player chạm vào
public class PowerUp : MonoBehaviour
{
    [Header("Loại quà")]
    [SerializeField] private PowerUpType type = PowerUpType.WeaponUpgrade;

    [Header("Chuyển động")]
    [SerializeField] private float fallSpeed = 2.5f;
    // Quà phập phồng nhẹ để thu hút sự chú ý
    [SerializeField] private float pulseSpeed = 5f;
    [SerializeField] private float pulseAmount = 0.1f;
    [SerializeField] private float destroyMargin = 1f;

    [Header("Phần thưởng khi đã đầy")]
    // Điểm nhận được nếu nhặt mà không thể dùng (vũ khí đã tối đa, mạng đã đầy)
    [SerializeField] private int bonusScore = 50;

    [Header("Hiệu ứng khi nhặt")]
    [SerializeField] private GameObject collectEffectPrefab;
    [SerializeField] private AudioClip collectSound;
    [SerializeField, Range(0f, 1f)] private float collectVolume = 0.6f;

    private float bottomLimit;
    private Vector3 baseScale;
    private bool collected = false;
    private ScoreManager scoreManager;

    private void Start()
    {
        baseScale = transform.localScale;

        Camera cam = Camera.main;
        bottomLimit = cam.transform.position.y - cam.orthographicSize - destroyMargin;

        scoreManager = FindFirstObjectByType<ScoreManager>();
    }

    private void Update()
    {
        // Rơi xuống chậm
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        // Phập phồng theo hình sin: scale dao động quanh scale gốc
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * pulse;

        // Ra khỏi mép dưới mà chưa ai nhặt thì hủy
        if (transform.position.y < bottomLimit)
        {
            Destroy(gameObject);
        }
    }

    // Unity tự gọi khi một Collider2D khác đi vào vùng Trigger của quà
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ Player mới nhặt được, và chỉ nhặt một lần
        if (collected || !other.CompareTag("Player"))
        {
            return;
        }

        collected = true;

        bool applied = false;

        switch (type)
        {
            case PowerUpType.WeaponUpgrade:
                PlayerShooting shooting = other.GetComponent<PlayerShooting>();
                applied = shooting != null && shooting.UpgradeWeapon();
                break;

            case PowerUpType.ExtraLife:
                LivesManager lives = FindFirstObjectByType<LivesManager>();
                applied = lives != null && lives.AddLife();
                break;
                
            case PowerUpType.Shield:
                PlayerShield shield = other.GetComponent<PlayerShield>();
                applied = shield != null && shield.Activate();
                break;
        }

        // Nhặt mà đã đầy thì đổi thành điểm thưởng, không bao giờ "phí"
        if (!applied && scoreManager != null)
        {
            scoreManager.AddScore(bonusScore);
        }

        if (collectEffectPrefab != null)
        {
            Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
        }

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, Camera.main.transform.position, collectVolume);
        }

        Destroy(gameObject);
    }

    public enum PowerUpType
    {
        WeaponUpgrade,  // Lên một cấp vũ khí
        ExtraLife,      // Thêm một mạng
        Shield          // Khiên đỡ một đòn
    }
}