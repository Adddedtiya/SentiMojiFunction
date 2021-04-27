using System;
using Newtonsoft.Json;


namespace SentiMojiFunction
{
    class FunctionResult
    {

        public FunctionResult(string sentiment, string[] points, LinesSentiment[] SentimentsPerLine)
        {
            _Talking_Points = points;
            _Emoji_Sentiment = sentiment;
            _sentiments_PerLines = SentimentsPerLine;
        }

        [JsonProperty("Talking Points")]
        readonly string[] _Talking_Points;

        [JsonProperty("Overall Sentiment")]
        readonly string _Emoji_Sentiment;

        [JsonProperty("Sentiments Breakdown")]
        readonly LinesSentiment[] _sentiments_PerLines;

    }
}
