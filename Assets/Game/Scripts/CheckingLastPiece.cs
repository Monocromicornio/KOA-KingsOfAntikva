using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckingLastPiece : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private int CountPeaces(string sType)
    {
        int count = 0;

        GameField[] gameFields = FindObjectsOfType<GameField>();

        foreach(GameField gameField in gameFields)
        {
            if(gameField.piece.gameObject.tag == sType)
            {
                if (gameField.piece.GetComponent<Piece>().type != PieceType.Bomb)
                {
                    if (gameField.piece.GetComponent<Piece>().type != PieceType.Flag)
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }
}
