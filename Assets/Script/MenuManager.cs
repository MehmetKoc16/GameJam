using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video; // Video iþlemleri için bu kütüphaneyi ekledik

public class MenuManager : MonoBehaviour
{
    [Header("Video ve Panel Ayarlarý")]
    public VideoPlayer introVideoPlayer; // Inspector'dan Video Player'ý sürükle
    public GameObject menuButtonsGroup;  // Inspector'dan 'Buttons' objesini sürükle
    public GameObject introVideoPanel;   // Inspector'dan 'IntroVideoPanel'i sürükle

    [Header("Sahne Ayarlarý")]
    public int oyunSahnesiIndexi = 1;    // Build Settings'teki oyun sahne sýrasý

    void Start()
    {
        // Oyun baþladýðýnda video bitiþ olayýný dinlemeye baþla
        if (introVideoPlayer != null)
        {
            introVideoPlayer.loopPointReached += VideoBitti;
        }
    }

    // --- GÝRÝÞ EKRANI (MAIN MENU SCENE) BUTONLARI ---

    public void OyunBasla()
    {
        // Eðer video player ve panel atanmýþsa videolu geçiþ yap
        if (introVideoPlayer != null && introVideoPanel != null)
        {
            menuButtonsGroup.SetActive(false); // Butonlarý gizle
            introVideoPanel.SetActive(true);   // Video ekranýný aç
            introVideoPlayer.Play();           // Videoyu oynat
        }
        else
        {
            // Eðer video ayarlanmamýþsa direkt oyuna gir (Hata vermesin)
            StartGameScene();
        }
    }

    // Video bitince bu fonksiyon otomatik çalýþýr
    void VideoBitti(VideoPlayer vp)
    {
        StartGameScene();
    }

    // Gerçek sahne yükleme iþlemi
    void StartGameScene()
    {
        SceneManager.LoadScene(oyunSahnesiIndexi);
        Time.timeScale = 1;
    }

    public void CikisYap()
    {
        Debug.Log("Oyundan cýkýldý");
        Application.Quit();
    }

    // --- ÖLÜM EKRANI (DEATH SCREEN) BUTONLARI ---

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1;
    }
}