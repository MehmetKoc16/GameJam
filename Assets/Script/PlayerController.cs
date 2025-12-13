using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // BİLEŞENLER
    public CharacterController controller;
    public Transform cameraTransform;
    public Animator animator;
    public TrailRenderer swordTrail;

    [Header("Silah Kontrolü")]
    // Buraya kılıç objesinin üzerindeki SwordDamage script'i bağlanacak
    public SwordDamage swordDamageControl; 

    // HIZ AYARLARI
    public float walkSpeed = 2f;
    public float runSpeed = 6f;
    public float rollSpeed = 8f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    // YERÇEKİMİ
    Vector3 velocity;
    public float gravity = -9.81f;

    // DURUM KONTROLÜ
    bool isRolling = false;
    bool isAttacking = false; 

    void Start()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        if (swordTrail != null) swordTrail.emitting = false;
        
        // Kılıç kontrolü artık SwordDamage script'i tarafından yapılıyor.
        // Bu Start'ta sadece bağlantının kontrolü yeterli.
        if (swordDamageControl == null)
        {
            Debug.LogError("PlayerController: Sword Damage Control referansı eksik!");
        }
    }

    void Update()
    {
        // HATA KORUMASI: CharacterController etkin değilse hareket etme
        if (controller == null || !controller.enabled) return;

        // 1. YERÇEKİMİ 
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // KİLİT NOKTA: Yuvarlanıyorsak VEYA Saldırıyorsak hareket kodlarını çalıştırma
        if (isRolling || isAttacking) return;

        // 2. GİRDİLERİ AL (Horizontal, Vertical, vb...)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // 3. HAREKET MANTIĞI (Yönlendirme ve Move çağrıları)
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float currentSpeed = isRunning ? runSpeed : walkSpeed;

            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime); 

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                StartCoroutine(RollRoutine(moveDir));
            }
        }

        // 4. ANİMASYON (SpeedSetFloat ayarları)
        float targetAnimSpeed = 0f;
        if (direction.magnitude >= 0.1f) targetAnimSpeed = isRunning ? 1f : 0.5f;

        float currentAnimSpeed = animator.GetFloat("Speed");
        float smoothedSpeed = Mathf.Lerp(currentAnimSpeed, targetAnimSpeed, Time.deltaTime * 10f);
        animator.SetFloat("Speed", smoothedSpeed);

        // 5. SALDIRI
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            StartAttack();
        }
    }

    // --- YUVARLANMA ---
    IEnumerator RollRoutine(Vector3 rollDirection)
    {
        // ... (Roll mantığı aynı kalır) ...
        isRolling = true;
        animator.SetTrigger("Roll");

        float rollDuration = 0.8f;
        float timer = 0;
        while (timer < rollDuration)
        {
             if (controller.enabled) 
             {
                controller.Move(rollDirection.normalized * rollSpeed * Time.deltaTime);
             }
            timer += Time.deltaTime;
            yield return null;
        }
        isRolling = false;
    }

    // --- SALDIRI BAŞLATMA ---
    void StartAttack()
    {
        isAttacking = true; // Hareketi kilitle
        animator.SetFloat("Speed", 0f); // Koşma animasyonunu kes
        animator.SetTrigger("Attack");

        // Emniyet sübabı (Animasyon Event'i kaçarsa kilidi açar)
        Invoke("ForceStopAttack", 1.2f);
    }

    // --- ANIMATION EVENT METOTLARI (KILIÇ KONTROLÜ) ---
    // Bu metotlar Animation Event'ler tarafından çağrılır.

    public void EnableHitbox()
    {
        if (swordDamageControl != null)
        {
            swordDamageControl.EnableHitbox(); // Kılıç Collider'ı AÇILDI
        }
        if (swordTrail != null) swordTrail.emitting = true;
    }

    public void DisableHitbox()
    {
        if (swordDamageControl != null)
        {
            swordDamageControl.DisableHitbox(); // Kılıç Collider'ı KAPATILDI
        }
        isAttacking = false; // Hareket kilidi kalktı
        CancelInvoke("ForceStopAttack");
        
        if (swordTrail != null) swordTrail.emitting = false;
    }

    // Emniyet Sübabı Fonksiyonu
    void ForceStopAttack()
    {
        Debug.LogWarning("ForceStopAttack çağrıldı. Animasyon Event'i eksik veya süre çok uzun.");
        DisableHitbox(); // Kılıç kontrolü ve kilidi açma
    }

    public void TrailAc() { if (swordTrail != null) swordTrail.emitting = true; }
    public void TrailKapat() { if (swordTrail != null) swordTrail.emitting = false; }
}