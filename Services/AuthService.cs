using Microsoft.IdentityModel.Tokens;
using RamyroTask.DTOs;
using RamyroTask.Entities;
using RamyroTask.Helpers;
using RamyroTask.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RamyroTask.Services
{
    public class AuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwtSettings;
        public AuthService(IUnitOfWork unitOfWork, JwtSettings jwtSettings)
        {
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettings;
        }

        public async Task<string?> AuthenticatePatientAsync(string email, string password)
        {
            var patient = await _unitOfWork.Patients.FirstOrDefaultAsync(p => p.Email == email);
            if (patient == null) return null;
            if (!BCrypt.Net.BCrypt.Verify(password, patient.PasswordHash)) return null;
            return GenerateJwtToken(patient.Id, "Patient", patient.Email);
        }

        public async Task<string?> AuthenticateDoctorAsync(string email, string password)
        {
            var doctor = await _unitOfWork.Doctors.FirstOrDefaultAsync(d => d.Email == email);
            if (doctor == null) return null;
            if (!BCrypt.Net.BCrypt.Verify(password, doctor.PasswordHash)) return null;
            return GenerateJwtToken(doctor.Id, "Doctor", doctor.Email);
        }

        public async Task<PatientDto?> RegisterPatientAsync(RegisterPatientRequest request)
        {
            var exists = await _unitOfWork.Patients.ExistsAsync(p => p.Email == request.Email);
            if (exists) return null;
            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };
            await _unitOfWork.Patients.AddAsync(patient);
            await _unitOfWork.SaveChangesAsync();

            return new PatientDto
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                DateOfBirth = patient.DateOfBirth,
                Email = patient.Email,
                PhoneNumber = patient.PhoneNumber,
                CreatedAt = patient.CreatedAt
            };
        }

        public async Task<DoctorDto?> RegisterDoctorAsync(RegisterDoctorRequest request)
        {
            var exists = await _unitOfWork.Doctors.ExistsAsync(d => d.Email == request.Email);
            if (exists) return null;
            var doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Specialization = request.Specialization,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };
            await _unitOfWork.Doctors.AddAsync(doctor);
            await _unitOfWork.SaveChangesAsync();

            return new DoctorDto
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                Specialization = doctor.Specialization,
                Email = doctor.Email,
                PhoneNumber = doctor.PhoneNumber,
                CreatedAt = doctor.CreatedAt
            };
        }

        private string GenerateJwtToken(Guid userId, string role, string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

