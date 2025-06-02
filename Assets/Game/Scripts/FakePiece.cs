using UnityEngine;

public class FakePiece : Piece
{
    [Header("Fake Piece")]
    [SerializeField]
    GameObject obj;

    [SerializeField]
    GameObject fake;

    void Start()
    {
        fake.SetActive(false);
        Vector3 vector3 = new Vector3(obj.transform.position.x, 0, obj.transform.position.z);
        fake = Instantiate(fake, vector3, transform.rotation);
        fake.transform.parent = transform;

        obj.SetActive(false);
        fake.SetActive(true);
        fake.transform.rotation = transform.rotation;

        AnimPiece anim = GetComponent<AnimPiece>();
        if(anim) anim.ChangeAnim(fake);
    }

    protected override void OnMouseDown()
    {
        if (!matchController.isBlueTurn) return;

        if (matchController.currentePiece != null)
        {
            matchController.currentePiece.SelectedAField(field);
        }
    }

    public void Reveal()
    {
        switch (gameType)
        {
            //In normal not reveal soldiers
            case GameMode.GameType.Normal:
                if (type == PieceType.Soldier) return;
                break;
            //in hard mode only revel flags and bombs
            case GameMode.GameType.Hard:
                if (type != PieceType.Flag && type != PieceType.Bomb) return;
                break;
        }

        obj.gameObject.SetActive(true);
        fake.SetActive(false);
        AnimPiece anim = GetComponent<AnimPiece>();
        if (anim) anim.ChangetoOld();
    }

    protected new void Destroy()
    {
        Reveal();
        base.Destroy();
    }

    public new void Win()
    {
        //base.Win();
        Reveal();
    }

    public override void SetFirstField(GameField field)
    {
        base.SetFirstField(field);
        transform.Rotate(0, 180, 0, Space.Self);
    }
}