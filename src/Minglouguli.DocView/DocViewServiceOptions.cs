using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Minglouguli.DocView;
public class DocViewServiceOptions
{
    public DocViewServiceOptions()
    {
        this.EntityTypes = new List<Type>();
    }
    public string DefaultSchema { get; set; }

    public string DefaultTagName { get; set; }

    public string DefaultMoreTo1Label { get; set; }

    public string Default1To1Label { get; set; }

    internal List<string> XmlDocUrl { get; set; }


    internal DocViewXmlDocument? DocViewXmlDocument { get; set; }

    /// <summary>
    /// 加载文档文件
    /// </summary>
    /// <param name="xmlPaths"></param>
    public void AddXmlDocument(IEnumerable<string> xmlPaths)
    {
        this.DocViewXmlDocument = new DocViewXmlDocument(xmlPaths);
    }


    internal List<Type> EntityTypes { get; set; }

    public void LoadEntityTypes(List<Type> types)
    {
        if (types?.Count > 0)
        {
            this.EntityTypes.AddRange(types);
            this.EntityTypes = this.EntityTypes.Distinct().ToList();
        }
    }

    public void LoadEntityTypesByDocvTableAttribute(List<Assembly> assemblies)
    {
        List<Type> types = new List<Type>();
        foreach (Assembly assembly in assemblies)
        {
            foreach (Type type in assembly.GetTypes())
            {
                var attr = type.GetCustomAttribute<DocvTableAttribute>();
                if (attr != null)
                {
                    types.Add(type);
                }
            }
        }
        this.EntityTypes.AddRange(types);

        this.EntityTypes = this.EntityTypes.Distinct().ToList();
    }

}
