using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HospitalAppointmentSystem.API.Models;
using HospitalAppointmentSystem.Application.Interfaces;
using HospitalAppointmentSystem.Domain.Entities;
using HospitalAppointmentSystem.Domain.Enums;

namespace HospitalAppointmentSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;
    private readonly IUserService _userService;

    public DoctorsController(IDoctorService doctorService, IUserService userService)
    {
        _doctorService = doctorService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var doctors = await _doctorService.GetAllAsync();
        var doctorDtos = doctors.Select(d => new DoctorDto
        {
            Id = d.Id,
            UserId = d.UserId,
            Specialty = d.Specialty,
            Phone = d.Phone,
            User = new UserDto
            {
                Id = d.User.Id,
                Email = d.User.Email,
                Name = d.User.Name,
                Role = d.User.Role,
                CreatedAt = d.User.CreatedAt
            }
        });

        return Ok(doctorDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);
        if (doctor == null)
            return NotFound();

        var doctorDto = new DoctorDto
        {
            Id = doctor.Id,
            UserId = doctor.UserId,
            Specialty = doctor.Specialty,
            Phone = doctor.Phone,
            User = new UserDto
            {
                Id = doctor.User.Id,
                Email = doctor.User.Email,
                Name = doctor.User.Name,
                Role = doctor.User.Role,
                CreatedAt = doctor.User.CreatedAt
            }
        };

        return Ok(doctorDto);
    }

    [HttpGet("specialty/{specialty}")]
    public async Task<IActionResult> GetBySpecialty(string specialty)
    {
        var doctors = await _doctorService.GetBySpecialtyAsync(specialty);
        var doctorDtos = doctors.Select(d => new DoctorDto
        {
            Id = d.Id,
            UserId = d.UserId,
            Specialty = d.Specialty,
            Phone = d.Phone,
            User = new UserDto
            {
                Id = d.User.Id,
                Email = d.User.Email,
                Name = d.User.Name,
                Role = d.User.Role,
                CreatedAt = d.User.CreatedAt
            }
        });

        return Ok(doctorDtos);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDoctorDto createDoctorDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Check if user already exists
        var existingUser = await _userService.GetByEmailAsync(createDoctorDto.User.Email);
        if (existingUser != null)
            return BadRequest(new { message = "User with this email already exists" });

        // Create user first
        var user = new User
        {
            Email = createDoctorDto.User.Email,
            PasswordHash = createDoctorDto.User.Password,
            Name = createDoctorDto.User.Name,
            Role = UserRole.Doctor
        };

        var createdUser = await _userService.CreateAsync(user);

        // Create doctor
        var doctor = new Doctor
        {
            UserId = createdUser.Id,
            Specialty = createDoctorDto.Specialty,
            Phone = createDoctorDto.Phone
        };

        var createdDoctor = await _doctorService.CreateAsync(doctor);
        
        // Get the complete doctor with user info
        var completeDoctorInfo = await _doctorService.GetByIdAsync(createdDoctor.Id);

        var doctorDto = new DoctorDto
        {
            Id = completeDoctorInfo!.Id,
            UserId = completeDoctorInfo.UserId,
            Specialty = completeDoctorInfo.Specialty,
            Phone = completeDoctorInfo.Phone,
            User = new UserDto
            {
                Id = completeDoctorInfo.User.Id,
                Email = completeDoctorInfo.User.Email,
                Name = completeDoctorInfo.User.Name,
                Role = completeDoctorInfo.User.Role,
                CreatedAt = completeDoctorInfo.User.CreatedAt
            }
        };

        return CreatedAtAction(nameof(GetById), new { id = createdDoctor.Id }, doctorDto);
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable([FromQuery] DateOnly date, [FromQuery] TimeOnly time)
    {
        var doctors = await _doctorService.GetAvailableDoctorsAsync(date, time);
        var doctorDtos = doctors.Select(d => new DoctorDto
        {
            Id = d.Id,
            UserId = d.UserId,
            Specialty = d.Specialty,
            Phone = d.Phone,
            User = new UserDto
            {
                Id = d.User.Id,
                Email = d.User.Email,
                Name = d.User.Name,
                Role = d.User.Role,
                CreatedAt = d.User.CreatedAt
            }
        });

        return Ok(doctorDtos);
    }
}
