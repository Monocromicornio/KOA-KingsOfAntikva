using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    [Header("Tracks")]
    [Tooltip("Intro track played once at the start. Leave empty to skip straight to the loop.")]
    [SerializeField]
    private AudioClip introTrack;

    [Tooltip("Main track that loops indefinitely after the intro finishes.")]
    [SerializeField]
    private AudioClip loopTrack;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (introTrack != null)
        {
            PlayIntro();
        }
        else if (loopTrack != null)
        {
            PlayLoop();
        }
    }

    /// <summary>
    /// Plays the intro track once, then transitions to the loop track when it ends.
    /// </summary>
    private void PlayIntro()
    {
        audioSource.clip = introTrack;
        audioSource.loop = false;
        audioSource.Play();
        StartCoroutine(WaitForIntroEnd());
    }

    /// <summary>
    /// Starts the loop track in continuous loop mode.
    /// </summary>
    private void PlayLoop()
    {
        if (loopTrack == null) return;

        audioSource.clip = loopTrack;
        audioSource.loop = true;
        audioSource.Play();
    }

    /// <summary>Stops all music playback and cancels any pending coroutines.</summary>
    public void StopMusic()
    {
        StopAllCoroutines();
        audioSource.Stop();
    }

    private IEnumerator WaitForIntroEnd()
    {
        // Wait until the intro track finishes playing
        while (audioSource.isPlaying)
        {
            yield return null;
        }

        PlayLoop();
    }
}
