
using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    public int swordDamageAmount = 100;
    private Collider swordCollider; 

    void Start()
    {
        swordCollider = GetComponent<Collider>();

        if (swordCollider == null)
        {
            Debug.LogError("SwordDamage script'i, üzerinde Collider olmayan bir objeye eklendi!");
            return;
        }

        // KRİTİK: Başlangıçta Collider'ı kapatın! Bu, yaklaştığınızda hasar almayı engeller.
        swordCollider.enabled = false;
    }

    // Animasyon Event'i ile çağrılacak metot (Kılıç sallanmaya başladığında)
    public void EnableHitbox()
    {
        if (swordCollider != null)
        {
            swordCollider.enabled = true;
            Debug.Log("Kılıç Collider AÇIK");
        }
    }

    // Animasyon Event'i ile çağrılacak metot (Kılıç sallama bittiğinde)
    public void DisableHitbox()
    {
        if (swordCollider != null)
        {
            swordCollider.enabled = false;
            Debug.Log("Kılıç Collider KAPALI");
        }
    }

    // Hasar verme mantığı
    private void OnTriggerEnter(Collider other)
    {
        // Sadece BOSS_HITBOX Tag'ine sahip ana gövdeye vurulduğunda devam et
        if (other.gameObject.CompareTag("BOSS_HITBOX"))
        {
            BossAI boss = other.GetComponent<BossAI>(); 
            Debug.Log("Kılıç Boss'un ana gövdesine çarptı.");

            if (boss != null)
            {
                Debug.Log($"Kılıç, Boss'un ana gövdesine temas etti. Hasar Verildi: {swordDamageAmount}");
                boss.TakeDamage(swordDamageAmount); 
                
                // Kılıç bir mermi değil, kalıcı obje olduğu için yok etmiyoruz.
            }
        }
    }
}