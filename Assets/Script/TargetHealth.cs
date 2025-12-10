using UnityEngine;
using UnityEngine.UI;

public class TargetHealth : MonoBehaviour
{
    [Header("Ayarlar")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    [Header("Baðlantýlar")]
    public Image healthBarImage;
    public Animator animator;

    [Header("Silah Ayarlarý")]
    // YENÝ: Kýlýcýn Rigidbody'sini buraya baðlayacaðýz
    public Rigidbody kilicRigidbody;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
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

        // YENÝ: Kýlýcý düþürme fonksiyonunu çaðýr
        KiliciDuser();
    }

    // YENÝ: Kýlýcý elden ayýran ve fiziði baþlatan fonksiyon
    void KiliciDuser()
    {
        if (kilicRigidbody != null)
        {
            // 1. Kýlýcý karakterden ayýr (Parent'ý sil)
            kilicRigidbody.transform.parent = null;

            // 2. Fiziði aç (Kinematik kapat)
            kilicRigidbody.isKinematic = false;

            // 3. YERÇEKÝMÝNÝ AÇ (Bunu eklemeyi unutmuþuz!)
            kilicRigidbody.useGravity = true;

            // 4. Collider'ý aç
            BoxCollider col = kilicRigidbody.GetComponent<BoxCollider>();
            if (col != null) col.enabled = true;

            // 5. Çok hafif bir hareket ver (Daha doðal düþsün diye)
            kilicRigidbody.AddTorque(Random.insideUnitSphere * 1f);
        }
    }

    void SetDamageDirection(string direction)
    {
        float xVal = 0;
        float zVal = 0;

        switch (direction)
        {
            case "On":  // Önden saldýrý
                zVal = 1f;
                break;
            case "Sag": // Saðdan saldýrý
                xVal = 1f;
                break;
            case "Sol": // Soldan saldýrý
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