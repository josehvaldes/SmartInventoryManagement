using FluentValidation;

namespace SmartInventory.API.Validators
{
    public class FormFileValidator : AbstractValidator<IFormFile>
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".png" };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        public FormFileValidator()
        {
            RuleFor(x => x.Length)
                .GreaterThan(0).WithMessage("File must not be empty.")
                .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage("File size must not exceed 10MB.");

            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("File name is required.")
                .Must(name => AllowedExtensions.Contains(Path.GetExtension(name).ToLowerInvariant()))
                .WithMessage($"Only the following file types are allowed: {string.Join(", ", AllowedExtensions)}.");
        }
    }
}
