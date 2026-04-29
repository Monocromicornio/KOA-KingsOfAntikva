using System.Collections;
using UnityEngine;

[RequireComponent(typeof(OfflinePiece))]
[RequireComponent(typeof(OfflineSelectablePiece))]
public class OfflineMovePiece : MonoBehaviour
{
    private BoardController board;
    private OfflinePiece piece;
    private OfflineSelectablePiece selectField;

    private GameField targetGameField;
    private Transform target => targetGameField != null ? targetGameField.transform : null;

    [SerializeField]
    private OfflineAnimPiece anim;

    [SerializeField]
    [Min(0)]
    private float moveSpeed = 1;

    [Header("Configurações de Animação de Movimento")]
    [SerializeField]
    [Min(0)]
    private float liftHeight = 0.5f;

    [SerializeField]
    [Min(0)]
    private float liftDuration = 0.3f;

    [SerializeField]
    [Min(0)]
    private float flyDuration = 1f;

    [SerializeField]
    [Min(0)]
    private float landDuration = 0.3f;

    [SerializeField]
    [Min(0)]
    private float tiltAngle = 15f;

    private void Awake()
    {
        piece = GetComponent<OfflinePiece>();
        selectField = GetComponent<OfflineSelectablePiece>();
    }

    private void Start()
    {
        TutorialBoardController tutorialBoard = FindFirstObjectByType<TutorialBoardController>();
        if (tutorialBoard != null)
        {
            board = tutorialBoard.GetBoardController();
        }
        
        if (board == null)
        {
            board = FindFirstObjectByType<BoardController>();
        }
    }

    public void NewTarget()
    {
        GameField fieldPiece = piece.targetField;
        targetGameField = selectField.GetEmptyFieldFromActive(fieldPiece);

        if (targetGameField == null) return;

        transform.LookAt(target);
        StartCoroutine(MovetoAnimated());
    }

    IEnumerator MovetoAnimated()
    {
        if (anim != null)
        {
            anim.SetAnimation("Walk", true);
        }

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = target.position;

        iTween.MoveTo(gameObject, iTween.Hash(
            "y", startPosition.y + liftHeight,
            "time", liftDuration,
            "easetype", iTween.EaseType.easeOutQuad
        ));

        yield return new WaitForSeconds(liftDuration);

        float totalFlyTime = flyDuration;
        
        iTween.RotateTo(gameObject, iTween.Hash(
            "x", transform.eulerAngles.x - tiltAngle,
            "time", totalFlyTime * 0.2f,
            "easetype", iTween.EaseType.easeInOutQuad
        ));

        yield return new WaitForSeconds(totalFlyTime * 0.2f);

        iTween.MoveTo(gameObject, iTween.Hash(
            "position", new Vector3(targetPosition.x, targetPosition.y + liftHeight, targetPosition.z),
            "time", totalFlyTime * 0.6f,
            "easetype", iTween.EaseType.linear
        ));

        iTween.RotateTo(gameObject, iTween.Hash(
            "x", transform.eulerAngles.x + tiltAngle,
            "time", totalFlyTime * 0.6f,
            "easetype", iTween.EaseType.easeInOutQuad
        ));

        yield return new WaitForSeconds(totalFlyTime * 0.6f);

        iTween.MoveTo(gameObject, iTween.Hash(
            "y", targetPosition.y,
            "time", landDuration,
            "easetype", iTween.EaseType.easeInQuad
        ));

        yield return new WaitForSeconds(landDuration);

        transform.position = targetPosition;
        
        if (anim != null)
        {
            anim.SetAnimation("Walk", false);
            anim.PlayMoveEndSound();
        }
        
        piece.CheckPieceOnField();
    }

    IEnumerator Moveto()
    {
        if (anim != null)
        {
            anim.SetAnimation("Walk", true);
        }

        while (IsFarFromTarget())
        {
            transform.Translate(Vector3.forward * Time.deltaTime * GetSpeed());
            yield return null;
        }

        transform.position = target.position;
        
        if (anim != null)
        {
            anim.SetAnimation("Walk", false);
            anim.PlayMoveEndSound();
        }
        
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
        if (target == null || board == null) return moveSpeed;
        
        Vector3 targetPos = target.position;

        float max = board.GetDistance() * 2;
        float dist = Vector3.Distance(targetPos, transform.position);

        return dist < max ? moveSpeed : moveSpeed + 1;
    }
}
