using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Piece))]
public class TrunckPiece : InteractivePiece
{
    public bool bluePiece { get; private set; }
    private GameObject trunck => piece.body;

    [Header("Trunck")]
    [SerializeField]
    private GameObject particle;

    private Animator animator => anim.anim;
    public bool opened = false;

    protected override void Awake()
    {
        base.Awake();
        force = int.MaxValue;
    }

    private void Start()
    {
        bluePiece = GetComponent<FakePiece>() == null;
        if (!bluePiece) trunck.SetActive(false);
    }

    protected override void CounterAttack(InteractivePiece target)
    {
        OpenChest();
        matchController.ChangeTurn();
    }

    public void OpenChest()
    {
        if (!bluePiece)
        {
            piece.SendMessage("Reveal");
            trunck.SetActive(true);
        }
        opened = true;

        anim.SetAnimation("Open", true);

        particle.SetActive(true);
        soundController.VictoryConfirm();
    }
}