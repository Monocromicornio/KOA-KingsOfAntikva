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

        FieldController[] fcs = FindObjectsOfType<FieldController>();

        foreach(FieldController fc in fcs)
        {
            if(fc.BusyPiece.gameObject.tag == sType)
            {
                if (fc.BusyPiece.GetComponent<Player>().Types != Player.ItemType.Bomba)
                {
                    if (fc.BusyPiece.GetComponent<Player>().Types != Player.ItemType.Bandeira)
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }
}
