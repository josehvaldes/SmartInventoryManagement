using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Common.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> GetUploadUrlAsync(string fileName, string contentType);
        Task<string> UploadAsync(Stream fileStream, string fileName, string contentType);
    }
}
