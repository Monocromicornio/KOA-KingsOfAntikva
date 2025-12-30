using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using com.onlineobject.objectnet;

public class OfflineMatchController : MatchController
{
    public new NetworkManager networkManager => OfflineNetworkManagerStub.Instance();
    public new bool hasConnection => false;

    private new void Awake()
    {
        instance = this;

        var gameField = GetPrivateField("game");
        if (gameField != null)
        {
            GameObject gameObj = gameField as GameObject;
            if (gameObj != null) gameObj.SetActive(false);
        }

        SetPropertyValue("currentTurn", TurnState.wait);
        SetPropertyValue("turn", TurnState.undefined);
        SetPropertyValue("myTurn", TurnState.homeTeam);

        SetFieldValue("allPieces", new System.Collections.Generic.List<Piece>());

        var exitField = GetPrivateField("exit");
        if (exitField != null)
        {
            UnityEngine.UI.Button exitButton = exitField as UnityEngine.UI.Button;
            if (exitButton != null)
            {
                exitButton.gameObject.SetActive(false);
            }
        }

        Debug.Log("[OfflineMatchController] Inicializado em modo offline");
    }

    private object GetPrivateField(string fieldName)
    {
        var field = typeof(MatchController).GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return field.GetValue(this);
        }
        return null;
    }

    private void SetPropertyValue(string propertyName, object value)
    {
        var property = typeof(MatchController).GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(this, value);
        }
        else
        {
            var backingField = typeof(MatchController).GetField($"<{propertyName}>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (backingField != null)
            {
                backingField.SetValue(this, value);
            }
        }
    }

    private void SetFieldValue(string fieldName, object value)
    {
        var field = typeof(MatchController).GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(this, value);
        }
    }

    new void Start()
    {
        playerSquad.LoadPieces();
        enemySquad.LoadPieces();
        StartCoroutine(StartGame());
    }

    private new IEnumerator StartGame()
    {
        yield return new WaitForSeconds(2);
        ChangeTurn();
    }

    public new void GoToMenu()
    {
        Debug.Log("[OfflineMatchController] Saindo da partida offline...");
        StopAllCoroutines();
        SceneManager.LoadScene("PositionParts");
    }

    public new void ChangeTurn()
    {
        ChangeTurnImmediate();
    }

    public new bool IsMyTurn()
    {
        return true;
    }
}
