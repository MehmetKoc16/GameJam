using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    // Kýlýç bir þeyin içinden geçtiðinde bu fonksiyon çalýþýr
    private void OnTriggerEnter(Collider other)
    {
        // Çarptýðýmýz þeyin etiketi "Enemy" mi?
        if (other.CompareTag("Enemy"))
        {
            // Konsola mesaj yaz (Test için)
            Debug.Log("Düþmana Vurdum: " + other.name);

            // Þimdilik düþmaný direkt yok edelim
            Destroy(other.gameObject);
        }
    }
}