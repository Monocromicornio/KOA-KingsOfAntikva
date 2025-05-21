using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Piece))]
[RequireComponent(typeof(SelectField))]
public class MovePiece : MonoBehaviour
{
    private MatchController matchController => MatchController.instance;
    private BoardController board => matchController.boardController;
    private Turn turn => matchController.turn;
    private Piece piece;

    private bool finished => matchController.finished;
    private GameField currentField => piece.GetCurrentField();

    private void Awake()
    {
        piece = GetComponent<Piece>();
    }

    private void ChangeField()
    {
        if (finished) return;

        transform.LookAt(currentField.transform);
        //PlayStep(); -> Sound
        StartCoroutine(Moveto());
    }

    IEnumerator Moveto()
    {
        float SpeedPlus = 0;

        if (CanRun(currentField.transform))
        {
            SpeedPlus = 1.0f;
        }

        while (!DistanceTarget(currentField.transform))
        {
            print("B");
            piece.SetAnimation("Walk", true);
            transform.Translate(Vector3.forward * Time.deltaTime * (piece.MoveSpeed + SpeedPlus));
            yield return null;
        }

        if (!turn.bChangeTurn)
        {
            turn.ChangeTurn();
        }
        //StopStep(); -> Sound
        //SetAnimation("Walk", false);
        
        yield return 0;
    }

    private bool CanRun(Transform target)
    {
        bool bRun = false;
        float dist;

        float MaxDist = board.GetDistance() * 2;

        if (target)
        {
            dist = Vector3.Distance(target.position, transform.position);           

            if (dist >= MaxDist)
            {
                bRun = true;
            }
        }

        return bRun;

    }

    IEnumerator MovetoAttack(GameObject pieace, bool attack)
    {
        if (finished) yield return null;
        
        turn.Liberate = false;
        //CancelMovement();

        Debug.Log("MovetoAttack pieace = " + pieace.name + " - attack = " + attack);

        float SpeedPlus = 0;

        if(CanRun(currentField.transform))
        {
            SpeedPlus = 1.0f;
        }

        while (!DistanceAttack(currentField.transform))
        {
            piece.SetAnimation("Walk", true);
            //transform.Translate(Vector3.forward * Time.deltaTime * (MoveSpeed + SpeedPlus));
            yield return null;
        }
        /*if (attack)
        {
            soundController.PreAttack();
            IEnumerator enumerator = IEattack(pieace, 1.0f);
            StartCoroutine(enumerator);
        }
        else
        {
            soundController.PreAttack();
            pieace.GetComponent<Piece>().CounterAttack(gameObject);
        }

        StopStep();
        SetAnimation("Walk", false);*/
        
        yield return 0;
    }

    private bool DistanceTarget(Transform target)
    {
        bool bdistance = false;
        float dist;

        if (target)
        {
            dist = Vector3.Distance(target.position, transform.position);
            if (dist <= 0.1f)            
            {
                bdistance = true;                
            }
        }

        return bdistance;

    }

    private bool DistanceAttack(Transform target)
    {
        bool bdistance = false;
        float dist;

        if (target)
        {
            dist = Vector3.Distance(target.position, transform.position);
            //print("Distance to other: " + dist);

            if (dist <= 1.5f)
            {
                bdistance = true;                
            }
        }

        return bdistance;

    }
}