using UnityEngine;

public class Mainsound : MonoBehaviour
{
    public AudioSource rainy,final_sound,campfire;

    public static Mainsound Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayRain()
    {
        rainy.Play();
    }

    public void PlayFire()
    {
        final_sound.Play();
    }

    public void PlayMainSound()
    {
        campfire.Play();
    }

    
}
