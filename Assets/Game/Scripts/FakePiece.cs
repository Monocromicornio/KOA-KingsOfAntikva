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
    GameObject fakePiece;

    private void Awake()
    {
        piece = GetComponent<Piece>();
    }

    private void OnEnable()
    {
        if (fake == null)
        {
            if (fakePiece == null)
            {
                PlayerSquad squad = matchController.playerSquad;
                PieceData pieceData = squad.pieceData;
                fakePiece = pieceData.fakePiece;
            }
            fake = Instantiate(fakePiece, transform.position, transform.rotation, transform);
        }
        ActiveFakePiece();
    }

    private void OnDisable()
    {
        ReturnToNormal();
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
        if (field == null || !field.select) return;
        matchController.currentePiece?.SelectedAField(field);
    }

    private void ReturnToNormal()
    {
        body?.SetActive(true);
        FixEngineBug();
        AnimPiece anim = GetComponent<AnimPiece>();
        if (anim) anim.ChangetoOld();
    }

    private void FixEngineBug()
    {
        if (fakePiece == null) return;
        
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.name.Contains(fakePiece.name))
            {
                child.SetActive(false);
            }
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
        ReturnToNormal();
    }

    private void OnDestroy()
    {
        ReturnToNormal();
        matchController?.RemovePieceFromEnemySquad(this);
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