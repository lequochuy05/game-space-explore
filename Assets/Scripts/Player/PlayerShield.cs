using UnityEngine;

// Khiên của Player: đỡ một đòn rồi vỡ, hoặc tự hết sau một khoảng thời gian
public class PlayerShield : MonoBehaviour
{
    [Header("Hình ảnh")]
    // Kéo object ShieldVisual (con của Player) vào đây
    [SerializeField] private GameObject shieldVisual;

    [Header("Thời gian")]
    // Khiên tồn tại tối đa bao lâu nếu không bị trúng (giây)
    [SerializeField] private float duration = 12f;
    // Bao nhiêu giây cuối thì nhấp nháy để cảnh báo sắp hết
    [SerializeField] private float warningTime = 3f;
    // Sau khi khiên vỡ, miễn sát thương thêm bao lâu (giây)
    [SerializeField] private float graceTime = 1.5f;

    [Header("Phập phồng")]
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float pulseAmount = 0.06f;

    [Header("Hiệu ứng và âm thanh")]
    [SerializeField] private GameObject breakEffectPrefab;
    [SerializeField] private AudioClip activateSound;
    [SerializeField] private AudioClip breakSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 0.6f;

    private bool isActive = false;
    private float remaining;
    private float graceEndTime = 0f;

    private SpriteRenderer visualRenderer;
    private Vector3 baseScale;
    private CameraShake cameraShake;

    // Cho script khác biết khiên đang bật hay không
    public bool IsActive => isActive;

    private void Awake()
    {
        visualRenderer = shieldVisual.GetComponent<SpriteRenderer>();
        baseScale = shieldVisual.transform.localScale;
        cameraShake = FindFirstObjectByType<CameraShake>();

        // Luôn bắt đầu ở trạng thái chưa có khiên
        shieldVisual.SetActive(false);
    }

    // Quà Shield gọi khi nhặt. Nếu đang có khiên thì làm mới thời gian
    public bool Activate()
    {
        isActive = true;
        remaining = duration;

        shieldVisual.SetActive(true);
        visualRenderer.enabled = true;

        PlaySound(activateSound);
        return true;
    }

    // PlayerCollision gọi mỗi khi tàu bị đánh trúng.
    // Trả về true nếu đòn bị chặn (tàu không mất mạng)
    public bool TryAbsorbHit()
    {
        // Vừa vỡ khiên xong: vẫn được miễn sát thương một lúc
        if (Time.time < graceEndTime)
        {
            return true;
        }

        if (!isActive)
        {
            return false;
        }

        // Khiên đỡ đòn rồi vỡ
        Deactivate(true);
        graceEndTime = Time.time + graceTime;
        return true;
    }

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        // Đếm ngược (dùng Time.deltaTime nên tự dừng khi Pause)
        remaining -= Time.deltaTime;

        if (remaining <= 0f)
        {
            Deactivate(false);
            return;
        }

        // Phập phồng theo hình sin quanh scale gốc
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        shieldVisual.transform.localScale = baseScale * pulse;

        // Những giây cuối: nhấp nháy để báo sắp hết
        if (remaining <= warningTime)
        {
            visualRenderer.enabled = Mathf.FloorToInt(remaining * 6f) % 2 == 0;
        }
        else
        {
            visualRenderer.enabled = true;
        }
    }

    // Tắt khiên. broken = true nếu bị vỡ do đỡ đòn, false nếu hết giờ
    private void Deactivate(bool broken)
    {
        isActive = false;
        shieldVisual.SetActive(false);
        visualRenderer.enabled = true;
        shieldVisual.transform.localScale = baseScale;

        if (broken)
        {
            if (breakEffectPrefab != null)
            {
                Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
            }

            PlaySound(breakSound);

            if (cameraShake != null)
            {
                cameraShake.Shake(0.15f, 0.1f);
            }
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, soundVolume);
        }
    }
}