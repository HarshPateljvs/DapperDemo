using DapperDemo.Domain.Entities;
using FluentValidation;

namespace DapperDemo.API.Validators
{
    public class EmployeeValidator : AbstractValidator<Employee>
    {
        public EmployeeValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(50).WithMessage("Name must be at most 50 characters.");

            RuleFor(x => x.Age)
                .InclusiveBetween(18, 60)
                .WithMessage("Age must be between 18 and 60.");
        }
    }
}
