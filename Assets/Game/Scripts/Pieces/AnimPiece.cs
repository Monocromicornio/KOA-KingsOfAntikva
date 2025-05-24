using UnityEngine;

[RequireComponent(typeof(Piece))]
public class AnimPiece : MonoBehaviour
{
    private Piece piece;

    [SerializeField]
    private Animator animator;
    public Animator anim { get; private set; }

    private void Awake()
    {
        anim = animator;
        piece = GetComponent<Piece>();
    }

    public void SetAnimation(string animName)
    {
        anim.SetTrigger(animName);
    }

    public void SetAnimation(string animName, bool value)
    {
        anim.SetBool(animName, value);
    }

    public void ChangeAnim(Animator newAnim)
    {
        anim = newAnim;
    }

    public void ChangeAnim(GameObject newAnim)
    {
        Animator anim = newAnim.GetComponent<Animator>();
        if (anim == null) return;
        ChangeAnim(anim);
    }
}