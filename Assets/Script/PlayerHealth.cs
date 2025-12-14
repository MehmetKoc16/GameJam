using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Coroutine için gerekli

public class PlayerHealth : MonoBehaviour
{
    [Header("Can Ayarları")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;

    [Header("UI Bağlantısı")]
    public Image healthBarFill;

    // --- YENİ EKLENEN REFERANSLAR ---
    private HitReactor _hitReactor;      // Animasyon yönü için (Eski sistemin)
    private PlayerController _controller; // Hareketi durdurmak için (Yeni sistem)

    void Start()
    {
        // Scriptleri otomatik bul
        _hitReactor = GetComponent<HitReactor>();
        _controller = GetComponent<PlayerController>();

        if (_hitReactor == null) Debug.LogWarning("HitReactor bulunamadı (Animasyon tepkisi olmayabilir).");
        if (_controller == null) Debug.LogWarning("PlayerController bulunamadı (Karakter hasar alınca duraksamayabilir).");

        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void Update()
    {
        // TEST: "H" tuşu ile kendine hasar ver
        if (Input.GetKeyDown(KeyCode.H))
        {
            Vector3 fakeAttackerPos = transform.position + transform.forward * 2f; 
            TakeDamage(10f, fakeAttackerPos);
        }
    }

    public void TakeDamage(float damageAmount, Vector3 attackerPos)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // --- HASAR TEPKİSİ VE STUN ---
        
        // 1. Eğer HitReactor varsa animasyonu ona oynat (Senin eski sistemin)
        if (_hitReactor != null)
        {
            _hitReactor.HandleHitReaction(attackerPos);
        }

        // 2. Eğer HitReactor yoksa veya hareket kontrolü ondaysa bile, 
        // GARANTİ OLSUN DİYE buradan da kısa süreliğine hareketi kilitliyoruz.
        if (_controller != null)
        {
            StopAllCoroutines(); // Üst üste hasar yerse süreyi sıfırla
            StartCoroutine(StunPlayer(0.5f)); // 0.5 saniye don
        }
    }

    // Karakteri kısa süre dondurup sonra tekrar açan fonksiyon
    IEnumerator StunPlayer(float duration)
    {
        if (_controller != null) _controller.enabled = false; // Hareketi Kapat
        
        yield return new WaitForSeconds(duration); // Bekle
        
        // Eğer ölmediysek hareketi geri aç
        if (!isDead && _controller != null)
        {
            _controller.enabled = true; // Hareketi Aç (DONMAYI ÇÖZEN KISIM)
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
        
        // Ölünce hareketi tamamen kapat
        if (_controller != null) _controller.enabled = false;
        
        // Ölüm animasyonu
        if (_hitReactor != null)
        {
            _hitReactor.HandleDeath();
        }
    }
}