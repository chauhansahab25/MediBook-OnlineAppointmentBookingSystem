# MediBook API Testing Guide (Postman)

This guide provides instructions on how to test the MediBook Microservices using Postman.

## 1. Environment Setup
Create a new environment in Postman with the following variables:
- `base_url_auth`: `http://localhost:5219/api/v1/auth`
- `base_url_provider`: `http://localhost:5096/api/v1/providers`
- `base_url_appointment`: `http://localhost:5177/api/v1/appointments`
- `base_url_schedule`: `http://localhost:5043/api/v1/slots`
- `base_url_review`: `http://localhost:5211/api/v1/reviews`
- `base_url_payment`: `http://localhost:5048/api/v1/payments`
- `token`: (Leave empty, will be populated after login)

## 2. Authentication Flow

### Register a New User
- **Method**: `POST`
- **URL**: `{{base_url_auth}}/register`
- **Body**:
```json
{
  "fullName": "Test Patient",
  "email": "patient@test.com",
  "password": "Password123!",
  "role": "Patient"
}
```

### Login
- **Method**: `POST`
- **URL**: `{{base_url_auth}}/login`
- **Body**:
```json
{
  "email": "patient@test.com",
  "password": "Password123!"
}
```
- **Action**: Copy the `accessToken` from the response and paste it into the `token` environment variable.

## 3. Provider Management

### Get All Providers
- **Method**: `GET`
- **URL**: `{{base_url_provider}}`

### Create Provider Profile (Requires Provider Role)
- **Method**: `POST`
- **URL**: `{{base_url_provider}}`
- **Headers**: `Authorization: Bearer {{token}}`
- **Body**:
```json
{
  "userId": 2,
  "specialization": "Cardiology",
  "qualification": "MD",
  "experienceYears": 10,
  "bio": "Expert cardiologist with 10 years experience.",
  "clinicName": "Heart Care Clinic",
  "clinicAddress": "123 Health St, City"
}
```

## 4. Appointment Booking

### Book an Appointment
- **Method**: `POST`
- **URL**: `{{base_url_appointment}}`
- **Headers**: `Authorization: Bearer {{token}}`
- **Body**:
```json
{
  "patientId": 1,
  "providerId": 1,
  "slotId": 101,
  "serviceType": "Consultation",
  "appointmentDate": "2026-06-01",
  "startTime": "10:00:00",
  "endTime": "10:30:00",
  "modeOfConsultation": "InPerson"
}
```

## 5. Payments (Razorpay Integration)

### Process Payment
- **Method**: `POST`
- **URL**: `{{base_url_payment}}`
- **Headers**: `Authorization: Bearer {{token}}`
- **Body**:
```json
{
  "appointmentId": 1,
  "amount": 500.00,
  "currency": "INR",
  "paymentMethod": "UPI"
}
```

## 6. Tips for Testing
- Always ensure the `Authorization` header is present for protected endpoints.
- Check the console logs of each microservice if you receive a `500 Internal Server Error`.
- Use the `Tests` tab in Postman to automatically set the token:
```javascript
var jsonData = pm.response.json();
pm.environment.set("token", jsonData.accessToken);
```
