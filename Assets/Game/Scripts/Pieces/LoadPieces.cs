using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadPieces : MonoBehaviour
{
    public TableData table;

    [SerializeField]
    GameObject[] pieces;

    void Start()
    {
        StartCoroutine(IETableCount());
    }

    private IEnumerator IETableCount()
    {
        while (true)
        {
            if (table.Loaded())
            {
                LoadPiecesScene();
            }
            yield return null;
        }        
    }

    private bool LoadPiecesScene()
    {
        bool bload = false;

        pieces = GameObject.FindGameObjectsWithTag("Player");

        if(pieces.Length>0)
        {
            bload = true;
        }

        return bload;
    }
}