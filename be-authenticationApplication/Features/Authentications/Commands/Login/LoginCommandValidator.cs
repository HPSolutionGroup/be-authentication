using be_localization.Abstractions;
using be_localization.Enums;
using FluentValidation;

namespace be_authenticationApplication.Features.Authentications.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator(IJsonLocalizationService localizer)
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(localizer.Get("auth", AuthKeys.EMAIL_REQUIRED))
                .EmailAddress()
                .WithMessage(localizer.Get("auth", AuthKeys.EMAIL_INVALID));

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(localizer.Get("auth", AuthKeys.PASSWORD_REQUIRED));

            RuleFor(x => x.BranchId)
                .NotEqual(Guid.Empty)
                .WithMessage(localizer.Get("auth", AuthKeys.BRANCH_REQUIRED));
        }
    }
}
