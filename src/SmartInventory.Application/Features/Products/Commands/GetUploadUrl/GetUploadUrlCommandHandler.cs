using SmartInventory.Application.Common.Interfaces;

namespace SmartInventory.Application.Features.Products.Commands.GetUploadUrl
{
    public class GetUploadUrlCommandHandler(IFileStorageService _fileStorage, ITenantContext tenantContext) : ICommandHandler<GetUploadUrlCommand, string>
    {
        public async Task<string> Handle(GetUploadUrlCommand request, CancellationToken cancellationToken)
        {
            var newFilename = $"{request.ProductSku}{Path.GetExtension(request.FileName)}";

            return await _fileStorage.GetUploadUrlAsync(
                tenantContext,
                newFilename,
                request.ContentType
            );
        }
    }
}
