using UnityEngine;

public class DamageTester : MonoBehaviour
{
    [Header("Test Ayarlar�")]
    public KeyCode saldiriTusu = KeyCode.O;
    public float hasarMiktari = 10f;

    [Header("Fizik Ayarlar�")]
    public float menzil = 5f; // I��n ne kadar uza�a gitsin?
    public LayerMask hedefKatmani; // Sadece 'Player' layer'�na sahip objelere �arps�n

    void Update()
    {
        // Debug i�in sahnede k�rm�z� bir lazer �izgisi �izelim (Sadece Scene ekran�nda g�r�n�r)
        Debug.DrawRay(transform.position, transform.forward * menzil, Color.red);

        if (Input.GetKeyDown(saldiriTusu))
        {
            FizikselSaldir();
        }
    }

    void FizikselSaldir()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, menzil, hedefKatmani))
        {
            // Debug: Neye �arpt���m�z� g�relim
            Debug.Log("Lazer �una �arpt�: " + hit.transform.name);

            // D�ZELTME: GetComponent yerine GetComponentInParent kullan�yoruz.
            // Bu, "�arpt���m par�ada script yoksa, ba�l� oldu�u ana objeye bak" demektir.
            TargetHealth hedefCan = hit.transform.GetComponentInParent<TargetHealth>();

            if (hedefCan != null)
            {
                string hesaplananYon = YonHesapla(hit.transform);
                Debug.Log($"<color=green>VURDU!</color> Mesafe: {hit.distance} | Y�n: {hesaplananYon}");
                hedefCan.TakeDamage(hasarMiktari, hesaplananYon);
            }
            else
            {
                // E�er script bulunamazsa bunu da yazd�ral�m ki bilelim
                Debug.LogWarning("Bir �eye �arpt�m ama �zerinde 'TargetHealth' scripti yok! �arpt���m �ey: " + hit.transform.name);
            }
        }
        else
        {
            Debug.Log("<color=yellow>ISKA!</color> Menzilde veya a��da hedef yok.");
        }
    }

    // Bu fonksiyon Tester'�n, Oyuncunun neresinde durdu�unu bulur
    string YonHesapla(Transform oyuncu)
    {
        // Tester'�n pozisyonunu, Oyuncunun koordinat sistemine �evir
        // Bu i�lem bize "Oyuncuya g�re ben neredeyim?" sorusunun cevab�n� verir.
        Vector3 yerelKonum = oyuncu.InverseTransformPoint(transform.position);

        // yerelKonum.z -> �leri (+) / Geri (-)
        // yerelKonum.x -> Sa� (+) / Sol (-)

        // �nce Z eksenine (�n/Arka) bakal�m
        // E�er Z de�eri, X de�erinin mutlak halinden b�y�kse, bask�n y�n �N veya ARKA'd�r.
        if (Mathf.Abs(yerelKonum.z) > Mathf.Abs(yerelKonum.x))
        {
            if (yerelKonum.z > 0)
                return "On";   // Oyuncunun �n�ndeyiz
            else
                return "Arka"; // Oyuncunun arkas�nday�z (Gerekirse eklersin)
        }
        else // De�ilse bask�n y�n SA� veya SOL'dur
        {
            if (yerelKonum.x > 0)
                return "Sag";  // Oyuncunun sa��nday�z
            else
                return "Sol";  // Oyuncunun solunday�z
        }
    }
}