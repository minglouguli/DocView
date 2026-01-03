using System.Reflection;

namespace Minglouguli.DocView;



public class DocViewService
{
    private readonly DocViewServiceOptions _options;

    public DocViewService(DocViewServiceOptions options)
    {
        this._options = options;
        this._dicEnumDes = new Dictionary<string, List<DocvEnumInfo>>();
    }

    private List<DocvTableEntity> GetAllMgDocTableEntity()
    {
        return this._options.EntityTypes.Select(s => this.GetMgDocTableEntity(s)).ToList();
    }

    private DocvTableEntity GetMgDocTableEntity(Type type)
    {
        var entity = new DocvTableEntity
        {
            Schema = this._options.DefaultSchema,
            TagName = this._options.DefaultTagName,
            Name = type.Name,
            FullName = type.FullName,
            Type = type,
        };
        var tableAttribute = type.GetCustomAttribute<DocvTableAttribute>();
        if (tableAttribute != null)
        {
            if (!string.IsNullOrEmpty(tableAttribute.TagName))
            {
                entity.TagName = tableAttribute.TagName;

            }
            if (!string.IsNullOrEmpty(tableAttribute.Schema))
            {
                entity.Schema = tableAttribute.Schema;
            }
        }


        entity.Description = this._options.DocViewXmlDocument?.GetObjectDescription(entity.FullName);


        var propertys = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField);
        int pos = 0;
        entity.Properties = propertys.Select(s =>
        {
            var pro = new DocvTableProperty
            {
                Name = s.Name,
                Type = s.PropertyType,
                TypeName = s.PropertyType.Name,
                Position = ++pos
            };

            if (this._options.DocViewXmlDocument != null)
            {

            }
            var comment = this._options.DocViewXmlDocument?.GetProDescription(entity.FullName, pro.Name);
            if (!string.IsNullOrEmpty(comment))
            {
                pro.Description = comment;
            }
            DocvForeignTableAttribute foreignTable = null;
#if NET8_0_OR_GREATER
            var foreignTable1 = s.GetCustomAttribute(typeof(DocvForeignTableAttribute<>));

            if (foreignTable1 is DocvForeignTableAttribute ft)
            {
                foreignTable = ft;
            }
#endif
            if (foreignTable == null)
            {
                foreignTable = s.GetCustomAttribute<DocvForeignTableAttribute>();
            }
            if (foreignTable != null)
            {
                pro.ForeignTable = this.GetMgDocForeignTableEntity(pro, foreignTable.Type, foreignTable.IsMulti);
            }
            else if (s.Name.EndsWith("Id") && s.Name.Length > 2)
            {
                var mayTableName = s.Name.Substring(0, s.Name.Length - 2);
                var mayTableType = this._options.EntityTypes.FirstOrDefault(f => f.Name == mayTableName);
                if (mayTableType != null)
                {
                    var fo = GetMgDocForeignTableEntity(pro, mayTableType);
                    if (fo != null)
                    {
                        pro.ForeignTable = fo;
                    }
                }
            }

            if (s.PropertyType.IsEnum)
            {
                var enumList = this.GetEnumDes(s.PropertyType);
                if (enumList?.Count > 0)
                {
                    pro.SubDescription = string.Join("; ", enumList.Select(s => string.IsNullOrEmpty(s.Des) ? $"{s.Name}:{s.Value}" : $"{s.Name}({s.Des}):{s.Value}"));
                }
                //   var enumDes = this._options.DocViewXmlDocument?.GetProDescription(entity.FullName, pro.Name);
            }

            return pro;
        }).ToList();

        return entity;
    }


    private Dictionary<string, List<DocvEnumInfo>> _dicEnumDes { get; set; }

    private List<DocvEnumInfo> GetEnumDes(Type enumType)
    {
        if (_dicEnumDes.ContainsKey(enumType.FullName))
        {
            return _dicEnumDes[enumType.FullName];
        }
        var result = new List<DocvEnumInfo>();
        if (enumType.IsEnum)
        {
            var fields = enumType.GetFields();
            foreach (var field in fields)
            {
                if (field.FieldType.IsEnum)
                {
                    result.Add(new DocvEnumInfo
                    {
                        Name = field.Name,
                        Des = this._options.DocViewXmlDocument?.GetEnumDescription(enumType.FullName, field.Name),
                        Value = (int)enumType.InvokeMember(field.Name, BindingFlags.GetField, null, null, null)
                    });
                }
            }
        }
        _dicEnumDes.Add(enumType.FullName, result);
        return result;
    }

    private DocvForeignTableEntity GetMgDocForeignTableEntity(DocvTableProperty property, Type type, bool? isMulti = null)
    {
        var entity = new DocvForeignTableEntity
        {
            Schema = this._options.DefaultSchema,
            TagName = this._options.DefaultTagName,
            Name = type.Name,
            FullName = type.FullName,
            Type = type,
            IsMult = isMulti
        };

        var tableAttribute = type.GetCustomAttribute<DocvTableAttribute>();
        if (tableAttribute != null)
        {
            if (!string.IsNullOrEmpty(tableAttribute.TagName))
            {
                entity.TagName = tableAttribute.TagName;

            }
            if (!string.IsNullOrEmpty(tableAttribute.Schema))
            {
                entity.Schema = tableAttribute.Schema;
            }
        }

        var propertys = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField);
        if (propertys?.Length > 0)
        {
            entity.PrimaryKeys = new List<string>();
            string hasIdkey = string.Empty;
            foreach (var pro in propertys)
            {
                if (pro.Name.ToLower() == "id")
                {
                    hasIdkey = pro.Name;
                }
                var pri = pro.GetCustomAttribute<DocvPrimaryKeyAttribute>();
                if (pri != null)
                {
                    entity.PrimaryKeys.Add(pro.Name);
                }
            }
            if (entity.PrimaryKeys.Count == 0)
            {
                entity.PrimaryKeys.Add(hasIdkey);
            }
        }

        return entity;
    }

    private List<DataBaseDbColumnTable> _dataBaseDbColumnTable;

    private List<DataBaseDbColumnTable> DataBaseDbColumnTable
    {
        get
        {
            return _dataBaseDbColumnTable;
        }
    }

    public void LoadDataBaseDbColumnTable(List<DataBaseDbColumnTable> list)
    {
        _dataBaseDbColumnTable = list;
    }

    public void Get(List<DataBaseDbColumnTable> list)
    {
        var entityList = this.GetAllMgDocTableEntity();
        var tableList = list.GroupBy(g => new
        {
            g.TABLE_SCHEMA,
            g.TABLE_NAME
        })
           .Select(s =>
           {
               string tableDes = null;

               var table = new DocvTableEntity
               {
                   Schema = s.Key.TABLE_SCHEMA,
                   Name = s.Key.TABLE_NAME,
               };
               var entity = entityList.FirstOrDefault(f => f.Id == table.Id);
               if (entity != null)
               {
                   table.Description = entity.Description;
               }
               table.Properties = s.Select(c =>
               {
                   var col = new DocvTableProperty
                   {
                       Name = c.COLUMN_NAME,
                       ColumnKey = c.COLUMN_KEY,
                       DbDataType = c.COLUMN_TYPE,
                       IsNull = c.IS_NULLABLE == "YES",
                       Default = c.COLUMN_DEFAULT,
                       Position = c.ORDINAL_POSITION,
                   };
                   if (entity != null)
                   {
                       var entityPro = entity.Properties.FirstOrDefault(f => f.Name == col.Name);
                       if (entityPro != null)
                       {
                           col.ForeignTable = entityPro.ForeignTable;
                           col.Type = entityPro.Type;
                           col.TypeName = entityPro.TypeName;
                           if (!string.IsNullOrEmpty(entityPro.Description))
                           {
                               col.Description = entityPro.Description;
                           }
                       }
                   }

                   return col;
               }).ToList();

               return table;
           })
           .OrderBy(g => g.TagName)
           .ThenBy(g => g.Id)
           .ToList()
           ;
    }

    public bool IsShow(DocvTableProperty s)
    {
        if (s.Name == "Id" || s.Name.Contains("Name") || s.Name.Contains("Title") || s.Name.EndsWith("Id"))
        {
            return true;
        }
        if (s.ForeignTable != null)
        {
            return true;
        }
        if (!string.IsNullOrEmpty(s.ColumnKey))
        {
            return true;
        }

        return false;
    }

    public async Task<EntityJsonData> GetEntityJsonData()
    {
        var tableList = this.GetAllMgDocTableEntity();

        var json = new EntityJsonData()
        {
            Nodes = new List<JsonNode>(),
            Connects = new List<JsonNodeConnect>()
        };
        var nodeDic = new Dictionary<string, JsonNode>();
        var connectDic = new Dictionary<string, JsonNodeConnect>();

        foreach (var item in tableList)
        {
            if (!nodeDic.ContainsKey(item.Id))
            {
                var JsonNode = new JsonNode
                {
                    Id = item.Id,
                    Schema = item.Schema,
                    Name = item.Name,
                    Des = item.Description,
                    Tag = item.TagName,
                    Properties = item.Properties.OrderBy(s => s.Position).Select(s =>
                    {
                        if (s.ForeignTable != null)
                        {
                            var fkey = $"{item.Id}-{s.Name}-{s.ForeignTable.Id}-Id";
                            if (!connectDic.ContainsKey(fkey))
                            {
                                var connect = new JsonNodeConnect
                                {
                                    Source = $"{item.Id}.{s.Name}",
                                    // Target = null,
                                    SourceNode = $"{item.Id}",
                                    TargetNode = $"{s.ForeignTable.Id}",
                                    Label = s.ForeignTable.IsMult == true ? this._options.DefaultMoreTo1Label : s.ForeignTable.IsMult == false ? this._options.Default1To1Label : null,
                                };

                                if (s.ForeignTable.PrimaryKeys?.Count > 0)
                                {
                                    connect.Target = $"{s.ForeignTable.Id}.{s.ForeignTable.PrimaryKeys[0]}";
                                }
                                else
                                {
                                    connect.Target = connect.TargetNode;
                                }

                                connectDic.Add(fkey, connect);
                                json.Connects.Add(connect);
                            }
                        }
                        return new JsonNodeProperty
                        {
                            Id = s.Name,
                            Name = s.Name,
                            Des = s.Description,
                            SubDes = s.SubDescription,
                            IsShow = IsShow(s)
                        };
                    }).ToList()
                };
                nodeDic.Add(item.Id, JsonNode);
                json.Nodes.Add(JsonNode);
            }
        }

        return json;
    }
}