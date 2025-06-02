using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    public static MatchController instance;

    [Header("Game objs")]
    public BoardController boardController;

    public GameMode gameMode;
    public GameMode.GameType gameType => gameMode.type;

    public bool finished { get; private set; }

    public Piece currentePiece { get; private set; }
    public bool isBlueTurn { get; private set; }

    [SerializeField]
    private GameObject game;
    public PlayerSquad playerSquad;
    public EnemySquad enemySquad;
    public MachinePlayer machinePlayer;

    [Header("Feedback")]
    public SoundController soundController;
    [SerializeField]
    private AudioSource auChangeTurn;

    private void Awake()
    {
        instance = this;
        game.SetActive(false);
        StartCoroutine(LoadGame());
    }

    private IEnumerator LoadGame()
    {
        while (!boardController.isFinished())
        {
            yield return new WaitForEndOfFrame();
        }
        game.SetActive(true);
        yield return new WaitForSeconds(2);
        isBlueTurn = false;
        StartCoroutine(ChangeTurn(0));
    }

    public void SetPiece(Piece piece)
    {
        currentePiece = piece;
    }

    public void RemovePieceFromSquad(Piece piece)
    {
        List<Piece> pieces;
        FakePiece fakePiece = piece.GetComponent<FakePiece>();
        if (fakePiece != null) {
            pieces = enemySquad.pieces;
            enemySquad.fakePieces.Remove(fakePiece);
        } else {
            pieces = playerSquad.pieces;
        }

        if (!pieces.Contains(piece)) return;
        pieces.Remove(piece);
    }

    public void ChangeTurn(string n)
    {
        if (finished) return;
        //Verify Victory
        print("\n\ncalled by " + n + "\n\n");
        StartCoroutine(ChangeTurn(2));
    }

    private IEnumerator ChangeTurn(float time)
    {
        print("TROCA DE TURNO");
        yield return new WaitForSeconds(time);
        isBlueTurn = !isBlueTurn;
        print("É a vez do " + (isBlueTurn ? "player" : " maquina "));
        if (!isBlueTurn)
        {
            machinePlayer.StartTurn();
        }
    }

    //Verify Check mate a "matar" as peças sobrando

    public void WinGame()
    {
        //if(tag == "Player")
        if (tag == "Enemy")
        {
            GameObject[] Enemies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (GameObject enemy in Enemies)
            {
                if (enemy.GetComponent<Piece>().type != PieceType.Flag)
                {
                    enemy.SendMessage("Destroy");
                }
                //Destroy(enemy);
            }

            GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject player in Players)
            {
                player.SendMessage("CelebrateVitory");
            }
        }
        else
        {
            GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject player in Players)
            {
                if (player.GetComponent<Piece>().type != PieceType.Flag)
                {
                    player.SendMessage("Destroy");
                }
            }

            GameObject[] Enemies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (GameObject enemy in Enemies)
            {
                enemy.SendMessage("CelebrateVitory");
                //Destroy(enemy);
            }
        }
    }
    
    /*void VictoryVerify()
    {
        if (iEnemyCountVictory == 0)
        {
            matchController.WinGame();
            currentePiece = null;

            GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject player in Players)
            {
                player.GetComponent<Piece>().SetVictory();
            }
        }

        if (iPlayerCountVictory == 0)
        {
            matchController.WinGame();
            currentePiece = null;

            GameObject[] Enemies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (GameObject enemy in Enemies)
            {
                enemy.GetComponent<Piece>().SetVictory();
            }
        }
    }*/
}
