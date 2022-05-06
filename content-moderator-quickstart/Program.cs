using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Azure.CognitiveServices.ContentModerator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace content_moderator_quickstart
{
    public class Program
    {
        static readonly string SUBSCRIPTION_KEY =
            Environment.GetEnvironmentVariable("AZURE_CONTENT_SUBSCRIPTION_KEY");

        static readonly string ENDPOINT =
            Environment.GetEnvironmentVariable("AZURE_CONTENT_ENDPOINT");

        // TEXT MODERATION
        // Name of the file that contains text
        private static readonly string TextFile = "TextFile.txt";
        // The name of the file to contain the output from the evaluation.
        private static string TextOutputFile = "TextModerationOutput.txt";

        // IMAGE MODERATION
        //The name of the file that contains the image URLs to evaluate.
        private static readonly string ImageUrlFile = "ImageFiles.txt";
        // The name of the file to contain the output from the evaluation.
        private static string ImageOutputFile = "ImageModerationOutput.json";

        public static void Main()
        {
            // Create an image review client
            ContentModeratorClient clientImage = Authenticate(SUBSCRIPTION_KEY, ENDPOINT);
            // Create a text review client
            ContentModeratorClient clientText = Authenticate(SUBSCRIPTION_KEY, ENDPOINT);
            // Create a human reviews client
            ContentModeratorClient clientReviews = Authenticate(SUBSCRIPTION_KEY, ENDPOINT);

            // Moderate text from text in a file
            ModerateText(clientText, TextFile, TextOutputFile);

            // Moderate images from list of image URLs
            ModerateImages(clientImage, ImageUrlFile, ImageOutputFile);
        }

        public static ContentModeratorClient Authenticate(string key, string endpoint)
        {
            ContentModeratorClient client = new ContentModeratorClient(new ApiKeyServiceClientCredentials(key));
            client.Endpoint = endpoint;

            return client;
        }

        /*
        * TEXT MODERATION
        * This example moderates text from file.
        */
        public static void ModerateText(ContentModeratorClient client, string inputFile, string outputFile)
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("TEXT MODERATION");
            Console.WriteLine();
            // Load the input text.
            string text = File.ReadAllText(inputFile);

            // Remove carriage returns
            text = text.Replace(Environment.NewLine, " ");
            // Convert string to a byte[], then into a stream (for parameter in ScreenText()).
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            MemoryStream stream = new MemoryStream(textBytes);

            Console.WriteLine("Screening {0}...", inputFile);
            // Format text

            // Save the moderation results to a file.
            using (StreamWriter outputWriter = new StreamWriter(outputFile, false))
            {
                using (client)
                {
                    // Screen the input text: check for profanity, classify the text into three categories,
                    // do autocorrect text, and check for personally identifying information (PII)
                    outputWriter.WriteLine("Autocorrect typos, check for matching terms, PII, and classify.");

                    // Moderate the text
                    var screenResult = client.TextModeration.ScreenText("text/plain", stream, "eng", true, true, null, true);
                    outputWriter.WriteLine(JsonConvert.SerializeObject(screenResult, Formatting.Indented));
                }

                outputWriter.Flush();
                outputWriter.Close();
            }

            Console.WriteLine("Results written to {0}", outputFile);
            Console.WriteLine();
        }

        // Contains the image moderation results for an image, 
        // including text and face detection results.
        public class EvaluationData
        {
            // The URL of the evaluated image.
            public string ImageUrl;

            // The image moderation results.
            public Evaluate ImageModeration;

            // The text detection results.
            public OCR TextDetection;

            // The face detection results;
            public FoundFaces FaceDetection;
        }

        /*
 * IMAGE MODERATION
 * This example moderates images from URLs.
 */
        public static void ModerateImages(ContentModeratorClient client, string urlFile, string outputFile)
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("IMAGE MODERATION");
            Console.WriteLine();
            // Create an object to store the image moderation results.
            List<EvaluationData> evaluationData = new List<EvaluationData>();

            using (client)
            {
                // Read image URLs from the input file and evaluate each one.
                using (StreamReader inputReader = new StreamReader(urlFile))
                {
                    while (!inputReader.EndOfStream)
                    {
                        string line = inputReader.ReadLine().Trim();
                        if (line != String.Empty)
                        {
                            Console.WriteLine("Evaluating {0}...", Path.GetFileName(line));
                            var imageUrl = new BodyModel("URL", line.Trim());


                            var imageData = new EvaluationData
                            {
                                ImageUrl = imageUrl.Value,

                                // Evaluate for adult and racy content.
                                ImageModeration = client.ImageModeration.EvaluateUrlInput("application/json", imageUrl, true)
                            };
                            Thread.Sleep(1000);

                            // Detect and extract text.
                            imageData.TextDetection =
                                client.ImageModeration.OCRUrlInput("eng", "application/json", imageUrl, true);
                            Thread.Sleep(1000);

                            // Detect faces.
                            imageData.FaceDetection =
                                client.ImageModeration.FindFacesUrlInput("application/json", imageUrl, true);
                            Thread.Sleep(1000);

                            // Add results to Evaluation object
                            evaluationData.Add(imageData);
                        }
                    }
                }
            }
            // Save the moderation results to a file.
            using (StreamWriter outputWriter = new StreamWriter(outputFile, false))
            {
                outputWriter.WriteLine(JsonConvert.SerializeObject(
                    evaluationData, Formatting.Indented));

                outputWriter.Flush();
                outputWriter.Close();
            }
            Console.WriteLine();
            Console.WriteLine("Image moderation results written to output file: " + outputFile);
            Console.WriteLine();
        }
    }
}