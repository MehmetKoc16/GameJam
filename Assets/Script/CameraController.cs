using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // Takip edilecek karakteri buraya sürükleyeceðiz

    public float mouseSensitivity = 2f;   // Mouse hassasiyeti
    public float distanceFromTarget = 4f; // Karakterden uzaklýk
    public Vector2 pitchMinMax = new Vector2(-40, 85); // Aþaðý/Yukarý bakma sýnýrý

    public float rotationSmoothTime = 0.12f; // Dönüþ yumuþatma süresi
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    float yaw;   // Yatay eksen (Sað-Sol)
    float pitch; // Dikey eksen (Yukarý-Aþaðý)

    void LateUpdate()
    {
        if (target == null) return;

        // 1. FARE GÝRDÝSÝNÝ AL
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 2. AÞAÐI/YUKARI BAKMAYI SINIRLA (Takla atmamasý için)
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        // 3. YUMUÞAK DÖNÜÞ (Smooth Damp)
        Vector3 targetRotation = new Vector3(pitch, yaw);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationSmoothVelocity, rotationSmoothTime);

        // 4. KAMERAYI DÖNDÜR
        transform.eulerAngles = currentRotation;

        // 5. KAMERAYI KARAKTERÝN ARKASINA KOY
        // (Vector3.up * 1.5f ekleyerek ayaklarýna deðil omuz hizasýna bakmasýný saðlýyoruz)
        transform.position = target.position - transform.forward * distanceFromTarget + Vector3.up * 1.5f;
    }
}