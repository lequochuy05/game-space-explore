using System.Collections;
using UnityEngine;

// Rung camera trong một khoảng thời gian ngắn (dùng khi tàu nổ)
public class CameraShake : MonoBehaviour
{
    // Vị trí gốc của camera, để trả về sau khi rung xong
    private Vector3 originalPosition;

    private void Awake()
    {
        originalPosition = transform.position;
    }

    // Gọi hàm này để rung: duration = thời gian (giây), magnitude = độ mạnh
    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Dịch camera một đoạn ngẫu nhiên trong vòng tròn bán kính magnitude
            Vector2 offset = Random.insideUnitCircle * magnitude;
            transform.position = originalPosition + new Vector3(offset.x, offset.y, 0f);

            elapsed += Time.deltaTime;
            yield return null; // Chờ sang frame kế tiếp
        }

        // Trả camera về vị trí gốc
        transform.position = originalPosition;
    }
}