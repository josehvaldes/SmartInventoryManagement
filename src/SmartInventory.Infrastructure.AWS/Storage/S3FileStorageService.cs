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
        public S3FileStorageService(IOptions<AwsSettings> options, IAmazonS3 s3Client, IAsyncPolicy policy)
        {
            _policy = policy;
            _awsSettings = options.Value;
            _bucketName = _awsSettings.S3BucketName;
            _s3Client = s3Client;
        }
        private string GetTenantScopedKey(string fileName, ITenantContext tenantContext)
        {
            if (!tenantContext.IsAvailable || tenantContext.TenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Tenant context is unavailable. Cannot generate a tenant-scoped key.");
            }
            return $"tenants/{tenantContext.TenantId}/products/{fileName}";
        }

        public async Task<string> GetUploadUrlAsync(ITenantContext tenantContext, string fileName, string contentType)
        {
            var key = GetTenantScopedKey(fileName, tenantContext);

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

        public async Task<string> UploadAsync(ITenantContext tenantContext, Stream fileStream, string fileName, string contentType)
        {
            var key = GetTenantScopedKey(fileName, tenantContext);

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
