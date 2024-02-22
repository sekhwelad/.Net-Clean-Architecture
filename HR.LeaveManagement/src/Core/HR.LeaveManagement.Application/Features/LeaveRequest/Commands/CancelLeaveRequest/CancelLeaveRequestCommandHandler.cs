using HR.LeaveManagement.Application.Contracts.Email;
using HR.LeaveManagement.Application.Contracts.Logging;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Models.Email;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.CancelLeaveRequest
{
    public class CancelLeaveRequestCommandHandler : IRequestHandler<CancelLeaveRequestCommand, Unit>
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IEmailSender _emailSender;
        private readonly IAppLogger<CancelLeaveRequestCommandHandler> _appLogger;


        public CancelLeaveRequestCommandHandler(ILeaveRequestRepository leaveRequestRepository, IAppLogger<CancelLeaveRequestCommandHandler> appLogger,
            , IEmailSender emailSender)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _emailSender = emailSender;
            _appLogger = appLogger; 
        }

        public async Task<Unit> Handle(CancelLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            var leaveRequest = await _leaveRequestRepository.GetByIdAsync(request.Id);  

            if(leaveRequest == null) 
                throw new NotFoundException(nameof(LeaveRequest),request.Id);

            leaveRequest.Cancelled = true;

            //If already approved,  Re-evaluate the employee's allocations for the leave type


            // Send confirmation email

            try
            {
                var email = new EmailMessage
                {
                    To = string.Empty, /* Get email from employee record*/
                    Body = $"Your leave request for {leaveRequest.StartDate:D} to {leaveRequest.EndDate:D}" +
                       $"has been updated successfully.",
                    Subject = "Leave Request Cancellation Submitted"
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
