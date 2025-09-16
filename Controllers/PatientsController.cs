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
    public class PatientsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AuthService _authService;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(IUnitOfWork unitOfWork, AuthService authService, ILogger<PatientsController> logger)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetAll()
        {
            try
            {
                var patients = await _unitOfWork.Patients.GetAllAsync();
                var result = patients.Select(p => new PatientDto
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    DateOfBirth = p.DateOfBirth,
                    Email = p.Email,
                    PhoneNumber = p.PhoneNumber,
                    CreatedAt = p.CreatedAt
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all patients");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<PatientDto>> Register([FromBody] RegisterPatientRequest request)
        {
            try
            {
                var patient = await _authService.RegisterPatientAsync(request);
                if (patient == null)
                {
                    return BadRequest("Patient with this email already exists");
                }

                _logger.LogInformation("New patient registered with ID: {PatientId}", patient.Id);
                return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering patient");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var token = await _authService.AuthenticatePatientAsync(request.Email, request.Password);
                if (token == null)
                {
                    return Unauthorized("Invalid email");
                }

                _logger.LogInformation("Patient logged in with email: {Email}", request.Email);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during patient login");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<PatientDto>> GetPatient(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
                if (userId != id)
                {
                    return Forbid();
                }

                var patient = await _unitOfWork.Patients.GetByIdAsync(id);
                if (patient == null)
                {
                    return NotFound();
                }

                var patientDto = new PatientDto
                {
                    Id = patient.Id,
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    DateOfBirth = patient.DateOfBirth,
                    Email = patient.Email,
                    PhoneNumber = patient.PhoneNumber,
                    CreatedAt = patient.CreatedAt
                };

                return Ok(patientDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patient with ID: {PatientId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<PatientDto>> UpdatePatient(Guid id, [FromBody] UpdatePatientDto updatePatientDto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
                if (userId != id)
                {
                    return Forbid();
                }

                var patient = await _unitOfWork.Patients.GetByIdAsync(id);
                if (patient == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(updatePatientDto.FirstName))
                    patient.FirstName = updatePatientDto.FirstName;
                if (!string.IsNullOrEmpty(updatePatientDto.LastName))
                    patient.LastName = updatePatientDto.LastName;
                if (updatePatientDto.DateOfBirth.HasValue)
                    patient.DateOfBirth = updatePatientDto.DateOfBirth.Value;
                if (!string.IsNullOrEmpty(updatePatientDto.Email))
                    patient.Email = updatePatientDto.Email;
                if (!string.IsNullOrEmpty(updatePatientDto.PhoneNumber))
                    patient.PhoneNumber = updatePatientDto.PhoneNumber;

                _unitOfWork.Patients.Update(patient);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Patient updated with ID: {PatientId}", id);

                var patientDto = new PatientDto
                {
                    Id = patient.Id,
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    DateOfBirth = patient.DateOfBirth,
                    Email = patient.Email,
                    PhoneNumber = patient.PhoneNumber,
                    CreatedAt = patient.CreatedAt
                };

                return Ok(patientDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient with ID: {PatientId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}

