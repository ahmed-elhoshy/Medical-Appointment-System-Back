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

- `GET /api/patients/{id}` - Get patient details
- `PUT /api/patients/{id}` - Update patient information

### Doctors (Requires JWT Token)

- `GET /api/doctors/{id}` - Get doctor details
- `PUT /api/doctors/{id}` - Update doctor information

### Appointments (Requires JWT Token)

- `POST /api/appointments` - Schedule new appointment (Patient only)
- `GET /api/appointments/{id}` - Get appointment details
- `GET /api/appointments/patient/{patientId}` - Get patient's appointments
- `GET /api/appointments/doctor/{doctorId}` - Get doctor's appointments
- `PUT /api/appointments/{id}/cancel` - Cancel appointment

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
