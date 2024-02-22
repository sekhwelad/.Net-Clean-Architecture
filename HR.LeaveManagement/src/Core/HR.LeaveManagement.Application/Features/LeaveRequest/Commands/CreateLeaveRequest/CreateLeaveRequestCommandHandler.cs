using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Email;
using HR.LeaveManagement.Application.Contracts.Logging;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.UpdateLeaveRequest;
using HR.LeaveManagement.Application.Models.Email;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.CreateLeaveRequest
{
    public class CreateLeaveRequestCommandHandler : IRequestHandler<CreateLeaveRequestCommand, Unit>
    {
            
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly IAppLogger<CreateLeaveRequestCommandHandler> _appLogger;

        public CreateLeaveRequestCommandHandler(IMapper mapper, IEmailSender emailSender, ILeaveRequestRepository leaveRequestRepository
            , ILeaveTypeRepository leaveTypeRepository, IAppLogger<CreateLeaveRequestCommandHandler> appLogger)
        {
            _mapper = mapper;
            _emailSender = emailSender;
            _leaveRequestRepository = leaveRequestRepository;
            _leaveTypeRepository = leaveTypeRepository;
            _appLogger = appLogger;
        }

        public async Task<Unit> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateLeaveRequestCommandValidator(_leaveTypeRepository);
            var validationResult = await validator.ValidateAsync(request);

            if(!validationResult.IsValid)
                throw new BadRequestException("Invalid Leave Request", validationResult);

            // Get requesting employee id


            // Check on employee;s allocation

            // If allocations aren't enough , return validation error with message

            var leaveRequest = _mapper.Map<Domain.LeaveRequest>(request);
            await _leaveRequestRepository.CreateAsync(leaveRequest);

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
