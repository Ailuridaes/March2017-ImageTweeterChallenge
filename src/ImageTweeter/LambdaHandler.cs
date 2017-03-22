using System.Linq;
using System.Collections.Generic;
using System.IO;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using S3Model = Amazon.S3.Model;
using Amazon.Rekognition;
using RekognitionModel = Amazon.Rekognition.Model;
using Tweetinvi;
using TweetinviModels = Tweetinvi.Models;
using Tweetinvi.Parameters;

[assembly:LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace ImageTweeter {
    public class LambdaHandler {
        public string Handler(S3Event uploadEvent, ILambdaContext context) {
            string bucket = uploadEvent.Records[0].S3.Bucket.Name;
            string objectKey = uploadEvent.Records[0].S3.Object.Key;
            var tempImagePath = $"/tmp/{objectKey}";

            // Level 1: Make this output when a file is uploaded to S3
            LambdaLogger.Log("Hello from The AutoMaTweeter!");
            LambdaLogger.Log(uploadEvent.Records.ToJson());
            LambdaLogger.Log($"Bucket name: {bucket}");
            LambdaLogger.Log($"Image name: {objectKey}");
            
            using (var client = new AmazonS3Client(Amazon.RegionEndpoint.USWest2)) 
            {
                S3Model.GetObjectRequest request = new S3Model.GetObjectRequest
                {
                    BucketName = bucket,
                    Key = objectKey
                };
                
                using (S3Model.GetObjectResponse response = client.GetObjectAsync(request).Result)  
                {
                    using(var fileStream = File.Create(tempImagePath)) {
                        response.ResponseStream.CopyTo(fileStream);
                    }
                }
            }
            
            // Level 3: Post the image and message to twitter
            var consumerKey = "GCNunS5DfXGwh8rvFAterxmXP";
            var consumerSecret = "fq03tBiAIAM7pB6DjRI8S69scCFiR3FibCbjz3HWfjEOPMLSQD";
            var accessToken = "842206967632338944-XMAH3FU86RSak57FVJmglXn4HAvNmpy";
            var accessTokenSecret = "fiFvNoEASqyqWo3FuFr5JKBYyWlILihVLlGCTSxfqAtlv";

            byte[] img = File.ReadAllBytes(tempImagePath);

            Auth.SetUserCredentials(consumerKey, consumerSecret, accessToken, accessTokenSecret);
            
            // Publish the Tweet
            Tweet.PublishTweetWithImage("Meow!", img);
            return "Run Complete";
        }
    }
}
