using System.Collections;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    public static MatchController instance;

    public BoardController boardController;

    public GameMode gameMode;
    public GameMode.GameType gameType => gameMode.type;

    public bool finished { get; private set; }

    public Piece currentePiece { get; private set; }
    public bool isBlueTurn { get; private set; }

    [SerializeField]
    GameObject game;

    [Header("Feedback")]
    public SoundController soundController;
    [SerializeField]
    AudioSource auChangeTurn;

    private void Awake()
    {
        instance = this;
        
        game.SetActive(false);
        isBlueTurn = true;
        StartCoroutine(LoadGame());
    }

    private IEnumerator LoadGame()
    {
        while (!boardController.isFinished())
        {
            yield return new WaitForEndOfFrame();
        }
        game.SetActive(true);
    }

    public void SetPiece(Piece piece)
    {
        currentePiece = piece;
    }

    public void ChangeTurn()
    {
        if (finished) return;
        isBlueTurn = !isBlueTurn;
        //Verify Victory
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
                    enemy.GetComponent<Piece>().SetDie();
                }
                //Destroy(enemy);
            }

            GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject player in Players)
            {
                player.GetComponent<Piece>().CelebrateVitory();
            }
        }
        else
        {
            GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject player in Players)
            {
                if (player.GetComponent<Piece>().type != PieceType.Flag)
                {
                    player.GetComponent<Piece>().SetDie();
                }
            }

            GameObject[] Enemies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (GameObject enemy in Enemies)
            {
                enemy.GetComponent<Piece>().CelebrateVitory();
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
