using FluentValidation;
using TransportPlatform.Ticketing.Api.Controllers;

namespace TransportPlatform.Ticketing.Api.Validation;

public class BuyTicketRequestValidator : AbstractValidator<BuyTicketRequest>
{
    public BuyTicketRequestValidator()
    {
        RuleFor(x => x.RouteId)
            .NotEmpty().WithMessage("RouteId is required.");

        RuleFor(x => x.SeatNumber)
            .GreaterThan(0).WithMessage("SeatNumber must be a positive number.");
    }
}

public class ValidateTicketRequestValidator : AbstractValidator<ValidateTicketRequest>
{
    public ValidateTicketRequestValidator()
    {
        RuleFor(x => x.InspectorId)
            .NotEmpty().WithMessage("InspectorId is required.");
    }
}

public class CancelTicketRequestValidator : AbstractValidator<CancelTicketRequest>
{
    public CancelTicketRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Cancellation reason is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
