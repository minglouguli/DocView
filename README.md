### 用于显示实体间的关系

### 使用
代码中进行配置
```C#
builder.Services.AddDocView(options =>
{
    options.DefaultSchema = "docviewdb";
    options.DefaultTagName = "docview";
    //实体所在的程序集里通过DocvTableAttribute特性加载实体类型
    options.LoadEntityTypesByDocvTableAttribute(new List<Assembly>
    {
        typeof(Program).Assembly
    });

    // 或直接加载对应的类型
    //options.LoadEntityTypes(new List<Type>{
    //    typeof(Article),
    //    typeof(User),
    //    typeof(ArticleCategory),
    //});

    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    var files = dir.GetFiles("*.xml");
    options.AddXmlDocument(files.Select(s => s.FullName));
});


app.UseDocViewUI(opt =>
{
  
});
```

### 实体类配置
```C#
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

```

### 访问
https://localhost:7144/docview/  访问当前项目下/docview 地址
 

<img width="2240" height="1070" alt="image" src="https://github.com/user-attachments/assets/3b87903d-701f-4501-99bd-2479a9ffac9d" />
