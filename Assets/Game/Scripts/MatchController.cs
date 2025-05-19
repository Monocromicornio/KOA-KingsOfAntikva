using UnityEngine;

public class MatchController : MonoBehaviour
{
    public static MatchController instance;

    public BoardController boardController;

    public Turn turn;
    public GameMode gameMode;
    public GameMode.GameType gameType
    {
        get
        {
            return gameMode.type;
        }
    }

    public bool finished { get; private set;  }

    [Header("Feedback")]
    public SoundController soundController;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {

    }
    
    public void WinGame()
    {
        //if(tag == "Player")
        if (tag == "Enemy")
        {
            GameObject[] Enemies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (GameObject enemy in Enemies)
            {
                if (enemy.GetComponent<Piece>().Types != Piece.ItemType.Bandeira)
                {
                    enemy.GetComponent<Piece>().SetDie();
                }
                //Destroy(enemy);
            }

            GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject player in Players)
            {
                player.GetComponent<Piece>().SetVictory();
                //Debug.Log("player = " + player.name);
                //Destroy(player);
            }
        }
        else
        {
            GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject player in Players)
            {
                if (player.GetComponent<Piece>().Types != Piece.ItemType.Bandeira)
                {
                    player.GetComponent<Piece>().SetDie();
                }
            }

            GameObject[] Enemies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (GameObject enemy in Enemies)
            {
                enemy.GetComponent<Piece>().SetVictory();
                //Destroy(enemy);
            }
        }
    }
}
