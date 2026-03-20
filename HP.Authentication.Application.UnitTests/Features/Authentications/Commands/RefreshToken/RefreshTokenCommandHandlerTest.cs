using FluentAssertions;
using HP.Authentication.Application.Abstractions.Identity;
using HP.Authentication.Application.Common;
using HP.Authentication.Application.Features.Authentications.Commands.RefreshToken;
using HP.Authentication.Application.Features.Authentications.DTOs;
using HP.Authentication.Domain.CustomException;
using HP.Authentication.Domain.Entities;
using HP.Authentication.Localization.Abstractions;
using HP.Authentication.Localization.Enums;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace HP.Authentication.Application.UnitTests.Features.Authentications.Commands.RefreshToken
{
    public class RefreshTokenCommandHandlerTest
    {
        private readonly Mock<IRefreshTokenManager> _refreshTokenManagerMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IUserContext> _userContextMock;
        private readonly Mock<IJsonLocalizationService> _localizerMock;

        private readonly RefreshTokenCommandHandler _handler;

        public RefreshTokenCommandHandlerTest()
        {
            _refreshTokenManagerMock = new Mock<IRefreshTokenManager>();
            _jwtServiceMock = new Mock<IJwtService>();
            _userContextMock = new Mock<IUserContext>();
            _localizerMock = new Mock<IJsonLocalizationService>();

            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);

            _handler = new RefreshTokenCommandHandler(
                _refreshTokenManagerMock.Object,
                _jwtServiceMock.Object,
                _userManagerMock.Object,
                _userContextMock.Object,
                _localizerMock.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnLoginResponse_When_RefreshTokenIsValid()
        {
            // Arrange
            var request = new RefreshTokenCommand
            {
                RefreshToken = "old-refresh-token",
                BranchId = Guid.NewGuid()
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Huy",
                Avatar = "avatar.png",
                IsActive = true
            };

            var roles = new List<string> { "Admin" };
            var ipAddress = "127.0.0.1";
            var newRefreshToken = "new-refresh-token";
            var newJwtToken = "new-jwt-token";

            _userContextMock
                .Setup(x => x.GetIpAddress())
                .Returns(ipAddress);

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns(newRefreshToken);

            _refreshTokenManagerMock
                .Setup(x => x.VerifyAndRotateTokenAsync(request.RefreshToken, newRefreshToken, ipAddress))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(roles);

            _jwtServiceMock
                .Setup(x => x.GenerateJWTToken(user, roles, request.BranchId))
                .Returns(newJwtToken);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<LoginResponse>();

            result.token.Should().Be(newJwtToken);
            result.refreshToken.Should().Be(newRefreshToken);
            result.UserId.Should().Be(user.Id);
            result.UserName.Should().Be("Huy");
            result.Avatar.Should().Be("avatar.png");
            result.branchName.Should().Be("");

            _userContextMock.Verify(x => x.GetIpAddress(), Times.Once);
            _jwtServiceMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
            _refreshTokenManagerMock.Verify(
                x => x.VerifyAndRotateTokenAsync(request.RefreshToken, newRefreshToken, ipAddress),
                Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(user), Times.Once);
            _jwtServiceMock.Verify(x => x.GenerateJWTToken(user, roles, request.BranchId), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_UseUnknown_When_UserNameIsNull()
        {
            // Arrange
            var request = new RefreshTokenCommand
            {
                RefreshToken = "old-refresh-token",
                BranchId = Guid.NewGuid()
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = null,
                Avatar = null,
                IsActive = true
            };

            var roles = new List<string> { "User" };
            var ipAddress = "127.0.0.1";
            var newRefreshToken = "new-refresh-token";
            var newJwtToken = "new-jwt-token";

            _userContextMock.Setup(x => x.GetIpAddress()).Returns(ipAddress);
            _jwtServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(newRefreshToken);

            _refreshTokenManagerMock
                .Setup(x => x.VerifyAndRotateTokenAsync(request.RefreshToken, newRefreshToken, ipAddress))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(roles);

            _jwtServiceMock
                .Setup(x => x.GenerateJWTToken(user, roles, request.BranchId))
                .Returns(newJwtToken);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.UserName.Should().Be("Unknown");
            result.Avatar.Should().BeNull();
        }

        [Fact]
        public async Task Handle_Should_ThrowUnAuthorizedException_When_UserIsInactive()
        {
            // Arrange
            var request = new RefreshTokenCommand
            {
                RefreshToken = "old-refresh-token",
                BranchId = Guid.NewGuid()
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Huy",
                IsActive = false
            };

            var ipAddress = "127.0.0.1";
            var newRefreshToken = "new-refresh-token";
            var message = "Your account is not active.";

            _userContextMock
                .Setup(x => x.GetIpAddress())
                .Returns(ipAddress);

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns(newRefreshToken);

            _refreshTokenManagerMock
                .Setup(x => x.VerifyAndRotateTokenAsync(request.RefreshToken, newRefreshToken, ipAddress))
                .ReturnsAsync(user);

            _localizerMock
                .Setup(x => x.Get("auth", AuthKeys.USER_NOT_ACTIVE))
                .Returns(message);

            // Act
            Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

            // Assert
            var exception = await act.Should().ThrowAsync<CustomException.UnAuthorizedException>();
            exception.Which.Message.Should().Be(message);

            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Never);
            _jwtServiceMock.Verify(x => x.GenerateJWTToken(It.IsAny<User>(), It.IsAny<IList<string>>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_CallDependencies_WithCorrectArguments()
        {
            // Arrange
            var branchId = Guid.NewGuid();
            var request = new RefreshTokenCommand
            {
                RefreshToken = "old-token",
                BranchId = branchId
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Tester",
                IsActive = true
            };

            var roles = new List<string> { "Manager" };
            var ipAddress = "192.168.1.10";
            var newRefreshToken = "rotated-token";
            var jwtToken = "jwt-token";

            _userContextMock.Setup(x => x.GetIpAddress()).Returns(ipAddress);
            _jwtServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(newRefreshToken);

            _refreshTokenManagerMock
                .Setup(x => x.VerifyAndRotateTokenAsync("old-token", "rotated-token", "192.168.1.10"))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles);

            _jwtServiceMock
                .Setup(x => x.GenerateJWTToken(user, roles, branchId))
                .Returns(jwtToken);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.token.Should().Be(jwtToken);

            _refreshTokenManagerMock.Verify(
                x => x.VerifyAndRotateTokenAsync("old-token", "rotated-token", "192.168.1.10"),
                Times.Once);

            _jwtServiceMock.Verify(
                x => x.GenerateJWTToken(user, roles, branchId),
                Times.Once);
        }
    }
}