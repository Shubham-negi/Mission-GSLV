using UnityEngine;
using UnityEditor;

using System.IO;
using System.Xml;

public class QwrNoloAndroidConfig
{
    public int callbackOrder
    {
        get { return 0; }
    }

    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.Android)
        {
            // OnPreprocessBuildAndroid(target, path);
        }
    }

    public void OnPostprocessBuild(BuildTarget target, string path)
    {

    }

    struct AndroidAttr
    {
        public string name;
        public string value;

        public AndroidAttr(string n, string v)
        {
            name = n;
            value = v;
        }
    }

    static AndroidAttr[] androidAttrs = new AndroidAttr[4] 
    {
        new AndroidAttr("com.nolovr.client.vr.appType", "3D"),
        new AndroidAttr("com.nolovr.client.vr.appData", "https://appservices.nolovr.com/nolohome/"),
        new AndroidAttr("com.nolovr.client.vr.appLog", "off"),
        new AndroidAttr("com.nolovr.client.vr.dataSource","dataPush")
    };

    public static void OnNoloSelected(bool selected)
    {
        // Read xml
        string xmlPath = Application.dataPath + "/Plugins/Android/AndroidManifest.xml";
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlPath);

        XmlNode appNode = xmlDoc.SelectSingleNode("/manifest/application");
        var nsURL = appNode.GetNamespaceOfPrefix("android");

        bool updated = false;

        for (int i = 0; i < androidAttrs.Length; ++i)
        {
            AndroidAttr attr = androidAttrs[i];
            XmlNode node = FindNode(xmlDoc, "/manifest/application/meta-data", "android:name", attr.name);
            if (selected)
            {
                if (node == null)
                {
                    var nodeMetaData = xmlDoc.CreateNode(XmlNodeType.Element, "meta-data", "");
                    var attrName = xmlDoc.CreateNode(XmlNodeType.Attribute, "name", nsURL);
                    attrName.Value = attr.name;
                    nodeMetaData.Attributes.SetNamedItem(attrName);

                    var attrValue = xmlDoc.CreateNode(XmlNodeType.Attribute, "value", nsURL);
                    attrValue.Value = attr.value;
                    nodeMetaData.Attributes.SetNamedItem(attrValue);

                    appNode.AppendChild(nodeMetaData);
                    updated = true;
                }
            }
            else
            {
                if(node != null)
                {
                    appNode.RemoveChild(node);
                    updated = true;
                }
            }
        }

        if (updated)
        {
            xmlDoc.Save(xmlPath);
            AssetDatabase.Refresh();
        }
    }

    static XmlNode FindNode(XmlDocument xmlDoc, string xpath, string attributeName, string attributeValue)
    {
        XmlNodeList nodes = xmlDoc.SelectNodes(xpath);
        //Debug.Log(nodes.Count);
        for (int i = 0; i < nodes.Count; i++)
        {
            XmlNode node = nodes.Item(i);

            XmlAttribute attr = node.Attributes[attributeName];
            if (attr == null)
                continue;

            string value = attr.Value;
            if (value == attributeValue)
            {
                return node;
            }
        }
        return null;
    }
}
