using FluentValidation;
using DapperDemo.Domain.Entities;

public class EmployeeRequestValidator : AbstractValidator<EmployeeRequest>
{
    public EmployeeRequestValidator()
    {
        RuleFor(x => x.Employee.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must be <= 100 characters");

        RuleFor(x => x.Employee.Age)
            .InclusiveBetween(18, 60).WithMessage("Age must be between 18 and 60");

        RuleForEach(x => x.Items).ChildRules(items =>
        {
            items.RuleFor(i => i.ItemName)
                .NotEmpty().WithMessage("Item name is required")
                .MaximumLength(50).WithMessage("Item name must be <= 50 characters");
        });
    }
}
