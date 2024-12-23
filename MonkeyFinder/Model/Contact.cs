using System.Text.Json.Serialization;

namespace MonkeyFinder.Model;

public class Monkey
{
    public string name { get; set; }
    public string phone { get; set; }
    public string email { get; set; }
    public string photo { get; set; }
    public string group { get; set; }
    public string description { get; set; }

    public string user { get; set; }

    public string id { get; set; }

    public string userId { get; set; } = "1";
}

[JsonSerializable(typeof(List<Monkey>))]
internal sealed partial class MonkeyContext : JsonSerializerContext{

}