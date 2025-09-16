using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RamyroTask.Data;
using RamyroTask.DTOs;
using RamyroTask.Entities;
using RamyroTask.Repositories;
using System.Security.Claims;
using AutoMapper;

namespace RamyroTask.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentsController> _logger;
        private readonly IMapper _mapper;

        public AppointmentsController(
            IUnitOfWork unitOfWork,
            ApplicationDbContext context,
            ILogger<AppointmentsController> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<AppointmentDto>> CreateAppointment([FromBody] CreateAppointmentDto createAppointmentDto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
                var patient = await _unitOfWork.Patients.GetByIdAsync(createAppointmentDto.PatientId);
                if (patient == null)
                {
                    return BadRequest("Patient not found");
                }
                if (patient.Id != userId)
                {
                    return Forbid();
                }

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(createAppointmentDto.DoctorId);
                if (doctor == null)
                {
                    return BadRequest("Doctor not found");
                }

                if (createAppointmentDto.AppointmentDate <= DateTime.UtcNow)
                {
                    return BadRequest("Appointment date must be in the future");
                }

                var appointment = _mapper.Map<Appointment>(createAppointmentDto);
                appointment.Status = AppointmentStatus.Scheduled;

                await _unitOfWork.Appointments.AddAsync(appointment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("New appointment created with ID: {AppointmentId}", appointment.Id);

                var appointmentDto = _mapper.Map<AppointmentDto>(appointment);

                return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDto>> GetAppointment(Guid id)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (appointment == null)
                {
                    return NotFound();
                }

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole == "Patient")
                {
                    if (appointment.PatientId != userId) return Forbid();
                }

                if (userRole == "Doctor")
                {
                    if (appointment.DoctorId != userId) return Forbid();
                }

                var appointmentDto = _mapper.Map<AppointmentDto>(appointment);

                return Ok(appointmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointment with ID: {AppointmentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetPatientAppointments(Guid patientId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
                if (userId != patientId)
                {
                    return Forbid();
                }

                var appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Where(a => a.PatientId == patientId)
                    .OrderBy(a => a.AppointmentDate)
                    .ToListAsync();

                var appointmentDtos = _mapper.Map<IEnumerable<AppointmentDto>>(appointments);

                return Ok(appointmentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointments for patient ID: {PatientId}", patientId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("doctor/{doctorId}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetDoctorAppointments(Guid doctorId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
                if (userId != doctorId)
                {
                    return Forbid();
                }

                var appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Where(a => a.DoctorId == doctorId)
                    .OrderBy(a => a.AppointmentDate)
                    .ToListAsync();

                var appointmentDtos = _mapper.Map<IEnumerable<AppointmentDto>>(appointments);

                return Ok(appointmentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointments for doctor ID: {DoctorId}", doctorId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<ActionResult> CancelAppointment(Guid id)
        {
            try
            {
                var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
                if (appointment == null)
                {
                    return NotFound();
                }

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole == "Patient" && appointment.PatientId != userId)
                {
                    return Forbid();
                }

                if (userRole == "Doctor" && appointment.DoctorId != userId)
                {
                    return Forbid();
                }

                if (appointment.Status == AppointmentStatus.Cancelled)
                {
                    return BadRequest("Appointment is already cancelled");
                }

                if (appointment.Status == AppointmentStatus.Completed)
                {
                    return BadRequest("Cannot cancel a completed appointment");
                }

                appointment.Status = AppointmentStatus.Cancelled;
                _unitOfWork.Appointments.Update(appointment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Appointment cancelled with ID: {AppointmentId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment with ID: {AppointmentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}/complete")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> CompleteAppointment(Guid id)
        {
            try
            {
                var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
                if (appointment == null)
                {
                    return NotFound();
                }

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
                if (appointment.DoctorId != userId)
                {
                    return Forbid();
                }

                if (appointment.Status == AppointmentStatus.Completed)
                {
                    return BadRequest("Appointment is already completed");
                }

                if (appointment.Status == AppointmentStatus.Cancelled)
                {
                    return BadRequest("Cannot complete a cancelled appointment");
                }

                appointment.Status = AppointmentStatus.Completed;
                _unitOfWork.Appointments.Update(appointment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Appointment completed with ID: {AppointmentId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing appointment with ID: {AppointmentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

