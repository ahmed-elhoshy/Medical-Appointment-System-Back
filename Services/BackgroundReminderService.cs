using Microsoft.EntityFrameworkCore;
using RamyroTask.Data;
using RamyroTask.Entities;

namespace RamyroTask.Services
{
    public class BackgroundReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundReminderService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(1);

        public BackgroundReminderService(IServiceProvider serviceProvider, ILogger<BackgroundReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background reminder service started");

            using var timer = new PeriodicTimer(_period);

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await CheckUpcomingAppointments();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking upcoming appointments");
                }
            }

            _logger.LogInformation("Background reminder service stopped");
        }

        private async Task CheckUpcomingAppointments()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var tomorrow = DateTime.UtcNow.AddDays(1);
            var dayAfterTomorrow = DateTime.UtcNow.AddDays(2);

            var upcomingAppointments = await context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.AppointmentDate >= tomorrow && 
                           a.AppointmentDate < dayAfterTomorrow && 
                           a.Status == AppointmentStatus.Scheduled)
                .ToListAsync();

            foreach (var appointment in upcomingAppointments)
            {
                _logger.LogInformation("Sending reminder for appointment {AppointmentId} - Patient: {PatientName} ({PatientEmail}), Doctor: {DoctorName} ({DoctorEmail}), Date: {AppointmentDate}", 
                    appointment.Id, 
                    $"{appointment.Patient.FirstName} {appointment.Patient.LastName}", 
                    appointment.Patient.Email,
                    $"{appointment.Doctor.FirstName} {appointment.Doctor.LastName}", 
                    appointment.Doctor.Email,
                    appointment.AppointmentDate);

                await Task.Delay(100);
            }

            _logger.LogInformation("Processed {Count} upcoming appointments for reminders", upcomingAppointments.Count);
        }
    }
}

