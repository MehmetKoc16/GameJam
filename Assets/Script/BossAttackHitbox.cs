using UnityEngine;

public class BossAttackHitbox : MonoBehaviour
{
    public int damageAmount = 20; 
    public Collider attackCollider;
    
    // Aynı anda birden fazla vurmayı engellemek için basit bir kontrol eklenebilir
    // Şimdilik senin yapını koruyorum.

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Yeni PlayerHealth scriptini arıyoruz
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // Saldırı pozisyonunu (kılıcın/hitbox'ın pozisyonu) gönderiyoruz
                playerHealth.TakeDamage(damageAmount, transform.position); 
                
                // Vurduktan sonra collider'ı kapatmak istersen:
                // attackCollider.enabled = false;
            }
        }
    }

    public void EnableBossHitbox()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
        }
    }
    
    public void DisableBossHitbox()
    {
        if (attackCollider != null) attackCollider.enabled = false;
    }
}