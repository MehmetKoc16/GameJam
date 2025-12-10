using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // BÝLEÞENLER
    public CharacterController controller;
    public Transform cameraTransform;
    public Animator animator;
    public TrailRenderer swordTrail;

    [Header("Silah Ayarlarý")]
    public Collider swordCollider; // YENÝ: Kýlýcýn collider'ýný buraya baðlayacaðýz

    // HIZ AYARLARI
    public float walkSpeed = 2f;
    public float runSpeed = 6f;
    public float rollSpeed = 8f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    // YERÇEKÝMÝ
    Vector3 velocity;
    public float gravity = -9.81f;

    // DURUM KONTROLÜ
    bool isRolling = false;
    bool isAttacking = false; // Saldýrý kilidi

    void Start()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        if (swordTrail != null) swordTrail.emitting = false;

        // Oyun baþlarken kýlýç kesmesin, kapalý olsun
        if (swordCollider != null) swordCollider.enabled = false;
    }

    void Update()
    {
        // 1. YERÇEKÝMÝ (Her zaman çalýþmalý)
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // KÝLÝT NOKTA: Yuvarlanýyorsak VEYA Saldýrýyorsak hareket kodlarýný çalýþtýrma
        if (isRolling || isAttacking) return;

        // 2. GÝRDÝLERÝ AL
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // 3. HAREKET MANTIÐI
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

        // 4. ANÝMASYON
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
        isRolling = true;
        animator.SetTrigger("Roll");

        float rollDuration = 0.8f;
        float timer = 0;
        while (timer < rollDuration)
        {
            controller.Move(rollDirection.normalized * rollSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        isRolling = false;
    }

    // --- SALDIRI BAÞLATMA ---
    void StartAttack()
    {
        isAttacking = true; // Hareketi kilitle
        animator.SetFloat("Speed", 0f); // Koþma animasyonunu kes
        animator.SetTrigger("Attack");

        // YENÝ: Saldýrý baþladý, kýlýcý AKTÝF ET (Kessin)
        if (swordCollider != null) swordCollider.enabled = true;

        // Emniyet sübabý
        Invoke("ForceStopAttack", 1.2f);
    }

    // --- HATAYI ÇÖZEN VE KÝLÝDÝ AÇAN FONKSÝYON ---
    public void AttackBitti()
    {
        isAttacking = false; // Kilidi aç, hareket edebilirsin
        CancelInvoke("ForceStopAttack");

        // YENÝ: Saldýrý bitti, kýlýcý KAPAT (Artýk kesmesin)
        if (swordCollider != null) swordCollider.enabled = false;
    }

    // Emniyet Sübabý Fonksiyonu
    void ForceStopAttack()
    {
        isAttacking = false;
        // YENÝ: Süre dolduysa kýlýcý kapat
        if (swordCollider != null) swordCollider.enabled = false;
    }

    public void TrailAc() { if (swordTrail != null) swordTrail.emitting = true; }
    public void TrailKapat() { if (swordTrail != null) swordTrail.emitting = false; }
}