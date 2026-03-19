using FluentValidation;
using HP.Authentication.Localization.Abstractions;
using HP.Authentication.Localization.Enums;

namespace HP.Authentication.Application.Features.Authentications.Commands.RefreshToken
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator(IJsonLocalizationService localizer)
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithMessage(localizer.Get("auth", AuthKeys.REFRESH_TOKEN_REQUIRED));

            RuleFor(x => x.BranchId)
                .NotEmpty()
                .WithMessage(localizer.Get("auth", AuthKeys.BRANCH_REQUIRED));
        }
    }
}
