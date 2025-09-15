using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RamyroTask.Data;
using RamyroTask.DTOs;
using RamyroTask.Entities;
using RamyroTask.Repositories;
using System.Security.Claims;

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

        public AppointmentsController(IUnitOfWork unitOfWork, ApplicationDbContext context, ILogger<AppointmentsController> logger)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _logger = logger;
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

                var appointment = new Appointment
                {
                    PatientId = createAppointmentDto.PatientId,
                    DoctorId = createAppointmentDto.DoctorId,
                    AppointmentDate = createAppointmentDto.AppointmentDate,
                    Reason = createAppointmentDto.Reason,
                    Status = AppointmentStatus.Scheduled
                };

                await _unitOfWork.Appointments.AddAsync(appointment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("New appointment created with ID: {AppointmentId}", appointment.Id);

                var appointmentDto = new AppointmentDto
                {
                    Id = appointment.Id,
                    PatientId = appointment.PatientId,
                    DoctorId = appointment.DoctorId,
                    AppointmentDate = appointment.AppointmentDate,
                    Reason = appointment.Reason,
                    Status = appointment.Status,
                    CreatedAt = appointment.CreatedAt,
                    PatientName = $"{patient.FirstName} {patient.LastName}",
                    DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                    DoctorSpecialization = doctor.Specialization
                };

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

                var appointmentDto = new AppointmentDto
                {
                    Id = appointment.Id,
                    PatientId = appointment.PatientId,
                    DoctorId = appointment.DoctorId,
                    AppointmentDate = appointment.AppointmentDate,
                    Reason = appointment.Reason,
                    Status = appointment.Status,
                    CreatedAt = appointment.CreatedAt,
                    PatientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}",
                    DoctorName = $"{appointment.Doctor.FirstName} {appointment.Doctor.LastName}",
                    DoctorSpecialization = appointment.Doctor.Specialization
                };

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

                var appointmentDtos = appointments.Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    PatientId = a.PatientId,
                    DoctorId = a.DoctorId,
                    AppointmentDate = a.AppointmentDate,
                    Reason = a.Reason,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                    DoctorName = $"{a.Doctor.FirstName} {a.Doctor.LastName}",
                    DoctorSpecialization = a.Doctor.Specialization
                });

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

                var appointmentDtos = appointments.Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    PatientId = a.PatientId,
                    DoctorId = a.DoctorId,
                    AppointmentDate = a.AppointmentDate,
                    Reason = a.Reason,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                    DoctorName = $"{a.Doctor.FirstName} {a.Doctor.LastName}",
                    DoctorSpecialization = a.Doctor.Specialization
                });

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
    }
}

