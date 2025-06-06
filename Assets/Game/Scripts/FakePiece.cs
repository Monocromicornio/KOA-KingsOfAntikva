using UnityEngine;

public class FakePiece : Piece
{
    [Header("Fake Piece")]
    [SerializeField]
    GameObject fake;

    void Start()
    {
        myTurn = TurnState.red;
        if (fake == null) return;
        ActiveFakePiece();
    }

    public void SetFakeObj(GameObject fake)
    {
        this.fake = fake;
        ActiveFakePiece();
    }

    private void ActiveFakePiece()
    {
        fake.SetActive(false);
        Vector3 vector3 = new Vector3(body.transform.position.x, 0, body.transform.position.z);
        fake = Instantiate(fake, vector3, transform.rotation);
        fake.transform.parent = transform;

        body.SetActive(false);
        fake.SetActive(true);
        fake.transform.rotation = transform.rotation;

        AnimPiece anim = GetComponent<AnimPiece>();
        if (anim) anim.ChangeAnim(fake);
    }

    protected override void OnMouseDown()
    {
        if (!field.select) return;
        matchController.currentePiece?.SelectedAField(field);
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

        body.gameObject.SetActive(true);
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
    
    protected override void OnDestroy()
    {
        print("DESTROY " + name);
        field?.SetPiece(null);
        matchController?.RemovePieceFromEnemySquad(this);
    }
}