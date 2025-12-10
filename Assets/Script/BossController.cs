using UnityEngine;

public class BossController : MonoBehaviour
{
// Karakterin hareket hızını ayarlamak için genel değişkenler
    [Header("Hareket Ayarları")]
    public float walkSpeed = 0.5f; // Yürüme Hızı
    public float runSpeed = 1.0f;  // Koşma Hızı (Maksimum hız)
    public float rotationSpeed = 500f; // Dönüş Hızı

    // Gerekli Bileşenler
    private Animator animator;
    private CharacterController characterController;

    // Başlangıçta bileşenleri al
    void Start()
    {
        // Karakterin Animator Component'ini alıyoruz
        animator = GetComponent<Animator>();

        // Karakteri hareket ettirmek için CharacterController kullanıyorsanız
        // (Eğer kullanmıyorsanız bu satırı silebilirsiniz)
        characterController = GetComponent<CharacterController>();

        // Kontrol: Eğer Animator yoksa uyarı ver
        if (animator == null)
        {
            Debug.LogError("Animator bileşeni bulunamadı! Lütfen karakter nesnesine ekleyin.");
        }
    }

    // Her karede çalışır
    void Update()
    {
        // 1. KLAVYE GİRDİLERİNİ ALMA
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D veya Sol/Sağ
        float verticalInput = Input.GetAxis("Vertical");   // W/S veya İleri/Geri

        // 2. HAREKET YÖNÜ VE HIZ HESAPLAMALARI
        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;
        
        // Blend Tree için animasyon hızı parametresi. 
        // Kullanıcının ne kadar ileri/geri yürüdüğünü temsil eder.
        float currentSpeedMagnitude = movementDirection.magnitude; // 0 ile 1 arasında bir değer

        // Karakterin hareket hızı
        float targetMoveSpeed;
        
        // Eğer hareket girdi büyüklüğü 0.1'den büyükse (hareket ediliyorsa)
        if (currentSpeedMagnitude > 0.1f)
        {
            // Kullanıcı Shift tuşuna basıyorsa koş, aksi halde yürü
            if (Input.GetKey(KeyCode.LeftShift))
            {
                targetMoveSpeed = runSpeed;
                // Animator için Hız Parametresi (Koşma: 1.0)
                animator.SetFloat("Speed", 1.0f); 
            }
            else
            {
                targetMoveSpeed = walkSpeed;
                // Animator için Hız Parametresi (Yürüme: 0.5)
                animator.SetFloat("Speed", 0.5f);
            }

            // Hareketi CharacterController ile uygula
            if (characterController != null)
            {
                Vector3 moveVector = movementDirection * targetMoveSpeed * Time.deltaTime;
                characterController.Move(moveVector);
            }
            else
            {
                // Eğer CharacterController yoksa, Transform ile hareket ettir (Basit)
                transform.Translate(movementDirection * targetMoveSpeed * Time.deltaTime, Space.World);
            }
            
            // 3. KARAKTER DÖNÜŞÜ
            // Karakteri hareket yönüne doğru döndür
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Karakter Duruyor (Girdi yok)
            targetMoveSpeed = 0f;
            // Animator için Hız Parametresi (Idle: 0.0)
            animator.SetFloat("Speed", 0.0f);
        }
    }
}
