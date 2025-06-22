using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CameraControl : MonoBehaviour
{
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
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
