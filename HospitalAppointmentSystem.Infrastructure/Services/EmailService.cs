using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HospitalAppointmentSystem.Application.Interfaces;

namespace HospitalAppointmentSystem.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ISendGridClient _sendGridClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(ISendGridClient sendGridClient, IConfiguration configuration, ILogger<EmailService> logger)
    {
        _sendGridClient = sendGridClient;
        _configuration = configuration;
        _logger = logger;
        _fromEmail = _configuration["SendGrid:FromEmail"] ?? "noreply@hospitalappointment.com";
        _fromName = _configuration["SendGrid:FromName"] ?? "Hospital Appointment System";
    }

    public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlContent, string plainTextContent = "")
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail, toName);
            
            var msg = MailHelper.CreateSingleEmail(
                from, 
                to, 
                subject, 
                string.IsNullOrEmpty(plainTextContent) ? htmlContent : plainTextContent, 
                htmlContent);

            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                _logger.LogInformation("Email sent successfully to {Email} with subject: {Subject}", toEmail, subject);
                return true;
            }
            else
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send email to {Email}. Status: {StatusCode}, Response: {Response}", 
                    toEmail, response.StatusCode, responseBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to {Email}", toEmail);
            return false;
        }
    }

    public async Task<bool> SendAppointmentConfirmationAsync(string patientEmail, string patientName, string doctorName, DateTime appointmentDate, TimeOnly appointmentTime, string reason)
    {
        var subject = "Appointment Confirmation - Hospital Appointment System";
        var htmlContent = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #2c5aa0;'>Appointment Confirmed</h2>
                    <p>Dear {patientName},</p>
                    <p>Your appointment has been successfully confirmed with the following details:</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Doctor:</strong> {doctorName}</p>
                        <p><strong>Date:</strong> {appointmentDate:dddd, MMMM dd, yyyy}</p>
                        <p><strong>Time:</strong> {appointmentTime:HH:mm}</p>
                    </div>
                    <p>Please arrive 15 minutes early for your appointment.</p>
                    <p>If you need to reschedule or cancel, please contact us as soon as possible.</p>
                    <p>Best regards,<br>Hospital Appointment System</p>
                </div>
            </body>
            </html>";

        var plainTextContent = $@"
            Appointment Confirmed
            
            Dear {patientName},
            
            Your appointment has been successfully confirmed:
            
            Doctor: {doctorName}
            Date: {appointmentDate:dddd, MMMM dd, yyyy}
            Time: {appointmentTime:HH:mm}
            
            Please arrive 15 minutes early for your appointment.
            
            Best regards,
            Hospital Appointment System";

        return await SendEmailAsync(patientEmail, patientName, subject, htmlContent, plainTextContent);
    }

    public async Task<bool> SendAppointmentCancellationAsync(string patientEmail, string patientName, string doctorName, DateTime appointmentDate, TimeOnly appointmentTime, string reason)
    {
        var subject = "Appointment Cancelled - Hospital Appointment System";
        var htmlContent = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #dc3545;'>Appointment Cancelled</h2>
                    <p>Dear {patientName},</p>
                    <p>We regret to inform you that your appointment has been cancelled:</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Doctor:</strong> {doctorName}</p>
                        <p><strong>Date:</strong> {appointmentDate:dddd, MMMM dd, yyyy}</p>
                        <p><strong>Time:</strong> {appointmentTime:HH:mm}</p>
                        <p><strong>Reason:</strong> {reason}</p>
                    </div>
                    <p>Please contact us to reschedule your appointment at your earliest convenience.</p>
                    <p>We apologize for any inconvenience caused.</p>
                    <p>Best regards,<br>Hospital Appointment System</p>
                </div>
            </body>
            </html>";

        var plainTextContent = $@"
            Appointment Cancelled
            
            Dear {patientName},
            
            Your appointment has been cancelled:
            
            Doctor: {doctorName}
            Date: {appointmentDate:dddd, MMMM dd, yyyy}
            Time: {appointmentTime:HH:mm}
            Reason: {reason}
            
            Please contact us to reschedule.
            
            Best regards,
            Hospital Appointment System";

        return await SendEmailAsync(patientEmail, patientName, subject, htmlContent, plainTextContent);
    }

    public async Task<bool> SendAppointmentReminderAsync(string patientEmail, string patientName, string doctorName, DateTime appointmentDate, TimeOnly appointmentTime)
    {
        var subject = "Appointment Reminder - Hospital Appointment System";
        var htmlContent = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #28a745;'>Appointment Reminder</h2>
                    <p>Dear {patientName},</p>
                    <p>This is a friendly reminder about your upcoming appointment:</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Doctor:</strong> {doctorName}</p>
                        <p><strong>Date:</strong> {appointmentDate:dddd, MMMM dd, yyyy}</p>
                        <p><strong>Time:</strong> {appointmentTime:HH:mm}</p>
                    </div>
                    <p>Please remember to arrive 15 minutes early.</p>
                    <p>If you need to reschedule or cancel, please contact us as soon as possible.</p>
                    <p>Best regards,<br>Hospital Appointment System</p>
                </div>
            </body>
            </html>";

        var plainTextContent = $@"
            Appointment Reminder
            
            Dear {patientName},
            
            Reminder for your upcoming appointment:
            
            Doctor: {doctorName}
            Date: {appointmentDate:dddd, MMMM dd, yyyy}
            Time: {appointmentTime:HH:mm}
            
            Please arrive 15 minutes early.
            
            Best regards,
            Hospital Appointment System";

        return await SendEmailAsync(patientEmail, patientName, subject, htmlContent, plainTextContent);
    }
}
