using UnityEngine;
using UnityEngine.AI; 
using System.Collections; 

// Boss'un olası durumlarını tanımlıyoruz
public enum BossState { Idle, Patrol, Chase, Attack, Hurt } 

public class BossAI : MonoBehaviour
{
    // Bileşenler
    private NavMeshAgent agent;
    private Animator animator;
    
    // --- Can ve Durum Ayarları ---
    public int maxHealth = 1000;
    
    public bool isDead = false;
    private int currentHealth;
    
    private Transform playerTarget;
    public BossState currentState = BossState.Idle;
    
    // --- Hız Ayarları ---
    public float patrolSpeed = 2f; 
    public float chaseSpeed = 5f;  
    
    // Devriye (Patrol) Ayarları
    public Transform[] patrolPoints; 
    private int currentPatrolIndex = 0;
    public float waitTimeAtPoint = 3f; 
    private float waitTimer;
    
    public BossAttackHitbox[] hitboxes;
    // Saldırı Ayarları
    public float attackRange = 2.5f; 

    // --- Yeni Hasar Ayarı ---
    public float hurtStunDuration = 0.5f; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent bileşeni bulunamadı.");
            return;
        }
        
        currentHealth = maxHealth;
        waitTimer = waitTimeAtPoint; 
        
        agent.stoppingDistance = 0.15f; 
        
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
            
            if(animator != null)
            {
                animator.SetBool("IsAttacking", true); 
            }
        }
        
        if (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) > attackRange * 1.2f)
        {
            if(animator != null) animator.SetBool("IsAttacking", false);
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
            
            // NavMesh Agent'ın durumunu kontrol et
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
            
            // HATA KORUMASI: NavMesh Agent'ın durumunu kontrol et
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.speed = patrolSpeed; 
                agent.isStopped = false; 
                
                if(animator != null) animator.SetBool("IsAttacking", false);
                
                if (patrolPoints.Length > 0)
                {
                    agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                }
            }
            else
            {
                // Agent devre dışıysa bile animasyonu sıfırla
                 if(animator != null) animator.SetBool("IsAttacking", false);
            }
        }
    }
    

    public void TakeDamage(int damageAmount)
    {
        if (isDead || currentState == BossState.Hurt) return; 

        if (isDead) return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Die();
        }

        StartCoroutine(HandleHurt());

        if (animator != null)
        {
            // Hasar Alma Trigger'ı
            animator.SetTrigger("TakeHit"); 
        }
    }

    IEnumerator HandleHurt()
    {
        BossState previousState = currentState; 
        currentState = BossState.Hurt; 

        if (agent.enabled) 
        {
            agent.isStopped = true; 
        }
        
        yield return new WaitForSeconds(hurtStunDuration);
        
        currentState = previousState;
        
        if (agent.enabled)
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

public void EnableBossHitboxes()
{
    if (hitboxes != null)
    {
        foreach (var hitbox in hitboxes)
        {
            if (hitbox != null)
            {
                hitbox.EnableBossHitbox(); // Her iki eli de AKTİF et
            }
        }
    }
}

// Bu metot Boss'un saldırı animasyonunun BİTİŞ Event'i ile çağrılır
public void DisableBossHitboxes()
{
    if (hitboxes != null)
    {
        foreach (var hitbox in hitboxes)
        {
            if (hitbox != null)
            {
                hitbox.DisableBossHitbox(); // Her iki eli de KAPAT
            }
        }
    }
}

    /// <summary>
    /// Boss'un ölüm işlemlerini yönetir.
    /// </summary>
    private void Die()
    {
        isDead = true;
        
        if (agent != null)
        {
            // Hareket etmesini tamamen durdur
            agent.isStopped = true;
            agent.enabled = false; 
        }

        if (animator != null)
        {
            // Ölüm Animasyonunu Tetikle
            // Animator'da IsDead Bool parametresi olmalıdır.
            animator.SetBool("IsDead", true); 
        }
    }
}