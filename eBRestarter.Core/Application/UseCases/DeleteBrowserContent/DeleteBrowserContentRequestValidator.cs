using FluentValidation;

namespace eBRestarter.Core.Application.UseCases.DeleteBrowserContent;

public class DeleteBrowserContentRequestValidator : AbstractValidator<DeleteBrowserContentRequest>
{
    public DeleteBrowserContentRequestValidator()
    {
        RuleFor(x => x.BrowserType)
            .IsInEnum()
            .WithMessage("A valid browser type must be selected.");

        RuleFor(x => x)
            .Must(x => x.DeleteCache || x.DeleteCookies)
            .WithMessage("At least one deletion option (Cache or Cookies) must be selected.");
    }
}
