namespace Minglouguli.DocViewExample.Entitys;

[DocvTable(schema: "docviewdb", tagName: "ArticleTag")]
public class ArticleCategory
{
    public long Id { get; set; }

    public string Title { get; set; }
}
