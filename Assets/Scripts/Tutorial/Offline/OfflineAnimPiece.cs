using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(OfflinePiece))]
[RequireComponent(typeof(AudioSource))]
public class OfflineAnimPiece : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField]
    private Animator animator;
    public Animator anim { get; private set; }
    private List<Animator> lastAnims = new List<Animator>();

    [Header("Particle")]
    [SerializeField]
    private GameObject gDie;

    [SerializeField]
    private GameObject endMoveParticles;

    [Header("Sound - Per Piece Clips")]
    [Tooltip("Sound played when this piece attacks.")]
    [SerializeField]
    private AudioClip attackClip;

    [Tooltip("Sound played when this piece dies.")]
    [SerializeField]
    private AudioClip dieClip;

    [Tooltip("Sound played when this piece finishes moving.")]
    [SerializeField]
    private AudioClip moveEndClip;

    private AudioSource audioSource;

    private void Awake()
    {
        anim = animator;
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Plays a one-shot AudioClip on this piece's AudioSource.
    /// </summary>
    private void PlayClip(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Plays the attack sound configured on this piece.
    /// </summary>
    public void PlayAttackSound()
    {
        PlayClip(attackClip);
    }

    /// <summary>
    /// Plays the movement-end sound and spawns end-move particles.
    /// </summary>
    public void PlayMoveEndSound()
    {
        PlayClip(moveEndClip);
        if (endMoveParticles != null)
        {
            Instantiate(endMoveParticles, transform.position, endMoveParticles.transform.rotation);
        }
    }

    public void SetAnimation(string animName)
    {
        SetTrigger(animName);
    }

    public void SetAnimation(string animName, bool value)
    {
        SetBool(animName, value);
    }

    private void SetTrigger(string animName)
    {
        if (anim != null)
        {
            anim.SetTrigger(animName);
        }
    }

    private void SetBool(string animName, bool value)
    {
        if (anim != null)
        {
            anim.SetBool(animName, value);
        }
    }

    public void ChangeAnim(Animator newAnim)
    {
        if (anim == null) return;
        var animState = anim.GetCurrentAnimatorStateInfo(0);
        newAnim.Play(animState.fullPathHash, 0, animState.normalizedTime);
        lastAnims.Add(anim);
        anim = newAnim;
    }

    public void ChangeAnim(GameObject newAnim)
    {
        Animator anim = newAnim.GetComponent<Animator>();
        if (anim == null) return;
        ChangeAnim(anim);
    }

    public void ChangetoOld()
    {
        if (lastAnims.Count == 0) return;
        ChangeAnim(lastAnims.Last());
    }

    private void Destroy()
    {
        PlayDieAnimation();
    }

    /// <summary>
    /// Triggers death animation and sounds immediately.
    /// All timing is controlled externally by OfflineInteractivePiece.deathAnimationDelay.
    /// </summary>
    public void PlayDieAnimation()
    {
        StartCoroutine(WaitForEndOfFrame(() => {
            PlayClip(dieClip);

            SetAnimation("Die", true);

            if (gDie != null)
            {
                Instantiate(gDie, transform.position, gDie.transform.rotation);
            }
        }));
    }

    public void Win()
    {
        StartCoroutine(WaitForEndOfFrame(() => {
            SetAnimation("Win", true);
        }));
    }

    IEnumerator WaitForEndOfFrame(UnityAction action)
    {
        yield return new WaitForEndOfFrame();
        action.Invoke();
    }
}
