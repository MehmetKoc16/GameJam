using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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
    // Buraya Asayı veya Yumrukları sürükleyeceksin
    public BossAttackHitbox[] hitboxes;
    public float attackRange = 15f;

    // --- HASAR ALMA ---
    [Header("Hasar Reaksiyon")]
    public float hurtStunDuration = 0.5f;

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

        //agent.stoppingDistance = 0.15f;

        // Hitboxları başlangıçta garanti kapat
        DisableHitbox();

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

            // Çok küçük hızlarda kaymayı önlemek için 0'a sabitle
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
            // Boss saldırırken oyuncuya dönsün
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            if (animator != null)
            {
                animator.SetBool("IsAttacking", true);
            }
        }

        // Oyuncu menzilden çıktıysa kovalamaya dön
        if (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) > attackRange * 1.2f)
        {
            if (animator != null) animator.SetBool("IsAttacking", false);

            // Saldırı iptal olunca hitboxları kapat ki havada açık kalmasın
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
                DisableHitbox(); // Savaş bitince silahı kapat

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
        // "|| currentState == BossState.Hurt" kısmını SİLDİM.
        // Artık sadece ölüyse tepki vermeyecek.
        if (isDead) return;

        // Eğer zaten hasar alma sürecindeysek, önceki bekleme sayacını durdurmalıyız
        // ki üst üste binmesin.
        StopAllCoroutines();

        currentHealth -= damageAmount;
        Debug.Log($"Boss hasar aldı. Kalan Can: {currentHealth}"); // Artık bunu her vuruşta göreceksin

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // --- DİĞER KISIMLAR AYNI ---
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

        // Trigger'ı sıfırlayıp tekrar çekiyoruz ki takılma olmasın
        if (animator != null)
        {
            animator.ResetTrigger("TakeHit");
            animator.SetTrigger("TakeHit");
        }
    }

    IEnumerator HandleHurt()
    {
        // --- DÜZELTME BURADA ---
        // Eğer şu an zaten HURT modundaysak, demek ki peş peşe dayak yiyoruz.
        // O zaman "eski durum" olarak HURT'ü değil, CHASE (Kovalama) modunu baz alalım.
        // Yoksa Boss sonsuza kadar Hurt modunda takılı kalır ve saldıramaz.
        BossState resumeState;

        if (currentState == BossState.Hurt)
        {
            resumeState = BossState.Chase;
        }
        else
        {
            resumeState = currentState;
        }
        // -----------------------

        currentState = BossState.Hurt;

        // Hasar anında hitboxları kapat (Adil oyun için)
        DisableHitbox();

        if (agent.enabled)
        {
            agent.isStopped = true;
        }

        yield return new WaitForSeconds(hurtStunDuration);

        // Bekleme bitince hesapladığımız moda geri dön (Chase veya Patrol)
        currentState = resumeState;

        if (agent.enabled && !isDead)
        {
            agent.isStopped = false;

            // Eğer devriyeye döneceksek rotayı güncelle
            if (currentState == BossState.Patrol && patrolPoints.Length > 0)
            {
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
            // Eğer kovalamaya döneceksek (ki genelde bu olur) oyuncuya koş
            else if (currentState == BossState.Chase && playerTarget != null)
            {
                agent.SetDestination(playerTarget.position);
            }
        }
    }

    // --- KRİTİK BÖLÜM: ANIMATION EVENTS İÇİN ---

    // Animasyon Event: Function ismine "EnableHitbox" yaz.
    public void EnableHitbox()
    {
        if (hitboxes != null)
        {
            foreach (var weapon in hitboxes)
            {
                if (weapon != null)
                {
                    // Silahın üzerindeki Collider'ı bulup açar
                    Collider col = weapon.GetComponent<Collider>();
                    if (col != null) col.enabled = true;
                }
            }
        }
    }

    // Animasyon Event: Function ismine "DisableHitbox" yaz.
    public void DisableHitbox()
    {
        if (hitboxes != null)
        {
            foreach (var weapon in hitboxes)
            {
                if (weapon != null)
                {
                    // Silahın üzerindeki Collider'ı bulup kapatır
                    Collider col = weapon.GetComponent<Collider>();
                    if (col != null) col.enabled = false;
                }
            }
        }
    }
    // -------------------------------------------

    private void Die()
    {
        if (isDead) return; // Zaten öldüyse tekrar çalışmasın

        isDead = true;
        DisableHitbox(); // Ölünce silahı zararsız hale getir

        // NavMeshAgent'ı tamamen kapatıyoruz
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (animator != null)
        {
            // ESKİSİ: animator.SetBool("IsDead", true);

            // YENİSİ (Trigger):
            // Bu trigger, Any State üzerinden ölüm animasyonuna geçişi sağlayacak.
            animator.SetTrigger("IsDead 0");
        }

        // İstersen collider'ı da kapatabilirsin ki cesedin içinden geçilebilsin
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
}