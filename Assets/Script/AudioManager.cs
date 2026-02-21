using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource step,lvl1Boss,lvl2Boss,swordSlice;

    public static AudioManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayStepSound()
    {
        step.Play();
    }

    public void PlayLevel1BossSound()
    {
        lvl1Boss.Play();
    }

    public void PlayLevel2BossSound()
    {
        lvl2Boss.Play();
    }

    public void PlaySwordSliceSound()
    {
        swordSlice.Play();
    }
}
