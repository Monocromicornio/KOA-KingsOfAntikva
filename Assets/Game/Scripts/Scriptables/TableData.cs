using UnityEngine;
using System.Xml;

[CreateAssetMenu(fileName = "NewTableData", menuName = "KOA/TableData")]
public class TableData : ScriptableObject
{
    [SerializeField]
    private string tableName; // The name of the table, used as the key in PlayerPrefs.

    [SerializeField]
    private string rootName; // The root element name in the XML structure.

    [SerializeField]
    private string[] fields; // An array of field names (columns) for the table.

    [SerializeField]
    private string[] records; // An array of values representing a single record (row) in the table.

    private XmlDocument xmlDoc = new XmlDocument(); // In-memory representation of the XML data.

    /// <summary>
    /// Initializes the table with a name, root element, and field names.
    /// </summary>
    public void Initialize(string tableName, string rootName, string[] fields)
    {
        this.tableName = tableName;
        this.rootName = rootName;
        this.fields = fields;
        records = new string[fields.Length];
    }

    /// <summary>
    /// Creates and saves the table structure in PlayerPrefs as an XML string.
    /// </summary>
    public void SaveTable()
    {
        string sXmlBase = $"<{tableName}><{rootName}><id>0</id>";

        foreach (string field in fields)
        {
            sXmlBase += $"<{field}></{field}>";
        }

        sXmlBase += $"</{rootName}></{tableName}>";

        PlayerPrefs.SetString(tableName, sXmlBase);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the table data from PlayerPrefs into the xmlDoc.
    /// </summary>
    public void LoadTable()
    {
        if (PlayerPrefs.HasKey(tableName))
        {
            string sXmlBase = PlayerPrefs.GetString(tableName);
            xmlDoc.LoadXml(sXmlBase);
        }
        else
        {
            Debug.LogWarning($"Table {tableName} not found in PlayerPrefs.");
        }
    }

    /// <summary>
    /// Updates the XML data in PlayerPrefs with the current state of xmlDoc.
    /// </summary>
    public void UpdateTable()
    {
        if (xmlDoc != null)
        {
            PlayerPrefs.SetString(tableName, xmlDoc.InnerXml);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogWarning("No XML data to update.");
        }
    }

    /// <summary>
    /// Adds a new record (row) to the table.
    /// </summary>
    /// <param name="newRecord">An array of values corresponding to the fields.</param>
    /// <example>
    /// Example usage:
    /// <code>
    /// string[] newRecord = { "John", "Doe", "30" };
    /// tableData.AddRecord(newRecord);
    /// </code>
    /// </example>
    public void AddRecord(string[] newRecord)
    {
        if (newRecord.Length != fields.Length)
        {
            Debug.LogError("Record length does not match the number of fields.");
            return;
        }

        XmlNode rootNode = xmlDoc.SelectSingleNode($"/{tableName}");
        XmlNode newRecordNode = xmlDoc.CreateElement(rootName);

        XmlNode idNode = xmlDoc.CreateElement("id");
        idNode.InnerText = (rootNode.ChildNodes.Count + 1).ToString();
        newRecordNode.AppendChild(idNode);

        for (int i = 0; i < fields.Length; i++)
        {
            XmlNode fieldNode = xmlDoc.CreateElement(fields[i]);
            fieldNode.InnerText = newRecord[i];
            newRecordNode.AppendChild(fieldNode);
        }

        rootNode.AppendChild(newRecordNode);
        UpdateTable();
    }

    /// <summary>
    /// Retrieves the value of a specific field in a specific row.
    /// </summary>
    /// <param name="field">The name of the field.</param>
    /// <param name="row">The row index.</param>
    /// <returns>The value of the field in the specified row, or null if not found.</returns>
    public string GetRecord(string field, int row)
    {
        if (!PlayerPrefs.HasKey(tableName)) return null;

        string sXmlBase = PlayerPrefs.GetString(tableName);
        xmlDoc.LoadXml(sXmlBase);

        XmlNodeList xnList = xmlDoc.GetElementsByTagName(rootName);

        if (row >= xnList.Count) return null;

        XmlNode rowNode = xnList[row];
        foreach (XmlNode childNode in rowNode.ChildNodes)
        {
            if (childNode.Name == field)
            {
                return childNode.InnerText;
            }
        }

        return null;
    }

    /// <summary>
    /// Edits the value of a specific field in a specific record.
    /// </summary>
    /// <param name="field">The name of the field.</param>
    /// <param name="record">The record index.</param>
    /// <param name="value">The new value for the field.</param>
    /// <example>
    /// Example usage:
    /// <code>
    /// tableData.EditRecord("Age", 0, "31");
    /// </code>
    /// </example>
    public void EditRecord(string field, int record, string value)
    {
        if (!PlayerPrefs.HasKey(tableName)) return;

        string sXmlBase = PlayerPrefs.GetString(tableName);
        xmlDoc.LoadXml(sXmlBase);

        XmlNodeList xnList = xmlDoc.GetElementsByTagName(rootName);

        if (record >= xnList.Count) return;

        XmlNode rowNode = xnList[record];
        foreach (XmlNode childNode in rowNode.ChildNodes)
        {
            if (childNode.Name == field)
            {
                childNode.InnerText = value;
                break;
            }
        }

        UpdateTable();
    }

    /// <summary>
    /// Retrieves the entire table as a 2D array of strings.
    /// </summary>
    /// <returns>A 2D array where rows represent records and columns represent fields.</returns>
    public string[,] GetTable()
    {
        if (!PlayerPrefs.HasKey(tableName)) return null;

        string sXmlBase = PlayerPrefs.GetString(tableName);
        xmlDoc.LoadXml(sXmlBase);

        XmlNodeList xnList = xmlDoc.GetElementsByTagName(rootName);

        int rows = xnList.Count;
        int cols = fields.Length;

        string[,] tableArray = new string[cols, rows];

        for (int i = 0; i < rows; i++)
        {
            XmlNode rowNode = xnList[i];
            for (int j = 0; j < cols; j++)
            {
                tableArray[j, i] = rowNode[fields[j]]?.InnerText ?? "";
            }
        }

        return tableArray;
    }

    /// <summary>
    /// Counts the number of records (rows) in the table.
    /// </summary>
    /// <returns>The number of records in the table.</returns>
    public int Count()
    {
        if (!PlayerPrefs.HasKey(tableName)) return 0;

        string sXmlBase = PlayerPrefs.GetString(tableName);
        xmlDoc.LoadXml(sXmlBase);

        XmlNodeList xnList = xmlDoc.GetElementsByTagName(rootName);

        return xnList.Count;
    }

    /// <summary>
    /// Checks if the table has been successfully loaded.
    /// </summary>
    /// <returns>True if the table has records, false otherwise.</returns>
    public bool Loaded()
    {
        return Count() > 0;
    }

    /// <summary>
    /// Deletes the table from PlayerPrefs.
    /// </summary>
    public void DeleteTable()
    {
        if (PlayerPrefs.HasKey(tableName))
        {
            PlayerPrefs.DeleteKey(tableName);
        }
    }
}