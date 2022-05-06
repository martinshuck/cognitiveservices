
using Microsoft.CognitiveServices.Speech;

namespace ASB.AI.Demo.Speech
{
    public class Program
    {
        static readonly string SUBSCRIPTION_KEY =
             Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY");

        static async Task Main()
        {
            var client = Authenticate(SUBSCRIPTION_KEY);

            await RecognizeSpeechAsync(client);
        }
        private static SpeechConfig Authenticate(string key)
        {
            // Creates an instance of a speech config with specified subscription key and service region.
            // Replace with your own subscription key // and service region (e.g., "westus").
            return SpeechConfig.FromSubscription(key, "australiaeast");
        }


        static async Task RecognizeSpeechAsync(SpeechConfig config)
        {
            // Creates a speech recognizer.
            using var recognizer = new SpeechRecognizer(config);

            Console.WriteLine("Say something...");

            // Starts speech recognition, and returns after a single utterance is recognized. The end of a
            // single utterance is determined by listening for silence at the end or until a maximum of 15
            // seconds of audio is processed.  The task returns the recognition text as result. 
            // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
            // shot recognition like command or query. 
            // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
            var result = await recognizer.RecognizeOnceAsync();

            // Checks result.
            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                Console.WriteLine($"We recognized: {result.Text}");
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                Console.WriteLine($"NOMATCH: Speech could not be recognized.");
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = CancellationDetails.FromResult(result);
                Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                    Console.WriteLine($"CANCELED: Did you update the subscription info?");
                }
            }
        }

    }
}