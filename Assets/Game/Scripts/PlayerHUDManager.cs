using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using System.Collections;

public class PlayerHUDManager : MonoBehaviour
{
    [Header("Referências de UI")]
    public TextMeshProUGUI nicknameText;
    public TextMeshProUGUI pontuationText;
    public RawImage avatarImage;

    private Texture2D avatarTexture;

    private void Start()
    {
        StartCoroutine(LoadPlayerHUD());
    }

    private IEnumerator LoadPlayerHUD()
    {        
        while (PlayerProfileManager.Instance == null || !SteamInitializer.Initialized)
            yield return null;
        
        Debug.Log("[PlayerHUDManager] Aguardando dados do perfil...");
        
        yield return new WaitUntil(() => PlayerProfileManager.Instance.pontuation >= 0);
        
        nicknameText.text = PlayerProfileManager.Instance.nickname;
        pontuationText.text = PlayerProfileManager.Instance.pontuation.ToString();
        
        Debug.Log($"[PlayerHUDManager] Perfil carregado - Nick: {PlayerProfileManager.Instance.nickname}, Pontos: {PlayerProfileManager.Instance.pontuation}");
                
        yield return StartCoroutine(LoadSteamAvatar());
    }

    private IEnumerator LoadSteamAvatar()
    {
        int imageID = SteamFriends.GetLargeFriendAvatar(SteamUser.GetSteamID());

        
        while (imageID == -1)
        {
            yield return new WaitForSeconds(0.1f);
            imageID = SteamFriends.GetLargeFriendAvatar(SteamUser.GetSteamID());
        }

        if (SteamUtils.GetImageSize(imageID, out uint width, out uint height))
        {
            byte[] image = new byte[width * height * 4];
            if (SteamUtils.GetImageRGBA(imageID, image, (int)(width * height * 4)))
            {
                avatarTexture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                avatarTexture.LoadRawTextureData(image);
                avatarTexture.Apply();
                avatarImage.texture = avatarTexture;
            }
        }
    }
    
    public void RefreshHUD()
    {
        pontuationText.text = PlayerProfileManager.Instance.pontuation.ToString();
    }
}
