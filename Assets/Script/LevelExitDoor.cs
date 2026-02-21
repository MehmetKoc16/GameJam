using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExitDoor : MonoBehaviour
{
    [Header("Ayarlar")]
    public string finalSceneName = "FinalSahnesi"; 
    
    // DİKKAT: Artık GameObject değil, direkt BossAI scriptini istiyoruz
    public BossAI bossScript; 

    private bool isPlayerNear = false; 

    void Update()
    {
        // Oyuncu kapıdaysa ve E'ye bastıysa
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            TryOpenDoor();
        }
    }

    void TryOpenDoor()
    {
        // KONTROL: Boss scripti var mı VE Boss'un 'isDead' değişkeni true mu?
        if (bossScript != null && bossScript.isDead == true)
        {
            Debug.Log("Boss ölü. Kapı açılıyor...");
            LoadFinalLevel();
        }
        else
        {
            Debug.Log("Kapı kilitli! Boss hala hayatta.");
        }
    }

    void LoadFinalLevel()
    {
        SceneManager.LoadScene(finalSceneName);
    }

    // --- TETİKLEYİCİLER ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            // Ekrana "Çıkmak için E'ye bas" yazdırabilirsin
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}