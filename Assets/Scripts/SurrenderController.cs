using com.onlineobject.objectnet;
using UnityEngine;
using UnityEngine.UI;

public class SurrenderController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private GameObject surrenderPanel;
    
    [SerializeField]
    private Button surrenderButton;
    
    [SerializeField]
    private Button confirmSurrenderButton;
    
    [SerializeField]
    private Button cancelSurrenderButton;
    
    private MatchController matchController;
    private NetworkManager networkManager;
    
    private void Awake()
    {
        matchController = MatchController.instance;
        networkManager = NetworkManager.Instance();
        
        if (surrenderButton != null)
        {
            surrenderButton.onClick.AddListener(ShowSurrenderDialog);
        }
        
        if (confirmSurrenderButton != null)
        {
            confirmSurrenderButton.onClick.AddListener(ConfirmSurrender);
        }
        
        if (cancelSurrenderButton != null)
        {
            cancelSurrenderButton.onClick.AddListener(HideSurrenderDialog);
        }
        
        if (surrenderPanel != null)
        {
            surrenderPanel.SetActive(false);
        }
    }
    
    private void Start()
    {
        if (matchController == null)
        {
            matchController = MatchController.instance;
        }
        
        if (networkManager == null)
        {
            networkManager = NetworkManager.Instance();
        }
    }
    
    public void ShowSurrenderDialog()
    {
        if (matchController.finished)
        {
            Debug.Log("[SurrenderController] O jogo já terminou");
            return;
        }
        
        if (surrenderPanel != null)
        {
            surrenderPanel.SetActive(true);
        }
    }
    
    public void HideSurrenderDialog()
    {
        if (surrenderPanel != null)
        {
            surrenderPanel.SetActive(false);
        }
    }
    
    public void ConfirmSurrender()
    {
        HideSurrenderDialog();
        
        Debug.Log("[SurrenderController] Jogador se rendeu");
        
        if (networkManager.HasConnection())
        {
            SurrenderNetwork();
        }
        else
        {
            SurrenderLocal();
        }
    }
    
    private void SurrenderLocal()
    {
        matchController.SetFinishGame(matchController.playerSquad.pieces.ToArray(), false);
        matchController.SetFinishGame(matchController.enemySquad.pieces.ToArray(), true);
        
        if (surrenderButton != null)
        {
            surrenderButton.gameObject.SetActive(false);
        }
    }
    
    private void SurrenderNetwork()
    {
        matchController.SetFinishGame(matchController.playerSquad.pieces.ToArray(), false);
        matchController.SetFinishGame(matchController.enemySquad.pieces.ToArray(), true);
        
        if (surrenderButton != null)
        {
            surrenderButton.gameObject.SetActive(false);
        }
        
        Debug.Log("[SurrenderController] Rendição executada. Saindo da partida em 3 segundos...");
        StartCoroutine(GoToMenuAfterDelay(3f));
    }
    
    private System.Collections.IEnumerator GoToMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        matchController.GoToMenu();
    }
}
