namespace Minglouguli.DocView;

public class DocvBaseTableEntity
{
    public string TagName { get; set; }
    public string Schema { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public string FullName { get; set; }

    public Type Type { get; set; }



    public string Id
    {
        get
        {
            return $"{Schema}.{Name}";
        }
    }

    public List<string> PrimaryKeys { get; set; }
}

public class DocvTableEntity : DocvBaseTableEntity
{
    public List<DocvTableProperty> Properties { get; set; }
}

public class DocvForeignTableEntity : DocvBaseTableEntity
{


    public bool? IsMult { get; set; }
}

public class DocvTableProperty
{
    public string Name { get; set; }

    public Type Type { get; set; }

    public string TypeName { get; set; }

    public string Description { get; set; }

    public string SubDescription { get; set; }

    public int Position { get; set; }

    public DocvForeignTableEntity ForeignTable { get; set; }

    public bool IsNull { get; set; }

    public string DbDataType { get; set; }

    public string ColumnKey { get; set; }

    public object Default { get; set; }
}

public class DocvEnumInfo
{
    public string Name { get; set; }

    public int Value { get; set; }

    public string? Des { get; set; }
}

//[Table(Name = "information_schema.columns")]
public class DataBaseDbColumnTable
{
    public string TABLE_CATALOG { get; set; }

    public string TABLE_SCHEMA { get; set; }

    public string TABLE_NAME { get; set; }

    public string COLUMN_NAME { get; set; }

    public int ORDINAL_POSITION { get; set; }

    public string COLUMN_DEFAULT { get; set; }

    /// <summary>
    /// YES  NO
    /// </summary>
    public string IS_NULLABLE { get; set; }

    public string DATA_TYPE { get; set; }

    public long? CHARACTER_MAXIMUM_LENGTH { get; set; }

    public long CHARACTER_OCTET_LENGTH { get; set; }

    public long NUMERIC_PRECISION { get; set; }

    public long NUMERIC_SCALE { get; set; }

    public int DATETIME_PRECISION { get; set; }

    public string CHARACTER_SET_NAME { get; set; }

    public string COLLATION_NAME { get; set; }

    public string COLUMN_TYPE { get; set; }

    public string COLUMN_KEY { get; set; }

    public string EXTRA { get; set; }

    public string PRIVILEGES { get; set; }

    public string COLUMN_COMMENT { get; set; }

    public string GENERATION_EXPRESSION { get; set; }

    public int SRS_ID { get; set; }
}

public class EntityJsonData
{
    public List<JsonNode> Nodes { get; set; }

    public List<JsonNodeConnect> Connects { get; set; }
}

public class JsonNode
{
    public string Id { get; set; }

    public string Schema { get; set; }

    public string Name { get; set; }

    public string Des { get; set; }

    public List<JsonNodeProperty> Properties { get; set; }

    public string Tag { get; set; }
}

public class JsonNodeProperty
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Des { get; set; }

    public string SubDes { get; set; }

    public bool IsPrimaryKey { get; set; }

    public bool IsForeignKey { get; set; }

    public bool IsShow { get; set; }
}

public class JsonNodeConnect
{
    public string Source { get; set; }

    public string Target { get; set; }

    public string SourceNode { get; set; }
    public string TargetNode { get; set; }

    public string? Label { get; set; }
}