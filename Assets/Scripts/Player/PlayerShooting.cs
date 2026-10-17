using TMPro;
using UnityEngine;

// Xử lý việc bắn đạn của Player: tự động bắn, âm thanh và vũ khí nhiều cấp
[RequireComponent(typeof(AudioSource))]
public class PlayerShooting : MonoBehaviour
{
    // Cấp vũ khí tối đa
    public const int MaxWeaponLevel = 3;

    [Header("Đạn")]
    // Prefab viên đạn (kéo từ Project vào Inspector)
    [SerializeField] private GameObject bulletPrefab;

    // Vị trí đạn xuất hiện (kéo object FirePoint vào Inspector)
    [SerializeField] private Transform firePoint;

    // Thời gian chờ giữa hai lần bắn (giây). Số càng nhỏ bắn càng nhanh
    [SerializeField] private float fireRate = 0.25f;

    // Bật = tự động bắn, Tắt = phải giữ SPACE để bắn
    [SerializeField] private bool autoFire = true;

    [Header("Vũ khí")]
    // Cấp vũ khí hiện tại (khi test có thể đặt thẳng 2 hoặc 3 trong Inspector)
    [SerializeField, Range(1, MaxWeaponLevel)] private int weaponLevel = 1;

    // Khoảng cách từ tâm tàu tới hai tia bên cạnh (đơn vị thế giới)
    [SerializeField] private float sideOffset = 0.45f;

    // Góc xòe của hai tia ngoài ở Lv.3 (độ)
    [SerializeField] private float spreadAngle = 12f;

    // Kéo object WeaponText vào đây (có thể để trống)
    [SerializeField] private TextMeshProUGUI weaponText;

    [Header("Âm thanh")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField, Range(0f, 1f)] private float shootVolume = 0.3f;

    private AudioSource audioSource;

    // Thời điểm sớm nhất được bắn lượt tiếp theo
    private float nextFireTime;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        UpdateWeaponText();
    }

    private void Update()
    {
        HandleShooting();
    }

    private void HandleShooting()
    {
        // Chưa hết thời gian chờ thì không bắn
        if (Time.time < nextFireTime)
        {
            return;
        }

        // Bắn nếu đang tự động bắn, hoặc người chơi đang giữ SPACE
        if (autoFire || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    // Bắn một loạt đạn, số tia phụ thuộc cấp vũ khí
    private void Shoot()
    {
        switch (weaponLevel)
        {
            case 1:
                // Một tia thẳng
                SpawnBullet(0f, 0f);
                break;

            case 2:
                // Hai tia song song, lệch trái và phải
                SpawnBullet(-sideOffset, 0f);
                SpawnBullet(sideOffset, 0f);
                break;

            default:
                // Ba tia: một thẳng, hai tia ngoài xòe ra
                SpawnBullet(0f, 0f);
                SpawnBullet(-sideOffset, spreadAngle);
                SpawnBullet(sideOffset, -spreadAngle);
                break;
        }

        // Chỉ phát một tiếng bắn cho cả loạt (nhiều tiếng chồng lên sẽ rất chói)
        if (shootSound != null)
        {
            audioSource.PlayOneShot(shootSound, shootVolume);
        }
    }

    // Tạo một viên đạn lệch ngang offsetX so với FirePoint, bay lệch angle độ so với phương thẳng đứng
    private void SpawnBullet(float offsetX, float angle)
    {
        Vector3 position = firePoint.position + Vector3.right * offsetX;
        GameObject obj = Instantiate(bulletPrefab, position, Quaternion.identity);

        Bullet bullet = obj.GetComponent<Bullet>();
        if (bullet != null)
        {
            // Xoay vector "thẳng lên" đi angle độ quanh trục Z
            // (góc dương nghiêng sang trái, góc âm nghiêng sang phải)
            bullet.SetDirection(Quaternion.Euler(0f, 0f, angle) * Vector3.up);
        }
    }

    // Quà vũ khí gọi. Trả về true nếu lên cấp được, false nếu đã tối đa
    public bool UpgradeWeapon()
    {
        if (weaponLevel >= MaxWeaponLevel)
        {
            return false;
        }

        weaponLevel++;
        UpdateWeaponText();
        return true;
    }

    // PlayerCollision gọi khi tàu mất mạng: giảm một cấp (thấp nhất là 1)
    public void DowngradeWeapon()
    {
        if (weaponLevel > 1)
        {
            weaponLevel--;
            UpdateWeaponText();
        }
    }

    // Hàm dành cho Level System: đổi tốc độ bắn theo level
    public void SetFireRate(float newFireRate)
    {
        fireRate = newFireRate;
    }

    private void UpdateWeaponText()
    {
        if (weaponText == null)
        {
            return;
        }

        weaponText.text = weaponLevel >= MaxWeaponLevel
            ? "WEAPON Lv." + weaponLevel + " MAX"
            : "WEAPON Lv." + weaponLevel;
    }
}