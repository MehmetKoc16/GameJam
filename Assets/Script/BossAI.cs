using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // YENİ: Sahne geçişleri için
using UnityEngine.Video;           // YENİ: Video oynatıcı kontrolü için

// Boss'un olası durumlarını tanımlıyoruz
public enum BossState { Idle, Patrol, Chase, Attack, Hurt }

public class BossAI : MonoBehaviour
{
    // BİLEŞENLER
    private NavMeshAgent agent;
    private Animator animator;

    // --- CAN VE DURUM AYARLARI ---
    [Header("Can ve Durum")]
    public int maxHealth = 1000;
    public int currentHealth;
    public bool isDead = false;
    private Transform playerTarget;
    public BossState currentState = BossState.Idle;

    // --- UI AYARLARI ---
    [Header("UI Ayarları")]
    public Image healthBarImage;

    // --- HIZ AYARLARI ---
    [Header("Hız Ayarları")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;

    // --- DEVRİYE (PATROL) ---
    [Header("Devriye Ayarları")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    public float waitTimeAtPoint = 3f;
    private float waitTimer;

    // --- SALDIRI (SİLAH / YUMRUK) ---
    [Header("Saldırı Ayarları")]
    public BossAttackHitbox[] hitboxes;
    public float attackRange = 15f;

    // --- HASAR ALMA ---
    [Header("Hasar Reaksiyon")]
    public float hurtStunDuration = 0.5f;

    [Header("Oyun Sonu Senaryosu")]
    // ARTIK TEK BİR SES DEĞİL, SES DİZİSİ İSTİYORUZ (Köşeli parantez [])
    public AudioSource[] environmentSounds;
    public VideoPlayer endingVideoPlayer;
    public GameObject videoUIObject;
    public string firstSceneName = "MainMenu";



    public GameObject sound,videoCanvas;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent bileşeni bulunamadı!");
            return;
        }

        currentHealth = maxHealth;
        waitTimer = waitTimeAtPoint;

        // --- HEALTH BAR BAŞLANGIÇ AYARI ---
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = 1.0f;
        }

        // Hitboxları başlangıçta garanti kapat
        DisableHitbox();

        // Başlangıçta video panelini kapalı olduğundan emin olalım
        if (videoUIObject != null)
        {
            videoUIObject.SetActive(false);
        }

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            currentState = BossState.Patrol;
            agent.speed = patrolSpeed;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
        else
        {
            currentState = BossState.Idle;
        }
    }

    void Update()
    {
        // Ölüm veya Hurt durumlarında AI mantığını çalıştırma
        if (isDead || agent == null || !agent.isOnNavMesh || currentState == BossState.Hurt)
        {
            return;
        }

        UpdateAnimations();

        switch (currentState)
        {
            case BossState.Idle:
            case BossState.Patrol:
                PatrolLogic();
                break;
            case BossState.Chase:
                ChaseLogic();
                break;
            case BossState.Attack:
                AttackLogic();
                break;
        }
    }

    void UpdateAnimations()
    {
        if (animator != null)
        {
            float currentSpeed = agent.velocity.magnitude;
            float normalizedSpeed = currentSpeed / chaseSpeed;

            if (currentSpeed < 0.1f)
            {
                animator.SetFloat("Speed", 0f);
            }
            else
            {
                animator.SetFloat("Speed", normalizedSpeed);
            }
        }
    }

    // --- DURUM MANTIĞI METOTLARI ---

    void PatrolLogic()
    {
        agent.speed = patrolSpeed;

        if (patrolPoints.Length == 0) return;

        bool isAtDestination = !agent.pathPending &&
                               agent.remainingDistance <= agent.stoppingDistance &&
                               agent.velocity.sqrMagnitude < 0.01f;

        if (isAtDestination)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                waitTimer = waitTimeAtPoint;
            }
        }
        else if (agent.remainingDistance > agent.stoppingDistance)
        {
            agent.isStopped = false;
        }
    }

    void ChaseLogic()
    {
        if (playerTarget == null)
        {
            StopCombat();
            return;
        }

        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(playerTarget.position);

        if (agent.remainingDistance <= attackRange && !agent.pathPending)
        {
            currentState = BossState.Attack;
        }
    }

    void AttackLogic()
    {
        agent.isStopped = true;

        if (playerTarget != null)
        {
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            if (animator != null)
            {
                animator.SetBool("IsAttacking", true);
            }
        }

        if (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) > attackRange * 1.2f)
        {
            if (animator != null) animator.SetBool("IsAttacking", false);
            DisableHitbox();
            agent.isStopped = false;
            currentState = BossState.Chase;
        }
    }

    // --- DIŞ FONKSİYONLAR ---

    public void StartCombat(Transform target)
    {
        if (currentState == BossState.Patrol || currentState == BossState.Idle)
        {
            playerTarget = target;
            currentState = BossState.Chase;

            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.speed = chaseSpeed;
            }
        }
    }

    public void StopCombat()
    {
        if (currentState == BossState.Chase || currentState == BossState.Attack)
        {
            playerTarget = null;
            currentState = BossState.Patrol;

            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.speed = patrolSpeed;
                agent.isStopped = false;

                if (animator != null) animator.SetBool("IsAttacking", false);
                DisableHitbox();

                if (patrolPoints.Length > 0)
                {
                    agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                }
            }
            else
            {
                if (animator != null) animator.SetBool("IsAttacking", false);
            }
        }
    }

    [ContextMenu("Apply Test Damage")]
    public void damage()
    {
        TakeDamage(250);
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        StopAllCoroutines();

        currentHealth -= damageAmount;

        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = (float)currentHealth / maxHealth;
        }

        Debug.Log($"Boss hasar aldı. Kalan Can: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (animator != null)
        {
            animator.SetBool("IsAttacking", false);
        }

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        StartCoroutine(HandleHurt());

        if (animator != null)
        {
            animator.ResetTrigger("TakeHit");
            animator.SetTrigger("TakeHit");
        }
    }

    IEnumerator HandleHurt()
    {
        BossState resumeState;

        if (currentState == BossState.Hurt)
        {
            resumeState = BossState.Chase;
        }
        else
        {
            resumeState = currentState;
        }

        currentState = BossState.Hurt;

        DisableHitbox();

        if (agent.enabled)
        {
            agent.isStopped = true;
        }

        yield return new WaitForSeconds(hurtStunDuration);

        currentState = resumeState;

        if (agent.enabled && !isDead)
        {
            agent.isStopped = false;

            if (currentState == BossState.Patrol && patrolPoints.Length > 0)
            {
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
            else if (currentState == BossState.Chase && playerTarget != null)
            {
                agent.SetDestination(playerTarget.position);
            }
        }
    }

    // --- ANIMATION EVENTS & HİTBOX ---

    public void EnableHitbox()
    {
        if (hitboxes != null)
        {
            foreach (var weapon in hitboxes)
            {
                if (weapon != null)
                {
                    Collider col = weapon.GetComponent<Collider>();
                    if (col != null) col.enabled = true;
                }
            }
        }
    }

    public void DisableHitbox()
    {
        if (hitboxes != null)
        {
            foreach (var weapon in hitboxes)
            {
                if (weapon != null)
                {
                    Collider col = weapon.GetComponent<Collider>();
                    if (col != null) col.enabled = false;
                }
            }
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        DisableHitbox();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Barı kapat
        if (healthBarImage != null)
        {
            healthBarImage.gameObject.SetActive(false);
        }

        // Ölüm animasyonunu tetikle
        if (animator != null)
        {
            animator.SetTrigger("IsDead 0");
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // NOT: Sahne geçişi burada yapılmaz. 
        // Animasyonun sonunda TriggerEndingSequence fonksiyonu çağrılmalı.
    }

    // --- YENİ EKLENEN FONKSİYONLAR (ANIMATION EVENT İLE ÇAĞRILACAK) ---

    // Bu fonksiyonu Unity'de Boss'un ölüm animasyonunun en sonuna Event olarak ekle
    public void TriggerEndingSequence()
    {
        Debug.Log("Boss öldü, final senaryosu başlıyor...");

        // 1. Çevre seslerinin hepsini tek tek kapat
        if (environmentSounds != null)
        {
            foreach (AudioSource sound in environmentSounds)
            {
                if (sound != null)
                {
                    sound.Stop();
                }
            }
        }
        // 2. Video Panelini/RawImage'i görünür yap
        if (videoUIObject != null)
        {
            videoUIObject.SetActive(true);
        }

        // 3. Videoyu başlat ve bitmesini bekle
        if (endingVideoPlayer != null)
        {
            endingVideoPlayer.Play();
            StartCoroutine(WaitAndLoadScene());
        }
        else
        {
            // Video atanmamışsa direkt sahne değiştir (Hata önlemi)
            Debug.LogWarning("Video Player atanmamış, direkt sahneye dönülüyor.");
            SceneManager.LoadScene(firstSceneName);
        }
    }

    public void StartVideo()
    {
        videoCanvas.SetActive(true);
        sound.SetActive(false);
    }

    IEnumerator WaitAndLoadScene()
    {
        // Videonun saniyesi kadar bekle + 0.5 saniye tampon süre
        yield return new WaitForSeconds((float)endingVideoPlayer.length + 0.5f);

        // İlk sahneyi yükle
        SceneManager.LoadScene(firstSceneName);
    }
}