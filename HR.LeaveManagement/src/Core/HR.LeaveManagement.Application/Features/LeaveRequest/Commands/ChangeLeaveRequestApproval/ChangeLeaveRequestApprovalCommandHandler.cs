using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Email;
using HR.LeaveManagement.Application.Contracts.Logging;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.CreateLeaveRequest;
using HR.LeaveManagement.Application.Models.Email;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.ChangeLeaveRequestApproval
{
    public class ChangeLeaveRequestApprovalCommandHandler : IRequestHandler<ChangeLeaveRequestApprovalCommand, Unit>
    {
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly IAppLogger<ChangeLeaveRequestApprovalCommandHandler> _appLogger;

        public ChangeLeaveRequestApprovalCommandHandler(IMapper mapper, IEmailSender emailSender, 
            ILeaveRequestRepository leaveRequestRepository, ILeaveTypeRepository leaveTypeRepository, IAppLogger<ChangeLeaveRequestApprovalCommandHandler> appLogger)
        {
            _mapper = mapper;
            _emailSender = emailSender;
            _leaveRequestRepository = leaveRequestRepository;
            _leaveTypeRepository = leaveTypeRepository;
            _appLogger = appLogger;
        }

        public async Task<Unit> Handle(ChangeLeaveRequestApprovalCommand request, CancellationToken cancellationToken)
        {
            var leaveRequest = await _leaveRequestRepository.GetByIdAsync(request.Id);

            if (leaveRequest == null) 
                throw new NotFoundException(nameof(LeaveRequest), request.Id);

            leaveRequest.Approved = request.Approved;
            await _leaveRequestRepository.UpdateAsync(leaveRequest);

            // if request is approved, get and update the employee's allocations
            // Send Confirmation Email
            try
            {
                var email = new EmailMessage
                {
                    To = string.Empty,
                    Body = $"Your leave request for {leaveRequest.StartDate:D} to {leaveRequest.EndDate:D}" +
                       $"has been updated successfully.",
                    Subject = "Leave Request Approval Updated"
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
