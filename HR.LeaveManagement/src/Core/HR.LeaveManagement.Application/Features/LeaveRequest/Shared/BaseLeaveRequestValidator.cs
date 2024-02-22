using FluentValidation;
using HR.LeaveManagement.Application.Contracts.Persistence;

namespace HR.LeaveManagement.Application.Features.LeaveRequest.Shared
{
    public class BaseLeaveRequestValidator : AbstractValidator<BaseLeaveRequest>
    {
        private readonly ILeaveTypeRepository _leaveTypeRepository;

        public BaseLeaveRequestValidator(ILeaveTypeRepository leaveTypeRepository)
        {
            _leaveTypeRepository = leaveTypeRepository;

            RuleFor(x => x.StartDate)
                .LessThan(x => x.EndDate)
                .WithMessage("{PropertyName} must be before {ComparisonValue}");

            RuleFor(x => x.EndDate)
                .GreaterThan(p => p.StartDate)
                .WithMessage("{PropertyName} must be after {ComparisonValue}");

            RuleFor(x => x.LeaveTypeId)
                .GreaterThan(0)
                .MustAsync(LeaveTypeMustExist)
                .WithMessage("{PropertyName} does not exist");
        }

        private async Task<bool> LeaveTypeMustExist(int id, CancellationToken token)
        {
            var leaveType = await _leaveTypeRepository.GetByIdAsync(id);
            return leaveType != null;
        }
    }
}
