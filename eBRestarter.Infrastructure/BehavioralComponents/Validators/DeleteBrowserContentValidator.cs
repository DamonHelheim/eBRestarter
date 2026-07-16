using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using FluentValidation;

namespace eBRestarter.Infrastructure.Validators;

public sealed partial class DeleteBrowserContentValidator : AbstractValidator<DeleteBrowserContentRequest>
{
    public DeleteBrowserContentValidator()
    {
        RuleFor(x => x.BrowserType)
            .IsInEnum()
            .WithMessage("A valid browser type must be selected.");

        RuleFor(x => x)
            .Must(x => x.DeleteCache || x.DeleteCookies)
            .WithMessage("At least one deletion option (Cache or Cookies) must be selected.");
    }
}
