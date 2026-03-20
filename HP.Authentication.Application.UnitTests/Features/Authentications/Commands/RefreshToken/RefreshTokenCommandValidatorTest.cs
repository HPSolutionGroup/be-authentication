using FluentAssertions;
using FluentValidation.TestHelper;
using HP.Authentication.Application.Features.Authentications.Commands.RefreshToken;
using HP.Authentication.Localization.Abstractions;
using HP.Authentication.Localization.Enums;
using Moq;
using Xunit;

namespace HP.Authentication.Application.UnitTests.Features.Authentications.Commands.RefreshToken
{
    public class RefreshTokenCommandValidatorTest
    {
        private readonly Mock<IJsonLocalizationService> _localizerMock;
        private readonly RefreshTokenCommandValidator _validator;

        public RefreshTokenCommandValidatorTest()
        {
            _localizerMock = new Mock<IJsonLocalizationService>();

            _localizerMock
                .Setup(x => x.Get("auth", AuthKeys.REFRESH_TOKEN_REQUIRED))
                .Returns("Refresh token không được để trống.");

            _localizerMock
                .Setup(x => x.Get("auth", AuthKeys.BRANCH_REQUIRED))
                .Returns("Vui lòng chọn chi nhánh.");

            _validator = new RefreshTokenCommandValidator(_localizerMock.Object);
        }

        [Fact]
        public void Validator_Should_HaveError_When_RefreshTokenIsEmpty()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = string.Empty,
                BranchId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
                  .WithErrorMessage("Refresh token không được để trống.");
        }

        [Fact]
        public void Validator_Should_HaveError_When_RefreshTokenIsNull()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = null!,
                BranchId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
                  .WithErrorMessage("Refresh token không được để trống.");
        }

        [Fact]
        public void Validator_Should_HaveError_When_BranchIdIsEmpty()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = "valid-refresh-token",
                BranchId = Guid.Empty
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BranchId)
                  .WithErrorMessage("Vui lòng chọn chi nhánh.");
        }

        [Fact]
        public void Validator_Should_NotHaveError_When_RequestIsValid()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = "valid-refresh-token",
                BranchId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.RefreshToken);
            result.ShouldNotHaveValidationErrorFor(x => x.BranchId);
            result.IsValid.Should().BeTrue();
        }
    }
}