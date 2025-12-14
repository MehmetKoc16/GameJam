using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;

    public float mouseSensitivity = 2f;

    // --- BURAYI DEĞİŞTİRDİK ---
    // Karakterin dev olduğu için mesafeyi artırdık (Eskiden 4'tü)
    public float distanceFromTarget = 20f;
    // Karakterin boyu uzadığı için kameranın bakacağı yüksekliği ayarlanabilir yaptık
    public float heightOffset = 10f;
    // ---------------------------

    public Vector2 pitchMinMax = new Vector2(-40, 85);

    public float rotationSmoothTime = 0.12f;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    float yaw;
    float pitch;

    [Header("Çarpışma Ayarları")]
    public LayerMask collisionLayers;
    public float cameraCollisionOffset = 0.2f;

    void LateUpdate()
    {
        if (target == null) return;

        // 1. FARE GİRDİSİ
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        // 2. YUMUŞAK DÖNÜŞ (Sadece Kamera Döner, Karakter Dönmez)
        Vector3 targetRotation = new Vector3(pitch, yaw);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        // 3. POZİSYON HESAPLAMA
        // Artık senin girdiğin 'heightOffset' kadar yukarı bakacak
        Vector3 focusPosition = target.position + Vector3.up * heightOffset;

        Vector3 cameraDirection = -transform.forward;
        float finalDistance = distanceFromTarget;

        // Çarpışma Kontrolü (SphereCast)
        RaycastHit hit;
        // Dev karakter için küre çapını da biraz artırdık (0.2 -> 0.5)
        if (Physics.SphereCast(focusPosition, 0.5f, cameraDirection, out hit, distanceFromTarget, collisionLayers))
        {
            finalDistance = hit.distance - 0.5f - cameraCollisionOffset;
            if (finalDistance < 0.2f) finalDistance = 0.2f;
        }

        transform.position = focusPosition + (cameraDirection * finalDistance);
    }
}