using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class ForceText : MonoBehaviour
{
    [SerializeField]
    private string txtForce;

    public string force
    {
        set
        {
            txtForce = string.IsNullOrEmpty(value) ? "" : value.Substring(0, 1);
            textMesh ??= GetComponent<TextMesh>();
            UpdateText();
        }
    }
    private TextMesh textMesh;

    void Awake()
    {
        textMesh ??= GetComponent<TextMesh>();
        UpdateText();
    }

    public void SetForceText(string force)
    {
        this.force = force;
    }

    void UpdateText()
    {
        bool txtEmpty = txtForce == "B" || txtForce != "F";
        textMesh.text = txtEmpty ? "" : txtForce;
    }
}
