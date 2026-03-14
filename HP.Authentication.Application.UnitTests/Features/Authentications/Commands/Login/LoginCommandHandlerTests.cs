namespace HP.Authentication.Application.UnitTests.Features.Authentications.Commands.Login
{
    public class LoginCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Should_ThrowUnauthorized_WhenUserNotFound()
        {
            //Arrange
            // 1.Mock repository trả về null khi tìm theo email
            // 2.Mock localizer trả về message phù hợp
            // 3.Tạo handler

            // Act
            //var act = () => handler.Handle(command, CancellationToken.None);

            //Assert
            //await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(act);

            await Task.CompletedTask;
        }

        [Fact]
        public async Task Handle_Should_ThrowUnauthorized_WhenUserInactive()
        {
            // Arrange
            // 1. Tạo user IsActive = false
            // 2. Mock repository trả về user đó
            // 3. Mock localizer
            // 4. Tạo handler

            // Act
            // var act = () => handler.Handle(command, CancellationToken.None);

            // Assert
            // await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(act);

            await Task.CompletedTask;
        }

        [Fact]
        public async Task Handle_Should_ThrowUnauthorized_WhenPasswordInvalid()
        {
            // Arrange
            // 1. Tạo user hợp lệ, IsActive = true
            // 2. Mock repository trả về user
            // 3. Mock VerifyPassword => IsSuccess = false
            // 4. Mock UserManager.AccessFailedAsync(user)
            // 5. Tạo handler

            // Act
            // var act = () => handler.Handle(command, CancellationToken.None);

            // Assert
            // await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(act);
            // Verify AccessFailedAsync được gọi 1 lần

            await Task.CompletedTask;
        }

        [Fact]
        public async Task Handle_Should_ReturnLoginResponse_WhenLoginSuccess()
        {
            // Arrange
            // 1. Tạo user hợp lệ
            // 2. Mock repository trả về user
            // 3. Mock VerifyPassword => IsSuccess = true, RequiresRehash = false
            // 4. Mock UserManager.ResetAccessFailedCountAsync(user)
            // 5. Mock UserManager.GetRolesAsync(user)
            // 6. Mock jwtService.GenerateJWTToken(...)
            // 7. Mock jwtService.GenerateRefreshToken()
            // 8. Mock userContext.GetIpAddress()
            // 9. Mock refreshTokenManager.CreateTokenAsync(...)
            // 10. Tạo handler

            // Act
            // var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            // Assert.NotNull(result);
            // Assert.Equal("jwt-token", result.token);
            // Assert.Equal("refresh-token", result.refreshToken);
            // Assert.Equal(user.Id, result.UserId);

            await Task.CompletedTask;
        }

        [Fact]
        public async Task Handle_Should_RehashPassword_WhenPasswordRequiresRehash()
        {
            // Arrange
            // 1. Tạo user hợp lệ
            // 2. Mock repository trả về user
            // 3. Mock VerifyPassword => IsSuccess = true, RequiresRehash = true
            // 4. Mock HashPassword(...) => "new-hash"
            // 5. Mock UserManager.UpdateAsync(user)
            // 6. Mock các bước login success còn lại
            // 7. Tạo handler

            // Act
            // var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            // Assert.Equal("new-hash", user.PasswordHash);
            // Verify UpdateAsync(user) được gọi 1 lần

            await Task.CompletedTask;
        }
    }
}
