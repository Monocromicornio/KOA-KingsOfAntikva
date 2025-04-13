using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    // Start is called before the first frame update

    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeView()
    {
        if(anim.GetBool("topview"))
        {
            anim.SetBool("topview", false);
        }
        else
        {
            anim.SetBool("topview", true);
        }
    }
}
