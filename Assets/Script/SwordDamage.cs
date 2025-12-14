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
        // Tag kontrolü
        if (other.CompareTag("BOSS_HITBOX"))
        {
            Debug.Log("Temas sağlandı: BOSS_HITBOX algılandı."); // Konsolda bunu görüyorsan Tag ve Collider sağlamdır.

            // --- EFEKT KISMI ---
            if (hitEffectPrefab != null)
            {
                GameObject vfx = Instantiate(hitEffectPrefab, other.ClosestPoint(transform.position), Quaternion.identity);
                Destroy(vfx, effectDuration);
            }

            // --- DÜZELTME BURADA ---
            // Çarptığımız obje (kol/bacak) sadece bir parçadır. 
            // Script ana karakterde olduğu için 'InParent' kullanmak ZORUNDAYIZ.
            BossAI boss = other.GetComponentInParent<BossAI>();

            if (boss != null)
            {
                boss.TakeDamage(swordDamageAmount);
                Debug.Log("BossAI bulundu ve hasar gönderildi.");
            }
            else
            {
                Debug.LogWarning("DİKKAT: BOSS_HITBOX var ama BossAI scripti Parent'ta bulunamadı!");
            }
        }
    }
}