using UnityEngine;

public class DamageTester : MonoBehaviour
{
    [Header("Test Ayarlarý")]
    public KeyCode saldiriTusu = KeyCode.Space;
    public float hasarMiktari = 10f;

    [Header("Fizik Ayarlarý")]
    public float menzil = 5f; // Iþýn ne kadar uzaða gitsin?
    public LayerMask hedefKatmani; // Sadece 'Player' layer'ýna sahip objelere çarpsýn

    void Update()
    {
        // Debug için sahnede kýrmýzý bir lazer çizgisi çizelim (Sadece Scene ekranýnda görünür)
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
            // Debug: Neye çarptýðýmýzý görelim
            Debug.Log("Lazer þuna çarptý: " + hit.transform.name);

            // DÜZELTME: GetComponent yerine GetComponentInParent kullanýyoruz.
            // Bu, "Çarptýðým parçada script yoksa, baðlý olduðu ana objeye bak" demektir.
            TargetHealth hedefCan = hit.transform.GetComponentInParent<TargetHealth>();

            if (hedefCan != null)
            {
                string hesaplananYon = YonHesapla(hit.transform);
                Debug.Log($"<color=green>VURDU!</color> Mesafe: {hit.distance} | Yön: {hesaplananYon}");
                hedefCan.TakeDamage(hasarMiktari, hesaplananYon);
            }
            else
            {
                // Eðer script bulunamazsa bunu da yazdýralým ki bilelim
                Debug.LogWarning("Bir þeye çarptým ama üzerinde 'TargetHealth' scripti yok! Çarptýðým þey: " + hit.transform.name);
            }
        }
        else
        {
            Debug.Log("<color=yellow>ISKA!</color> Menzilde veya açýda hedef yok.");
        }
    }

    // Bu fonksiyon Tester'ýn, Oyuncunun neresinde durduðunu bulur
    string YonHesapla(Transform oyuncu)
    {
        // Tester'ýn pozisyonunu, Oyuncunun koordinat sistemine çevir
        // Bu iþlem bize "Oyuncuya göre ben neredeyim?" sorusunun cevabýný verir.
        Vector3 yerelKonum = oyuncu.InverseTransformPoint(transform.position);

        // yerelKonum.z -> Ýleri (+) / Geri (-)
        // yerelKonum.x -> Sað (+) / Sol (-)

        // Önce Z eksenine (Ön/Arka) bakalým
        // Eðer Z deðeri, X deðerinin mutlak halinden büyükse, baskýn yön ÖN veya ARKA'dýr.
        if (Mathf.Abs(yerelKonum.z) > Mathf.Abs(yerelKonum.x))
        {
            if (yerelKonum.z > 0)
                return "On";   // Oyuncunun önündeyiz
            else
                return "Arka"; // Oyuncunun arkasýndayýz (Gerekirse eklersin)
        }
        else // Deðilse baskýn yön SAÐ veya SOL'dur
        {
            if (yerelKonum.x > 0)
                return "Sag";  // Oyuncunun saðýndayýz
            else
                return "Sol";  // Oyuncunun solundayýz
        }
    }
}