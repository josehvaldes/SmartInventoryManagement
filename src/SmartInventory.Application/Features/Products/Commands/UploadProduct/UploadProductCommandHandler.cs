using SmartInventory.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Products.Commands.UploadProduct
{
    public class UploadProductCommandHandler(IFileStorageService fileStorageService) : ICommandHandler<UploadProductCommand, string>
    {
        public async Task<string> Handle(UploadProductCommand command, CancellationToken cancellationToken)
        {
            var extension = Path.GetExtension(command.file.FileName);
            var newFilename = $"{command.productSKU}{extension}";

            using var stream = command.file.OpenReadStream();
            var fileUrl = await fileStorageService.UploadAsync(
                stream,
                newFilename,
                command.file.ContentType);

            return fileUrl;

        }
    }
}
