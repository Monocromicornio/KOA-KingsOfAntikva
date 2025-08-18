using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Piece))]
public class FakePiece : MonoBehaviour
{
    private MatchController matchController => MatchController.instance;
    private GameMode.GameType gameType => matchController.gameType;

    AnimPiece anim;
    public Piece piece { get; private set; }
    GameObject body => piece.body;
    GameField field => piece.field;

      private bool registered;

    [Header("Fake Piece")]
    [SerializeField]
    GameObject fake;

    private void Awake()
    {
        piece = GetComponent<Piece>();
        anim = GetComponent<AnimPiece>();        
    }

    private void Start()
    {
        if (matchController != null)
        {
            matchController.AddPieceFromEnemySquad(this);
            registered = true;
        }
        else
        {
            Debug.LogWarning("MatchController not ready for FakePiece registration.", this);
        }
    }

    private void OnEnable()
    {
        if (!registered)
        {
            if (matchController != null)
            {
                matchController.AddPieceFromEnemySquad(this);
                registered = true;
            }
            else
            {
                StartCoroutine(TryRegisterAgain());
            }
        }

        if (matchController == null) return;

        if (fake == null)
        {
            PlayerSquad squad = matchController.playerSquad;
            PieceData pieceData = squad.pieceData;
            fake = Instantiate(pieceData.fakePiece, transform.position, transform.rotation, transform);
        }
        ActiveFakePiece();
            
    }

     IEnumerator TryRegisterAgain()
    {
        while (!registered && matchController == null)
        {
            yield return null;
        }

        if (!registered && matchController != null)
        {
            matchController.AddPieceFromEnemySquad(this);
            registered = true;
        }
    }

    private void OnDisable()
    {
        ReturnToNormal();
    }

    private void ActiveFakePiece()
    {
        if (!body.activeSelf) return;

        Vector3 vector3 = new Vector3(body.transform.position.x, 0, body.transform.position.z);
        fake.transform.position = vector3;
        fake.transform.rotation = transform.rotation;

        body.SetActive(false);
        fake.SetActive(true);
        fake.transform.rotation = transform.rotation;

        if (anim) anim.ChangeAnim(fake);
    }

    private void OnMouseDown()
    {
        if (field == null || !field.select) return;
        matchController.currentePiece?.SelectedAField(field);
    }

    private void ReturnToNormal()
    {
        if (body == null || fake == null) return;
        if (body.activeSelf && !fake.activeSelf) return;

        body?.SetActive(true);
        fake?.SetActive(false);

        AnimPiece anim = GetComponent<AnimPiece>();
        if (anim)
        {
            anim.ChangetoOld();
        }
    }

    public void Reveal()
    {
        /*switch (gameType)
        {
            //In normal not reveal soldiers
            case GameMode.GameType.Normal:
                if (type == PieceType.Soldier) return;
                break;
            //in hard mode only revel flags and bombs
            case GameMode.GameType.Hard:
                if (type != PieceType.Flag && type != PieceType.Bomb) return;
                break;
        }*/
        ReturnToNormal();
    }

    private void OnDestroy()
    {
        ReturnToNormal();
        matchController?.OnDestroyFakePiece(this);
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