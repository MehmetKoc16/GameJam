using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Listeleri kullanmak için gerekli

public class PlayerController : MonoBehaviour
{
    // BİLEŞENLER
    public CharacterController controller;
    public Transform cameraTransform;
    public Animator animator;
    public TrailRenderer swordTrail;

    [Header("Ses Ayarları (YENİ)")]
    public AudioSource audioSource; // Karakterin üzerine eklediğin AudioSource
    public List<AudioClip> adimSesleri; // Kesip hazırladığın adım seslerini buraya sürükle
    public AudioClip ziplamaSesi;       // Zıplama "Hıhh!" sesi
    public AudioClip yuvarlanmaSesi;    // Yuvarlanma efekti
    public AudioClip saldiriSesi;       // Kılıç savurma sesi (Whoosh)

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

        // Eğer AudioSource atamayı unuttuysan otomatik ekleyelim
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        Cursor.lockState = CursorLockMode.Locked;
        if (swordTrail != null) swordTrail.emitting = false;
        if (swordCollider != null) swordCollider.enabled = false;
    }

    void Update()
    {
        // 1. YER KONTROLÜ
        isGrounded = controller.isGrounded;

        // Yerçekimi sıfırlama
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
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isRolling && !isAttacking)
        {
            Jump();
        }

        // --- HAREKET VEKÖTÜRÜNÜ HESAPLA ---
        Vector3 moveDirection = Vector3.zero;

        if (!isRolling && !isAttacking && direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float currentSpeed = isRunning ? runSpeed : walkSpeed;

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
            moveDirection = Vector3.zero;
        }

        // --- FİZİK UYGULAMA ---
        if (!isRolling)
        {
            velocity.y += gravity * Time.deltaTime;
            Vector3 finalMove = (moveDirection) + velocity;
            controller.Move(finalMove * Time.deltaTime);
        }
    }

    // --- SES FONKSİYONLARI (Animation Event Burayı Çağıracak) ---
    public void OynatAdimSesi()
    {
        // Eğer havadaysak veya liste boşsa ses çalma
        if (!isGrounded || adimSesleri.Count == 0) return;

        // Sesin tonunu rastgele değiştir (0.8 ile 1.1 arası) -> Doğallık katar
        audioSource.pitch = Random.Range(0.85f, 1.1f);

        // Listeden rastgele bir ses seç
        int rastgeleIndex = Random.Range(0, adimSesleri.Count);

        // Sesi bir kere oynat
        audioSource.PlayOneShot(adimSesleri[rastgeleIndex], 0.6f); // 0.6f ses şiddeti
    }

    // YENİ EKLEDİĞİMİZ SALDIRI SESİ ÇAĞRI FONKSİYONU
    public void OynatSaldiriSesiEvent()
    {
        // Bu fonksiyonu tam kılıcın hızlandığı yerde animasyondan çağıracağız.
        if (saldiriSesi != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f); // Hafif varyasyon kat
            audioSource.PlayOneShot(saldiriSesi, 1f);
        }
    }

    void OynatSes(AudioClip klip, float siddet = 1f)
    {
        if (klip != null)
        {
            audioSource.pitch = 1f; // Diğer seslerde pitch normal kalsın
            audioSource.PlayOneShot(klip, siddet);
        }
    }

    // --- JUMP ---
    void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        OynatSes(ziplamaSesi, 0.8f); // Zıplama sesi çal

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
        OynatSes(yuvarlanmaSesi, 1f); // Yuvarlanma sesi çal

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

    // --- SALDIRI FONKSİYONLARI ---
    void StartAttack()
    {
        isAttacking = true;
        animator.SetFloat("Speed", 0f);
        animator.SetTrigger("Attack");
        // OynatSes(saldiriSesi, 1f); // <-- BURAYI YORUM SATIRI YAPTIM / KALDIRDIM. Sesi artık animasyon içinden Event ile çağıracağız.

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

    // BU FONKSİYONLARA EK OLARAK:
    // Kılıç sesini de Event olarak `TrailAc()`'nin hemen yanına veya biraz sonrasına ekleyebilirsin.
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