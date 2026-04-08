using MediatR;
using SmartInventory.Application.Common.Exceptions;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Products.Commands.UploadProduct
{
    public class UploadProductCommandHandler(IFileStorageService fileStorageService,
        IApplicationDbContext db) : ICommandHandler<UploadProductCommand, string>
    {
        public async Task<string> Handle(UploadProductCommand command, CancellationToken cancellationToken)
        {
            var product = db.Products.FirstOrDefault(p => p.SKU == command.productSKU);
            if (product == null)
                throw EntityNotFoundException.For<Product>(command.productSKU);

            var extension = Path.GetExtension(command.file.FileName);
            var newFilename = $"{command.productSKU}{extension}";

            using var stream = command.file.OpenReadStream();
            var fileUrl = await fileStorageService.UploadAsync(
                stream,
                newFilename,
                command.file.ContentType);

            product.UpdateImageUrl(fileUrl, command.updatedBy);

            return fileUrl;
        }
    }
}
