# Medical Appointment System - Backend API

A comprehensive medical appointment management system built with ASP.NET Core Web API, Entity Framework Core, and JWT authentication.

## Features

- **Patient Management**: Register, login, view profile, and update information
- **Doctor Management**: Register, login, view profile, and update information
- **Appointment Scheduling**: Patients can schedule appointments with doctors
- **Appointment Management**: View, cancel appointments for both patients and doctors
- **JWT Authentication**: Secure API endpoints with role-based access
- **Background Services**: Automated email reminders for upcoming appointments
- **Comprehensive Logging**: Serilog integration for detailed logging
- **Unit of Work Pattern**: Clean architecture with repository pattern
- **Entity Framework Core**: Code-first approach with SQL Server

## Technology Stack

- ASP.NET Core 8.0 Web API
- Entity Framework Core 8.0
- SQL Server Database
- JWT Bearer Authentication
- Serilog for Logging
- Swagger/OpenAPI Documentation

## Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code


The API will be available at:

- HTTPS: `https://localhost:7xxx`
- HTTP: `http://localhost:5xxx`
- Swagger UI: `https://localhost:7xxx/swagger`

## API Endpoints

### Authentication

- `POST /api/patients/register` - Register a new patient
- `POST /api/patients/login` - Patient login
- `POST /api/doctors/register` - Register a new doctor
- `POST /api/doctors/login` - Doctor login

### Patients (Requires JWT Token)

- `GET /api/patients` - Get all patients details
- `GET /api/patients/{id}` - Get patient details
- `PUT /api/patients/{id}` - Update patient information

### Doctors (Requires JWT Token)

- `GET /api/doctors` - Get all doctors details
- `GET /api/doctors/{id}` - Get doctor details
- `PUT /api/doctors/{id}` - Update doctor information

### Appointments (Requires JWT Token)

- `POST /api/appointments` - Schedule new appointment (Patient only)
- `GET /api/appointments/{id}` - Get appointment details
- `GET /api/appointments/patient/{patientId}` - Get patient's appointments
- `GET /api/appointments/doctor/{doctorId}` - Get doctor's appointments
- `PUT /api/appointments/{id}/cancel` - Cancel appointment
- `PUT /api/appointments/{id}/complete` - Complete appointment


## Authentication

All protected endpoints require a JWT token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

## Sample API Usage

### 1. Register a Patient

```bash
curl -X POST "https://localhost:7xxx/api/patients/register" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "dateOfBirth": "1990-01-01",
    "email": "john.doe@email.com",
    "phoneNumber": "123-456-7890"
  }'
```

### 2. Login as Patient

```bash
curl -X POST "https://localhost:7xxx/api/patients/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@email.com"
  }'
```

### 3. Schedule Appointment (with JWT token)

```bash
curl -X POST "https://localhost:7xxx/api/appointments" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{
    "patientId": 1,
    "doctorId": 1,
    "appointmentDate": "2024-12-25T10:00:00Z",
    "reason": "Regular checkup"
  }'
```

## Background Services

The application includes a background service that runs every hour to check for appointments scheduled in the next 24 hours and logs reminder messages. Check the console output or log files for reminder notifications.

## Logging

Logs are written to:

- Console output
- File: `logs/log-{date}.txt`

Log levels and configuration can be adjusted in `appsettings.json`.

Log example after testing

[11:38:27 INF] Background reminder service started

[11:38:31 INF] Patient logged in with email: user@examplew.com

[11:39:21 INF] Doctor logged in with email: user@examplew.com

[11:39:46 INF] New appointment created with ID: 01247177-3cea-4c4c-c1fb-08ddf4fc9786

[11:39:55 INF] Doctor logged in with email: user@examplew.com

[11:40:02 INF] Appointment completed with ID: 01247177-3cea-4c4c-c1fb-08ddf4fc9786

[11:45:58 INF] New appointment created with ID: 56ad5448-d2fa-480d-c1fc-08ddf4fc9786

[12:02:55 INF] Doctor updated with ID: ce970ffb-8840-4cd3-b316-8ee4ab1789d9

[12:19:58 INF] Patient updated with ID: 91c83dbc-7497-4710-a97d-d973833a15e8

[12:38:27 INF] Sending reminder for appointment 32caecc5-29b2-4784-7c6e-08ddf4f2c073 - Patient: Ahmed Elhoshy (user@examplew.com), Doctor: string string (user@example.com), Date: 09/17/2025 19:30:00

[12:38:27 INF] Processed 1 upcoming appointments for reminders

[13:04:27 INF] New patient registered with ID: 6e62d113-f2ad-4597-a6e3-4259069bbcb7

[13:05:45 INF] New doctor registered with ID: 823b7969-557a-4326-b59d-4250b771a1cd

[13:05:48 INF] Doctor logged in with email: ahmedelhoshy@gmail.com

[13:38:27 INF] Sending reminder for appointment 32caecc5-29b2-4784-7c6e-08ddf4f2c073 - Patient: Ahmed Elhoshy (user@examplew.com), Doctor: string string (user@example.com), Date: 09/17/2025 19:30:00


## Database Schema

### Patients Table

- Id (Primary Key)
- FirstName, LastName
- DateOfBirth
- Email (Unique)
- PhoneNumber
- CreatedAt

### Doctors Table

- Id (Primary Key)
- FirstName, LastName
- Specialization
- Email (Unique)
- PhoneNumber
- CreatedAt

### Appointments Table

- Id (Primary Key)
- PatientId (Foreign Key)
- DoctorId (Foreign Key)
- AppointmentDate
- Reason
- Status (Scheduled/Completed/Cancelled)
- CreatedAt

## Security Features

- JWT token-based authentication
- Role-based authorization (Patient/Doctor)
- Users can only access their own data
- Input validation and error handling
- Secure password handling (email-based auth for simplicity)
