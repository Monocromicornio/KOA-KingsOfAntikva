using System.Collections;
using System.Collections.Generic;
using com.onlineobject.objectnet;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    public static MatchController instance;
    public static bool online = false;
    public NetworkManager networkManager => NetworkManager.Instance();

    [Header("Game objs")]
    public BoardController boardController;

    public GameMode gameMode;
    public GameMode.GameType gameType => gameMode.type;

    public bool finished { get; private set; }

    public Piece currentePiece { get; private set; }
    private bool blueTurn = false; //False to start with blue, true for red
    public TurnState turn { get; private set; }

    [SerializeField]
    private GameObject game;
    public PlayerSquad playerSquad;
    public EnemySquad enemySquad;
    public MachinePlayer machinePlayer;

    [Header("Feedback")]
    public SoundController soundController;
    [SerializeField]
    private AudioSource auChangeTurn;

    protected virtual void Awake()
    {
        instance = this;
        game.SetActive(false);
        turn = TurnState.wait;
        OnSceneLoaded("");
    }

    public void OnSceneLoaded(string sceneName)
    {
        //print("sceneName " + sceneName);
        //if (!online) return;
        StartCoroutine(LoadGame());
    }

    private IEnumerator LoadGame()
    {
        while (!boardController.isFinished())
        {
            yield return new WaitForEndOfFrame();
        }

        game.SetActive(true);
        SpawnPieces();
        StartCoroutine(ChangeTurn(0));
    }

    public void SpawnPieces()
    {
        playerSquad.LoadPieces(networkManager.IsServerConnection());
    }

    public void SetPiece(Piece piece)
    {
        currentePiece = piece;
    }

    public void RemovePieceFromSquad(Piece piece)
    {
        List<Piece> pieces;
        FakePiece fakePiece = piece.GetComponent<FakePiece>();
        if (fakePiece != null)
        {
            pieces = enemySquad.pieces;
            enemySquad.fakePieces.Remove(fakePiece);
        }
        else
        {
            pieces = playerSquad.pieces;
        }

        if (!pieces.Contains(piece)) return;
        pieces.Remove(piece);
    }

    public void MadeActionOnTurn()
    {
        turn = TurnState.wait;
    }

    public void ChangeTurn()
    {
        if (finished) return;
        bool endGame = CheckEndGame();
        if (endGame)
        {
            WinGame();
            return;
        }

        StartCoroutine(ChangeTurn(2));
    }

    private IEnumerator ChangeTurn(float time)
    {
        print("---------------------|||------------------------");
        yield return new WaitForSeconds(time);
        blueTurn = !blueTurn;
        turn = blueTurn ? TurnState.blue : TurnState.red;
        print("É a vez do " + turn);
        if (turn == TurnState.red)
        {
            machinePlayer.StartTurn();
        }
    }

    public void OpenChest(TrunckPiece piece)
    {
        if (finished) return;
        if (piece.bluePiece)
        {
            SetEnemyWin();
            return;
        }

        SetPlayerWin();
        WinGame();
    }

    private void SetPlayerWin()
    {
        SetFinishGame(playerSquad.pieces.ToArray(), true);
        SetFinishGame(enemySquad.pieces.ToArray(), false);
    }

    private void SetEnemyWin()
    {
        SetFinishGame(enemySquad.pieces.ToArray(), true);
        SetFinishGame(playerSquad.pieces.ToArray(), false);
    }

    private void WinGame()
    {
        if (finished) return;
        finished = true;

        //VOLTAR AO MENU
    }

    public void SetFinishGame(Piece[] pieces, bool win)
    {
        foreach (Piece piece in pieces)
        {
            if (piece.type == PieceType.Flag)
            {
                piece.SendMessage("OpenChest");
                continue;
            }
            piece.SendMessage(win? "Win" : "Lose");
        }
    }

    private bool CheckEndGame()
    {
        int players = CountActivePiece(playerSquad.pieces);
        if (players == 0)
        {
            SetEnemyWin();
            return true;
        }

        int enemies = CountActivePiece(enemySquad.pieces);
        if (enemies == 0)
        {
            SetPlayerWin();
            return true;
        }

        return false;
    }

    private int CountActivePiece(List<Piece> pieces)
    {
        int amount = 0;
        foreach (Piece piece in pieces)
        {
            if (piece.type == PieceType.Flag || piece.type == PieceType.Bomb) continue;
            amount++;
        }

        return amount;
    }
}
