using RamyroTask.Entities;

namespace RamyroTask.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Patient> Patients { get; }
        IGenericRepository<Doctor> Doctors { get; }
        IGenericRepository<Appointment> Appointments { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}


