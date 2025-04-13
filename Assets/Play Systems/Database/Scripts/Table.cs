using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

namespace Playsystems
{
    public class Table : MonoBehaviour
    {

        string Tablename;
        string Root;

        public string[] Fields;

        string[] Records;

        [SerializeField]
        bool bDeleteTablet = false;

        XmlDocument xmlDoc = new XmlDocument();

        // Use this for initialization
        void Start()
        {
            Tablename = "tb_" + gameObject.name;

            Root = gameObject.name;

            int iFildsCount = Fields.Length;

            Records = new string[iFildsCount];

            if (bDeleteTablet)
            {
                if (PlayerPrefs.HasKey(Tablename) == true)
                {
                    PlayerPrefs.DeleteKey(Tablename);
                }
            }

            CreateTable();

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void CreateTable()
        {
            string sXmlBase = "";

            sXmlBase = "<" + Tablename + ">";
            sXmlBase = sXmlBase + "<" + Root + ">";
            sXmlBase = sXmlBase + "<id>0</id>";            

            foreach(string sField in Fields)
            {
                sXmlBase = sXmlBase + "<" + sField + ">" + "" + "</" + sField + ">";
            }

            sXmlBase = sXmlBase + "</" + Root + ">";
            sXmlBase = sXmlBase + "</" + Tablename + ">";


            if (PlayerPrefs.HasKey(Tablename) == false)
            {
                PlayerPrefs.SetString(Tablename, sXmlBase);
                PlayerPrefs.Save();

                Debug.Log("Saved " + Tablename + " in PlayerPrefs");

                Debug.Log("sXmlBase: " + sXmlBase);
            }
            else
            {                

                sXmlBase = PlayerPrefs.GetString(Tablename).ToString();

                xmlDoc.LoadXml(sXmlBase);              

                Debug.Log("XML  = " + xmlDoc.InnerXml);
            }            

        }

        public bool InsertToTable()
        {
            bool bSave = false;

            if (PlayerPrefs.HasKey(Tablename) == true)
            {

                string sXmlBase = "";

                sXmlBase = PlayerPrefs.GetString(Tablename).ToString();

                xmlDoc.LoadXml(sXmlBase);

                Debug.Log("XML  = " + xmlDoc.InnerXml);                

                XmlNodeList xnList = xmlDoc.GetElementsByTagName(Root);

                int iCountRegisters = xnList.Count - 1;

                Debug.Log("iCountRegisters  = " + iCountRegisters);

                int iId = int.Parse(xnList.Item(iCountRegisters)["id"].InnerText);                

                Debug.Log("iId  = " + iId);

                XmlNode XmlRoot = xmlDoc.CreateElement(Root);
                XmlNode XmlId = xmlDoc.CreateElement("id");
                XmlId.InnerText = (iId + 1).ToString();
                XmlRoot.AppendChild(XmlId);

                int iIndexFields = 0;

                foreach (string sField in Fields)
                {
                    XmlNode xmlNode = xmlDoc.CreateElement(sField);
                    xmlNode.InnerText = Records[iIndexFields];
                    XmlRoot.AppendChild(xmlNode);
                    iIndexFields++;
                }

                xmlDoc.SelectSingleNode("/" + Tablename).AppendChild(XmlRoot);

                Debug.Log("XML  = " + xmlDoc.InnerXml);


                PlayerPrefs.SetString(Tablename, xmlDoc.InnerXml.ToString());
                PlayerPrefs.Save();

                bSave = true;

                //Debug.Log("Adicionou dados PlayerPrefs");
            }

            return bSave;
        }

        public void AddRecord(int iField, string sData)
        {

            Debug.Log("Records count = " + Records.Length);

            Records[iField] = sData;

            Debug.Log("Records[iField] = " + Records[iField]);

            foreach (string sRecord in Records)
            {
                Debug.Log("Records = " + sRecord);
            }
         }

        public void AddRecord(string sField, string sData)
        {

            int iIndex = 0;

            foreach (string sfield in Fields)
            {               
                if (sfield== sField)
                {                    

                    Records[iIndex] = sData;
                    Debug.Log("Records[iIndex] = " + Records[iIndex].ToString());
                }

                Debug.Log("sfield = " + sfield);
                Debug.Log("iIndex = " + iIndex);

                iIndex++;

            }            

            foreach (string sRecord in Records)
            {
                Debug.Log("Records = " + sRecord);
            
            }
            
        }

        public string GetRecord(string Field, int Row)
        {
            string sRecord = "";

            string sXmlBase = "";            

            XmlDocument xmlDoc = new XmlDocument();

            if (PlayerPrefs.HasKey(Tablename) == true)
            {

                sXmlBase = PlayerPrefs.GetString(Tablename).ToString();

                xmlDoc.LoadXml(sXmlBase);

                XmlNodeList xnList = xmlDoc.GetElementsByTagName(Root);

                int iIndex = 1;
                int iIndexRow = 0;
                int iIndexField = 0;

                foreach (string sfield in Fields)
                {
                    if (sfield == Field)
                    {
                        iIndexField = iIndex;
                    }

                    iIndex++;

                    Debug.Log("sfield = " + sfield);
                    Debug.Log("iIndex = " + iIndex);

                }

                foreach (XmlNode xn in xmlDoc.DocumentElement.ChildNodes)
                {

                    if (Row == iIndexRow)
                    {
                        if (xnList.Item(iIndexRow).InnerText != "")
                            sRecord = xn.ChildNodes.Item(iIndexField).InnerText;
                    }

                    iIndexRow++;
                }               

            }

            Debug.Log("sRecord = " + sRecord);

            return sRecord;
        }


        public void EditRecord(string Field, int Record, string Value)
        {
            string sXmlBase = "";            

            //sXmlBase = PlayerPrefs.GetString(Tablename).ToString();

            sXmlBase = xmlDoc.InnerXml;

            xmlDoc.LoadXml(sXmlBase);

            XmlNodeList xnList = xmlDoc.GetElementsByTagName(Root);

            int iIndex = 1;
            int iIndexRow = 0;
            int iIndexField = 0;

            foreach (string sfield in Fields)
            {
                if (sfield == Field)
                {
                    iIndexField = iIndex;
                }

                iIndex++;

                Debug.Log("sfield = " + sfield);
                Debug.Log("iIndex = " + iIndex);

            }

            foreach (XmlNode xn in xmlDoc.DocumentElement.ChildNodes)
            {

                if (Record == iIndexRow)
                {
                    if (xnList.Item(iIndexRow).InnerText != "")
                    {
                        xn.ChildNodes.Item(iIndexField).InnerText = Value;
                        break;
                    }
                }

                iIndexRow++;
            }

            Debug.Log("xmlDoc.InnerXml.ToString() = " + xmlDoc.InnerXml.ToString());

        }

        public bool UpdateTable()
        {
            bool bSave = false;

            Debug.Log("xmlDoc.InnerXml.ToString() = " + xmlDoc.InnerXml.ToString());

            PlayerPrefs.SetString(Tablename, xmlDoc.InnerXml.ToString());
            PlayerPrefs.Save();

            bSave = true;

            return bSave;

        }

        public string[,] GetTable ()
        {

            string[,] sTabletArray;            

            string sXmlBase = "";

            XmlDocument xmlDoc = new XmlDocument();

            sXmlBase = PlayerPrefs.GetString(Tablename).ToString();

            xmlDoc.LoadXml(sXmlBase);

            XmlNodeList xnList = xmlDoc.GetElementsByTagName(Root);

            int iRowsCount = xmlDoc.DocumentElement.ChildNodes.Count;
            int iIndexRow = 0;
            int iIndexField = 0;

            sTabletArray = new string[Fields.Length, iRowsCount];

            foreach (string sfield in Fields)
            {

                foreach (XmlNode xn in xmlDoc.DocumentElement.ChildNodes)
                {

                    if (iIndexRow < iRowsCount)
                    {
                        sTabletArray[iIndexField, iIndexRow] = xn.ChildNodes.Item(iIndexField).InnerText;

                        Debug.Log("sTabletArray[" + iIndexField + "," + iIndexRow + "] = " + sTabletArray[iIndexField, iIndexRow]);

                        iIndexRow++;
                    }

                    if (iIndexRow == iRowsCount)
                    {
                        iIndexRow = 0;
                    }


                }

                iIndexField++;
            }


            return sTabletArray;
        }

        public int Count()
        {
            int iRowsCount = 0;

            string sXmlBase = "";

            XmlDocument xmlDoc = new XmlDocument();

            sXmlBase = PlayerPrefs.GetString(Tablename).ToString();

            if (sXmlBase != "")
            {
                xmlDoc.LoadXml(sXmlBase);

                XmlNodeList xnList = xmlDoc.GetElementsByTagName(Root);

                iRowsCount = xmlDoc.DocumentElement.ChildNodes.Count;
            }

            return iRowsCount;
        }

        public bool Loaded()
        {
            bool bload = false;

            if(Count()>0)
            {
                bload = true;
            }

            return bload;
        }

        public void DeleteTable()
        {
            if (PlayerPrefs.HasKey(Tablename) == true)
            {
                PlayerPrefs.DeleteKey(Tablename);
            }  
        }

        

    }
}
