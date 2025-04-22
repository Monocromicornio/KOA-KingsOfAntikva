using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadPieces : MonoBehaviour
{
    public Playsystems.Table table;

    int iPiecesRecord = 0;
    bool bLoaded = false;
    GameObject[] pieces;


    // Start is called before the first frame update
    void Start()
    {

        StartCoroutine(IETableCount());

        //Debug.Log("table Count = " + table.Count());

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator IETableCount()
    {
        while (true)
        {
            if (table.Loaded())
            {
                iPiecesRecord = table.Count();
                StartCoroutine(IELoadPiecesScene());
                Debug.Log("table Count = " + table.Count());    
                
            }
            yield return null;
        }        
    }

    private IEnumerator IELoadPiecesScene()
    {
        while (true)
        {
            if (LoadPiecesScene())
            {
                iPiecesRecord = table.Count();
                Debug.Log("table Count = " + table.Count());
                break;
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

    private void ArrangePieces()
    {



    }

}