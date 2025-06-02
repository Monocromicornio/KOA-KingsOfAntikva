using System.Collections;
using UnityEngine;

public class TrunckPiece : InteractivePiece
{
    public bool bluePiece { get; private set; }

    [Header("Trunck")]
    [SerializeField]
    private GameObject trunck;
    [SerializeField]
    private GameObject particle;

    protected override void Awake()
    {
        base.Awake();
        force = int.MaxValue;
        bluePiece = GetComponent<FakePiece>() == null;
        if (!bluePiece) trunck.SetActive(false);
    }

    protected override void CounterAttack(InteractivePiece target)
    {
        StartCoroutine(OpenChest());
    }

    public IEnumerator OpenChest()
    {
        if (!bluePiece)
        {
            piece.SendMessage("Reveal");
            trunck.SetActive(true);
        }
        anim.SetAnimation("Open", true);

        particle.SetActive(true);
        soundController.VictoryConfirm();

        yield return new WaitForSeconds(1);
        matchController.OpenChest(this);
    }
}