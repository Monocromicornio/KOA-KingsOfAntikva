using UnityEngine;

public class SoundController : MonoBehaviour
{
    [Header("Field")]
    [SerializeField]
    AudioSource preAttack;
    [SerializeField]
    AudioSource steps;
    [SerializeField]
    AudioSource run;

    [Header("Field")]
    [SerializeField]
    AudioSource select;
    [SerializeField]
    AudioSource cancel;

    [Header("Victory")]
    [SerializeField]
    AudioSource victoryPeaple;
    [SerializeField]
    AudioSource victoryConfirm;

    [Header("Soldier")]
    [SerializeField]
    AudioSource dieSoldier;
    [SerializeField]
    AudioSource attackSoldier,
                downSoldier;

    public void PreAttack()
    {
        preAttack.Play();
    }

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

    public void DieSoldier()
    {
        dieSoldier.Play();
    }

    public void AttackSoldier()
    {
        attackSoldier.Play();
    }

    public void DownSoldier()
    {
        downSoldier.Play();
    }

    public void Steps()
    {
        steps.Play();
    }

    public void StopSteps()
    {
        steps.Stop();
    }

    public void Run()
    {
        run.Play();
    }

    public void StopRun()
    {
        run.Play();
    }
}