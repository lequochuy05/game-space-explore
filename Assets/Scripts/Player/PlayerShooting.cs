using UnityEngine;

// Xử lý việc bắn đạn của Player (hỗ trợ tự động bắn)
public class PlayerShooting : MonoBehaviour
{
    // Prefab viên đạn (kéo từ Project vào Inspector)
    [SerializeField] private GameObject bulletPrefab;

    // Vị trí đạn xuất hiện (kéo object FirePoint vào Inspector)
    [SerializeField] private Transform firePoint;

    // Thời gian chờ giữa hai lần bắn (giây). Số càng nhỏ bắn càng nhanh
    [SerializeField] private float fireRate = 0.25f;

    // Bật = tự động bắn, Tắt = phải giữ SPACE để bắn
    [SerializeField] private bool autoFire = true;

    // Thời điểm sớm nhất được bắn viên tiếp theo
    private float nextFireTime;

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
        if (autoFire || Input.GetKey(KeyCode.Space))
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        // Tạo một bản sao của bulletPrefab tại FirePoint
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }

    // Hàm dành cho Level System (làm sau): tăng tốc độ bắn theo level
    public void SetFireRate(float newFireRate)
    {
        fireRate = newFireRate;
    }
}
