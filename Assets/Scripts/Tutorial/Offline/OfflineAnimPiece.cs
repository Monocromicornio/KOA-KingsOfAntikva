using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(OfflinePiece))]
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

    public void PlayDieAnimation()
    {
        StartCoroutine(WaitForEndOfFrame(() => {
            SetAnimation("Die", true);
            StartCoroutine(DieEffect());
        }));
    }

    private IEnumerator DieEffect()
    {
        yield return new WaitForSeconds(0.5f);

        if (auDie) auDie.Play();

        yield return new WaitForSeconds(2);
        
        if (gDie != null)
        {
            Instantiate(gDie, transform.position, gDie.transform.rotation);
        }
        
        if (auDown) auDown.Play();
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
