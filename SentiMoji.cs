using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure;
using Azure.AI.TextAnalytics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SentiMojiFunction
{
    public static class SentiMoji
    {

        static readonly AzureKeyCredential Key = new AzureKeyCredential(Credentials.AzureTextAnalitics);
        static readonly Uri endpoint = new Uri(Credentials.AzureEndpoint);

        static readonly TextAnalyticsClient client = new TextAnalyticsClient(endpoint, Key);

        [FunctionName("SentiMoji")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req
            )
        {
            string TextInput = await DeserializerRequest(req);

            if (TextInput.Length < 3)
                return new BadRequestObjectResult("Input Text is too short (min is 3) ");
            
            if ( TextInput.Length > 5000)
                return new BadRequestObjectResult("Input Text is too long ! (Max is 5000) ");
            
            List<string> TextSplitIntoList = new List<string>();
            TextSplitIntoList.AddRange(TextInput.Split('\n'));
            TextSplitIntoList.RemoveAll(x => string.IsNullOrEmpty(x));

            if (TextSplitIntoList.Count > 10)
                return new BadRequestObjectResult("Input text has too many lines (Maximum of 10 Lines)");
            
            string[] TextSplitIntoLines = TextSplitIntoList.ToArray();

            TextSentiment Sentiment = await GetSentiment(TextInput);

            String[] TalkingPoints = await TextAnalisis(TextInput);

            String SentimentEmoji = ConvertSentimentToEmoji(Sentiment);

            LinesSentiment[] SentimentsBreakdown = await GetSentimentLines(TextSplitIntoLines);

            FunctionResult result = new FunctionResult(SentimentEmoji, TalkingPoints, SentimentsBreakdown);

            return new OkObjectResult(JsonConvert.SerializeObject(result));
        }

        private static async Task<String> DeserializerRequest(HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string x  = data?.Text;
            if (string.IsNullOrEmpty(x))
                return "";
            return x;
        }

        private static async Task<LinesSentiment[]> GetSentimentLines(string[] texts)
        {
            AnalyzeSentimentResultCollection documentSentiment = await client.AnalyzeSentimentBatchAsync(texts);

            int count = texts.Length;

            LinesSentiment[] sentiments = new LinesSentiment[count];

            for (int i = 0; i < count; i++)
            {
                LinesSentiment currentsentiment = new LinesSentiment(
                    text  : texts[i],
                    emoji : ConvertSentimentToEmoji(documentSentiment[i].DocumentSentiment.Sentiment)
                    );
                sentiments[i] = currentsentiment;
            }

            return sentiments;
        }

        private static async Task<TextSentiment> GetSentiment(string text)
        {
            DocumentSentiment documentSentiment = await client.AnalyzeSentimentAsync(text);

            return documentSentiment.Sentiment;

        }

        private static string ConvertSentimentToEmoji(TextSentiment sentiment)
        {
            string x = sentiment switch
            {
                TextSentiment.Positive => "🙂",
                TextSentiment.Neutral => "😐",
                TextSentiment.Negative => "🙁",
                TextSentiment.Mixed => "😖",
                _ => "❓",
            };
            return x;
        }


        private static async Task<string[]> TextAnalisis(string text)
        {
            var KeyPhrases = await client.ExtractKeyPhrasesAsync(text);

            List<string> tags = new List<string>();

            foreach (string key in KeyPhrases.Value)
            {
                tags.Add(key);
            }

            return tags.ToArray();
        }


    }
}
