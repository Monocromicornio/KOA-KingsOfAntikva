using UnityEngine;
using TMPro;
using System.Collections;

public class TurnTimer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private TextMeshProUGUI timerText;

    [Header("Timer Settings")]
    [SerializeField]
    private float turnDuration = 60f;

    [SerializeField]
    private Color normalColor = Color.white;

    [SerializeField]
    private Color warningColor = Color.yellow;

    [SerializeField]
    private Color dangerColor = Color.red;

    [SerializeField]
    private float warningThreshold = 30f;

    [SerializeField]
    private float dangerThreshold = 10f;

    private MatchController matchController;
    private float currentTime;
    private bool isRunning = false;
    private bool hasStarted = false;
    private int consecutiveSkips = 0;
    private TurnState lastTurnState = TurnState.undefined;

    private void Start()
    {
        matchController = MatchController.instance;
        
        if (matchController == null)
        {
            Debug.LogError("[TurnTimer] MatchController não encontrado!");
            enabled = false;
            return;
        }

        if (timerText == null)
        {
            Debug.LogError("[TurnTimer] TextMeshProUGUI não configurado!");
            enabled = false;
            return;
        }

        ResetTimer();
        UpdateTimerDisplay();
        
        timerText.gameObject.SetActive(false);
    }

    private void Update()
    {
        return;
        if (!isRunning || matchController.finished)
            return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            OnTimeExpired();
        }

        UpdateTimerDisplay();
    }

    public void StartTimer()
    {
        if (!hasStarted)
        {
            hasStarted = true;
            timerText.gameObject.SetActive(true);
            Debug.Log("[TurnTimer] Timer iniciado na primeira jogada");
        }

        ResetTimer();
        isRunning = true;

        if (matchController.turn != lastTurnState)
        {
            consecutiveSkips = 0;
            lastTurnState = matchController.turn;
        }

        Debug.Log($"[TurnTimer] Timer resetado para {turnDuration} segundos");
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = turnDuration;
        UpdateTimerDisplay();
    }

    private void OnTimeExpired()
    {
        Debug.Log("[TurnTimer] Tempo esgotado! Passando turno automaticamente...");
        
        StopTimer();
        
        consecutiveSkips++;
        
        if (consecutiveSkips >= 2 && matchController.IsMyTurn())
        {
            Debug.Log("[TurnTimer] Jogador passou 2 vezes seguidas - Desistência!");
            HandleForfeit();
        }
        else
        {
            matchController.ChangeTurn();
        }
    }

    private void HandleForfeit()
    {
        Debug.Log("[TurnTimer] Aplicando desistência por timeout");
        
        if (matchController.IsMyTurn())
        {
            matchController.Surrender();
            matchController.SetFinishGame(matchController.playerSquad.pieces.ToArray(), false);
            matchController.SetFinishGame(matchController.enemySquad.pieces.ToArray(), true);
        }
        else
        {
            matchController.SetFinishGame(matchController.playerSquad.pieces.ToArray(), true);
            matchController.SetFinishGame(matchController.enemySquad.pieces.ToArray(), false);
        }
       
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        
        timerText.text = $"{minutes:00}:{seconds:00}";
        
        if (currentTime <= dangerThreshold)
        {
            timerText.color = dangerColor;
        }
        else if (currentTime <= warningThreshold)
        {
            timerText.color = warningColor;
        }
        else
        {
            timerText.color = normalColor;
        }
    }

    public void OnPlayerMadeMove()
    {
        if (!hasStarted)
        {
            StartTimer();
        }
        
        consecutiveSkips = 0;
    }

    public void OnTurnChanged()
    {
        if (hasStarted)
        {
            StartTimer();
        }
    }

    public float GetRemainingTime()
    {
        return currentTime;
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void ResumeTimer()
    {
        if (hasStarted && !matchController.finished)
        {
            isRunning = true;
        }
    }
}
