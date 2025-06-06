using UnityEngine;

[RequireComponent(typeof(Piece))]
public class FakePiece : MonoBehaviour
{
    private MatchController matchController => MatchController.instance;
    private GameMode.GameType gameType => matchController.gameType;

    public Piece piece { get; private set; }
    GameObject body => piece.body;
    GameField field => piece.field;
    PieceType type => piece.type;

    [Header("Fake Piece")]
    [SerializeField]
    GameObject fake;

    private void Awake()
    {
        piece = GetComponent<Piece>();
    }

    private void Start()
    {
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
        if (!body.activeSelf) return;

        Vector3 vector3 = new Vector3(body.transform.position.x, 0, body.transform.position.z);
        fake = Instantiate(fake, vector3, transform.rotation);
        fake.transform.parent = transform;

        body.SetActive(false);
        fake.SetActive(true);
        fake.transform.rotation = transform.rotation;

        AnimPiece anim = GetComponent<AnimPiece>();
        if (anim) anim.ChangeAnim(fake);
    }

    private void OnMouseDown()
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

    private void Destroy()
    {
        Reveal();
    }

    private void Win()
    {
        Reveal();
    }
}