using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PlayerController : MonoBehaviour
{
    // BİLEŞENLER
    public CharacterController controller;
    public Transform cameraTransform;
    public Animator animator;
    public TrailRenderer swordTrail;

    [Header("Ses Ayarları")]
    public AudioSource audioSource;
    public List<AudioClip> adimSesleri;
    public AudioClip ziplamaSesi;
    public AudioClip yuvarlanmaSesi;
    public AudioClip saldiriSesi;

    [Header("Silah Kontrolü")]
    // !!! ÖNEMLİ !!!: Inspector'da elindeki Kılıç Objesini buraya sürükle!
    public SwordDamage swordDamageControl; 

    // HIZ AYARLARI
    public float walkSpeed = 2f;
    public float runSpeed = 6f;
    public float rollSpeed = 8f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    // YERÇEKİMİ VE ZIPLAMA
    Vector3 velocity;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    bool isGrounded;

    // DURUM KONTROLÜ
    bool isRolling = false;
    bool isAttacking = false;
    
    // Emniyet Sübabı Süresi
    private const float ATTACK_DURATION_SAFETY = 1.2f;


    void Start()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponent<Animator>();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        Cursor.lockState = CursorLockMode.Locked;
        
        // Başlangıçta kılıç izi ve hasarı kapalı olsun
        if (swordTrail != null) swordTrail.emitting = false;
        
        if (swordDamageControl != null)
        {
             swordDamageControl.DisableHitbox();
        }
    }

    void Update()
    {
        if (controller == null || !controller.enabled) return;

        // Yer Kontrolü ve Yerçekimi
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        // Girdileri Al
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        
        // Zıplama
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isRolling && !isAttacking)
        {
            Jump();
        }

        Vector3 moveDirection = Vector3.zero;

        // Hareket ve Dönüş
        if (!isRolling && !isAttacking && direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float currentSpeed = isRunning ? runSpeed : walkSpeed;

            moveDirection = moveDir.normalized * currentSpeed;

            // Yuvarlanma
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                StartCoroutine(RollRoutine(moveDir));
            }
        }

        // Animasyon Hızı
        if (!isRolling && !isAttacking)
        {
            float targetAnimSpeed = (direction.magnitude >= 0.1f) ? (isRunning ? 1f : 0.5f) : 0f;
            float smoothedSpeed = Mathf.Lerp(animator.GetFloat("Speed"), targetAnimSpeed, Time.deltaTime * 10f);
            animator.SetFloat("Speed", smoothedSpeed);
        }

        if (animator != null && HasParameter("IsGrounded"))
        {
            animator.SetBool("IsGrounded", isGrounded);
        }

        // Saldırı Başlatma
        if (Input.GetMouseButtonDown(0) && !isAttacking && !isRolling && isGrounded)
        {
            StartAttack();
            moveDirection = Vector3.zero; // Saldırırken kaymayı önle
        }

        // Fizik Uygulama
        if (!isRolling)
        {
            velocity.y += gravity * Time.deltaTime;
            Vector3 finalMove = (moveDirection) + velocity;
            controller.Move(finalMove * Time.deltaTime);
        }
    }

    // --- ANIMATION EVENTS (KÖPRÜ FONKSİYONLAR) ---
    // Animasyondaki Event'e Function Name olarak "EnableHitbox" yazacaksın.
    public void EnableHitbox()
    {
        Debug.Log("Kılıç hitbox açıldı.");
        // 1. Kılıç Scriptine ulaş ve aç
        if (swordDamageControl != null)
        {
            swordDamageControl.EnableHitbox(); 
        }
        
        // 2. Kılıç izini aç
        if (swordTrail != null) swordTrail.emitting = true;
    }

    // Animasyondaki Event'e Function Name olarak "DisableHitbox" yazacaksın.
    public void DisableHitbox()
    {
        Debug.Log("Kılıç hitbox kapandı.");
        // 1. Kılıç Scriptine ulaş ve kapat
        if (swordDamageControl != null)
        {
            swordDamageControl.DisableHitbox(); 
        }
        
        // 2. Kılıç izini kapat ve saldırı durumunu bitir
        if (swordTrail != null) swordTrail.emitting = false;
        
        isAttacking = false; 
        CancelInvoke("ForceStopAttack");
    }

    // --- YARDIMCI METOTLAR ---

    void StartAttack()
    {
        isAttacking = true;
        if(animator != null)
        {
             animator.SetFloat("Speed", 0f);
             animator.SetTrigger("Attack"); 
        }
        
        OynatSaldiriSesiEvent(); // Sesi hemen oynatabiliriz veya anim event'e de koyabilirsin
        Invoke("ForceStopAttack", ATTACK_DURATION_SAFETY);
    }

    void ForceStopAttack()
    {
        if (isAttacking)
        {
            // Debug.LogWarning("ForceStopAttack devreye girdi.");
            DisableHitbox();
        }
    }
    
    void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        OynatSes(ziplamaSesi, 0.8f); 

        if (animator != null && HasParameter("Jump"))
        {
            animator.SetTrigger("Jump");
        }
    }

    IEnumerator RollRoutine(Vector3 rollDirection)
    {
        isRolling = true;
        animator.SetTrigger("Roll");
        OynatSes(yuvarlanmaSesi, 1f); 

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

    // --- SES ve DİĞERLERİ ---
    void OynatSes(AudioClip klip, float siddet = 1f, float pitchMin = 1f, float pitchMax = 1f)
    {
        if (klip != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(pitchMin, pitchMax);
            audioSource.PlayOneShot(klip, siddet);
        }
    }

    public void OynatAdimSesi()
    {
        if (!isGrounded || adimSesleri.Count == 0) return;
        int rastgeleIndex = Random.Range(0, adimSesleri.Count);
        OynatSes(adimSesleri[rastgeleIndex], 0.6f, 0.85f, 1.1f); 
    }

    public void OynatSaldiriSesiEvent()
    {
        OynatSes(saldiriSesi, 1f, 0.9f, 1.1f);
    }
    
    bool HasParameter(string paramName)
    {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
}