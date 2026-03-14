using HP.Authentication.Domain.CustomException;
using HP.Authentication.Localization.Abstractions;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Text.Json;

namespace HP.Authentication.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IJsonLocalizationService _localizer;

        public ExceptionMiddleware
            (
                RequestDelegate next,
                ILogger<ExceptionMiddleware> logger,
                IJsonLocalizationService localizer
            )
        {
            _next = next;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                var endpoint = context.GetEndpoint();

                if (endpoint?.Metadata?.GetMetadata<IAuthorizeData>() != null)
                {
                    if (!context.User.Identity.IsAuthenticated)
                    {
                        throw new CustomException.UnAuthorizedException("USER_NOT_LOGGED_IN");
                    }

                    var authorizeData = endpoint.Metadata.GetMetadata<IAuthorizeData>();
                    if (authorizeData.Roles != null)
                    {
                        var roles = authorizeData.Roles.Split(',');
                        if (!roles.Any(role => context.User.IsInRole(role)))
                        {
                            throw new CustomException.ForbbidenException("ACCESS_DENIED");
                        }
                    }
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            string result;

            switch (exception)
            {
                case CustomException.InvalidDataException invalidDataEx:
                    code = HttpStatusCode.BadRequest;
                    result = invalidDataEx.Message;
                    break;
                case CustomException.DataNotFoundException dataNotFoundEx:
                    code = HttpStatusCode.NotFound;
                    result = dataNotFoundEx.Message;
                    break;
                case CustomException.DataExistException dataExistEx:
                    code = HttpStatusCode.Conflict;
                    result = dataExistEx.Message;
                    break;
                case CustomException.UnAuthorizedException unauthorizedEx:
                    code = HttpStatusCode.Unauthorized;
                    result = unauthorizedEx.Message;
                    break;
                case CustomException.ForbbidenException forbiddenEx:
                    code = HttpStatusCode.Forbidden;
                    result = forbiddenEx.Message;
                    break;
                case CustomException.InternalServerErrorException internalServerEx:
                    code = HttpStatusCode.InternalServerError;
                    result = internalServerEx.Message;
                    break;

                case FluentValidation.ValidationException validationEx:
                    code = HttpStatusCode.BadRequest;
                    result = validationEx.Errors.FirstOrDefault()?.ErrorMessage ?? "Validation failed";
                    break;
                default:
                    _logger.LogError(exception, "Đã xảy ra lỗi khống xác định !!!");
                    result = "Đã xảy ra lỗi khống xác định !!!";
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            var response = new
            {
                statusCode = context.Response.StatusCode,
                message = result
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }

}
