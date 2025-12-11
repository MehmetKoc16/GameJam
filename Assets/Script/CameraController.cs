using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // Takip edilecek karakteri buraya sürükleyeceğiz

    public float mouseSensitivity = 2f;   // Mouse hassasiyeti
    public float distanceFromTarget = 4f; // Karakterden uzaklık
    public Vector2 pitchMinMax = new Vector2(-40, 85); // Aşağı/Yukarı bakma sınırı

    public float rotationSmoothTime = 0.12f; // Dönüş yumuşatma süresi
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    float yaw;   // Yatay eksen (Sağ-Sol)
    float pitch; // Dikey eksen (Yukarı-Aşağı)

    void LateUpdate()
    {
        if (target == null) return;

        // 1. FARE GİRDİSİNİ AL
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 2. AŞAĞI/YUKARI BAKMAYI SINIRLA (Takla atmaması için)
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        // 3. YUMUŞAK DÖNÜŞ (Smooth Damp)
        Vector3 targetRotation = new Vector3(pitch, yaw);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationSmoothVelocity, rotationSmoothTime);

        // 4. KAMERAYI DÖNDÜR
        transform.eulerAngles = currentRotation;

        // 5. KAMERAYI KARAKTERİN ARKASINA KOY
        // (Vector3.up * 1.5f ekleyerek ayaklarına değil omuz hizasına bakmasını sağlıyoruz)
        transform.position = target.position - transform.forward * distanceFromTarget + Vector3.up * 1.5f;
    }
}
