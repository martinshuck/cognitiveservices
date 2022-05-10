
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace ASB.AI.Demo.FaceApi
{
    public class Program
    {
        // Used for the Identify and Delete examples.        
        static string personGroupId = Guid.NewGuid().ToString();  
        
        // Used for all examples.
        // URL for the images.
        const string IMAGE_BASE_FOLDER = @"..\..\..\samples\";       

        static readonly string SUBSCRIPTION_KEY =
            Environment.GetEnvironmentVariable("AZURE_FACEAPI_SUBSCRIPTION_KEY");

        static readonly string ENDPOINT =
            Environment.GetEnvironmentVariable("AZURE_FACEAPI_ENDPOINT");
        

        public static void Main()
        {
            // Recognition model 4 was released in 2021 February.
            // It is recommended since its accuracy is improved
            // on faces wearing masks compared with model 3,
            // and its overall accuracy is improved compared
            // with models 1 and 2.
            const string RECOGNITION_MODEL4 = RecognitionModel.Recognition04;

            // Authenticate.
            IFaceClient client = Authenticate(ENDPOINT, SUBSCRIPTION_KEY);

            // Detect - get features from faces.
            //DetectFaceExtract(client, IMAGE_BASE_URL, RECOGNITION_MODEL4).Wait();
            // Find Similar - find a similar face from a list of faces.
            //FindSimilar(client, IMAGE_BASE_URL, RECOGNITION_MODEL4).Wait();
            // Verify - compare two images if the same person or not.
            Verify(client, IMAGE_BASE_FOLDER, RECOGNITION_MODEL4).Wait();            

        }

        /// <summary>
        /// AUTHENTICATE
        /// Uses subscription key and region to create a client.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static IFaceClient Authenticate(string endpoint, string key)
        {
            return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
        }

        /*
        * VERIFY
        * The Verify operation takes a face ID from DetectedFace or PersistedFace and either another face ID 
        * or a Person object and determines whether they belong to the same person. If you pass in a Person object, 
        * you can optionally pass in a PersonGroup to which that Person belongs to improve performance.
        */
        public static async Task Verify(IFaceClient client, string baseFolder, string recognitionModel03)
        {
            Console.WriteLine("========VERIFY========");
            Console.WriteLine();

            List<string> targetImageFileNames = new List<string> { "DriversLicense.jpg" };
            string sourceImageFileName1 = "nzpassport.png";
            string sourceImageFileName2 = "photo.jpg";

            List<Guid> targetFaceIds = new List<Guid>();
            foreach (var imageFileName in targetImageFileNames)
            {
                // Detect faces from target image url.
                List<DetectedFace> detectedFaces = await DetectFaceRecognize(client, $"{baseFolder}{imageFileName} ", recognitionModel03);
                targetFaceIds.Add(detectedFaces[0].FaceId.Value);
                Console.WriteLine($"{detectedFaces.Count} faces detected from image `{imageFileName}`.");
            }

            // Detect faces from source image file 1.
            List<DetectedFace> detectedFaces1 = await DetectFaceRecognize(client, $"{baseFolder}{sourceImageFileName1} ", recognitionModel03);
            Console.WriteLine($"{detectedFaces1.Count} faces detected from image `{sourceImageFileName1}`.");
            Guid sourceFaceId1 = detectedFaces1[0].FaceId.Value;

            // Detect faces from source image file 2.
            List<DetectedFace> detectedFaces2 = await DetectFaceRecognize(client, $"{baseFolder}{sourceImageFileName2} ", recognitionModel03);
            Console.WriteLine($"{detectedFaces2.Count} faces detected from image `{sourceImageFileName2}`.");
            Guid sourceFaceId2 = detectedFaces2[0].FaceId.Value;

            // Verification example for faces of the same person.
            VerifyResult verifyResult1 = await client.Face.VerifyFaceToFaceAsync(sourceFaceId1, targetFaceIds[0]);
            Console.WriteLine(
                verifyResult1.IsIdentical
                    ? $"Faces from {sourceImageFileName1} & {targetImageFileNames[0]} are of the same (Positive) person, similarity confidence: {verifyResult1.Confidence}."
                    : $"Faces from {sourceImageFileName1} & {targetImageFileNames[0]} are of different (Negative) persons, similarity confidence: {verifyResult1.Confidence}.");

            // Verification example for faces of different persons.
            VerifyResult verifyResult2 = await client.Face.VerifyFaceToFaceAsync(sourceFaceId2, targetFaceIds[0]);
            Console.WriteLine(
                verifyResult2.IsIdentical
                    ? $"Faces from {sourceImageFileName2} & {targetImageFileNames[0]} are of the same (Negative) person, similarity confidence: {verifyResult2.Confidence}."
                    : $"Faces from {sourceImageFileName2} & {targetImageFileNames[0]} are of different (Positive) persons, similarity confidence: {verifyResult2.Confidence}.");

            Console.WriteLine();
        }

        // Detect faces from image file for recognition purpose. This is a helper method for other functions in this quickstart.
        private static async Task<List<DetectedFace>> DetectFaceRecognize(IFaceClient faceClient, string imageName, string recognition_model)
        {
            // Detect faces from image file. Since only recognizing, use the recognition model 1.
            // We use detection model 3 because we are not retrieving attributes.
            IList<DetectedFace> detectedFaces = await faceClient.Face.DetectWithStreamAsync(File.OpenRead(imageName), recognitionModel: recognition_model, detectionModel: DetectionModel.Detection03, returnFaceAttributes: new List<FaceAttributeType> { FaceAttributeType.QualityForRecognition });
            List<DetectedFace> sufficientQualityFaces = new List<DetectedFace>();
            foreach (DetectedFace detectedFace in detectedFaces)
            {
                var faceQualityForRecognition = detectedFace.FaceAttributes.QualityForRecognition;
                if (faceQualityForRecognition.HasValue && (faceQualityForRecognition.Value >= QualityForRecognition.Medium))
                {
                    sufficientQualityFaces.Add(detectedFace);
                }
            }
            Console.WriteLine($"{detectedFaces.Count} face(s) with {sufficientQualityFaces.Count} having sufficient quality for recognition detected from image `{Path.GetFileName(imageName)}`");

            return sufficientQualityFaces.ToList();
        }
    }
}

