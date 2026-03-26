using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Polly;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Infrastructure.AWS.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Infrastructure.AWS.Storage
{
    public class S3FileStorageService : IFileStorageService
    {
        private readonly AwsSettings _awsSettings;
        private readonly string _bucketName;
        private readonly IAmazonS3 _s3Client;
        private readonly IAsyncPolicy _policy;
        public S3FileStorageService(IOptions<AwsSettings> options, IAsyncPolicy policy)
        {
            _policy = policy;
            _awsSettings = options.Value;
            _bucketName = _awsSettings.S3BucketName;
            _s3Client = new AmazonS3Client(_awsSettings.AccessKey, 
                _awsSettings.SecretKey, 
                Amazon.RegionEndpoint.GetBySystemName(_awsSettings.Region));
        }

        public async Task<string> GetUploadUrlAsync(string fileName, string contentType)
        {
            var key = $"products/{Guid.NewGuid()}-{fileName}";

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Verb = HttpVerb.PUT, // Upload
                Expires = DateTime.UtcNow.AddMinutes(5),
                ContentType = contentType
            };

            return await _policy.ExecuteAsync(async () =>
            {
                var url = await _s3Client.GetPreSignedURLAsync(request);
                return url;
            });
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
        {
            var key = $"products/{Guid.NewGuid()}-{fileName}";

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType,
                AutoCloseStream = false
            };

            return await _policy.ExecuteAsync(async () =>
            {
                await _s3Client.PutObjectAsync(request);
                // Public URL (if bucket is public or via CloudFront later)
                return $"https://{_bucketName}.s3.amazonaws.com/{key}";
            });
            
        }
    }
}
