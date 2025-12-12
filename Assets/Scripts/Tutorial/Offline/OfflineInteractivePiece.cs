using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(OfflinePiece))]
public class OfflineInteractivePiece : MonoBehaviour
{
    private OfflinePiece _piece;
    public OfflinePiece piece 
    { 
        get 
        {
            if (_piece == null)
            {
                _piece = GetComponent<OfflinePiece>();
            }
            return _piece;
        }
        private set => _piece = value;
    }

    [SerializeField]
    protected OfflineAnimPiece anim;

    [SerializeField]
    public int force;

    protected virtual void Awake()
    {
        piece = GetComponent<OfflinePiece>();
    }

    public virtual void Notify(bool success, OfflineInteractivePiece target)
    {
        OfflinePiece toDestroy = success ? target.piece : piece;

        string message = success ? "Success" : "Failed";
        float time = success ? toDestroy.timeToDestroy : 0;
        StartCoroutine(WaitToSendMessage(time + 1, message));

        toDestroy.SetLose();
    }

    private IEnumerator WaitToSendMessage(float time, string message)
    {
        yield return new WaitForSeconds(time);
        SendMessage(message, SendMessageOptions.DontRequireReceiver);
    }

    protected virtual void ForceChallenge(OfflineInteractivePiece target)
    {
        if (force >= target.force)
        {
            Notify(true, target);
            return;
        }

        target.CounterAttack(this);
    }

    protected IEnumerator FeedbackAttack(UnityAction action)
    {
        yield return new WaitForSeconds(1);
        
        if (anim != null)
        {
            anim.SetAnimation("Attack");
        }
        
        action.Invoke();
    }

    protected virtual void Attack(OfflineInteractivePiece target)
    {
        Debug.Log($"[OfflineInteractivePiece] Attack called! Attacker: {piece.name}, Target: {(target != null ? target.name : "null")}");
        
        if (target == null) return;
        
        Debug.Log($"[OfflineInteractivePiece] Triggering piece attacked event");
        TutorialEvents.TriggerPieceAttacked(piece, target.piece);
        
        UnityAction action = () => ForceChallenge(target);
        StartCoroutine(FeedbackAttack(action));
    }

    protected virtual void CounterAttack(OfflineInteractivePiece target)
    {
        if (target == null) return;
        UnityAction action = () => target.Notify(false, this);
        StartCoroutine(FeedbackAttack(action));
    }

    protected virtual void InstaKillAttack(OfflineInteractivePiece target)
    {
        Debug.Log($"[OfflineInteractivePiece] InstaKillAttack called! Attacker: {piece.name}, Target: {(target != null ? target.name : "null")}");
        
        if (target == null) return;
        
        Debug.Log($"[OfflineInteractivePiece] Triggering piece attacked event for InstaKill");
        TutorialEvents.TriggerPieceAttacked(piece, target.piece);
        
        UnityAction action = () => Notify(true, target);
        StartCoroutine(FeedbackAttack(action));
    }
}
