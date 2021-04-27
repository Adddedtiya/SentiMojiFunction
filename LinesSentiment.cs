using Newtonsoft.Json;

namespace SentiMojiFunction
{
    class LinesSentiment
    {
        
        public LinesSentiment(string text, string emoji)
        {
            _text = text;
            _emoji = emoji;
        }

        [JsonProperty("Text")]
        readonly string _text;

        [JsonProperty("Emoji")]
        readonly string _emoji;
    }
}
