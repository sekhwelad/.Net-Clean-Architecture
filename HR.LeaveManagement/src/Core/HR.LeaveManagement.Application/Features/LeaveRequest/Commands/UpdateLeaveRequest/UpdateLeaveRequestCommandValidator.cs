using FluentValidation;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Features.LeaveRequest.Shared;

namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.UpdateLeaveRequest
{
    public class UpdateLeaveRequestCommandValidator : AbstractValidator<UpdateLeaveRequestCommand>
    {
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly ILeaveRequestRepository _leaveRequestRepository;

        public UpdateLeaveRequestCommandValidator(ILeaveTypeRepository leaveTypeRepository, ILeaveRequestRepository leaveRequestRepository)
        {
            _leaveTypeRepository = leaveTypeRepository;
            _leaveRequestRepository = leaveRequestRepository;
            Include(new BaseLeaveRequestValidator(_leaveTypeRepository));

            RuleFor(x => x.Id)
                .NotNull()
                .MustAsync(LeaveRequestMustExist)
                .WithMessage("{PropertName} must be present");
        }

        private async Task<bool> LeaveRequestMustExist(int id, CancellationToken cancellationToken)
        {
            var leaveAllocation = await _leaveRequestRepository.GetByIdAsync(id);
            return leaveAllocation != null;
        }
    }
}
