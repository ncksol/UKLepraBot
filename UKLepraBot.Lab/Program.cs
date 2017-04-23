using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace UKLepraBot.Lab
{
    class Program
    {
        static void Main(string[] args)
        {
            var str = @"{
  ""items"": [
    {
      ""triggers"": [ ""путин"" ],
      ""replies"": [ { ""text"": ""путин - хуютин"" } ]
    },
    {
      ""triggers"": [ ""корбин"" ],
      ""replies"": [ { ""text"": ""корбин - хуёрбин"" } ]
    },
    {
      ""triggers"": [ ""навальный"" ],
      ""replies"": [ { ""text"": ""навальный - овальный"" } ]
    },
    {
      ""triggers"": [ ""слава роботам"" ],
      ""replies"": [
        { ""text"": ""Воистину слава!"" },
        { ""sticker"": ""BQADBAADHQADmDVxAh2h6gc7L-sLAg"" }
      ]
    },
    {
      ""isalwaysreply"": ""true"",
      ""triggers"": [ ""пошел нахуй"", ""иди нахуй"", ""охуел"", ""пошёл нахуй"", ""нахуй пойди"", ""иди в пизду"", ""сука"" ],
      ""replies"": [
        { ""text"": ""отвали козлина!"" }
      ]
    }
  ]
}
";
            var a = JsonConvert.DeserializeObject<ReactionsList>(str);
        }
    }
    
}
