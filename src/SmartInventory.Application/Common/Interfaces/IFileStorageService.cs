using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Common.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> GetUploadUrlAsync(ITenantContext tenantContext, string fileName, string contentType);
        Task<string> UploadAsync(ITenantContext tenantContext, Stream fileStream, string fileName, string contentType);
    }
}
