using Microsoft.AspNetCore.Mvc;
using HospitalAppointmentSystem.API.Models;
using HospitalAppointmentSystem.Application.Interfaces;
using HospitalAppointmentSystem.Domain.Entities;
using HospitalAppointmentSystem.Domain.Enums;
using System.Linq;

namespace HospitalAppointmentSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IDoctorService _doctorService;

    public AuthController(IUserService userService, IDoctorService doctorService)
    {
        _userService = userService;
        _doctorService = doctorService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var token = await _userService.AuthenticateAsync(loginDto.Email, loginDto.Password);
        
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new { message = "Invalid email or password" });

        var user = await _userService.GetByEmailAsync(loginDto.Email);
        
        return Ok(new
        {
            token,
            user = new UserDto
            {
                Id = user!.Id,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            }
        });
    }

    [HttpGet("health")]
    public async Task<IActionResult> HealthCheck()
    {
        try
        {
            // Simple database connectivity test
            var userCount = await _userService.GetAllAsync();
            return Ok(new { status = "healthy", message = "Database connection working", userCount = userCount.Count() });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = "unhealthy", message = ex.Message });
        }
    }

    [HttpPost("test-create")]
    public async Task<IActionResult> TestCreateUser()
    {
        try
        {
            var testUser = new User
            {
                Email = $"test{DateTime.Now.Ticks}@hospital.com",
                PasswordHash = "TestPassword123!",
                Name = "Test User",
                Role = UserRole.Patient
            };

            var createdUser = await _userService.CreateAsync(testUser);
            return Ok(new { success = true, message = "User created successfully", userId = createdUser.Id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            // Debug logging
            Console.WriteLine($"[DEBUG] Register attempt - Email: {createUserDto?.Email}, Name: {createUserDto?.Name}, Role: {createUserDto?.Role}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"[DEBUG] ModelState invalid: {string.Join(", ", ModelState.SelectMany(x => x.Value?.Errors ?? Enumerable.Empty<Microsoft.AspNetCore.Mvc.ModelBinding.ModelError>()).Select(x => x.ErrorMessage))}");
                return BadRequest(ModelState);
            }

            // Check if user already exists
            Console.WriteLine($"[DEBUG] Checking if user exists: {createUserDto?.Email}");
            var existingUser = await _userService.GetByEmailAsync(createUserDto?.Email ?? string.Empty);
            if (existingUser != null)
            {
                Console.WriteLine($"[DEBUG] User already exists: {createUserDto.Email}");
                return BadRequest(new { message = "User with this email already exists" });
            }

            Console.WriteLine($"[DEBUG] Creating user object...");
            var user = new User
            {
                Email = createUserDto.Email,
                PasswordHash = createUserDto.Password, // Will be hashed in the service
                Name = createUserDto.Name,
                Role = createUserDto.Role
            };

            Console.WriteLine($"[DEBUG] Calling _userService.CreateAsync...");
            var createdUser = await _userService.CreateAsync(user);
            Console.WriteLine($"[DEBUG] User created successfully with ID: {createdUser.Id}");

            // Si el usuario es un doctor, crear automáticamente el perfil de doctor
            if (createdUser.Role == UserRole.Doctor)
            {
                try
                {
                    var doctor = new Doctor
                    {
                        UserId = createdUser.Id,
                        Specialty = "General", // Especialidad por defecto
                        Phone = "", // Se puede actualizar después
                        User = createdUser
                    };
                    
                    await _doctorService.CreateAsync(doctor);
                    Console.WriteLine($"[DEBUG] Doctor profile created for user ID: {createdUser.Id}");
                }
                catch (Exception ex)
                {
                    // Log el error pero no fallar el registro del usuario
                    Console.WriteLine($"[ERROR] Error creando perfil de doctor: {ex.Message}");
                }
            }

            return CreatedAtAction(nameof(Login), new
            {
                user = new UserDto
                {
                    Id = createdUser.Id,
                    Email = createdUser.Email,
                    Name = createdUser.Name,
                    Role = createdUser.Role,
                    CreatedAt = createdUser.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Register failed: {ex.Message}");
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { message = "Registration failed", error = ex.Message });
        }
    }
}
