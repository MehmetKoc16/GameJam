using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MenuManager : MonoBehaviour
{
    [Header("Video ve Panel Ayarlarý")]
    public VideoPlayer introVideoPlayer;
    public GameObject menuButtonsGroup;
    public GameObject introVideoPanel;

    [Header("Efekt Ayarlarý")] // YENÝ: Efektleri buraya sürükleyeceðiz
    public GameObject[] menuEffects; // Ateþ, Duman, Yaðmur objelerini buraya at

    [Header("Sahne Ayarlarý")]
    public int oyunSahnesiIndexi = 1;

    void Start()
    {
        if (introVideoPlayer != null)
        {
            introVideoPlayer.loopPointReached += VideoBitti;
        }
    }

    // --- GÝRÝÞ EKRANI (MAIN MENU SCENE) BUTONLARI ---

    public void OyunBasla()
    {
        if (introVideoPlayer != null && introVideoPanel != null)
        {
            // 1. Butonlarý gizle
            menuButtonsGroup.SetActive(false);

            // 2. Efektleri kapat (YENÝ KISIM)
            // Listeye eklediðin tüm efektleri (ateþ, duman, yaðmur) kapatýr
            if (menuEffects != null)
            {
                foreach (GameObject efekt in menuEffects)
                {
                    if (efekt != null)
                    {
                        efekt.SetActive(false);
                    }
                }
            }

            // 3. Video panelini aç ve oynat
            introVideoPanel.SetActive(true);
            introVideoPlayer.Play();
        }
        else
        {
            StartGameScene();
        }
    }

    void VideoBitti(VideoPlayer vp)
    {
        StartGameScene();
    }

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
    public void VideoyuGec()
    {
        // Videoyu durdur (Ses arkada kalmasýn diye garanti olsun)
        if (introVideoPlayer != null)
        {
            introVideoPlayer.Stop();
        }

        // Direkt oyunu baþlatma fonksiyonunu çaðýr
        StartGameScene();
    }
}