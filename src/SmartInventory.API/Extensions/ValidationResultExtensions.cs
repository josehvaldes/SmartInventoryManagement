using FluentValidation.Results;
using SmartInventory.Application.Common.Exceptions;

namespace SmartInventory.API.Extensions
{
    public static class ValidationResultExtensions
    {
        public static void ThrowIfInvalid(this ValidationResult result)
        {
            if (result.IsValid) return;

            var errors = result.Errors
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).Distinct().ToArray());

            throw new ValidationException(errors);
        }
    }
}
