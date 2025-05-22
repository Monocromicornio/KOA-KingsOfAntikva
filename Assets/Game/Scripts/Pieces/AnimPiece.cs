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

    public void SetAnimation(string AnimName, bool bstatus)
    {
        anim.SetBool(AnimName, bstatus);
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