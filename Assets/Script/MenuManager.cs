using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // --- GÝRÝÞ EKRANI (MAIN MENU SCENE) BUTONLARI ---

    public void OyunBasla()
    {
        // Oyun sahnesini yükler (Build Settings'te 1. sýrada olduðunu varsayýyoruz)
        SceneManager.LoadScene(1);
        Time.timeScale = 1; // Oyunun akýþýný garantiye almak için
    }

    public void CikisYap()
    {
        Debug.Log("Oyundan cýkýldý");
        Application.Quit(); // Oyunu gerçekten kapatmak için
    }

    // --- ÖLÜM EKRANI (DEATH SCREEN) BUTONLARI ---

    // Bunu 'REPLY' butonuna baðla
    public void RestartGame()
    {
        // Þu an hangi sahne açýksa onu baþtan yükler
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // Karakter ölünce zamaný durdurduysak tekrar akmasýný saðlar
        Time.timeScale = 1;
    }

    // Bunu 'MAIN MENU' butonuna baðla
    public void LoadMainMenu()
    {
        // Ana menüye döner (Genelde Build Settings'te 0. sýradadýr)
        SceneManager.LoadScene(0);
        Time.timeScale = 1;
    }
}