using FluentValidation;
using StadiumAnalytics.Application.DTOs;
using System;

namespace StadiumAnalytics.Application.Validators;

public class SensorEventCreateDtoValidator : AbstractValidator<SensorEventCreateDto>
{
    public SensorEventCreateDtoValidator()
    {
        RuleFor(x => x.Gate)
            .NotEmpty().WithMessage("Gate is required.")
            .MaximumLength(50).WithMessage("Gate cannot exceed 50 characters.");

        RuleFor(x => x.NumberOfPeople)
            .GreaterThan(0).WithMessage("Number of people must be greater than 0.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required.")
            .Must(type => type.Equals("Enter", StringComparison.OrdinalIgnoreCase) || 
                          type.Equals("Leave", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Type must be either 'Enter' or 'Leave'.");

        RuleFor(x => x.Timestamp)
            .NotEmpty().WithMessage("Timestamp is required.");
    }
}
