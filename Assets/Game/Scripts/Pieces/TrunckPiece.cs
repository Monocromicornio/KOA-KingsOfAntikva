using System.Collections;
using UnityEngine;

public class TrunckPiece : InteractivePiece
{
    public bool bluePiece { get; private set; }

    [SerializeField]
    private GameObject particle;

    protected override void Awake()
    {
        base.Awake();
        bluePiece = GetComponent<FakePiece>() == null;
        force = 0;
    }

    protected override void Notify(bool sucess, InteractivePiece target)
    {
        StartCoroutine(OpenChest());
    }

    public IEnumerator OpenChest()
    {
        //anim.SetBool("Open", true);

        particle.SetActive(true);
        soundController.VictoryConfirm();
        
        yield return new WaitForSeconds(1);
        matchController.OpenChest(this);
    }
}