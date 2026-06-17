using UnityEngine;

public class SoundController : MonoBehaviour
{
    [Header("Music")]
    [SerializeField]
    AudioSource music;

    [Header("UI")]
    [SerializeField]
    AudioSource select;
    [SerializeField]
    AudioSource cancel;
        

    public void Cancel()
    {
        cancel.Play();
    }

    public void Select()
    {
        select.Play();
    }

    /// <summary>Stops the background music.</summary>
    public void StopMusic()
    {
        if (music != null && music.isPlaying)
            music.Stop();
    }
    
}
