﻿using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Email;
using HR.LeaveManagement.Application.Contracts.Identity;
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
        private readonly IUserService _userService;
        private readonly ILeaveAllocationRepository _leaveAllocationRepository;

        public CreateLeaveRequestCommandHandler(IMapper mapper, IEmailSender emailSender, ILeaveRequestRepository leaveRequestRepository
            , ILeaveTypeRepository leaveTypeRepository, IAppLogger<CreateLeaveRequestCommandHandler> appLogger,
            IUserService userService, ILeaveAllocationRepository leaveAllocationRepository)
        {
            _mapper = mapper;
            _emailSender = emailSender;
            _leaveRequestRepository = leaveRequestRepository;
            _leaveTypeRepository = leaveTypeRepository;
            _appLogger = appLogger;
            _userService = userService;
            _leaveAllocationRepository = leaveAllocationRepository;
        }

        public async Task<Unit> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateLeaveRequestCommandValidator(_leaveTypeRepository);
            var validationResult = await validator.ValidateAsync(request);

            if(!validationResult.IsValid)
                throw new BadRequestException("Invalid Leave Request", validationResult);

            // Get requesting employee id
            var employeeId = _userService.UserId;


            // Check on employee's allocation
            var allocation = await _leaveAllocationRepository.GetUserAllocations(employeeId, request.LeaveTypeId);

            // If allocations aren't enough , return validation error with message

            if(allocation == null)
            {
                validationResult.Errors.Add(new FluentValidation.Results.ValidationFailure(nameof(request.LeaveTypeId),
                    "You do not have any allocation for this leave type."));

                throw new BadRequestException("Invalid Leave Request", validationResult);
            }

            int daysRequested = (int)(request.EndDate - request.StartDate).TotalDays;

            if(daysRequested > allocation.NumberOfDays) 
            {
                validationResult.Errors.Add(new FluentValidation.Results.ValidationFailure(nameof(request.EndDate),
                        "You do not have enough days for this request."));

                throw new BadRequestException("Invalid Leave Request", validationResult);
            }


            // Create leave request
            var leaveRequest = _mapper.Map<Domain.LeaveRequest>(request);
            leaveRequest.RequestingEmployeeId = employeeId;
            leaveRequest.DateRequested = DateTime.Now;
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
