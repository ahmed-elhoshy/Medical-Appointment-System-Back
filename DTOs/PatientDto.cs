using System.ComponentModel.DataAnnotations;

namespace RamyroTask.DTOs
{
    public class PatientDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePatientDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        public DateTime DateOfBirth { get; set; }
        
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class UpdatePatientDto
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }
        
        [MaxLength(100)]
        public string? LastName { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        [EmailAddress]
        [MaxLength(255)]
        public string? Email { get; set; }
        
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
    }
}

