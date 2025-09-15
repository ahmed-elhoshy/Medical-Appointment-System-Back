using System.ComponentModel.DataAnnotations;
using RamyroTask.Entities;

namespace RamyroTask.DTOs
{
    public class AppointmentDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public AppointmentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSpecialization { get; set; } = string.Empty;
    }

    public class CreateAppointmentDto
    {
        [Required]
        public Guid PatientId { get; set; }
        
        [Required]
        public Guid DoctorId { get; set; }
        
        [Required]
        public DateTime AppointmentDate { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    public class UpdateAppointmentDto
    {
        public DateTime? AppointmentDate { get; set; }
        
        [MaxLength(500)]
        public string? Reason { get; set; }
        
        public AppointmentStatus? Status { get; set; }
    }
}

