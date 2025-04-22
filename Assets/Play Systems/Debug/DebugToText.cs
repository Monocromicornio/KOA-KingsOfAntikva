using UnityEngine;
using UnityEngine.UI;

public class DebugToText : MonoBehaviour
{
    [SerializeField]
    bool bDebugOn = false;

    [SerializeField]
    Text txtDebug;    

    //public string output = "";
    //public string stack = "";

    //void OnEnable()
    //{
    //    Application.logMessageReceived += HandleLog;
    //}

    //void OnDisable()
    //{
    //    Application.logMessageReceived -= HandleLog;
    //}

    //void HandleLog(string logString, string stackTrace, LogType type)
    //{
    //    output = logString;
    //    stack = stackTrace;
    //}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowDebug(string stext)
    {
        if (bDebugOn)
        {
            txtDebug.text = stext;
        }
    }
}
