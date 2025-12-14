using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    public int damageAmount = 100;

    void OnTriggerEnter(Collider other)
    {
        // Sadece BOSS_HITBOX Tag'ine sahip ana gövde Collider'ýna vurulduðunda hasar ver
        if (other.CompareTag("BOSS_HITBOX"))
        {
            BossAI boss = other.GetComponent<BossAI>();

            if (boss != null)
            {
                boss.TakeDamage(damageAmount);

                // Mermi/Yetenek objesi olduðu için yok et
                Destroy(gameObject);
            }
        }
    }
}