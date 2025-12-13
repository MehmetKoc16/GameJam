using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Can Ayarları")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false; // Ölüm durumunu ekledik

    [Header("UI Bağlantısı")]
    public Image healthBarFill;

    // HitReactor scriptine referans (Animasyonları bu yönetecek)
    private HitReactor _hitReactor;

    void Start()
    {
        // Aynı obje üzerindeki HitReactor scriptini bul
        _hitReactor = GetComponent<HitReactor>();

        if (_hitReactor == null)
        {
            Debug.LogWarning("PlayerHealth: HitReactor scripti bu objede bulunamadı. Hasar tepkileri çalışmayacaktır.");
        }

        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void Update()
    {
        // TEST: "H" tuşuna basınca kendine zarar ver
        if (Input.GetKeyDown(KeyCode.H))
        {
            // Simülasyon: Tam karşımızda duran biri vurmuş gibi yapalım.
            Vector3 fakeAttackerPos = transform.position + transform.forward * 2f; 

            TakeDamage(10f, fakeAttackerPos);
        }
    }

    /// <summary>
    /// Oyuncuya hasar verir ve tepki sistemini tetikler.
    /// </summary>
    /// <param name="damageAmount">Alınacak hasar miktarı.</param>
    /// <param name="attackerPos">Saldıranın dünya pozisyonu (Tepki animasyonu için kullanılır).</param>
    public void TakeDamage(float damageAmount, Vector3 attackerPos)
    {
        if (isDead) return; // Zaten ölmüşsek işlem yapma

        currentHealth -= damageAmount;

        // Can 0'ın altına düşmesin
        if (currentHealth < 0) currentHealth = 0;

        // UI'ı güncelle
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // --- Hasar Tepkisi ---
        // Eğer ölmediysek ve HitReactor scripti varsa tepki ver
        if (_hitReactor != null)
        {
            _hitReactor.HandleHitReaction(attackerPos);
        }
    }

    public void Heal(float healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        
        Debug.Log("OYUNCU ÖLDÜ!");
        
        // Ölüm durumunda hareketi durdur, ragdoll/animasyon tetikle
        // Örn: if (GetComponent<PlayerController>() != null) GetComponent<PlayerController>().enabled = false;
        
        if (_hitReactor != null)
        {
            _hitReactor.HandleDeath(); // Ölüm animasyonunu HitReactor'a devret
        }
    }
}