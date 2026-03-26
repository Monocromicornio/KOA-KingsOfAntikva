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
            Debug.LogWarning("[NetworkMaterialSwap] Renderer não encontrado.");
            return;
        }

        NetworkManager nm = NetworkManager.Instance();
        if (nm == null)
        {
            Debug.LogWarning("[NetworkMaterialSwap] NetworkManager não encontrado.");
            return;
        }

        bool isHost = nm.IsServerConnection();

        // Tenta pegar o Piece automaticamente se não tiver sido atribuído no Inspector
        if (piece == null)
            piece = GetComponentInParent<Piece>();

        // Se existir Piece, usa a lógica completa
        if (piece != null)
        {
            bool isMyPiece = piece.isMyPiece;

            // Host:
            // minha peça = azul / inimiga = vermelho
            // Client:
            // minha peça = vermelho / inimiga = azul
            bool useHostMaterials = (isHost == isMyPiece);

            currentRenderer.materials = useHostMaterials ? hostMaterials : clientMaterials;

            Debug.Log($"[NetworkMaterialSwap] PEÇA | isHost: {isHost} | isMyPiece: {isMyPiece} | useHostMaterials: {useHostMaterials}");
        }
        else
        {
            // Se não for peça/personagem, decide só por host/client
            currentRenderer.materials = isHost ? hostMaterials : clientMaterials;

            Debug.Log($"[NetworkMaterialSwap] OBJETO COMUM | isHost: {isHost}");
        }
    }
}