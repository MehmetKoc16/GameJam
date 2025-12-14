using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMainMenu : MonoBehaviour
{
    [Header("Ayarlar")]
    public float beklemeSuresi = 5.0f;
    public string yuklenecekSahneAdi = "MainMenu";

    // Start yerine OnEnable kullanýyoruz.
    // Bu fonksiyon, obje her "SetActive(true)" yapýldýðýnda otomatik çalýþýr.
    void OnEnable()
    {
        // Önceki sayaçlarý temizle (Garanti olsun)
        StopAllCoroutines();
        // Yeni sayacý baþlat
        StartCoroutine(SahneYukle());
    }

    // Eðer obje süre bitmeden kapanýrsa sayacý durdurmak için:
    void OnDisable()
    {
        StopAllCoroutines();
    }

    System.Collections.IEnumerator SahneYukle()
    {
        Debug.Log($"Sayaç baþladý: {beklemeSuresi} saniye sonra {yuklenecekSahneAdi} yüklenecek.");

        yield return new WaitForSeconds(beklemeSuresi);

        SceneManager.LoadScene(yuklenecekSahneAdi);
    }
}