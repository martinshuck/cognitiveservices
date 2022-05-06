using Azure;
using Azure.AI.TextAnalytics;

namespace ASB.AI.Demo.Langugage
{
    public class Program
    {
        static readonly string SUBSCRIPTION_KEY =
             Environment.GetEnvironmentVariable("AZURE_TEXT_ANALYTICS_KEY");

        static readonly string ENDPOINT =
            Environment.GetEnvironmentVariable("AZURE_TEXT_ANALYTICS_ENDPOINT");

        public static async Task Main()
        {
            var client = Authenticate(ENDPOINT, SUBSCRIPTION_KEY);

            string documentA = @"ASB supports #GooglePay  I can always put in a good word 😉#asbbank";

            string documentB = @"ASB  Bank in NZ got most pathetic service . never responds to even complaints . stay away from this rubbish #ASBBank";

            string documentC = @"I'm much more confident with crypto than with banks or fiat currency because I can actually control it, and the money supply is transparent, stated up front #cryptocurrencies  #Anzbank #ASBbank";

            string documentD = @"#westpac #anzbank #asbbank #kiwibank #westerunion. Your services are no longer required. We’ll take over from here @defiwallet1";

            var documents = new List<string>
            {
                documentA,
                documentB,
                documentC,
                documentD
            };

            AnalyzeSentiment(client, documents);
        }

        private static TextAnalyticsClient Authenticate(string endpoint, string key)
        {
            return new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(key));
        }


        public static async Task AnalyzeSentiment(TextAnalyticsClient client, List<string> documents)
        {
            Response<AnalyzeSentimentResultCollection> response = client.AnalyzeSentimentBatch(documents);
            AnalyzeSentimentResultCollection sentimentPerDocuments = response.Value;

            int i = 0;
            Console.WriteLine($"Results of Azure Text Analytics \"Sentiment Analysis\" Model, version: \"{sentimentPerDocuments.ModelVersion}\"");
            Console.WriteLine("");

            foreach (AnalyzeSentimentResult sentimentInDocument in sentimentPerDocuments)
            {
                Console.WriteLine($"On document with Text: \"{documents[i++]}\"");
                Console.WriteLine("");

                if (sentimentInDocument.HasError)
                {
                    Console.WriteLine("  Error!");
                    Console.WriteLine($"  Document error: {sentimentInDocument.Error.ErrorCode}.");
                    Console.WriteLine($"  Message: {sentimentInDocument.Error.Message}");
                }
                else
                {
                    Console.WriteLine($"Document sentiment is {sentimentInDocument.DocumentSentiment.Sentiment}, with confidence scores: ");
                    Console.WriteLine($"  Positive confidence score: {sentimentInDocument.DocumentSentiment.ConfidenceScores.Positive}.");
                    Console.WriteLine($"  Neutral confidence score: {sentimentInDocument.DocumentSentiment.ConfidenceScores.Neutral}.");
                    Console.WriteLine($"  Negative confidence score: {sentimentInDocument.DocumentSentiment.ConfidenceScores.Negative}.");
                    Console.WriteLine("");
                    Console.WriteLine($"  Sentence sentiment results:");

                    foreach (SentenceSentiment sentimentInSentence in sentimentInDocument.DocumentSentiment.Sentences)
                    {
                        Console.WriteLine($"  For sentence: \"{sentimentInSentence.Text}\"");
                        Console.WriteLine($"  Sentiment is {sentimentInSentence.Sentiment}, with confidence scores: ");
                        Console.WriteLine($"    Positive confidence score: {sentimentInSentence.ConfidenceScores.Positive}.");
                        Console.WriteLine($"    Neutral confidence score: {sentimentInSentence.ConfidenceScores.Neutral}.");
                        Console.WriteLine($"    Negative confidence score: {sentimentInSentence.ConfidenceScores.Negative}.");
                        Console.WriteLine("");
                    }
                }
                Console.WriteLine("");
            }
        }

    }
}