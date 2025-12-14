using UnityEngine;

public class BossWeapon : MonoBehaviour
{
    public int damageAmount = 20; // Boss kaç vursun?
    public bool isWeaponActive = true; // Silah şu an tehlikeli mi?

    private void OnTriggerEnter(Collider other)
    {
        // Sadece silah aktifse ve çarpan obje "Player" ise
        if (isWeaponActive && other.CompareTag("Player"))
        {
            // Oyuncunun can scriptini bulmaya çalışıyoruz
            // NOT: Senin scriptinin adı "PlayerHealth" veya "PlayerStats" olabilir.
            // Aşağıdaki isimi kendi scriptinin adıyla değiştirmelisin.
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // --- SEÇENEK 1: Eğer TakeDamage sadece sayı istiyorsa ---
                // playerHealth.TakeDamage(damageAmount);
 
                // --- SEÇENEK 2: Eğer TakeDamage sayı + pozisyon istiyorsa (Önceki konuşmalara istinaden) ---
                playerHealth.TakeDamage(damageAmount, transform.position);

                Debug.Log("Boss oyuncuya vurdu! Kalan Can: Bilinmiyor (Scriptten bak)");
                
                // Bir vuruşta 50 kere hasar vermemesi için silahı geçici kapatabiliriz
                isWeaponActive = false; 
                Invoke("ResetWeapon", 1.0f); // 1 saniye sonra tekrar vurabilir
            }
        }
    }

    void ResetWeapon()
    {
        isWeaponActive = true;
    }
}