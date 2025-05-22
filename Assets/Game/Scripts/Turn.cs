using System.Collections;
using UnityEngine;

public class Turn : MonoBehaviour
{
    MatchController matchController
    {
        get
        {
            return MatchController.instance;
        }
    }
    public Piece currentePiece { get; private set; }
    public bool isPlayerTurn { get; private set; }
    public bool Liberate=false;

    [SerializeField]
    AudioSource auChangeTurn;

    bool bVictory=false;
    bool finished
    {
        get
        {
            return matchController.finished;
        }
    }

    int iPlayerCountVictory = 0;
    int iEnemyCountVictory = 0;

    bool bLoad = false;

    private void Start()
    {
        isPlayerTurn = true;
        Liberate = true;
    }

    private void Update()
    {
        //VictoryVerify();
    }

    public void SetPiece(Piece piece)
    {
        currentePiece = piece;
    }

    public bool bChangeTurn = false;

    public bool bSuicid = false;

    public void ChangeTurn()
    {

        if (bLoad)
        {
            if (!finished)
            {
                if (!bChangeTurn)
                {
                    bChangeTurn = true;
                    IEnumerator enumerator = IEChangeTurn(1.5f);
                    StartCoroutine(enumerator);
                    Debug.Log("Turn ChangeTurn - bChangeTurn = " + bChangeTurn);
                }
            }
        }

    }

    private IEnumerator IEChangeTurn(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        //if (Piece.GetComponent<Player>().tag == "Player")        

        PieacesCount();

        if (!bSuicid)
        {
            isPlayerTurn = !isPlayerTurn;

            if(isPlayerTurn) auChangeTurn.Play();
            bChangeTurn = false;
            StartCoroutine(IELiberateTurn(0.5f));
        }
    }

    private IEnumerator IELiberateTurn(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        KillStuckPiece();
        Liberate = true;
    }

    public void PieacesCount()
    {
        int iPlayerCount = 0;
        int iEnemyCount = 0;


        GameObject[] Enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in Enemies)
        {
            if (enemy.GetComponent<Piece>().Types.ToString() != "Bandeira")
            {
                if (enemy.GetComponent<Piece>().Types.ToString() != "Bomba")
                {
                    //enemy.GetComponent<Player>().SetDie();                
                    iEnemyCount++;
                }
            }

        }        

        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in Players)
        {
            if (player.GetComponent<Piece>().Types.ToString() != "Bandeira")
            {
                if (player.GetComponent<Piece>().Types.ToString() != "Bomba")
                {
                    //player.GetComponent<Player>().SetDie();                
                    iPlayerCount++;
                }
            }
        }

        iPlayerCountVictory = iPlayerCount;
        iEnemyCountVictory = iEnemyCount;
    }

    void VictoryVerify()
    {
        if (bLoad)
        {
            if (!bVictory)
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

                    bVictory = true;
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

                    bVictory = true;
                }
            }
        }
    }

    public void LoadPieces()
    {
        GameObject[] Enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

        iPlayerCountVictory = Players.Length;
        iEnemyCountVictory = Enemies.Length;

        bLoad = true;
    }

    private void KillStuckPiece()
    {
        if(isPlayerTurn)
        {
            Debug.Log("KillStuckPiece - iPlayerCountVictory = " + iPlayerCountVictory);
            if(iPlayerCountVictory==1)
            {
                GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

                foreach (GameObject player in Players)
                {
                    if (player.GetComponent<Piece>().Types.ToString() != "Bandeira")
                    {
                        if (player.GetComponent<Piece>().Types.ToString() != "Bomba")
                        {
                            int[] iHousesFree = player.GetComponent<Piece>().CheckHouses();
                            Debug.Log("KillStuckPiece - iHousesFree.Length = " + iHousesFree.Length);

                            if (iHousesFree.Length==0)
                            {
                                bSuicid = true;
                                player.GetComponent<Piece>().SetSuicide();
                            }
                        }
                    }
                }
            }            
        }
        else
        {
            if (iEnemyCountVictory == 1)
            {
                GameObject[] Players = GameObject.FindGameObjectsWithTag("Enemy");

                foreach (GameObject player in Players)
                {
                    if (player.GetComponent<Piece>().Types.ToString() != "Bandeira")
                    {
                        if (player.GetComponent<Piece>().Types.ToString() != "Bomba")
                        {
                            int[] iHousesFree = player.GetComponent<Piece>().HousesFree();
                            Debug.Log("KillStuckPiece - iHousesFree.Length = " + iHousesFree.Length);

                            if (iHousesFree.Length == 0)
                            {
                                bSuicid = true;
                                player.GetComponent<Piece>().SetSuicide();
                            }
                        }
                    }
                }
            }
        }

    }
}
