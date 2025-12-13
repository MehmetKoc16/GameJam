using UnityEngine;

public class HitReactor : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController; // Hareketi durdurmak için
    private PlayerHealth playerHealth;       // PlayerHealth script'ine erişim için YENİ REFERANS
    
    // Animator Parametre İsimleri (Unity'de ne kullandıysanız aynı olmalı)
    private readonly string takeHitTrigger = "TakeHit"; 
    private readonly string isDeadBool = "IsDead"; 
    
    // Hasar alındıktan sonra bekleme süresi (oyuncuyu kontrolsüz tutmak için)
    public float hitReactionTime = 0.5f;
    private float hitTimer = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        playerHealth = GetComponent<PlayerHealth>(); // Aynı objeden PlayerHealth'i al

        if (animator == null)
        {
            Debug.LogError("HitReactor: Animator bileşeni bulunamadı!");
        }
        if (playerHealth == null)
        {
            Debug.LogError("HitReactor: PlayerHealth bileşeni bulunamadı! Sağlık kontrolü yapılamayacak.");
        }
    }
    
    void Update()
    {
        // Hasar reaksiyonu süresini yönet
        if (hitTimer > 0)
        {
            hitTimer -= Time.deltaTime;
            
            // Reaksiyon bittiğinde, PlayerController'ı tekrar etkinleştir
            // HATA DÜZELTİLDİ: PlayerHealth.isDead yerine playerHealth.isDead kullanıldı.
            if (hitTimer <= 0 && playerController != null && !playerController.enabled) 
            {
                // Yalnızca PlayerHealth referansı varsa VE oyuncu ölmemişse hareketi geri ver
                if (playerHealth != null && !playerHealth.isDead) 
                {
                    playerController.enabled = true;
                }
            }
        }
    }

    /// <summary>
    /// Hasar alındığında tetiklenir ve hasar yönüne göre animasyon parametresi ayarlar.
    /// </summary>
    /// <param name="attackerPos">Saldıranın pozisyonu.</param>
    public void HandleHitReaction(Vector3 attackerPos)
    {
        if (animator == null || playerHealth == null || playerHealth.isDead) return;
        
        // 1. Hasar Yönünü Hesaplama: Karakterin arkasından mı, önünden mi vuruldu?
        Vector3 hitDirection = (transform.position - attackerPos).normalized;
        float angle = Vector3.SignedAngle(transform.forward, hitDirection, Vector3.up);
        
        // 2. Animasyon Tetikleme:
        animator.SetTrigger(takeHitTrigger);
        
        // 3. Geçici Hareketsizlik:
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        hitTimer = hitReactionTime;
    }

    /// <summary>
    /// Oyuncunun canı 0'a ulaştığında çağrılır ve ölüm animasyonunu başlatır.
    /// </summary>
    public void HandleDeath()
    {
        if (animator == null) return;
        
        // Karakteri öldü olarak işaretle
        animator.SetBool(isDeadBool, true); 
        
        // Hareketi kesin olarak durdur
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        Debug.Log("HitReactor: Ölüm tepkisi tetiklendi.");
    }
}