using com.onlineobject.objectnet;
using UnityEngine;

public class NetworkMaterialSwap : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material[] hostMaterials;   // azul
    [SerializeField] private Material[] clientMaterials; // vermelho
    [SerializeField] private Piece piece;

    private void Awake()
    {
        NetworkScriptsReference.IgnoreType(typeof(NetworkMaterialSwap));
    }

    private void Start()
    {
        ApplyMaterials();
    }

    private void ApplyMaterials()
    {
        Renderer currentRenderer = targetRenderer != null ? targetRenderer : GetComponentInChildren<Renderer>();
        if (currentRenderer == null)
        {
            Debug.LogWarning("[NetworkMaterialSwap] Renderer n�o encontrado.");
            return;
        }

        NetworkManager nm = NetworkManager.Instance();
        if (nm == null)
        {
            Debug.LogWarning("[NetworkMaterialSwap] NetworkManager n�o encontrado.");
            return;
        }

        // Partida offline (contra IA): n�o realiza troca de material
        MatchController matchController = MatchController.instance;
        if (matchController != null && !matchController.hasConnection)
        {
            return;
        }

        bool isHost = nm.IsServerConnection();

        // Tenta pegar o Piece automaticamente se n�o tiver sido atribu�do no Inspector
        if (piece == null)
            piece = GetComponentInParent<Piece>();

        // Se existir Piece, usa a l�gica completa
        if (piece != null)
        {
            bool isMyPiece = piece.isMyPiece;

            // Host:
            // minha pe�a = azul / inimiga = vermelho
            // Client:
            // minha pe�a = vermelho / inimiga = azul
            bool useHostMaterials = (isHost == isMyPiece);

            currentRenderer.materials = useHostMaterials ? hostMaterials : clientMaterials;

            Debug.Log($"[NetworkMaterialSwap] PE�A | isHost: {isHost} | isMyPiece: {isMyPiece} | useHostMaterials: {useHostMaterials}");
        }
        else
        {
            // Se n�o for pe�a/personagem, decide s� por host/client
            currentRenderer.materials = isHost ? hostMaterials : clientMaterials;

            Debug.Log($"[NetworkMaterialSwap] OBJETO COMUM | isHost: {isHost}");
        }
    }
}