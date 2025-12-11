using UnityEngine;
using UnityEngine.SceneManagement; // Sahne deðiþimi için þart

public class MenuManager : MonoBehaviour
{
    // Butona bu fonksiyonu baðlayacaðýz
    public void OyunaBasla()
    {
        // "1" numaralý sahneyi yükle (Birazdan ayarlayacaðýz)
        SceneManager.LoadScene(1);
    }

    public void CikisYap()
    {
        Debug.Log("Oyundan Çýkýldý!");
        Application.Quit();
    }
}