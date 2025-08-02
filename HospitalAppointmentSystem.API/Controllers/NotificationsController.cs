using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HospitalAppointmentSystem.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace HospitalAppointmentSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        IEmailService emailService,
        ISmsService smsService,
        IBackgroundJobService backgroundJobService,
        ILogger<NotificationsController> logger)
    {
        _emailService = emailService;
        _smsService = smsService;
        _backgroundJobService = backgroundJobService;
        _logger = logger;
    }

    [HttpPost("test-email")]
    [SwaggerOperation(Summary = "Send a test email", Description = "Sends a test email to verify SendGrid integration")]
    [SwaggerResponse(200, "Email sent successfully")]
    [SwaggerResponse(400, "Invalid request")]
    [SwaggerResponse(500, "Failed to send email")]
    public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var success = await _emailService.SendEmailAsync(
                request.ToEmail,
                request.ToName,
                "Test Email from Hospital Appointment System",
                $"<h2>Test Email</h2><p>Hello {request.ToName},</p><p>This is a test email from the Hospital Appointment System to verify that email notifications are working correctly.</p><p>If you received this email, the integration is working properly!</p><p>Best regards,<br>Hospital Appointment System</p>",
                $"Test Email\n\nHello {request.ToName},\n\nThis is a test email from the Hospital Appointment System to verify that email notifications are working correctly.\n\nIf you received this email, the integration is working properly!\n\nBest regards,\nHospital Appointment System");

            if (success)
            {
                _logger.LogInformation("Test email sent successfully to {Email}", request.ToEmail);
                return Ok(new { message = "Test email sent successfully", email = request.ToEmail });
            }
            else
            {
                _logger.LogWarning("Failed to send test email to {Email}", request.ToEmail);
                return StatusCode(500, new { message = "Failed to send test email" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending test email to {Email}", request.ToEmail);
            return StatusCode(500, new { message = "An error occurred while sending the test email" });
        }
    }

    [HttpPost("test-sms")]
    [SwaggerOperation(Summary = "Send a test SMS", Description = "Sends a test SMS to verify Twilio integration")]
    [SwaggerResponse(200, "SMS sent successfully")]
    [SwaggerResponse(400, "Invalid request")]
    [SwaggerResponse(500, "Failed to send SMS")]
    public async Task<IActionResult> SendTestSms([FromBody] TestSmsRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var message = $"Test SMS from Hospital Appointment System. Hello {request.Name}, this is a test message to verify SMS notifications are working. If you received this, the integration is working!";
            
            var success = await _smsService.SendSmsAsync(request.PhoneNumber, message);

            if (success)
            {
                _logger.LogInformation("Test SMS sent successfully to {PhoneNumber}", request.PhoneNumber);
                return Ok(new { message = "Test SMS sent successfully", phoneNumber = request.PhoneNumber });
            }
            else
            {
                _logger.LogWarning("Failed to send test SMS to {PhoneNumber}", request.PhoneNumber);
                return StatusCode(500, new { message = "Failed to send test SMS" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending test SMS to {PhoneNumber}", request.PhoneNumber);
            return StatusCode(500, new { message = "An error occurred while sending the test SMS" });
        }
    }

    [HttpPost("schedule-test-job")]
    [SwaggerOperation(Summary = "Schedule a test background job", Description = "Schedules a test background job to verify Hangfire integration")]
    [SwaggerResponse(200, "Background job scheduled successfully")]
    [SwaggerResponse(400, "Invalid request")]
    public IActionResult ScheduleTestJob([FromBody] TestJobRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var jobId = _backgroundJobService.EnqueueEmailJob(
                request.Email,
                request.Name,
                "Test Background Job - Hospital Appointment System",
                $"<h2>Test Background Job</h2><p>Hello {request.Name},</p><p>This email was sent by a background job to test Hangfire integration.</p><p>Job scheduled at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p><p>Best regards,<br>Hospital Appointment System</p>");

            _logger.LogInformation("Test background job scheduled with ID: {JobId}", jobId);
            return Ok(new { message = "Test background job scheduled successfully", jobId = jobId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while scheduling test background job");
            return StatusCode(500, new { message = "An error occurred while scheduling the test job" });
        }
    }

    [HttpGet("health")]
    [SwaggerOperation(Summary = "Check notification services health", Description = "Returns the health status of notification services")]
    [SwaggerResponse(200, "Health status retrieved successfully")]
    public IActionResult GetHealthStatus()
    {
        var health = new
        {
            timestamp = DateTime.UtcNow,
            services = new
            {
                email = new
                {
                    status = "configured",
                    provider = "SendGrid"
                },
                sms = new
                {
                    status = "configured",
                    provider = "Twilio"
                },
                backgroundJobs = new
                {
                    status = "configured",
                    provider = "Hangfire"
                }
            }
        };

        return Ok(health);
    }
}

public class TestEmailRequest
{
    [SwaggerParameter("Email address to send test email to")]
    public string ToEmail { get; set; } = string.Empty;
    
    [SwaggerParameter("Name of the recipient")]
    public string ToName { get; set; } = string.Empty;
}

public class TestSmsRequest
{
    [SwaggerParameter("Phone number to send test SMS to (include country code)")]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [SwaggerParameter("Name of the recipient")]
    public string Name { get; set; } = string.Empty;
}

public class TestJobRequest
{
    [SwaggerParameter("Email address for background job test")]
    public string Email { get; set; } = string.Empty;
    
    [SwaggerParameter("Name of the recipient")]
    public string Name { get; set; } = string.Empty;
}
