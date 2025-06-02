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
        OpenChest();
        StartCoroutine(NotifyController(1));
    }

    public void OpenChest()
    {
        if (!bluePiece)
        {
            piece.SendMessage("Reveal");
            trunck.SetActive(true);
        }
        anim.SetAnimation("Open", true);

        particle.SetActive(true);
        soundController.VictoryConfirm();
    }

    private IEnumerator NotifyController(float time)
    {
        yield return new WaitForSeconds(time);
        matchController.OpenChest(this);
    }
}