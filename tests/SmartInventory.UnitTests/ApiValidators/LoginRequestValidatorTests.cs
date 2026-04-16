using FluentAssertions;
using SmartInventory.API.Validators;
using SmartInventory.Contracts.Requests.Login;
using Xunit;

namespace SmartInventory.UnitTests.ApiValidators
{
    public class LoginRequestValidatorTests
    {
        private readonly LoginRequestValidator _validator;

        public LoginRequestValidatorTests()
        {
            _validator = new LoginRequestValidator();
        }

        private static LoginRequest ValidRequest() => new()
        {
            Username = "admin",
            Password = "secret123"
        };

        [Fact]
        public void Should_Pass_When_Request_Is_Valid()
        {
            var result = _validator.Validate(ValidRequest());
            result.IsValid.Should().BeTrue();
        }

        // --- Username ---

        [Fact]
        public void Should_Fail_When_Username_Is_Empty()
        {
            var request = new LoginRequest { Username = string.Empty, Password = "secret123" };

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.Username) && e.ErrorMessage == "Username is required.");
        }

        [Fact]
        public void Should_Fail_When_Username_Is_Whitespace()
        {
            var request = new LoginRequest { Username = "   ", Password = "secret123" };

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.Username) && e.ErrorMessage == "Username is required.");
        }

        // --- Password ---

        [Fact]
        public void Should_Fail_When_Password_Is_Empty()
        {
            var request = new LoginRequest { Username = "admin", Password = string.Empty };

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.Password) && e.ErrorMessage == "Password is required.");
        }

        [Fact]
        public void Should_Fail_When_Password_Is_Whitespace()
        {
            var request = new LoginRequest { Username = "admin", Password = "   " };

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.Password) && e.ErrorMessage == "Password is required.");
        }

        [Fact]
        public void Should_Fail_When_Both_Username_And_Password_Are_Empty()
        {
            var request = new LoginRequest { Username = string.Empty, Password = string.Empty };

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(2);
            result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Username));
            result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Password));
        }
    }
}
