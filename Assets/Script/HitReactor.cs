using UnityEngine;

[RequireComponent(typeof(Animator))] // Bu scriptin olduðu yerde Animator olmak zorunda
public class HitReactor : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // Bu fonksiyon dýþarýdan (Can scriptinden veya Düþman silahýndan) çaðrýlacak
    public void HandleHitReaction(Vector3 attackerPosition)
    {
        // 1. Düþmanýn (veya hasar kaynaðýnýn) yönünü hesapla
        Vector3 directionToAttacker = attackerPosition - transform.position;
        directionToAttacker.y = 0; // Yükseklik farkýný önemseme

        // 2. Yönü karakterin local (yerel) koordinatlarýna çevir
        // Bu sayede "Sað", karakterin saðý olur; dünyanýn saðý deðil.
        Vector3 localDir = transform.InverseTransformDirection(directionToAttacker);
        localDir.Normalize();

        // 3. Animator parametrelerini güncelle
        _animator.SetFloat("HitX", localDir.x);
        _animator.SetFloat("HitZ", localDir.z);

        // 4. Trigger'ý çek ve animasyonu baþlat
        _animator.SetTrigger("GetHit");

        // Debug için konsola yönü yazdýralým (Test ettikten sonra silebilirsin)
        Debug.Log($"Darbe Yönü: {localDir} | X: {localDir.x}, Z: {localDir.z}");
    }
}