using UnityEngine;
using TMPro;
using System.Collections;
using System;

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

    MatchController matchController;
    private float currentTime;
    private bool isRunning = false;
    private bool hasStarted = false;
    private int consecutiveSkips = 0;

    private bool CheckIsMyTurn()
    {
        if (matchController == null)
        {
            matchController = MatchController.instance;
        }

        return matchController.IsMyTurn();
    }
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
        if (!isRunning || matchController.finished)
            return;

        currentTime -= Time.unscaledDeltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            OnTimeExpired();
        }

        UpdateTimerDisplay();
    }

    public void StartTimer()
    {
        bool isMyTurn = CheckIsMyTurn();
        Debug.Log($"[TurnTimer] ========== START TIMER ==========");
        Debug.Log($"[TurnTimer] IsMyTurn: {isMyTurn}");
        Debug.Log($"[TurnTimer] consecutiveSkips: {consecutiveSkips}");
        
        if (!hasStarted)
        {
            hasStarted = true;
            timerText.gameObject.SetActive(true);
            Debug.Log("[TurnTimer] Timer iniciado na primeira jogada");
        }

        ResetTimer();
        isRunning = true;

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
        bool isMyTurn = CheckIsMyTurn();
        Debug.Log($"[TurnTimer] ========== TEMPO ESGOTADO ==========");
        Debug.Log($"[TurnTimer] IsMyTurn: {isMyTurn}");
        Debug.Log($"[TurnTimer] consecutiveSkips ANTES: {consecutiveSkips}");
        
        StopTimer();

        if (isMyTurn)
        {
            TurnTimerEvents.OnPlayerTimerEnded?.Invoke();

            consecutiveSkips++;
            Debug.Log($"[TurnTimer] >>> INCREMENTANDO consecutiveSkips (era {consecutiveSkips - 1}, agora é {consecutiveSkips})");

            if (consecutiveSkips >= 2)
            {
                Debug.Log("[TurnTimer] !!! DESISTÊNCIA POR 2 TIMEOUTS CONSECUTIVOS !!!");
                HandleForfeit();
                return;
            }
            else
            {
                matchController.ChangeTurn();
            }
        }
        else
        {
            Debug.Log($"[TurnTimer] Tempo esgotado do OPONENTE - consecutiveSkips mantém em {consecutiveSkips}");
        }
    }

    private void HandleForfeit()
    {
        Debug.Log("[TurnTimer] Aplicando desistência por timeout");

        if (CheckIsMyTurn())
        {
            matchController.Surrender();
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
        bool isMyTurn = CheckIsMyTurn();
        Debug.Log($"[TurnTimer] ========== JOGADOR FEZ MOVIMENTO ==========");
        Debug.Log($"[TurnTimer] IsMyTurn: {isMyTurn}");
        Debug.Log($"[TurnTimer] consecutiveSkips ANTES: {consecutiveSkips}");

        StopTimer();

        if (isMyTurn)
        {
            consecutiveSkips = 0;
            Debug.Log("[TurnTimer] <<< RESETANDO consecutiveSkips para 0 (jogador local fez movimento)");
        }
        else
        {
            Debug.Log($"[TurnTimer] Movimento do OPONENTE - consecutiveSkips mantém em {consecutiveSkips}");
        }
    }

    public void OnTurnChanged()
    {
        Debug.Log($"[TurnTimer] ========== TURNO MUDOU ==========");
        Debug.Log($"[TurnTimer] consecutiveSkips: {consecutiveSkips}");
        
        StartTimer();
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

public static class TurnTimerEvents
{
    public static Action OnPlayerTimerEnded;

}