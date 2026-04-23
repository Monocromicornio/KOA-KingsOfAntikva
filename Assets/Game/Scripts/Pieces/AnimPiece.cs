using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.onlineobject.objectnet;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Piece))]
[RequireComponent(typeof(AudioSource))]
public class AnimPiece : NetworkBehaviour
{
    private MatchController matchController => MatchController.instance;
    private bool hasConnection => matchController.hasConnection;
    private SoundController soundController => matchController.soundController;

    [Header("Animation")]
    [SerializeField]
    private Animator animator;
    public Animator anim { get; private set; }
    private List<Animator> lastAnims = new List<Animator>();

    [Header("Particle")]
    [SerializeField]
    private GameObject gDie;

    [Header("Sound - Per Piece Clips")]
    [Tooltip("Sound played when this piece attacks.")]
    [SerializeField]
    private AudioClip attackClip;

    [Tooltip("Sound played when this piece dies.")]
    [SerializeField]
    private AudioClip dieClip;

    //[Tooltip("Sound played when this piece falls/collapses after dying.")]
   // [SerializeField]
    //private AudioClip downClip;

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
    /// Plays the movement-end sound configured on this piece.
    /// </summary>
    public void PlayMoveEndSound()
    {
        PlayClip(moveEndClip);
    }

    public void SetAnimation(string animName)
    {
        if (hasConnection) NetworkExecute<string>(SetTrigger, animName);
        else SetTrigger(animName);
    }

    public void SetAnimation(string animName, bool value)
    {
        Debug.Log("Setting animation bool " + animName + " to value " + value + " on piece " + gameObject.name);
        if (hasConnection) NetworkExecute<string, bool>(SetBool, animName, value);
        else SetBool(animName, value);
    }

    private void SetTrigger(string animName)
    {
        Debug.Log("Calling Trigger " + animName + " on piece " + gameObject.name);
        anim.SetTrigger(animName);
    }

    private void SetBool(string animName, bool value)
    {
        anim.SetBool(animName, value);
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
        Animator anim = newAnim.GetComponentInChildren<Animator>();
        if (anim == null) return;
        ChangeAnim(anim);
    }

    public void ChangetoOld()
    {
        if (lastAnims.Count == 0) return;
        ChangeAnim(lastAnims.Last());
    }

    private bool isDying = false;

    private void Destroy()
    {
        if (isDying) return;
        isDying = true;
        StartCoroutine(WaitForEndOfFrame(() => { DieEffect(); }));
    }

    /// <summary>
    /// Triggers death animation and sounds immediately using this piece's own clips.
    /// All timing is controlled externally by InteractivePiece.deathAnimationDelay.
    /// </summary>
    private void DieEffect()
    {
        PlayClip(dieClip);

        SetAnimation("Die", true);

        if (gDie != null)
        {
            Instantiate(gDie, transform.position, gDie.transform.rotation);
        }

       // PlayClip(downClip);
    }

    public void Win()
    {
        StartCoroutine(WaitForEndOfFrame(() => {
            SetAnimation("Win", true);
            soundController.VictoryPeaple();
        }));
    }

    IEnumerator WaitForEndOfFrame(UnityAction action)
    {
        yield return new WaitForEndOfFrame();
        action.Invoke();
    }
}