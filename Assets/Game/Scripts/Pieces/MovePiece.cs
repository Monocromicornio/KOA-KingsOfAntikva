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
    private SelectField selectField;

    private bool finished => matchController.finished;
    private GameField targetGameField;
    private Transform target => targetGameField.transform;

    [SerializeField]
    [Min(0)]
    float moveSpeed = 1;

    private void Awake()
    {
        piece = GetComponent<Piece>();
        selectField = GetComponent<SelectField>();
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
        piece.SetAnimation("Walk", true);

        while (IsFarFromTarget())
        {
            transform.Translate(Vector3.forward * Time.deltaTime * GetSpeed());
            yield return null;
        }

        transform.position = target.position;
        piece.SetAnimation("Walk", false);
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
}