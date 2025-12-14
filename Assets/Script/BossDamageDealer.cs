using UnityEngine;

public class BossDamageDealer : MonoBehaviour
{
    [Header("Hasar Ayarları")]
    public float damageAmount = 20f; // Boss ne kadar vursun?

    private void OnTriggerEnter(Collider other)
    {
        // 1. Çarptığımız obje "Player" etiketine sahip mi?
        if (other.CompareTag("Player"))
        {
            // 2. Oyuncunun can scriptini (PlayerHealth) bul
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // 3. Hasar fonksiyonunu çalıştır.
                // Senin PlayerHealth kodun hasar miktarının yanında vuranın pozisyonunu da istiyordu.
                // 'transform.position' diyerek silahın o anki konumunu yolluyoruz.
                playerHealth.TakeDamage(damageAmount, transform.position);
                
                Debug.Log("Boss oyuncuya vurdu!");
            }
        }
    }
}