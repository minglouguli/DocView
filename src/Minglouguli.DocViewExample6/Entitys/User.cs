namespace Minglouguli.DocViewExample.Entitys;

[DocvTable(tagName: "UserTag")]
public class User
{
    public long Id { get; set; }

    public string Account { get; set; }

    public string NickName { get; set; }

    public string? Avatar { get; set; }

    public long AddTime { get; set; }

}
