using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddPlayers : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    GameObject gGame;

    bool bStart = false;
    BoardController board;

    Turn turn;

    void Start()
    {
        board = FindObjectOfType<BoardController>();
        turn = FindObjectOfType<Turn>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!bStart)
        {
            if (board.isFinished())
            {
                StartCoroutine(IEStartGame(1.0f));
                bStart = true;
            }
        }
    }

    IEnumerator IEStartGame(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        gGame.SetActive(true);
        turn.LoadPieces();
        Destroy(this);
    }
}
