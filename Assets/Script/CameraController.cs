using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;

    public float mouseSensitivity = 2f;
    public float distanceFromTarget = 4f;
    public Vector2 pitchMinMax = new Vector2(-40, 85);

    public float rotationSmoothTime = 0.12f;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    float yaw;
    float pitch;

    // --- YENİ EKLENEN DEĞİŞKENLER ---
    [Header("Çarpışma Ayarları")]
    public LayerMask collisionLayers; // Kameranın çarpacağı katmanlar (Default, Ground vb.)
    public float cameraCollisionOffset = 0.2f; // Duvara yapışmasın diye minik boşluk
                                               // -------------------------------

    void LateUpdate()
    {
        if (target == null) return;

        // 1. FARE GİRDİSİ VE SINIRLAMA
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        // 2. YUMUŞAK DÖNÜŞ
        Vector3 targetRotation = new Vector3(pitch, yaw);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        // --- 3. AKILLI ÇARPIŞMA (SPHERECAST) ---

        // Karakterin omuz hizası (Başlangıç noktası)
        Vector3 focusPosition = target.position + Vector3.up * 1.5f;
        Vector3 cameraDirection = -transform.forward;
        float finalDistance = distanceFromTarget;

        // KÜRE YARIÇAPI: Kameranın "kalınlığı". Merdivenlere takılmasını sağlar.
        float sphereRadius = 0.2f;

        RaycastHit hit;

        // Raycast yerine SphereCast kullanıyoruz:
        if (Physics.SphereCast(focusPosition, sphereRadius, cameraDirection, out hit, distanceFromTarget, collisionLayers))
        {
            // Çarptığı yerden küre yarıçapı kadar öne gel ki içine girmesin
            finalDistance = hit.distance - sphereRadius - cameraCollisionOffset;

            // Kameranın karakterin içine girmesini engelle (Min mesafe)
            if (finalDistance < 0.2f) finalDistance = 0.2f;
        }

        transform.position = focusPosition + (cameraDirection * finalDistance);
    }
}