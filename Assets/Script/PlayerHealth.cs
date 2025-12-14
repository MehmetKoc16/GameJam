using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Can Ayarları")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    [Header("UI Bağlantıları")]
    public Image healthBarImage; // Can barı görseli
    public GameObject deathPanel; // Ölünce açılacak siyah panel

    [Header("Animasyon ve Fizik")]
    public Animator animator;
    public Rigidbody kilicRigidbody; // Oyuncunun elindeki kılıcın Rigidbody'si

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();

        // Oyun başladığında panel açıksa kapatalım
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }
    }

    // BossAttackHitbox'tan çağrılan fonksiyon
    public void TakeDamage(float amount, Vector3 attackerPos)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Ölmediysek hasar alma animasyonunu oynat
            if (animator != null)
            {
                // Gelen saldırının yönünü hesapla (Vektör Matematiği)
                CalculatHitDirection(attackerPos);
                animator.SetTrigger("GetHit");
            }
        }
    }

    void CalculatHitDirection(Vector3 attackerPos)
    {
        // Saldıranın pozisyonunu oyuncunun yerel koordinatlarına çeviriyoruz
        // Bu sayede "Benim sağımda mı solumda mı?" sorusunun cevabını alırız.
        Vector3 incomingDir = attackerPos - transform.position;
        Vector3 localDir = transform.InverseTransformDirection(incomingDir);

        float xVal = 0;
        float zVal = 0;

        // localDir.x > 0 ise saldırı SAĞDAN gelmiştir
        // localDir.x < 0 ise saldırı SOLDAN gelmiştir
        // localDir.z > 0 ise saldırı ÖNDEN gelmiştir

        // Basit bir mantıkla en baskın yönü seçelim:
        if (Mathf.Abs(localDir.x) > Mathf.Abs(localDir.z))
        {
            // Yanlardan gelen darbe daha baskın
            if (localDir.x > 0) xVal = 1f; // Sağ
            else xVal = -1f; // Sol
        }
        else
        {
            // Önden veya arkadan gelen darbe (Şimdilik önden varsayalım)
            zVal = 1f;
        }

        // Animator parametrelerini güncelle
        if (animator != null)
        {
            animator.SetFloat("HitX", xVal);
            animator.SetFloat("HitZ", zVal);
        }
    }

    void Die()
    {
        if (isDead) return; // Zaten öldüyse tekrar çalışmasın

        Debug.Log("OYUNCU ÖLDÜ!");
        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Kılıcı düşür
        KiliciDuser();

        // 1. Ölüm Panelini Görünür Yap
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }

        // 2. Mouse İmlecini Serbest Bırak (Yeniden Başlat butonuna tıklayabilmek için)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // İstersen burada oyuncu hareketini kilitleyen kodu da çağırabilirsin
        // GetComponent<PlayerController>().enabled = false; gibi.
    }

    void KiliciDuser()
    {
        if (kilicRigidbody != null)
        {
            kilicRigidbody.transform.parent = null; // Kılıcı elden ayır
            kilicRigidbody.isKinematic = false;     // Fizik motorunu aç
            kilicRigidbody.useGravity = true;       // Yerçekimini aç

            BoxCollider col = kilicRigidbody.GetComponent<BoxCollider>();
            if (col != null) col.enabled = true;    // Collider'ı aç ki yere çarpsın

            kilicRigidbody.AddTorque(Random.insideUnitSphere * 5f); // Havalı bir düşüş efekti
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = currentHealth / maxHealth;
        }
    }
}