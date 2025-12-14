using UnityEngine;
using UnityEngine.UI;

public class SimpleTargetLock : MonoBehaviour
{
    [Header("Ayarlar")]
    public float lockRange = 20f;
    public float rotationSpeed = 10f; // Dönüş hızı arttırıldı
    public LayerMask enemyLayer;

    [Header("Referanslar")]
    public Transform cameraTransform; 
    public Image lockOnIcon;

    public bool isLocked = false;
    private Transform currentTarget;

    void Start()
    {
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
        if (lockOnIcon != null) lockOnIcon.enabled = false;
    }

    // LateUpdate kullanıyoruz ki diğer scriptler işini bitirdikten sonra biz yönü zorla düzeltelim
    void LateUpdate() 
    {
        // Q tuşu kontrolü (Update yerine burada da çalışır)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isLocked) Unlock();
            else FindAndLock();
        }

        if (isLocked && currentTarget != null)
        {
            // 1. Kamerayı Boss'a Döndür
            HandleRotation();
            
            // 2. UI Simgesini Taşı
            HandleUI();

            // 3. Mesafe Kontrolü
            if (Vector3.Distance(transform.position, currentTarget.position) > lockRange) Unlock();
        }
        else if (isLocked && currentTarget == null)
        {
            Unlock(); // Hedef öldüyse kilidi aç
        }
    }

    void FindAndLock()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, lockRange, enemyLayer);
        foreach (Collider enemy in enemies)
        {
            currentTarget = enemy.transform;
            isLocked = true;
            if (lockOnIcon != null) lockOnIcon.enabled = true;
            return; 
        }
    }

    void Unlock()
    {
        isLocked = false;
        currentTarget = null;
        if (lockOnIcon != null) lockOnIcon.enabled = false;
    }

    void HandleRotation()
    {
        // Hedefe doğru yönü hesapla
        Vector3 direction = currentTarget.position - transform.position;
        direction.y = 0; // Karakterin yukarı/aşağı eğilmesini engelle

        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // --- KRİTİK KISIM ---
        // Sadece kamerayı değil, bu scriptin bağlı olduğu KARAKTERİ (transform) döndürüyoruz.
        // Böylece kamera da karakterin peşinden gelecektir.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        
        // Eğer kameran karakterden bağımsızsa, kamerayı da ayrıca döndürmek gerekebilir:
        // cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, Quaternion.LookRotation(currentTarget.position - cameraTransform.position), rotationSpeed * Time.deltaTime);
    }

    void HandleUI()
    {
        if (lockOnIcon != null)
        {
            Vector3 targetPos = currentTarget.position + Vector3.up * 1.5f;
            // Hedef arkamızda kalsa bile UI saçmalamasın diye kontrol
            if(Vector3.Dot((targetPos - cameraTransform.position).normalized, cameraTransform.forward) > 0)
            {
                lockOnIcon.transform.position = Camera.main.WorldToScreenPoint(targetPos);
                lockOnIcon.gameObject.SetActive(true);
            }
            else
            {
                lockOnIcon.gameObject.SetActive(false); // Arkamızdaysa gizle
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lockRange);
    }
}