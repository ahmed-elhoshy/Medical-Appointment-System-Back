using AutoMapper;
using RamyroTask.DTOs;
using RamyroTask.Entities;

namespace RamyroTask.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Patient, PatientDto>();
            CreateMap<Doctor, DoctorDto>();
            
            CreateMap<Appointment, AppointmentDto>()
                .ForMember(dest => dest.PatientName, 
                    opt => opt.MapFrom(src => $"{src.Patient.FirstName} {src.Patient.LastName}"))
                .ForMember(dest => dest.DoctorName, 
                    opt => opt.MapFrom(src => $"{src.Doctor.FirstName} {src.Doctor.LastName}"))
                .ForMember(dest => dest.DoctorSpecialization, 
                    opt => opt.MapFrom(src => src.Doctor.Specialization));

            CreateMap<CreateAppointmentDto, Appointment>();
            CreateMap<UpdatePatientDto, Patient>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<UpdateDoctorDto, Doctor>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
