namespace Minglouguli.DocViewExample.Entitys;

[DocvTable(schema: "docviewdb", tagName: "ArticleTag")]
public class ArticleCategory
{
    /// <summary>
    /// 类目Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 类目标题
    /// </summary>
    public required string Title { get; set; }
}
