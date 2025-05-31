using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Piece))]
[RequireComponent(typeof(SelectablePiece))]
public class MovePiece : MonoBehaviour
{
    private MatchController matchController => MatchController.instance;
    private BoardController board => matchController.boardController;
    private Piece piece;
    private SelectablePiece selectField;

    private bool finished => matchController.finished;
    private GameField targetGameField;
    private Transform target => targetGameField.transform;

    [SerializeField]
    AnimPiece anim;

    [SerializeField]
    [Min(0)]
    float moveSpeed = 1;

    private void Awake()
    {
        piece = GetComponent<Piece>();
        selectField = GetComponent<SelectablePiece>();
    }

    public void NewTarget()
    {
        if (finished) return;

        GameField fieldPiece = piece.targetField;
        targetGameField = selectField.GetEmptyFieldFromActive(fieldPiece);

        if (targetGameField == null) return;

        transform.LookAt(target);
        //PlayStep(); -> Sound
        StartCoroutine(Moveto());
    }

    IEnumerator Moveto()
    {
        anim.SetAnimation("Walk", true);

        while (IsFarFromTarget())
        {
            transform.Translate(Vector3.forward * Time.deltaTime * GetSpeed());
            yield return null;
        }

        transform.position = target.position;
        anim.SetAnimation("Walk", false);
        piece.CheckPieceOnField();
    }

    private bool IsFarFromTarget()
    {
        if (target == null) return false;

        Vector3 targetPos = target.position;
        float dist = Vector3.Distance(targetPos, transform.position);
        return dist > 0.1f;
    }

    private float GetSpeed()
    {
        if (target == null) return 0;
        Vector3 targetPos = target.position;

        float max = board.GetDistance() * 2;
        float dist = Vector3.Distance(targetPos, transform.position);

        return dist < max ? moveSpeed : moveSpeed + 1;
    }

    /*private void PlayStep()
    {
        if (Types == ItemType.Soldado
        || (gameType != GameMode.GameType.Training && tag == "Enemy"))
        {
            soundController.Run();
        }

        soundController.Steps();
    }

    private void StopStep()
    {
        if (Types == ItemType.Soldado
        || (gameType != GameMode.GameType.Training && tag == "Enemy"))
        {
            soundController.StopRun();
        }

        soundController.StopSteps();
    }*/
}