// BossAttackHitbox.cs (Basitleştirildi)

using UnityEngine;

public class BossAttackHitbox : MonoBehaviour
{
    public int damageAmount = 20; 
    private Collider attackCollider;
    private bool hasHit = false; // Tek bir saldırı döngüsünde birden fazla vurmayı engeller

    void Start()
    {
        attackCollider = GetComponent<Collider>();
        if (attackCollider != null)
        {
            attackCollider.enabled = false; 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            // KRİTİK: Hasar verirken Boss'un pozisyonunu gönderiyoruz
            playerHealth.TakeDamage(damageAmount, transform.position); 
            // ... (diğer kapatma/temas kodları) ...
        }
    }
    }
    
    // --- BOSS AI KONTROL FONKSİYONLARI ---
    
    public void EnableBossHitbox()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
            hasHit = false; // Yeni saldırı döngüsü için sıfırla
        }
    }
    
    public void DisableBossHitbox()
    {
        if (attackCollider != null) attackCollider.enabled = false;
    }
}