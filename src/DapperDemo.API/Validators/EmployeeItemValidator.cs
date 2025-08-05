using DapperDemo.Domain.Entities;
using FluentValidation;

namespace DapperDemo.API.Validators
{
    public class EmployeeItemValidator : AbstractValidator<EmployeeItem>
    {
        public EmployeeItemValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("EmployeeId must be greater than 0.");

            RuleFor(x => x.ItemName)
                .NotEmpty().WithMessage("ItemName is required.")
                .MaximumLength(100).WithMessage("ItemName must be at most 100 characters.");
        }
    }
}
