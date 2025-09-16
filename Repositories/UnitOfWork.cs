using Microsoft.EntityFrameworkCore.Storage;
using RamyroTask.Data;
using RamyroTask.Entities;

namespace RamyroTask.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Patients = new GenericRepository<Patient>(_context);
            Doctors = new GenericRepository<Doctor>(_context);
            Appointments = new GenericRepository<Appointment>(_context);
        }

        public IGenericRepository<Patient> Patients { get; }
        public IGenericRepository<Doctor> Doctors { get; }
        public IGenericRepository<Appointment> Appointments { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}


