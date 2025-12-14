using UnityEngine;
using UnityEngine.UI;

public class TargetHealth : MonoBehaviour
{
    [Header("Ayarlar")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    [Header("UI Baglantilari")]
    public Image healthBarImage;

    [Header("Ölüm Ekranı")] // --- YENİ EKLENEN KISIM ---
    public GameObject deathPanel; // Unity Inspector'da buraya Siyah Paneli sürükle

    [Header("Animasyon ve Fizik")]
    public Animator animator;
    public Rigidbody kilicRigidbody; // Kılıcın Rigidbody'si

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

    public void TakeDamage(float amount, string direction)
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
            if (animator != null)
            {
                SetDamageDirection(direction);
                animator.SetTrigger("GetHit");
            }
        }
    }

    void Die()
    {
        Debug.Log("Karakter Öldü!");
        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Kılıcı düşürme fonksiyonunu çağır
        KiliciDuser();

        // --- YENİ EKLENEN KISIM: Panel Açma ve Mouse ---

        // 1. Ölüm Panelini Görünür Yap
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }

        // 2. Mouse İmlecini Serbest Bırak ve Göster (Tıklama yapabilmek için)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Kılıcı elden ayıran ve fiziği başlatan fonksiyon
    void KiliciDuser()
    {
        if (kilicRigidbody != null)
        {
            // 1. Kılıcı karakterden ayır (Parent'ı sil)
            kilicRigidbody.transform.parent = null;

            // 2. Fiziği aç (Kinematik kapat)
            kilicRigidbody.isKinematic = false;

            // 3. Yerçekimini aç
            kilicRigidbody.useGravity = true;

            // 4. Collider'ı aç
            BoxCollider col = kilicRigidbody.GetComponent<BoxCollider>();
            if (col != null) col.enabled = true;

            // 5. Çok hafif bir hareket ver (Daha doğal düşsün diye)
            kilicRigidbody.AddTorque(Random.insideUnitSphere * 1f);
        }
    }

    void SetDamageDirection(string direction)
    {
        float xVal = 0;
        float zVal = 0;

        switch (direction)
        {
            case "On":  // Önden saldırı
                zVal = 1f;
                break;
            case "Sag": // Sağdan saldırı
                xVal = 1f;
                break;
            case "Sol": // Soldan saldırı
                xVal = -1f;
                break;
        }

        if (animator != null)
        {
            animator.SetFloat("HitX", xVal);
            animator.SetFloat("HitZ", zVal);
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