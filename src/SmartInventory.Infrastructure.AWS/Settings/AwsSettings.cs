using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Infrastructure.AWS.Settings
{
    public class AwsSettings
    {
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;

        public string S3BucketName { get; set; } = string.Empty;
    }
}
