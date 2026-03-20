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
                .Setup(x => x.Get("auth", AuthKeys.EMAIL_REQUIRED))
                .Returns("Email không được để trống.");

            _localizerMock
                .Setup(x => x.Get("auth", AuthKeys.EMAIL_INVALID))
                .Returns("Email không đúng định dạng.");

            _localizerMock
                .Setup(x => x.Get("auth", AuthKeys.PASSWORD_REQUIRED))
                .Returns("Mật khẩu không được để trống.");

            _localizerMock
                .Setup(x => x.Get("auth", AuthKeys.BRANCH_REQUIRED))
                .Returns("Vui lòng chọn chi nhánh.");

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
            result.Errors.Should().Contain(x =>
                x.PropertyName == nameof(LoginCommand.Email) &&
                x.ErrorMessage == "Email không được để trống.");
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
            result.Errors.Should().Contain(x =>
                x.PropertyName == nameof(LoginCommand.Email) &&
                x.ErrorMessage == "Email không đúng định dạng.");
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
            result.Errors.Should().Contain(x =>
                x.PropertyName == nameof(LoginCommand.Password) &&
                x.ErrorMessage == "Mật khẩu không được để trống.");
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
            result.Errors.Should().Contain(x =>
                x.PropertyName == nameof(LoginCommand.BranchId) &&
                x.ErrorMessage == "Vui lòng chọn chi nhánh.");
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