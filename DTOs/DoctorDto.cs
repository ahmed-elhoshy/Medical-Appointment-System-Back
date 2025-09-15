using System.ComponentModel.DataAnnotations;

namespace RamyroTask.DTOs
{
    public class DoctorDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateDoctorDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Specialization { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class UpdateDoctorDto
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }
        
        [MaxLength(100)]
        public string? LastName { get; set; }
        
        [MaxLength(200)]
        public string? Specialization { get; set; }
        
        [EmailAddress]
        [MaxLength(255)]
        public string? Email { get; set; }
        
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
    }
}

