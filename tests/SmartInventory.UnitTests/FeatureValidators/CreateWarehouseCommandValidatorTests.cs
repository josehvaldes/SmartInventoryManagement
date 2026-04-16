using FluentAssertions;
using NSubstitute;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Warehouses.Commands.CreateWarehouse;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;
using SmartInventory.UnitTests.Common;
using Xunit;

namespace SmartInventory.UnitTests.FeatureValidators
{
    public class CreateWarehouseCommandValidatorTests
    {
        private readonly IApplicationDbContext _context;
        private readonly CreateWarehouseCommandValidator _validator;

        public CreateWarehouseCommandValidatorTests()
        {
            _context = Substitute.For<IApplicationDbContext>();
            _validator = new CreateWarehouseCommandValidator(_context);
        }

        private static CreateWarehouseCommand ValidCommand() => new(
            Code: "WH-001",
            Name: "Main Warehouse",
            WarehouseType: WarehouseType.Main,
            Street: "123 Main St",
            City: "Springfield",
            State: "IL",
            PostalCode: "62701",
            Country: "US",
            Capacity: 500m,
            ManagerName: "John Doe",
            ManagerEmail: "john.doe@example.com",
            ManagerPhone: "555-1234"
        );

        private void SetupWarehousesDbSet(List<Warehouse> warehouses)
        {
            var mockSet = MockDbSetHelper.CreateMockDbSet(warehouses);
            _context.Warehouses.Returns(mockSet);
        }

        private static Warehouse CreateWarehouseWithCode(string code) => Warehouse.Create(
            code: code,
            name: "Existing Warehouse",
            address: null,
            warehouseType: WarehouseType.Regional,
            capacity: null,
            managerName: string.Empty,
            managerEmail: string.Empty,
            managerPhone: string.Empty,
            createdBy: "seed"
        );

        // --- Valid command ---

        [Fact]
        public async Task Should_Pass_When_Command_Is_Valid()
        {
            SetupWarehousesDbSet([]);

            var result = await _validator.ValidateAsync(ValidCommand(), TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        // --- Code ---

        [Fact]
        public async Task Should_Fail_When_Code_Is_Empty()
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { Code = string.Empty };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(command.Code) &&
                e.ErrorMessage == "Warehouse code is required.");
        }

        [Fact]
        public async Task Should_Fail_When_Code_Exceeds_20_Characters()
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { Code = new string('A', 21) };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(command.Code) &&
                e.ErrorMessage == "Warehouse code must not exceed 20 characters.");
        }

        [Fact]
        public async Task Should_Pass_When_Code_Is_Exactly_20_Characters()
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { Code = new string('A', 20) };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Should_Fail_When_Code_Already_Exists()
        {
            var existingCode = "WH-001";
            SetupWarehousesDbSet([CreateWarehouseWithCode(existingCode)]);
            var command = ValidCommand() with { Code = existingCode };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.Code) &&
                e.ErrorMessage == "Warehouse code must be unique.");
        }

        [Fact]
        public async Task Should_Pass_When_Code_Is_Unique()
        {
            SetupWarehousesDbSet([CreateWarehouseWithCode("WH-999")]);
            var command = ValidCommand() with { Code = "WH-001" };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        // --- Name ---

        [Fact]
        public async Task Should_Fail_When_Name_Is_Empty()
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { Name = string.Empty };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.Name) &&
                e.ErrorMessage == "Warehouse name is required.");
        }

        [Fact]
        public async Task Should_Fail_When_Name_Exceeds_200_Characters()
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { Name = new string('A', 201) };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.Name) &&
                e.ErrorMessage == "Warehouse name must not exceed 200 characters.");
        }

        [Fact]
        public async Task Should_Pass_When_Name_Is_Exactly_200_Characters()
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { Name = new string('A', 200) };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        // --- Capacity ---

        [Fact]
        public async Task Should_Fail_When_Capacity_Is_Zero()
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { Capacity = 0m };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.Capacity) &&
                e.ErrorMessage == "Capacity must be a positive value.");
        }

        [Fact]
        public async Task Should_Fail_When_Capacity_Is_Negative()
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { Capacity = -1m };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.Capacity) &&
                e.ErrorMessage == "Capacity must be a positive value.");
        }

        [Fact]
        public async Task Should_Pass_When_Capacity_Is_Null()
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { Capacity = null };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Should_Pass_When_Capacity_Is_Positive()
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { Capacity = 0.01m };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        // --- ManagerName ---

        [Fact]
        public async Task Should_Fail_When_ManagerName_Exceeds_100_Characters()
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { ManagerName = new string('A', 101) };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.ManagerName) &&
                e.ErrorMessage == "Manager name must not exceed 100 characters.");
        }

        [Fact]
        public async Task Should_Pass_When_ManagerName_Is_Exactly_100_Characters()
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { ManagerName = new string('A', 100) };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Should_Pass_When_ManagerName_Is_Empty()
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { ManagerName = string.Empty };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        // --- ManagerEmail ---

        [Theory]
        [InlineData("not-an-email")]
        [InlineData("missing@")]
        [InlineData("@nodomain.com")]
        public async Task Should_Fail_When_ManagerEmail_Is_Invalid(string email)
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { ManagerEmail = email };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.ManagerEmail) &&
                e.ErrorMessage == "A valid manager email address is required.");
        }

        [Fact]
        public async Task Should_Pass_When_ManagerEmail_Is_Valid()
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { ManagerEmail = "manager@warehouse.com" };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task Should_Pass_When_ManagerEmail_Is_Null_Or_Whitespace(string? email)
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { ManagerEmail = email! };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        // --- ManagerPhone ---

        [Fact]
        public async Task Should_Fail_When_ManagerPhone_Exceeds_20_Characters()
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { ManagerPhone = new string('1', 21) };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.ManagerPhone) &&
                e.ErrorMessage == "Manager phone must not exceed 20 characters.");
        }

        [Fact]
        public async Task Should_Pass_When_ManagerPhone_Is_Exactly_20_Characters()
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { ManagerPhone = new string('1', 20) };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Should_Pass_When_ManagerPhone_Is_Empty()
        {
            SetupWarehousesDbSet([]);
            var command = ValidCommand() with { ManagerPhone = string.Empty };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }
    }
}
