using System.Collections.Generic;
using UnityEngine;

public class GameField : Field
{
    private const float DEFAULT_ICON_HEIGHT = 2.5f;

    private static readonly Dictionary<FieldDirection, float> DirectionYRotations = new Dictionary<FieldDirection, float>
    {
        { FieldDirection.Down, 0f },
        { FieldDirection.Up, 180f },
        { FieldDirection.Right, 270f },
        { FieldDirection.Left, 90f }
    };

    private MatchController matchController => MatchController.instance;

    public bool select => selectFeedback.activeSelf;
    public bool canSelect => canSelectFeedback.activeSelf;

    [SerializeField]
    GameObject selectFeedback, canSelectFeedback;

    private MeshRenderer arrowRenderer;
    private GameObject spawnedIcon;

    private void Awake()
    {
        selectFeedback?.SetActive(false);
        canSelectFeedback?.SetActive(false);

        if (selectFeedback != null)
        {
            arrowRenderer = selectFeedback.GetComponent<MeshRenderer>();
        }
    }

    private void Start()
    {
        if (matchController == null) return;

        if (matchController.networkManager.IsClientConnection())
        {
            forceText.transform.eulerAngles = new Vector3(90, 0, 180);
        }
    }

    private void Update()
    {

        bool isTutorialMode = TutorialModeController.IsTutorialActive();
        
        if (!isTutorialMode)
        {
            if (matchController == null) return;

            if (canSelectFeedback.activeSelf && !matchController.IsMyTurn())
            {
                canSelectFeedback.SetActive(false);
            }
        }
    }

    private void OnMouseOver()
    {
        bool isTutorialMode = TutorialModeController.IsTutorialActive();
        
        if (isTutorialMode)
        {
            canSelectFeedback.SetActive(true);
            return;
        }
        
        if (matchController == null || !matchController.IsMyTurn()) return;

        canSelectFeedback.SetActive(true);
    }

    private void OnMouseExit()
    {
        canSelectFeedback.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (!canSelect) return;

     
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() == true)
        {
            return;
        }

        if (select)
        {
            Piece.activePiece?.SelectedAField(this);
            OfflinePiece.activePiece?.SelectedAField(this);
            return;
        }
        
        piece?.Select();
        offlinePiece?.Select();
    }

    public void Select()
    {
        selectFeedback.SetActive(true);
        ResetArrowVisual();
    }

    /// <summary>
    /// Shows the arrow feedback rotated toward the given direction.
    /// </summary>
    public void Select(FieldDirection direction)
    {
        selectFeedback.SetActive(true);
        ResetArrowVisual();

        if (DirectionYRotations.TryGetValue(direction, out float yRotation))
        {
            selectFeedback.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }

    /// <summary>
    /// Marks the field as selected for attack. Hides the arrow and spawns an attack icon above the enemy piece.
    /// </summary>
    public void SelectAsAttack(GameObject attackPrefab, float heightOffset = DEFAULT_ICON_HEIGHT)
    {
        selectFeedback.SetActive(true);
        HideArrowVisual();

        Transform target = GetPieceTransform();
        if (attackPrefab != null && target != null)
        {
            ClearIcon();
            spawnedIcon = Instantiate(attackPrefab, target.position + Vector3.up * heightOffset, Quaternion.identity);
        }
    }

    /// <summary>
    /// Shows an origin indicator icon on this field (used for the selected piece's own field).
    /// </summary>
    public void SelectAsOrigin(GameObject originPrefab, float heightOffset = 0.1f)
    {
        if (originPrefab == null) return;

        ClearIcon();
        spawnedIcon = Instantiate(originPrefab, transform.position + Vector3.up * heightOffset, Quaternion.identity, transform);
    }

    /// <summary>
    /// Clears any dynamically spawned icon from this field.
    /// </summary>
    public void ClearIcon()
    {
        if (spawnedIcon != null)
        {
            Destroy(spawnedIcon);
            spawnedIcon = null;
        }
    }

    public void Deselect()
    {
        selectFeedback.SetActive(false);
        ResetArrowVisual();
        ClearIcon();
    }

    /// <summary>
    /// Hides the arrow mesh renderer while keeping selectFeedback active for click detection.
    /// </summary>
    private void HideArrowVisual()
    {
        if (arrowRenderer != null)
        {
            arrowRenderer.enabled = false;
        }
    }

    /// <summary>
    /// Restores the arrow mesh renderer to its default visible state.
    /// </summary>
    private void ResetArrowVisual()
    {
        if (arrowRenderer != null)
        {
            arrowRenderer.enabled = true;
        }

        if (selectFeedback != null)
        {
            selectFeedback.transform.localRotation = Quaternion.identity;
        }
    }

    /// <summary>
    /// Returns the transform of the piece (online or offline) currently on this field.
    /// </summary>
    private Transform GetPieceTransform()
    {
        if (piece != null) return piece.transform;
        if (offlinePiece != null) return offlinePiece.transform;
        return null;
    }
}
