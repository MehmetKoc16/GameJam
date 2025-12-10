using UnityEngine;
using UnityEngine.AI; 

// Boss'un olası durumlarını tanımlıyoruz
public enum BossState { Idle, Patrol, Chase, Attack }

public class BossAI : MonoBehaviour
{
    // Bileşenler
    private NavMeshAgent agent;
    private Animator animator;
    
    // --- Can ve Durum Ayarları ---
    public int maxHealth = 1000;
    private int currentHealth;
    public bool isDead = false;
    private Transform playerTarget;
    public BossState currentState = BossState.Idle;
    
    // --- Hız Ayarları ---
    public float patrolSpeed = 2f; // Devriye (Yürüme) Hızı
    public float chaseSpeed = 5f;  // Kovalama (Koşma) Hızı
    
    // Devriye (Patrol) Ayarları
    public Transform[] patrolPoints; // Inspector'dan atayacağınız devriye noktaları
    private int currentPatrolIndex = 0;
    public float waitTimeAtPoint = 3f; // Devriye noktasında bekleme süresi
    private float waitTimer;
    
    // Saldırı Ayarları
    public float attackRange = 2.5f; // Saldırı menzili

    void Start()
    {
        // Bileşenleri Al
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent bileşeni bulunamadı. Lütfen Boss objesine ekleyin.");
            return;
        }
        
        // Canı başlat
        currentHealth = maxHealth;
        waitTimer = waitTimeAtPoint; 
        
        // Başlangıç durumunu ayarla ve ilk hedefi belirle
        if (patrolPoints.Length > 0)
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
        if (isDead) return; // Boss öldüyse hiçbir şey yapma

        // Agent aktif değilse (NavMesh'te değilse) veya yol hesaplamıyorsa, çık.
        if (agent == null || !agent.isOnNavMesh) 
        {
            return; 
        }

        // Animasyon Hızını Yönet
        UpdateAnimations();

        // Ana yapay zeka döngüsü
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
            // NavMeshAgent'ın anlık hızını al ve Animator'a gönder
            float currentSpeed = agent.velocity.magnitude;
            animator.SetFloat("Speed", currentSpeed); 
        }
    }

    // --- DURUM MANTIĞI METOTLARI ---

    void PatrolLogic()
    {
        // Agent'ın hızını yürüme hızına ayarla
        if (agent.speed != patrolSpeed && agent.speed != 0) 
        {
            agent.speed = patrolSpeed;
        }

        if (patrolPoints.Length == 0) return;

        // Hedefe ulaşıldı mı kontrol et
        bool isPathValid = !agent.pathPending && agent.remainingDistance != Mathf.Infinity;

        if (isPathValid && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Hedefe ulaşıldı, bekleme başlat
            waitTimer -= Time.deltaTime;
            agent.speed = 0; // Boss'u durdur

            if (waitTimer <= 0)
            {
                // Bir sonraki noktaya geç
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                agent.speed = patrolSpeed;
                waitTimer = waitTimeAtPoint; // Sayacı sıfırla
            }
        }
    }

    void ChaseLogic()
    {
        if (playerTarget == null)
        {
            StopCombat(); 
            return;
        }
        
        // Hızı KOŞMA hızına ayarla
        agent.isStopped = false;
        agent.speed = chaseSpeed; 
        agent.SetDestination(playerTarget.position);

        // Saldırı menzilini kontrol et
        if (agent.remainingDistance <= attackRange && !agent.pathPending)
        {
            currentState = BossState.Attack;
        }
    }

    void AttackLogic()
    {
        agent.isStopped = true; // Boss'u saldırı sırasında durdur
        
        // Oyuncuya doğru dön
        if (playerTarget != null)
        {
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            
            // Saldırı Animasyonu Tetikle
            if(animator != null)
            {
                animator.SetBool("IsAttacking", true);
            }
        }
        
        // Oyuncu saldırı menzilinden çıkarsa tekrar kovalamaya dön
        if (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) > attackRange * 1.2f)
        {
            if(animator != null) animator.SetBool("IsAttacking", false);
            agent.isStopped = false;
            currentState = BossState.Chase;
        }
    }

    // --- DIŞ FONKSİYONLAR ---

    // BossAggroController tarafından çağrılır (KOŞMA başlatılır)
    public void StartCombat(Transform target)
    {
        if (currentState == BossState.Patrol || currentState == BossState.Idle)
        {
            playerTarget = target;
            currentState = BossState.Chase; 
            agent.isStopped = false;
            agent.speed = chaseSpeed; 
            Debug.Log("Savaş Başladı! Boss, KOŞMA (Chase) durumuna geçti.");
        }
    }

    // BossAggroController tarafından çağrılır (YÜRÜME'ye geri döner)
    public void StopCombat()
    {
        if (currentState == BossState.Chase || currentState == BossState.Attack)
        {
            playerTarget = null;
            currentState = BossState.Patrol; 
            agent.speed = patrolSpeed; 
            agent.isStopped = false;
            if(animator != null) animator.SetBool("IsAttacking", false);
            Debug.Log("Oyuncu menzilden çıktı. YÜRÜME (Patrol) moduna geri dönülüyor.");
            
            if (patrolPoints.Length > 0)
            {
                 agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
        }
    }
    
    /// <summary>
    /// Boss'un hasar almasını, animasyonu tetiklemesini ve ölümü kontrol eder.
    /// </summary>
    /// <param name="damageAmount">Alınan hasar miktarı.</param>
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        // 1. Canı Azaltma
        currentHealth -= damageAmount;
        
        Debug.Log($"Boss hasar aldı. Kalan Can: {currentHealth}");

        // 2. Hasar Animasyonunu Tetikle
        if (animator != null)
        {
            animator.SetTrigger("Hurt"); 
        }
        
        // 3. Ölüm Kontrolü
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // İsteğe bağlı: Hasar alırken saldırı durumundan çıkarıp kısa süre duraklatma kodu buraya eklenebilir.
        }
    }

    /// <summary>
    /// Boss'un ölüm işlemlerini yönetir.
    /// </summary>
    private void Die()
    {
        isDead = true;
        
        // Boss'un hareketini ve çarpışmasını durdur
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Ölüm Animasyonunu Tetikle 
        if (animator != null)
        {
            // Örneğin: animator.SetBool("IsDead", true); veya direkt ölüm durumuna geçiş
            Debug.Log("Boss Öldü! Animasyon tetiklendi.");
        }
        
        // Burada ganimet düşürme, oyun sonu ekranı tetikleme gibi kodlar yer alır.
    }
}