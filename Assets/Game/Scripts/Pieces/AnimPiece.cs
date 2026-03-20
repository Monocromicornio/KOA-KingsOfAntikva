using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.onlineobject.objectnet;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Piece))]
public class AnimPiece : NetworkBehaviour
{
    private MatchController matchController => MatchController.instance;
    private bool hasConnection => matchController.hasConnection;
    private SoundController soundController => matchController.soundController;
    private GameMode gameMode => matchController.gameMode;

    [Header("Animation")]
    [SerializeField]
    private Animator animator;
    public Animator anim { get; private set; }
    private List<Animator> lastAnims = new List<Animator>();

    [Header("Particle")]
    [SerializeField]
    private GameObject gDie;

    [Header("Sound")]
    [SerializeField]
    private AudioSource auDie;

    [SerializeField]
    private AudioSource auDown;

    private void Awake()
    {
        anim = animator;
    }

    public void SetAnimation(string animName)
    {
        if (hasConnection) NetworkExecute<string>(SetTrigger, animName);
        else SetTrigger(animName);
    }

    public void SetAnimation(string animName, bool value)
    {
        if (hasConnection) NetworkExecute<string, bool>(SetBool, animName, value);
        else SetBool(animName, value);
    }

    private void SetTrigger(string animName)
    {
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
    /// Triggers death animation and sounds immediately.
    /// All timing is controlled externally by InteractivePiece.deathAnimationDelay.
    /// </summary>
    private void DieEffect()
    {
        bool dieSoldier = gameMode.type == GameMode.GameType.Hard && tag == "Enemy";

        if (dieSoldier)
        {
            soundController.DieSoldier();
        }
        else
        {
            if (auDie) auDie.Play();
        }

        SetAnimation("Die", true);

        if (gDie != null)
        {
            Instantiate(gDie, transform.position, gDie.transform.rotation);
        }

        if (dieSoldier)
        {
            soundController.DownSoldier();
        }
        else
        {
            if (auDown) auDown.Play();
        }
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