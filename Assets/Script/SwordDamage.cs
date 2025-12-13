using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    private Collider swordCollider; 
    public int swordDamageAmount = 100;

    [Header("Efekt Ayarları")]
    public GameObject hitEffectPrefab;
    public float effectDuration = 2.0f; // Efekt kaç saniye sonra silinsin?

    void Start()
    {
        swordCollider = GetComponent<Collider>();
        if (swordCollider == null)
        {
            Debug.LogError("SwordDamage: Collider bulunamadı!");
            return;
        }
        
        swordCollider.enabled = false; 
    }

    public void EnableHitbox()
    {
        if (swordCollider != null) swordCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        if (swordCollider != null) swordCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BOSS_HITBOX"))
        {
            // --- EFEKT KISMI (GÜNCELLENDİ) ---
            if (hitEffectPrefab != null)
            {
                // 1. Efekti oluştur ve "vfx" adında bir kutuya koy
                GameObject vfx = Instantiate(hitEffectPrefab, other.ClosestPoint(transform.position), Quaternion.identity);
                
                // 2. Bu "vfx" objesini, belirlediğimiz süre (effectDuration) dolunca yok et
                Destroy(vfx, effectDuration);
            }

            // --- BOSS HASAR KISMI (DÜZELTİLMİŞ HALİ) ---
            // Hatırlatma: BossAI scripti genelde ana objede olur, o yüzden GetComponentInParent kullanıyoruz.
            BossAI boss = other.GetComponentInParent<BossAI>(); 
            
            if (boss != null)
            {
                boss.TakeDamage(swordDamageAmount); 
            }
        }
    }
}