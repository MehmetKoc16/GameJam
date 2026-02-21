using UnityEngine;

public class BossAggroController : MonoBehaviour
{
    // Inspector'da Boss'un ana objesindeki BossAI script'ini buraya sürükleyin
    public BossAI bossAI; 
    
    private void Start()
    {
        // Eğer atanmadıysa, otomatik olarak ebeveyn (Boss) objesinden BossAI'ı bulmaya çalış
        if (bossAI == null)
        {
            bossAI = GetComponentInParent<BossAI>();
        }

        if (bossAI == null)
        {
            Debug.LogError("BossAI script'i bulunamadı. Lütfen Inspector'dan atayın.");
        }
    }

    // Bir obje Trigger alanına girdiğinde çağrılır
    private void OnTriggerEnter(Collider other)
    {
        // Giren objenin "Player" etiketine sahip olup olmadığını kontrol et
        if (other.CompareTag("Player"))
        {
            Debug.Log("Alana girdi.");
            if (bossAI != null)
            {
                // BossAI'daki Combat başlatma metodunu çağır (KOŞMA başlar)
                bossAI.StartCombat(other.transform); 
            }
        }
    }

    // Bir obje Trigger alanından çıktığında çağrılır
    private void OnTriggerExit(Collider other)
    {
        // Çıkan objenin "Player" etiketine sahip olup olmadığını kontrol et
        if (other.CompareTag("Player"))
        {
            if (bossAI != null)
            {
                // BossAI'daki Combat durdurma metodunu çağır (YÜRÜME'ye geri döner)
                bossAI.StopCombat();
            }
        }
    }
}