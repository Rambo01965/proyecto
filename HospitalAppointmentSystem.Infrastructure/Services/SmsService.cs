using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HospitalAppointmentSystem.Application.Interfaces;

namespace HospitalAppointmentSystem.Infrastructure.Services;

public class SmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsService> _logger;
    private readonly string _fromPhoneNumber;

    public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _fromPhoneNumber = _configuration["Twilio:FromPhoneNumber"] ?? "";

        // Initialize Twilio
        var accountSid = _configuration["Twilio:AccountSid"];
        var authToken = _configuration["Twilio:AuthToken"];
        
        if (!string.IsNullOrEmpty(accountSid) && !string.IsNullOrEmpty(authToken))
        {
            TwilioClient.Init(accountSid, authToken);
        }
        else
        {
            _logger.LogWarning("Twilio credentials not configured. SMS functionality will be disabled.");
        }
    }

    public async Task<bool> SendSmsAsync(string toPhoneNumber, string message)
    {
        try
        {
            if (string.IsNullOrEmpty(_fromPhoneNumber))
            {
                _logger.LogWarning("Twilio FromPhoneNumber not configured. Cannot send SMS to {PhoneNumber}", toPhoneNumber);
                return false;
            }

            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_fromPhoneNumber),
                to: new PhoneNumber(toPhoneNumber)
            );

            if (messageResource.Status == MessageResource.StatusEnum.Sent || 
                messageResource.Status == MessageResource.StatusEnum.Queued ||
                messageResource.Status == MessageResource.StatusEnum.Accepted)
            {
                _logger.LogInformation("SMS sent successfully to {PhoneNumber}. MessageSid: {MessageSid}", 
                    toPhoneNumber, messageResource.Sid);
                return true;
            }
            else
            {
                _logger.LogError("Failed to send SMS to {PhoneNumber}. Status: {Status}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}", 
                    toPhoneNumber, messageResource.Status, messageResource.ErrorCode, messageResource.ErrorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending SMS to {PhoneNumber}", toPhoneNumber);
            return false;
        }
    }

    public async Task<bool> SendAppointmentConfirmationSmsAsync(string phoneNumber, string patientName, string doctorName, DateTime appointmentDate, TimeOnly appointmentTime)
    {
        var message = $"Hi {patientName}, your appointment with Dr. {doctorName} is confirmed for {appointmentDate:MMM dd, yyyy} at {appointmentTime:HH:mm}. Please arrive 15 minutes early. - Hospital Appointment System";
        return await SendSmsAsync(phoneNumber, message);
    }

    public async Task<bool> SendAppointmentCancellationSmsAsync(string phoneNumber, string patientName, string doctorName, DateTime appointmentDate, TimeOnly appointmentTime, string reason)
    {
        var message = $"Hi {patientName}, your appointment with Dr. {doctorName} on {appointmentDate:MMM dd, yyyy} at {appointmentTime:HH:mm} has been cancelled. Reason: {reason}. Please contact us to reschedule. - Hospital Appointment System";
        return await SendSmsAsync(phoneNumber, message);
    }

    public async Task<bool> SendAppointmentReminderSmsAsync(string phoneNumber, string patientName, string doctorName, DateTime appointmentDate, TimeOnly appointmentTime)
    {
        var message = $"Reminder: Hi {patientName}, you have an appointment with Dr. {doctorName} tomorrow at {appointmentTime:HH:mm}. Please arrive 15 minutes early. - Hospital Appointment System";
        return await SendSmsAsync(phoneNumber, message);
    }
}
