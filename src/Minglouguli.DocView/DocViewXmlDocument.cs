using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Minglouguli.DocView;


public class DocViewXmlDocument
{
    public DocViewXmlDocument(IEnumerable<string> xmlPaths)
    {
        if (xmlPaths == null)
        {
            xmlPaths = new List<string>();
        }
        this.xmlpaths = xmlPaths;
        this.doc();
    }
    private IEnumerable<string> xmlpaths;

    private List<XmlElement> _elementsList;

    private void doc()
    {
        var elementsList = new List<XmlElement>();
        foreach (var xmlpath in this.xmlpaths)
        {
            if (System.IO.File.Exists(xmlpath))
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(xmlpath);
                var elements = xmldoc.SelectNodes("//member");

                foreach (XmlElement item in elements)
                {
                    elementsList.Add(item);
                }
            }

        }
        this._elementsList = elementsList;
        //  var tableDescription = elementsList.FirstOrDefault(w => string.Equals(w.Attributes["name"].Value, ("T:JYOnline.Entity." + item), StringComparison.CurrentCultureIgnoreCase));

    }

    public string GetObjectDescription(string classFullName)
    {
        if(string.IsNullOrEmpty(classFullName))
        {
            return string.Empty;
        }
        var pro = this._elementsList.FirstOrDefault(w => string.Equals(w.Attributes["name"].Value, ("T:" + classFullName), StringComparison.CurrentCultureIgnoreCase));
        if (pro != null)
        {
            var ss = pro.SelectSingleNode("summary");
            if (ss != null)
            {
                return ss.InnerText.Trim();
            }
        }
        return string.Empty;
    }

    public string GetProDescription(string classFullName, string proName)
    {
        var pro = this._elementsList.FirstOrDefault(w => string.Equals(w.Attributes["name"].Value, ("P:" + classFullName + "." + proName), StringComparison.CurrentCultureIgnoreCase));
        if (pro != null)
        {
            var ss = pro.SelectSingleNode("summary");
            if (ss != null)
            {
                return ss.InnerText.Trim();
            }
        }

        return "";
    }


    public string GetEnumDescription(string classFullName, string proName)
    {
        var pro = this._elementsList.FirstOrDefault(w => string.Equals(w.Attributes["name"].Value, ("F:" + classFullName + "." + proName), StringComparison.CurrentCultureIgnoreCase));
        if (pro != null)
        {
            var ss = pro.SelectSingleNode("summary");
            if (ss != null)
            {
                return ss.InnerText.Trim();
            }
        }

        return "";
    }

    public string GetMethodDescription(string classFullName, string methodName, bool hasParams = true)
    {
        if (methodName == "Authorization")
        {

        }
        XmlElement pro;
        if (hasParams)
        {
            pro = this._elementsList.FirstOrDefault(w => w.Attributes["name"].Value.StartsWith(("M:" + classFullName + "." + methodName + "("), true, null));

        }
        else
        {
            pro = this._elementsList.FirstOrDefault(w => w.Attributes["name"].Value.Equals(("M:" + classFullName + "." + methodName), StringComparison.InvariantCultureIgnoreCase));
        }
        if (pro != null)
        {
            var ss = pro.SelectSingleNode("summary");
            if (ss != null)
            {
                return ss.InnerText.Trim();
            }
        }
        return "";
    }

    public string GetParamDescription(string classFullName, string methodName, string pamamName)
    {
        var pro = this._elementsList.FirstOrDefault(w => w.Attributes["name"].Value.StartsWith(("M:" + classFullName + "." + methodName + "("), true, null));
        if (pro != null)
        {
            var ss = pro.SelectNodes("param");
            if (ss != null && ss.Count > 0)
            {
                foreach (XmlElement item in ss)
                {

                    if (item.Attributes["name"].Value == pamamName)
                    {
                        return item.InnerText.Trim();
                    }
                }
            }
        }
        return "";
    }

}