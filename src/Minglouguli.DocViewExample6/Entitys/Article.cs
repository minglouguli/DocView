
namespace Minglouguli.DocViewExample.Entitys;

[DocvTable(tagName:"ArticleTag")]
public class Article
{
    public long Id { get; set; }

    public string Title { get; set; }

    public long AddTime { get; set; }

    [DocvForeignTable(typeof(User))]
    public long UserId { get; set; }

    [DocvForeignTable(typeof(ArticleCategory))]
    public long ArticleCategoryId { get; set; }

    public string? Content { get; set; }
}
