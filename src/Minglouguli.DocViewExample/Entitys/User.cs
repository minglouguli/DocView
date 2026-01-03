namespace Minglouguli.DocViewExample.Entitys;

[DocvTable(tagName: "UserTag")]
public class User
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public required string Account { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public required string NickName { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// 添加时间
    /// </summary>
    public long AddTime { get; set; }

}
