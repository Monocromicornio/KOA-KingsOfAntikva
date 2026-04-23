using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Plays a sound when the cursor hovers over this object.
/// Works with UI elements (requires EventSystem + Graphic Raycaster) and
/// 3D objects (requires a Collider + Physics Raycaster on the Camera).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class HoverSound : MonoBehaviour, IPointerEnterHandler
{
    [Header("Sound")]
    [Tooltip("Clip played on hover. If empty, uses the AudioSource's default clip.")]
    [SerializeField]
    private AudioClip hoverClip;

    [Tooltip("Minimum interval in seconds between hover sounds to avoid rapid repeats.")]
    [SerializeField]
    private float cooldown = 0.1f;

    private AudioSource audioSource;
    private float lastPlayTime;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// Plays the hover sound respecting the cooldown interval.
    /// </summary>
    private void PlayHoverSound()
    {
        if (Time.unscaledTime - lastPlayTime < cooldown) return;

        lastPlayTime = Time.unscaledTime;

        if (hoverClip != null)
        {
            audioSource.PlayOneShot(hoverClip);
        }
        else if (audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }

    /// <summary>
    /// Called by the EventSystem when the pointer enters a UI element.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayHoverSound();
    }

    /// <summary>
    /// Called by Unity when the mouse enters a 3D Collider.
    /// Requires a Physics Raycaster on the Camera for EventSystem integration,
    /// or simply a Collider on this GameObject.
    /// </summary>
    private void OnMouseEnter()
    {
        PlayHoverSound();
    }
}
