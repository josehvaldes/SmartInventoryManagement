using Microsoft.AspNetCore.Http;
using System.IO;
namespace SmartInventory.UnitTests.Products
{
    internal class MockFormFile : IFormFile
    {
        public MockFormFile(string fileName, string contentType)
        {
            Name = "file";
            ContentType = contentType;
            ContentDisposition = $"form-data; name=\"{fileName}\"; filename=\"{fileName}\"";
            Headers = new HeaderDictionary();
            Length = 0; // Set length to 0 for the mock implementation
            FileName = fileName;
        }

        public string ContentType { get; }

        public string ContentDisposition { get; }

        public IHeaderDictionary Headers { get; }

        public long Length { get; }

        public string Name { get; }

        public string FileName { get; }

        public void CopyTo(Stream target)
        {
            // Do nothing for the mock implementation
        }

        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        {
            // Do nothing for the mock implementation
            return Task.CompletedTask;
        }

        public Stream OpenReadStream()
        {
            // Do nothing for the mock implementation
            return Stream.Null;
        }
    }
}
