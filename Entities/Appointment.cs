using System.ComponentModel.DataAnnotations;

namespace RamyroTask.Entities
{
    public enum AppointmentStatus
    {
        Scheduled = 0,
        Completed = 1,
        Cancelled = 2
    }

    public class Appointment
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid PatientId { get; set; }
        
        [Required]
        public Guid DoctorId { get; set; }
        
        [Required]
        public DateTime AppointmentDate { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
        
        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual Patient Patient { get; set; } = null!;
        public virtual Doctor Doctor { get; set; } = null!;
    }
}

