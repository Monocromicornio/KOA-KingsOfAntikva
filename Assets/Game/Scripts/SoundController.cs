using UnityEngine;

public class SoundController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    AudioSource select;
    [SerializeField]
    AudioSource cancel;

    [Header("Victory")]
    [SerializeField]
    AudioSource victoryPeaple;
    [SerializeField]
    AudioSource victoryConfirm;

    public void Cancel()
    {
        cancel.Play();
    }

    public void Select()
    {
        select.Play();
    }

    public void VictoryPeaple()
    {
        victoryPeaple.Play();
    }

    public void VictoryConfirm()
    {
        victoryConfirm.Play();
    }
}
