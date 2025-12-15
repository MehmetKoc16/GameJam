using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMainMenu : MonoBehaviour
{
    [Header("Ayarlar")]
    public float beklemeSuresi = 5.0f;
    public string yuklenecekSahneAdi = "MainMenu";

    void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(SahneYukle());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    System.Collections.IEnumerator SahneYukle()
    {
        Debug.Log($"Sayaç baþladý: {beklemeSuresi} saniye sonra {yuklenecekSahneAdi} yüklenecek.");

        yield return new WaitForSeconds(beklemeSuresi);

        // --- DÜZELTME BURADA ---
        // Mouse'u serbest býrak (Merkeze kilitli kalmasýn)
        Cursor.lockState = CursorLockMode.None;

        // Mouse imlecini görünür yap
        Cursor.visible = true;
        // -----------------------

        SceneManager.LoadScene(yuklenecekSahneAdi);
    }
}