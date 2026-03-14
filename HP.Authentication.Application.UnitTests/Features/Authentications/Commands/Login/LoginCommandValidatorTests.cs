using FluentAssertions;
using HP.Authentication.Application.Features.Authentications.Commands.Login;
using HP.Authentication.Localization.Abstractions;
using HP.Authentication.Localization.Enums;
using Moq;

namespace HP.Authentication.Application.UnitTests.Features.Authentications.Commands.Login
{
    public class LoginCommandValidatorTests
    {
        private readonly Mock<IJsonLocalizationService> _localizerMock;
        private readonly LoginCommandValidator _validator;

        public LoginCommandValidatorTests()
        {
            _localizerMock = new Mock<IJsonLocalizationService>();

            _localizerMock
                .Setup(x => x.Get("auth", It.IsAny<AuthKeys>(), It.IsAny<object[]>()))
                .Returns("validation error");

            _validator = new LoginCommandValidator(_localizerMock.Object);
        }

        [Fact]
        public async Task Validate_Should_HaveError_WhenEmailIsEmpty()
        {
            var command = new LoginCommand
            {
                Email = "",
                Password = "123456",
                BranchId = Guid.NewGuid()
            };

            var result = await _validator.ValidateAsync(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == nameof(LoginCommand.Email));
        }

        [Fact]
        public async Task Validate_Should_HaveError_WhenEmailIsInvalid()
        {
            var command = new LoginCommand
            {
                Email = "abc",
                Password = "123456",
                BranchId = Guid.NewGuid()
            };

            var result = await _validator.ValidateAsync(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == nameof(LoginCommand.Email));
        }

        [Fact]
        public async Task Validate_Should_HaveError_WhenPasswordIsEmpty()
        {
            var command = new LoginCommand
            {
                Email = "test@gmail.com",
                Password = "",
                BranchId = Guid.NewGuid()
            };

            var result = await _validator.ValidateAsync(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == nameof(LoginCommand.Password));
        }

        [Fact]
        public async Task Validate_Should_HaveError_WhenBranchIdIsEmpty()
        {
            var command = new LoginCommand
            {
                Email = "test@gmail.com",
                Password = "123456",
                BranchId = Guid.Empty
            };

            var result = await _validator.ValidateAsync(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == nameof(LoginCommand.BranchId));
        }

        [Fact]
        public async Task Validate_Should_Pass_WhenCommandIsValid()
        {
            var command = new LoginCommand
            {
                Email = "test@gmail.com",
                Password = "123456",
                BranchId = Guid.NewGuid()
            };

            var result = await _validator.ValidateAsync(command);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
    }
}
