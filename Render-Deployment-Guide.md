# Render Deployment Guide - MediBook Microservices

This guide outlines the steps to deploy the MediBook microservices to [Render.com](https://render.com).

## 1. Database Setup
Render provides managed PostgreSQL databases.
- Create a new PostgreSQL database on Render.
- Copy the **Internal Database URL** for service-to-service communication or **External Database URL** for local testing.
- Ensure the connection string is updated in each service's `appsettings.json` or as an environment variable.

## 2. Deploying Services (Web Service)

### Using Docker (Recommended)
Each service includes a `Dockerfile`. Render can automatically build and deploy these.
1. Connect your GitHub repository to Render.
2. Select **New > Web Service**.
3. Choose the repository.
4. Set the **Root Directory** to the specific service folder (e.g., `AuthService`).
5. Set the **Runtime** to `Docker`.
6. Add Environment Variables:
   - `ConnectionStrings__DefaultConnection`: Your Render PostgreSQL URL (Ensure `sslmode=require` is appended).
   - `JwtSettings__Secret`: A secure 32-character key.
   - `ASPNETCORE_ENVIRONMENT`: `Production`.

### Deployment Order
It is recommended to deploy in this order:
1. **AuthService**: Foundation for identity.
2. **ProviderService** & **ScheduleService**: Core domain services.
3. **AppointmentService**: Orchestrates bookings.
4. **PaymentService**, **ReviewService**, **MedicalRecordService**: Supporting services.
5. **NotificationService**: Event-driven or background notifications.

## 3. Environment Variables Mapping

| Service | Variable Name | Description |
|---------|---------------|-------------|
| All | `ConnectionStrings__DefaultConnection` | PostgreSQL Connection String |
| AuthService | `JwtSettings__Secret` | JWT Signing Key |
| AppointmentService | `ServiceUrls__AuthService` | URL of deployed AuthService |
| AppointmentService | `ServiceUrls__ProviderService` | URL of deployed ProviderService |

## 4. Health Checks
Configure health check paths in Render settings to ensure zero-downtime deployments:
- Path: `/swagger/index.html` (or a dedicated `/health` endpoint if implemented).

## 5. Scaling
- For free tier, services will sleep after 15 minutes of inactivity.
- For production, use the **Starter** or **Standard** plans to keep services always active.

## 6. Common Issues
- **Migration Errors**: Ensure the database is accessible from the Render environment. The first service to start will usually run the migrations.
- **Port Mapping**: Render automatically detects the port from the Dockerfile (usually 80 or 8080). Ensure your `Program.cs` listens on the port provided by the `PORT` environment variable.
