
namespace Minglouguli.DocViewExample.Entitys;

/// <summary>
/// 文章
/// </summary>
[DocvTable(tagName: "ArticleTag")]
public class Article
{
    /// <summary>
    /// 文章ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// 添加时间
    /// </summary>
    public long AddTime { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    [DocvForeignTable(typeof(User))]
    public long UserId { get; set; }

    /// <summary>
    /// 类目ID
    /// </summary>
    [DocvForeignTable<ArticleCategory>]
    public long ArticleCategoryId { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public ArticleStatus Status { get; set; }
}

public enum ArticleStatus
{
    /// <summary>
    /// 未发布
    /// </summary>
    UnPublish = 0,

    /// <summary>
    /// 草稿
    /// </summary>
    Draft = 2,

    /// <summary>
    /// 已发布
    /// </summary>
    Published = 1
}