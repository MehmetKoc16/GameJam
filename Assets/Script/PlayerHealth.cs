using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Can Ayarlarý")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI Baðlantýsý")]
    public Image healthBarFill;

    // HitReactor scriptine referans (Animasyonlarý bu yönetecek)
    private HitReactor _hitReactor;

    void Start()
    {
        // Ayný obje üzerindeki HitReactor scriptini bul
        _hitReactor = GetComponent<HitReactor>();

        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void Update()
    {
        // TEST: "H" tuþuna basýnca kendine zarar ver
        if (Input.GetKeyDown(KeyCode.H))
        {
            // Test ederken "nereden vurdu?" sorusuna cevap vermemiz lazým.
            // Simülasyon: Tam karþýmýzda (transform.forward) duran biri vurmuþ gibi yapalým.
            // Böylece karakterin "Önden Darbe Alma" (Hit Back) animasyonuna girmesi gerekir.
            Vector3 fakeAttackerPos = transform.position + transform.forward;

            TakeDamage(10, fakeAttackerPos);
        }
    }

    // Hasar Alma Fonksiyonu GÜNCELLENDÝ: Artýk saldýranýn pozisyonunu da istiyor
    public void TakeDamage(float damageAmount, Vector3 attackerPos)
    {
        currentHealth -= damageAmount;

        // Can 0'ýn altýna düþmesin
        if (currentHealth < 0) currentHealth = 0;

        // UI'ý güncelle
        UpdateHealthUI();

        // --- YENÝ EKLENEN KISIM ---
        // Eðer ölmediysek ve HitReactor scripti varsa tepki ver
        if (currentHealth > 0 && _hitReactor != null)
        {
            _hitReactor.HandleHitReaction(attackerPos);
        }
        // --------------------------

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
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
        Debug.Log("ÖLDÜNÜZ!");
        // Buraya ragdoll açma veya ölüm animasyonu gelecek
        // GetComponent<Animator>().SetTrigger("Die"); gibi.
    }
}