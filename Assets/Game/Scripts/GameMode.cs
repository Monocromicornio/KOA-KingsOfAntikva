using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMode : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    int itype = 0;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetGameType()
    {
        if(itype==0)
        {
            itype = PlayerPrefs.GetInt("GameMode");
        }
        //Type 1 = Trannig - Show pieces any time
        //Type 2 = Normal - Show pieces when dying
        //Type 3 = Hard - No show pieces
        return itype;
    }

    public void SetGameType(int gametype)
    {
        PlayerPrefs.SetInt("GameMode", gametype);
    }
}
