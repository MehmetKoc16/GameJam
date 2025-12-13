using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    private Collider swordCollider; 
    public int swordDamageAmount = 100;

    public GameObject hitEffectPrefab;

    void Start()
    {
        swordCollider = GetComponent<Collider>();
        if (swordCollider == null)
        {
            Debug.LogError("SwordDamage: Collider bulunamadı! Lütfen kılıç objesine bir Collider ekleyin.");
            return;
        }
        
        // Başlangıçta Collider'ı kapat
        swordCollider.enabled = false; 
    }

    // --- MERKEZİ KONTROL FONKSİYONLARI ---

    // PlayerController tarafından kılıcı açmak için çağrılır
    public void EnableHitbox()
    {
        if (swordCollider != null)
        {
            swordCollider.enabled = true;
            // Debug.Log("Kılıç Hitbox AÇIK"); // Test için
        }
    }

    // PlayerController tarafından kılıcı kapatmak için çağrılır
    public void DisableHitbox()
    {
        if (swordCollider != null)
        {
            swordCollider.enabled = false;
            // Debug.Log("Kılıç Hitbox KAPALI"); // Test için
        }
    }

    // Hasar verme mantığı
    private void OnTriggerEnter(Collider other)
    {
        // SADECE BOSS_HITBOX TAG'ine sahip objeye hasar ver
        if (other.CompareTag("BOSS_HITBOX"))
        {
            Instantiate(hitEffectPrefab, other.ClosestPoint(transform.position), Quaternion.identity);
            BossAI boss = other.GetComponent<BossAI>(); 
            if (boss != null)
            {
                boss.TakeDamage(swordDamageAmount); 
            }
        }
    }
}