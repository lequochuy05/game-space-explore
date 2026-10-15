using System.Collections;
using UnityEngine;

// Điều khiển cả đội hình Drone: bay vào màn hình, lắc lư trái phải
// và chọn ngẫu nhiên một Drone để bắn đạn. Các Drone là object CON của đội hình
public class Formation : MonoBehaviour
{
    private float hoverY;
    private float enterSpeed;
    private float swayAmplitude;
    private float swaySpeed;
    private float shotInterval;

    // Đội hình đã vào đúng vị trí chưa
    private bool isHovering = false;
    private float hoverStartTime;

    // WaveManager gọi hàm này ngay sau khi tạo đội hình
    public void Setup(float hoverY, float enterSpeed, float swayAmplitude, float swaySpeed, float shotInterval)
    {
        this.hoverY = hoverY;
        this.enterSpeed = enterSpeed;
        this.swayAmplitude = swayAmplitude;
        this.swaySpeed = swaySpeed;
        this.shotInterval = shotInterval;
    }

    private void Update()
    {
        // Hết Drone thì đội hình tự biến mất (WaveManager nhận ra và qua wave)
        if (transform.childCount == 0)
        {
            Destroy(gameObject);
            return;
        }

        if (!isHovering)
        {
            EnterScreen();
        }
        else
        {
            Sway();
        }
    }

    // Bay xuống từ phía trên màn hình cho đến vị trí hoverY
    private void EnterScreen()
    {
        transform.Translate(Vector3.down * enterSpeed * Time.deltaTime, Space.World);

        if (transform.position.y <= hoverY)
        {
            isHovering = true;
            hoverStartTime = Time.time;

            // Vào vị trí rồi mới bắt đầu bắn
            StartCoroutine(ShootLoop());
        }
    }

    // Lắc lư trái phải theo hình sin
    private void Sway()
    {
        float x = Mathf.Sin((Time.time - hoverStartTime) * swaySpeed) * swayAmplitude;
        transform.position = new Vector3(x, hoverY, 0f);
    }

    // Cứ sau một khoảng thời gian ngẫu nhiên, chọn một Drone còn sống để bắn
    private IEnumerator ShootLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(shotInterval * 0.7f, shotInterval * 1.3f));

            // Lấy tất cả Drone là con của đội hình
            Drone[] drones = GetComponentsInChildren<Drone>();

            if (drones.Length > 0)
            {
                drones[Random.Range(0, drones.Length)].Shoot();
            }
        }
    }
}