using FluentAssertions;
using SmartInventory.API.Validators;
using SmartInventory.Contracts.Requests.Warehouses;
using Xunit;

namespace SmartInventory.UnitTests.ApiValidators
{
    public class CreateWarehouseRequestValidatorTests
    {
        private readonly CreateWarehouseRequestValidator _validator;

        public CreateWarehouseRequestValidatorTests()
        {
            _validator = new CreateWarehouseRequestValidator();
        }

        private static CreateWarehouseRequest ValidRequest() => new()
        {
            Code = "WH-001",
            Name = "Main Warehouse",
            Capacity = 1000m,
            ManagerName = "John Doe",
            ManagerEmail = "john.doe@example.com",
            ManagerPhone = "555-0100"
        };

        [Fact]
        public void Should_Pass_When_Request_Is_Valid()
        {
            var result = _validator.Validate(ValidRequest());
            result.IsValid.Should().BeTrue();
        }

        // --- Code ---

        [Fact]
        public void Should_Fail_When_Code_Is_Empty()
        {
            var request = ValidRequest();
            request.Code = string.Empty;

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.Code) && e.ErrorMessage == "Warehouse code is required.");
        }

        [Fact]
        public void Should_Fail_When_Code_Exceeds_20_Characters()
        {
            var request = ValidRequest();
            request.Code = new string('A', 21);

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.Code) && e.ErrorMessage == "Warehouse code must not exceed 20 characters.");
        }

        [Fact]
        public void Should_Pass_When_Code_Is_Exactly_20_Characters()
        {
            var request = ValidRequest();
            request.Code = new string('A', 20);

            var result = _validator.Validate(request);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.Code));
        }

        // --- Name ---

        [Fact]
        public void Should_Fail_When_Name_Is_Empty()
        {
            var request = ValidRequest();
            request.Name = string.Empty;

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.Name) && e.ErrorMessage == "Warehouse name is required.");
        }

        [Fact]
        public void Should_Fail_When_Name_Exceeds_200_Characters()
        {
            var request = ValidRequest();
            request.Name = new string('A', 201);

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.Name) && e.ErrorMessage == "Warehouse name must not exceed 200 characters.");
        }

        [Fact]
        public void Should_Pass_When_Name_Is_Exactly_200_Characters()
        {
            var request = ValidRequest();
            request.Name = new string('A', 200);

            var result = _validator.Validate(request);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.Name));
        }

        // --- Capacity ---

        [Fact]
        public void Should_Pass_When_Capacity_Is_Null()
        {
            var request = ValidRequest();
            request.Capacity = null;

            var result = _validator.Validate(request);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.Capacity));
        }

        [Fact]
        public void Should_Fail_When_Capacity_Is_Zero()
        {
            var request = ValidRequest();
            request.Capacity = 0m;

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.Capacity) && e.ErrorMessage == "Capacity must be a positive value.");
        }

        [Fact]
        public void Should_Fail_When_Capacity_Is_Negative()
        {
            var request = ValidRequest();
            request.Capacity = -1m;

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.Capacity) && e.ErrorMessage == "Capacity must be a positive value.");
        }

        // --- ManagerName ---

        [Fact]
        public void Should_Fail_When_ManagerName_Exceeds_100_Characters()
        {
            var request = ValidRequest();
            request.ManagerName = new string('A', 101);

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.ManagerName) && e.ErrorMessage == "Manager name must not exceed 100 characters.");
        }

        [Fact]
        public void Should_Pass_When_ManagerName_Is_Exactly_100_Characters()
        {
            var request = ValidRequest();
            request.ManagerName = new string('A', 100);

            var result = _validator.Validate(request);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.ManagerName));
        }

        [Fact]
        public void Should_Pass_When_ManagerName_Is_Empty()
        {
            var request = ValidRequest();
            request.ManagerName = string.Empty;

            var result = _validator.Validate(request);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.ManagerName));
        }

        // --- ManagerEmail ---

        [Fact]
        public void Should_Pass_When_ManagerEmail_Is_Empty()
        {
            var request = ValidRequest();
            request.ManagerEmail = string.Empty;

            var result = _validator.Validate(request);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.ManagerEmail));
        }

        [Fact]
        public void Should_Pass_When_ManagerEmail_Is_Whitespace()
        {
            var request = ValidRequest();
            request.ManagerEmail = "   ";

            var result = _validator.Validate(request);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.ManagerEmail));
        }

        [Fact]
        public void Should_Fail_When_ManagerEmail_Is_Invalid_Format()
        {
            var request = ValidRequest();
            request.ManagerEmail = "not-an-email";

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.ManagerEmail) && e.ErrorMessage == "A valid manager email address is required.");
        }

        [Fact]
        public void Should_Pass_When_ManagerEmail_Is_Valid()
        {
            var request = ValidRequest();
            request.ManagerEmail = "manager@warehouse.com";

            var result = _validator.Validate(request);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.ManagerEmail));
        }

        // --- ManagerPhone ---

        [Fact]
        public void Should_Fail_When_ManagerPhone_Exceeds_20_Characters()
        {
            var request = ValidRequest();
            request.ManagerPhone = new string('1', 21);

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.ManagerPhone) && e.ErrorMessage == "Manager phone must not exceed 20 characters.");
        }

        [Fact]
        public void Should_Pass_When_ManagerPhone_Is_Exactly_20_Characters()
        {
            var request = ValidRequest();
            request.ManagerPhone = new string('1', 20);

            var result = _validator.Validate(request);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.ManagerPhone));
        }

        [Fact]
        public void Should_Pass_When_ManagerPhone_Is_Empty()
        {
            var request = ValidRequest();
            request.ManagerPhone = string.Empty;

            var result = _validator.Validate(request);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.ManagerPhone));
        }
    }
}
