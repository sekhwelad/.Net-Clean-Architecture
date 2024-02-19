using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Logging;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Features.LeaveType.Queries.GetAllLeaveTypes;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveType.Commands.UpdateLeaveType
{
    public class UpdateLeaveTypeCommandHandler : IRequestHandler<UpdateLeaveTypeCommand, Unit>
    {
        private readonly IMapper _mapper;
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly IAppLogger<UpdateLeaveTypeCommandHandler> _logger;


        public UpdateLeaveTypeCommandHandler(IMapper mapper, ILeaveTypeRepository leaveTypeRepository
            , IAppLogger<UpdateLeaveTypeCommandHandler> logger)
        {
            _mapper = mapper;
            _leaveTypeRepository = leaveTypeRepository;
            _logger = logger;
        }
        public async Task<Unit> Handle(UpdateLeaveTypeCommand request, CancellationToken cancellationToken)
        {
            // Validate Incoming data
            var validator = new UpdateLeaveTypeCommandValidator(_leaveTypeRepository);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation errors in update for {0} - {1}", nameof(LeaveType), request.Id);
                throw new BadRequestException("Invalid Leave Type", validationResult);
            }

            // Convert to domain entity object
            var leaveTypeToUpdate = _mapper.Map<Domain.LeaveType>(request);

            // Add to Db
            await _leaveTypeRepository.UpdateAsync(leaveTypeToUpdate);

            // Return Unit value
            return Unit.Value;
        }
    }
}
