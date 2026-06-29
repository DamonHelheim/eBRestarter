using eBRestarter.Core.Application.Ports.Inbound.UseCases.DeleteBrowserContent;
using eBRestarter.Core.Application.UseCases.DeleteBrowserContent;
using eBRestarter.Core.Application.Models.Errors;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;
using FluentValidation;

namespace eBRestarter.Core.Application.Validators;

public sealed class DeleteBrowserContentValidator : AbstractValidator<DeleteBrowserContentRequest>
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
