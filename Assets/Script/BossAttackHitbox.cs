using UnityEngine;

public class BossAttackHitbox : MonoBehaviour
{
    [Header("Hasar Ayarı")]
    public float damageAmount = 20f; // Boss vurduğunda kaç can gitsin?

    // Collider'a bir şey girdiğinde çalışır
    private void OnTriggerEnter(Collider other)
    {
        // Çarpan şey "Player" etiketine sahip mi?
        if (other.CompareTag("Player"))
        {
            // Oyuncunun üzerindeki PlayerHealth scriptini bul
            // NOT: Senin oyuncundaki scriptin adı PlayerHealth ise bunu kullan.
            // Eğer adı farklıysa (örn: PlayerController), aşağıyı ona göre değiştir.
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // Hasarı gönder (Pozisyon bilgisiyle beraber)
                playerHealth.TakeDamage(damageAmount, transform.position);
                
                Debug.Log("Boss oyuncuya vurdu!");

                // Bir vuruşta defalarca hasar vermemek için collider'ı hemen kapatabiliriz
                // (İsteğe bağlı, eğer çok seri hasar yiyorsan bu satırı aç)
                // GetComponent<Collider>().enabled = false; 
            }
        }
    }
}