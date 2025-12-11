using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // BİLEŞENLER
    public CharacterController controller;
    public Transform cameraTransform;
    public Animator animator;
    public TrailRenderer swordTrail;

    [Header("Silah Ayarları")]
    public Collider swordCollider;

    // HIZ AYARLARI
    public float walkSpeed = 2f;
    public float runSpeed = 6f;
    public float rollSpeed = 8f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    // YERÇEKİMİ VE ZIPLAMA
    Vector3 velocity;
    public float gravity = -9.81f;

    // JUMP AYARLARI
    public float jumpHeight = 3f;
    bool isGrounded;

    // DURUM KONTROLÜ
    bool isRolling = false;
    bool isAttacking = false;

    void Start()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        if (swordTrail != null) swordTrail.emitting = false;
        if (swordCollider != null) swordCollider.enabled = false;
    }

    void Update()
    {
        // 1. YER KONTROLÜ
        isGrounded = controller.isGrounded;

        // Yerçekimi sıfırlama (Yerdeysek ve aşağı düşüyorsak hızı sabitle)
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // --- GİRDİLERİ AL ---
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // --- ZIPLAMA KONTROLÜ ---
        // Space tuşu ile zıplama (Hareket kodlarından önce kontrol ediyoruz)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isRolling && !isAttacking)
        {
            Jump();
        }

        // --- HAREKET VEKÖTÜRÜNÜ HESAPLA (Henüz hareket etme!) ---
        Vector3 moveDirection = Vector3.zero;

        // Eğer yuvarlanmıyor ve saldırmıyorsak hareket hesapla
        if (!isRolling && !isAttacking && direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float currentSpeed = isRunning ? runSpeed : walkSpeed;

            // Yatay hareketi vektöre ekle
            moveDirection = moveDir.normalized * currentSpeed;

            // Yuvarlanma tetikleyicisi
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                StartCoroutine(RollRoutine(moveDir));
            }
        }

        // --- ANİMASYON ---
        if (!isRolling && !isAttacking)
        {
            float targetAnimSpeed = (direction.magnitude >= 0.1f) ? (isRunning ? 1f : 0.5f) : 0f;
            float currentAnimSpeed = animator.GetFloat("Speed");
            float smoothedSpeed = Mathf.Lerp(currentAnimSpeed, targetAnimSpeed, Time.deltaTime * 10f);
            animator.SetFloat("Speed", smoothedSpeed);
        }

        if (animator != null && HasParameter("IsGrounded"))
        {
            animator.SetBool("IsGrounded", isGrounded);
        }

        // --- SALDIRI ---
        if (Input.GetMouseButtonDown(0) && !isAttacking && !isRolling && isGrounded)
        {
            StartAttack();
            moveDirection = Vector3.zero; // Saldırırken kaymayı önlemek için
        }

        // --- FİZİK UYGULAMA (TEK SEFERDE) ---

        // Eğer yuvarlanıyorsak, hareketi Coroutine yönetiyor, burası sadece yerçekimini uygular
        // Eğer saldırmıyorsak veya yuvarlanmıyorsak normal hareket uygula
        if (!isRolling)
        {
            // Yerçekimini velocity.y'ye ekle
            velocity.y += gravity * Time.deltaTime;

            // Yatay Hareket (moveDirection) + Dikey Hareket (velocity) birleştiriliyor
            // moveDirection zaten hız ile çarpılmıştı, o yüzden sadece Time.deltaTime ile çarpıyoruz
            Vector3 finalMove = (moveDirection) + velocity;

            // TEK VE NİHAİ MOVE ÇAĞRISI
            controller.Move(finalMove * Time.deltaTime);
        }
    }

    // --- JUMP ---
    void Jump()
    {
        // Fizik formülü: v = sqrt(2 * g * h)
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        if (animator != null)
        {
            animator.SetTrigger("Jump");
        }
    }

    // --- YUVARLANMA ---
    IEnumerator RollRoutine(Vector3 rollDirection)
    {
        isRolling = true;
        animator.SetTrigger("Roll");

        float rollDuration = 0.8f;
        float timer = 0;

        // Yuvarlanırken yerçekimi sıfırlanmasın diye mevcut Y hızını koruyabilir veya sıfırlayabilirsin.
        // Basitlik için sadece ileri itiyoruz:
        while (timer < rollDuration)
        {
            // Yuvarlanırken de yerçekimi olması için velocity.y'yi hesaba katmalıyız ama
            // basit kalması için sadece ileri itiyoruz:
            controller.Move(rollDirection.normalized * rollSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        isRolling = false;
    }

    // --- SALDIRI FONKSİYONLARI ---
    void StartAttack()
    {
        isAttacking = true;
        animator.SetFloat("Speed", 0f);
        animator.SetTrigger("Attack");
        if (swordCollider != null) swordCollider.enabled = true;
        Invoke("ForceStopAttack", 1.2f);
    }

    public void AttackBitti()
    {
        isAttacking = false;
        CancelInvoke("ForceStopAttack");
        if (swordCollider != null) swordCollider.enabled = false;
    }

    void ForceStopAttack()
    {
        isAttacking = false;
        if (swordCollider != null) swordCollider.enabled = false;
    }

    public void TrailAc() { if (swordTrail != null) swordTrail.emitting = true; }
    public void TrailKapat() { if (swordTrail != null) swordTrail.emitting = false; }

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