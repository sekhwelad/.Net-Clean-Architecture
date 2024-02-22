using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Email;
using HR.LeaveManagement.Application.Contracts.Logging;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Models.Email;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.UpdateLeaveRequest
{
    public class UpdateLeaveRequestCommandHandler : IRequestHandler<UpdateLeaveRequestCommand, Unit>
    {
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly IAppLogger<UpdateLeaveRequestCommandHandler> _appLogger;
        public UpdateLeaveRequestCommandHandler(IMapper mapper, IEmailSender emailSender, ILeaveRequestRepository leaveRequestRepository
            , ILeaveTypeRepository leaveTypeRepository, IAppLogger<UpdateLeaveRequestCommandHandler> appLogger)
        {
            _mapper = mapper;
            _emailSender = emailSender;
            _leaveRequestRepository = leaveRequestRepository;
            _leaveTypeRepository = leaveTypeRepository;
            _appLogger = appLogger;
        }

        public async Task<Unit> Handle(UpdateLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            var leaveRequest = await _leaveRequestRepository.GetByIdAsync(request.Id);

            if (leaveRequest is null)
                throw new NotFoundException(nameof(LeaveRequest), request.Id);

            var validator = new UpdateLeaveRequestCommandValidator(_leaveTypeRepository, _leaveRequestRepository);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
                throw new BadRequestException("Invalid Leave Request", validationResult);

            _mapper.Map(request, leaveRequest);

            // Send Confirmation Email
            try
            {
                var email = new EmailMessage
                {
                    To = string.Empty,
                    Body = $"Your leave request for {request.StartDate:D} to {request.EndDate:D}" +
                       $"has been updated successfully.",
                    Subject = "Leave Request Submitted"
                };

                await _emailSender.SendEmail(email);
            }
            catch (Exception ex)
            {
                _appLogger.LogWarning(ex.Message);
                throw;
            }

            return Unit.Value;
        }
    }
}
