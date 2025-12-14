using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    private Collider swordCollider;
    public int swordDamageAmount = 100;

    [Header("Efekt Ayarları")]
    public GameObject hitEffectPrefab;
    public float effectDuration = 2.0f;

    void Start()
    {
        swordCollider = GetComponent<Collider>();
        
        if (swordCollider == null)
        {
            Debug.LogError("SwordDamage: Bu objede Collider (Box/Capsule) bulunamadı!");
            return;
        }

        // Başlangıçta kılıç kapalı olsun (Sadece saldırınca açılsın)
        swordCollider.enabled = false;
        
        // Çarpışma fiziği değil, tetikleyici olarak çalışmalı
        swordCollider.isTrigger = true; 
    }

    // PlayerController tarafından çağrılacak
    public void EnableHitbox()
    {
        if (swordCollider != null) swordCollider.enabled = true;
    }

    // PlayerController tarafından çağrılacak
    public void DisableHitbox()
    {
        if (swordCollider != null) swordCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Sadece BOSS_HITBOX etiketli yerlere (kol, bacak, gövde) vur
        if (other.CompareTag("BOSS_HITBOX"))
        {
            Debug.Log("Kılıç Boss'a temas etti!");

            // --- EFEKT KISMI ---
            if (hitEffectPrefab != null)
            {
                // Temas noktasına efekt koy
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                GameObject vfx = Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);
                Destroy(vfx, effectDuration);
            }

            // --- HASAR GÖNDERME ---
            // BossAI scripti genelde Hitbox'ın ebeveynindedir (Parent).
            // Hitbox kolunda olsa bile ana gövdedeki can scriptini bulur.
            BossAI boss = other.GetComponentInParent<BossAI>();

            if (boss != null)
            {
                boss.TakeDamage(swordDamageAmount);
                Debug.Log("Hasar BossAI scriptine iletildi.");
                
                // İstersen bir vuruşta birden fazla kez hasar vermemesi için
                // vurduğu an collider'ı kapatabilirsin (Opsiyonel):
                // DisableHitbox(); 
            }
            else
            {
                // Yedek kontrol: Belki direkt scriptin olduğu objeye vurmuşuzdur
                boss = other.GetComponent<BossAI>();
                if (boss != null) boss.TakeDamage(swordDamageAmount);
            }
        }
    }
}