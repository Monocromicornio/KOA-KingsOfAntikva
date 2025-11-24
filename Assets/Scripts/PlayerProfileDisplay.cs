using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using System.Collections;

#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using HeathenEngineering.SteamworksIntegration;
#endif

public class PlayerProfileDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private RawImage avatarImage;
    private Texture2D avatarTexture;

    [SerializeField]
    private TextMeshProUGUI playerNameText;
    
    [SerializeField]
    private TextMeshProUGUI pointsText;
    
    [SerializeField]
    private GameObject highlightBorder;
    
    [Header("Animation Settings")]
    [SerializeField]
    private float highlightScale = 1.15f;
    
    [SerializeField]
    private float animationDuration = 0.3f;
    
    [SerializeField]
    private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Player Info")]
    [SerializeField]
    private bool isLocalPlayer = true;
    
    [SerializeField]
    private ulong customSteamId = 0;
    
    private MatchController matchController;
    private PlayerProfileManager profileManager;
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private bool isHighlighted = false;
    private Coroutine highlightCoroutine;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        
        if (highlightBorder != null)
        {
            highlightBorder.SetActive(false);
        }
    }
    
    private void Start()
    {
        matchController = MatchController.instance;
        profileManager = PlayerProfileManager.Instance;
        
        LoadPlayerProfile();
    }
    
    private void LoadPlayerProfile()
    {
        ulong steamId = isLocalPlayer ? SteamUser.GetSteamID().m_SteamID : customSteamId;
        
        if (steamId == 0)
        {
            if (isLocalPlayer)
            {
                steamId = SteamUser.GetSteamID().m_SteamID;
            }
            else
            {
                Debug.LogWarning("[PlayerProfileDisplay] Steam ID do oponente ainda não foi definido");
                if (playerNameText != null)
                {
                    playerNameText.text = "Aguardando...";
                }
                if (pointsText != null)
                {
                    pointsText.text = "--- pts";
                }
                return;
            }
        }
        
        Debug.Log($"[PlayerProfileDisplay] Carregando perfil - isLocal: {isLocalPlayer}, SteamID: {steamId}");
        
#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
        UserData userData = UserData.Get(steamId);
        
        if (playerNameText != null)
        {
            playerNameText.text = userData.Name;
        }
        
        if (avatarImage != null)
        {
            userData.LoadAvatar((texture) =>
            {
                if (avatarImage != null && texture != null)
                {
                    avatarImage.texture = texture;
                }
            });
        }
#else
        if (playerNameText != null)
        {
            string friendName = SteamFriends.GetFriendPersonaName(new CSteamID(steamId));
            playerNameText.text = friendName;
            Debug.Log($"[PlayerProfileDisplay] Nome carregado: {friendName} para Steam ID: {steamId}");
        }
        
        if (avatarImage != null)
        {
            LoadSteamAvatar(steamId);
        }
#endif
        
        if (pointsText != null && profileManager != null)
        {
            if (isLocalPlayer)
            {
                pointsText.text = $"{profileManager.pontuation} pts";
            }
            else
            {
                int opponentPoints = profileManager.GetOpponentPoints(steamId);
                if (opponentPoints >= 0)
                {
                    pointsText.text = $"{opponentPoints} pts";
                    Debug.Log($"[PlayerProfileDisplay] Pontos do oponente: {opponentPoints}");
                }
                else
                {
                    pointsText.text = "--- pts";
                    Debug.Log("[PlayerProfileDisplay] Pontos do oponente não disponíveis");
                }
            }
        }
    }
    
    private void LoadSteamAvatar(ulong steamId)
    {
        CSteamID cSteamId = new CSteamID(steamId);
        int avatarInt = SteamFriends.GetLargeFriendAvatar(cSteamId);
        
        if (avatarInt > 0)
        {
            StartCoroutine(LoadAvatarTexture(avatarInt));
        }
    }
    
    private IEnumerator LoadAvatarTexture(int avatarInt)
    {
        int imageID = avatarInt;

        while (imageID == -1)
        {
            yield return new WaitForSeconds(0.1f);
            imageID = avatarInt;
        }

        if (SteamUtils.GetImageSize(imageID, out uint width, out uint height))
        {
            byte[] image = new byte[width * height * 4];
            if (SteamUtils.GetImageRGBA(imageID, image, (int)(width * height * 4)))
            {
                avatarTexture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                avatarTexture.LoadRawTextureData(image);
                avatarTexture.Apply();
                
                if (avatarImage != null)
                {
                    avatarImage.texture = avatarTexture;
                }
            }
        }
    }
    
    private void Update()
    {
        if (matchController == null)
        {
            matchController = MatchController.instance;
            return;
        }
        
        bool shouldBeHighlighted = IsMyTurn();
        
        if (shouldBeHighlighted != isHighlighted)
        {
            isHighlighted = shouldBeHighlighted;
            
            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
            }
            
            highlightCoroutine = StartCoroutine(AnimateHighlight(isHighlighted));
        }
    }
    
    private bool IsMyTurn()
    {
        if (matchController == null || !matchController.hasConnection)
        {
            return isLocalPlayer && matchController != null && matchController.currentTurn == matchController.myTurn;
        }
        
        TurnState currentTurn = matchController.currentTurn;
        TurnState myTurn = matchController.myTurn;
        
        if (isLocalPlayer)
        {
            return currentTurn == myTurn;
        }
        else
        {
            return currentTurn != myTurn;
        }
    }
    
    private IEnumerator AnimateHighlight(bool highlight)
    {
        float targetScaleFactor = highlight ? highlightScale : 1f;
        Vector3 targetScale = originalScale * targetScaleFactor;
        Vector3 startScale = rectTransform.localScale;
        
        if (highlightBorder != null)
        {
            highlightBorder.SetActive(highlight);
        }
        
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            float curveValue = scaleCurve.Evaluate(t);
            
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
            
            yield return null;
        }
        
        rectTransform.localScale = targetScale;
        highlightCoroutine = null;
    }
    
    public void SetCustomSteamId(ulong steamId)
    {
        customSteamId = steamId;
        isLocalPlayer = false;
        LoadPlayerProfile();
    }
    
    public void SetIsLocalPlayer(bool isLocal)
    {
        isLocalPlayer = isLocal;
        LoadPlayerProfile();
    }
    
    public void UpdatePoints(int points)
    {
        if (pointsText != null)
        {
            pointsText.text = $"{points} pts";
        }
    }
    
    public void UpdateOpponentPoints(int points)
    {
        if (!isLocalPlayer && pointsText != null)
        {
            pointsText.text = $"{points} pts";
            Debug.Log($"[PlayerProfileDisplay] Pontos do oponente atualizados: {points}");
        }
    }
    
    public bool IsLocalPlayer()
    {
        return isLocalPlayer;
    }
    
    public void RefreshProfile()
    {
        LoadPlayerProfile();
    }
}
