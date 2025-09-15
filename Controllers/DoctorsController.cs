using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RamyroTask.DTOs;
using RamyroTask.Entities;
using RamyroTask.Repositories;
using RamyroTask.Services;
using System.Security.Claims;

namespace RamyroTask.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AuthService _authService;
        private readonly ILogger<DoctorsController> _logger;

        public DoctorsController(IUnitOfWork unitOfWork, AuthService authService, ILogger<DoctorsController> logger)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<DoctorDto>> Register([FromBody] RegisterDoctorRequest request)
        {
            try
            {
                var doctor = await _authService.RegisterDoctorAsync(request);
                if (doctor == null)
                {
                    return BadRequest("Doctor with this email already exists");
                }

                _logger.LogInformation("New doctor registered with ID: {DoctorId}", doctor.Id);
                return CreatedAtAction(nameof(GetDoctor), new { id = doctor.Id }, doctor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering doctor");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var token = await _authService.AuthenticateDoctorAsync(request.Email, request.Password);
                if (token == null)
                {
                    return Unauthorized("Invalid email");
                }

                _logger.LogInformation("Doctor logged in with email: {Email}", request.Email);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during doctor login");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<DoctorDto>> GetDoctor(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
                if (userId != id)
                {
                    return Forbid();
                }

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
                if (doctor == null)
                {
                    return NotFound();
                }

                var doctorDto = new DoctorDto
                {
                    Id = doctor.Id,
                    FirstName = doctor.FirstName,
                    LastName = doctor.LastName,
                    Specialization = doctor.Specialization,
                    Email = doctor.Email,
                    PhoneNumber = doctor.PhoneNumber,
                    CreatedAt = doctor.CreatedAt
                };

                return Ok(doctorDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctor with ID: {DoctorId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<DoctorDto>> UpdateDoctor(Guid id, [FromBody] UpdateDoctorDto updateDoctorDto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
                if (userId != id)
                {
                    return Forbid();
                }

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
                if (doctor == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(updateDoctorDto.FirstName))
                    doctor.FirstName = updateDoctorDto.FirstName;
                if (!string.IsNullOrEmpty(updateDoctorDto.LastName))
                    doctor.LastName = updateDoctorDto.LastName;
                if (!string.IsNullOrEmpty(updateDoctorDto.Specialization))
                    doctor.Specialization = updateDoctorDto.Specialization;
                if (!string.IsNullOrEmpty(updateDoctorDto.Email))
                    doctor.Email = updateDoctorDto.Email;
                if (!string.IsNullOrEmpty(updateDoctorDto.PhoneNumber))
                    doctor.PhoneNumber = updateDoctorDto.PhoneNumber;

                _unitOfWork.Doctors.Update(doctor);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Doctor updated with ID: {DoctorId}", id);

                var doctorDto = new DoctorDto
                {
                    Id = doctor.Id,
                    FirstName = doctor.FirstName,
                    LastName = doctor.LastName,
                    Specialization = doctor.Specialization,
                    Email = doctor.Email,
                    PhoneNumber = doctor.PhoneNumber,
                    CreatedAt = doctor.CreatedAt
                };

                return Ok(doctorDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating doctor with ID: {DoctorId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

