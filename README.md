# MediBook Backend Services - Setup & API Documentation

## Overview
MediBook is a microservices-based healthcare appointment booking system with 8 independent services.

## System Architecture & Diagrams
For a comprehensive visual overview of the MediBook system, please refer to the [MediBook Diagrams](./MediBook-Diagrams.md) file. This document includes:
- **Architecture Diagram**: Microservices layout and routing.
- **Database ER Diagram**: Entity relationships across the PostgreSQL database.
- **System Design Diagram**: API and business logic layering.
- **Service Flow Diagram**: Sequence for appointment booking.
- **UML Use Case Diagram**: Patient, Provider, and Admin workflows.

## Prerequisites
- .NET 8.0 SDK
- PostgreSQL Database (Render.com hosted)
- Visual Studio 2022 or VS Code

## Database Configuration
All services connect to the same PostgreSQL database on Render.com:
- **Host**: dpg-d7lfl9v7f7vs73b2cclg-a.oregon-postgres.render.com
- **Port**: 5432
- **Database**: medibookdb
- **Username**: medibookdb_user
- **Password**: ZmbLOr176sBpgjEgFdsrZNOwjObplDWR

## Quick Start

### Option 1: Start All Services (Recommended)
```powershell
cd E:\MediBook-OnlineAppointmentBookingSystem
.\start-all-services.ps1
```

### Option 2: Start Individual Services
```powershell
# AuthService
cd E:\MediBook-OnlineAppointmentBookingSystem\AuthService
dotnet run

# ProviderService
cd E:\MediBook-OnlineAppointmentBookingSystem\ProviderService
dotnet run

# AppointmentService
cd E:\MediBook-OnlineAppointmentBookingSystem\AppointmentService
dotnet run

# ScheduleService
cd E:\MediBook-OnlineAppointmentBookingSystem\ScheduleService
dotnet run

# ReviewService
cd E:\MediBook-OnlineAppointmentBookingSystem\ReviewService
dotnet run

# MedicalRecordService
cd E:\MediBook-OnlineAppointmentBookingSystem\MedicalRecordService
dotnet run

# PaymentService
cd E:\MediBook-OnlineAppointmentBookingSystem\PaymentService
dotnet run

# NotificationService
cd E:\MediBook-OnlineAppointmentBookingSystem\NotificationService
dotnet run
```

## Service Ports & Swagger URLs

| Service | Port | Swagger URL |
|---------|------|-------------|
| AuthService | 5219 | http://localhost:5219/swagger |
| ProviderService | 5096 | http://localhost:5096/swagger |
| AppointmentService | 5177 | http://localhost:5177/swagger |
| ScheduleService | 5043 | http://localhost:5043/swagger |
| ReviewService | 5211 | http://localhost:5211/swagger |
| MedicalRecordService | 5238 | http://localhost:5238/swagger |
| PaymentService | 5048 | http://localhost:5048/swagger |
| NotificationService | 5192 | http://localhost:5192/swagger |

## API Endpoints

### 1. AuthService (Port 5219)
**Base URL**: `http://localhost:5219/api/v1/auth`

#### Public Endpoints
- `POST /register` - Register new user
- `POST /login` - User login
- `POST /refresh` - Refresh JWT token
- `POST /logout` - User logout

#### Protected Endpoints (Requires JWT)
- `GET /profile` - Get current user profile
- `PUT /profile` - Update current user profile
- `PUT /password` - Change password
- `DELETE /deactivate` - Deactivate account

#### Admin Endpoints (Requires Admin Role)
- `GET /users` - Get all users
- `PUT /users/{id}` - Update user by ID
- `DELETE /users/{id}` - Delete user by ID

### 2. ProviderService (Port 5096)
**Base URL**: `http://localhost:5096/api/v1/providers`

#### Public Endpoints
- `GET /` - Get all providers
- `GET /{id}` - Get provider by ID
- `GET /user/{userId}` - Get provider by user ID
- `GET /search?term={term}` - Search providers

#### Protected Endpoints
- `POST /` - Create provider profile
- `PUT /{id}` - Update provider profile

#### Admin Endpoints
- `DELETE /{id}` - Delete provider
- `PUT /{id}/verify` - Verify provider
- `PUT /{id}/unverify` - Unverify provider

### 3. AppointmentService (Port 5177)
**Base URL**: `http://localhost:5177/api/v1/appointments`

- `GET /` - Get all appointments
- `GET /{id}` - Get appointment by ID
- `GET /patient/{patientId}` - Get appointments by patient
- `GET /provider/{providerId}` - Get appointments by provider
- `POST /` - Create appointment
- `PUT /{id}` - Update appointment
- `PUT /{id}/cancel` - Cancel appointment
- `PUT /{id}/reschedule` - Reschedule appointment

### 4. ScheduleService (Port 5043)
**Base URL**: `http://localhost:5043/api/v1/slots`

- `GET /{providerId}?date={date}` - Get available slots
- `POST /` - Create slot
- `POST /generateRecurring` - Create recurring slots
- `PUT /{slotId}/book` - Book slot
- `DELETE /{slotId}` - Delete slot

### 5. ReviewService (Port 5211)
**Base URL**: `http://localhost:5211/api/v1/reviews`

- `GET /` - Get all reviews
- `GET /{id}` - Get review by ID
- `GET /provider/{providerId}` - Get reviews by provider
- `POST /` - Create review
- `PUT /{id}` - Update review
- `DELETE /{id}` - Delete review

### 6. MedicalRecordService (Port 5238)
**Base URL**: `http://localhost:5238/api/v1/records`

- `GET /` - Get all medical records
- `GET /{id}` - Get record by ID
- `GET /{id}/download` - Download record file

### 7. PaymentService (Port 5048)
**Base URL**: `http://localhost:5048/api/v1/payments`

- `GET /` - Get payment history
- `GET /{id}` - Get payment by ID
- `POST /` - Process payment
- `POST /refund` - Refund payment

### 8. NotificationService (Port 5192)
**Base URL**: `http://localhost:5192/api/v1/notifications`

- `GET /` - Get all notifications
- `POST /` - Send notification
- `PUT /{id}/read` - Mark as read
- `PUT /read-all` - Mark all as read
- `DELETE /{id}` - Delete notification

## JWT Authentication

### Token Format
All protected endpoints require JWT token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

### Token Configuration
- **Issuer**: MediBookAuthService
- **Audience**: MediBookClients
- **Expiry**: 60 minutes
- **Secret Key**: YourSuperSecretKeyHereMustBe32CharsLong!!

### User Roles
- **Patient**: Can book appointments, view records, submit reviews
- **Provider**: Can manage schedule, view appointments, manage profile
- **Admin**: Full access to all resources

## Testing the APIs

### 1. Register a User
```bash
POST http://localhost:5219/api/v1/auth/register
Content-Type: application/json

{
  "fullName": "John Doe",
  "email": "john@example.com",
  "password": "Password123!",
  "role": "Patient"
}
```

### 2. Login
```bash
POST http://localhost:5219/api/v1/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "Password123!"
}
```

Response will include `accessToken` and `refreshToken`.

### 3. Use Token for Protected Endpoints
```bash
GET http://localhost:5219/api/v1/auth/profile
Authorization: Bearer <your-access-token>
```

## Database Migrations

All services automatically run migrations on startup. To manually run migrations:

```powershell
cd E:\MediBook-OnlineAppointmentBookingSystem\AuthService
dotnet ef database update

cd E:\MediBook-OnlineAppointmentBookingSystem\ProviderService
dotnet ef database update

# Repeat for other services...
```

## Troubleshooting

### Port Already in Use
If a port is already in use, kill the process:
```powershell
# Find process using port 5219
netstat -ano | findstr :5219

# Kill process by PID
taskkill /PID <process-id> /F
```

### Database Connection Issues
- Verify PostgreSQL database is accessible
- Check connection string in appsettings.json
- Ensure SSL Mode is set to "Require"

### CORS Issues
All services are configured with `AllowAll` CORS policy for development.

## Frontend Integration

The Angular frontend is configured to connect to these services in:
`medibook-frontend/src/environments/environment.ts`

Ensure all service URLs match the ports listed above.

## Production Deployment

For production:
1. Update connection strings in appsettings.json
2. Change JWT secret key
3. Configure proper CORS policies
4. Enable HTTPS
5. Set up reverse proxy (e.g., Nginx)
6. Use environment variables for sensitive data

## Support

For issues or questions, check:
- Swagger documentation at each service's /swagger endpoint
- Application logs in the console
- Database logs on Render.com dashboard
