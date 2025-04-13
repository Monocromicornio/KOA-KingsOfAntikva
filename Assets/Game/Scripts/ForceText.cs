using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceText : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    string sForce;
    TextMesh txtForce;    

    void Start()
    {        

        if (sForce != "B")
        {
            txtForce = transform.Find("TxtForce").GetComponent<TextMesh>();
            txtForce.text = sForce;
        }
        else if (sForce != "F")
        {
            txtForce = transform.Find("TxtForce").GetComponent<TextMesh>();
            txtForce.text = sForce;
        }
        else
        {
            txtForce = transform.Find("TxtForce").GetComponent<TextMesh>();
            txtForce.text = "";
        }

    }

}
