# MediBook Project Diagrams

This document contains Mermaid diagrams for the MediBook Online Appointment Booking System.

## 1. Architecture Diagram
This diagram illustrates the overall microservices architecture of the system.

```mermaid
graph TD
    Client[Angular Frontend]

    subgraph Microservices Layer
        Auth[Auth Service<br/>:5219]
        Prov[Provider Service<br/>:5096]
        Appt[Appointment Service<br/>:5177]
        Sched[Schedule Service<br/>:5043]
        Rev[Review Service<br/>:5211]
        Med[Medical Record Service<br/>:5238]
        Pay[Payment Service<br/>:5048]
        Notif[Notification Service<br/>:5192]
    end

    DB[(PostgreSQL Database<br/>Render.com)]

    Client -->|REST API| Auth
    Client -->|REST API| Prov
    Client -->|REST API| Appt
    Client -->|REST API| Sched
    Client -->|REST API| Rev
    Client -->|REST API| Med
    Client -->|REST API| Pay
    Client -->|REST API| Notif

    Auth --> DB
    Prov --> DB
    Appt --> DB
    Sched --> DB
    Rev --> DB
    Med --> DB
    Pay --> DB
    Notif --> DB
```

## 2. Database ER Diagram
This entity-relationship diagram shows the core database tables and their relationships across the various microservices.

```mermaid
erDiagram
    USER ||--o{ PROVIDER : "1 to 0..1"
    USER ||--o{ APPOINTMENT : "Patient (1 to many)"
    USER ||--o{ PAYMENT : "Patient (1 to many)"
    USER ||--o{ REVIEW : "Patient (1 to many)"
    
    PROVIDER ||--o{ AVAILABILITY_SLOT : "1 to many"
    PROVIDER ||--o{ APPOINTMENT : "1 to many"
    
    AVAILABILITY_SLOT ||--o{ APPOINTMENT : "1 to 0..1"
    
    APPOINTMENT ||--o| MEDICAL_RECORD : "1 to 1"
    APPOINTMENT ||--o| PAYMENT : "1 to 1"
    APPOINTMENT ||--o| REVIEW : "1 to 1"

    USER {
        int UserId PK
        string FullName
        string Email
        string PasswordHash
        string Role
        bool IsActive
    }
    
    PROVIDER {
        int ProviderId PK
        int UserId FK
        string Specialization
        string ClinicName
        double AvgRating
        bool IsVerified
    }
    
    AVAILABILITY_SLOT {
        int SlotId PK
        int ProviderId FK
        DateTime Date
        TimeSpan StartTime
        TimeSpan EndTime
        bool IsBooked
    }

    APPOINTMENT {
        int AppointmentId PK
        int PatientId FK "User"
        int ProviderId FK "Provider"
        int SlotId FK "Slot"
        string ServiceType
        string Status
        string ModeOfConsultation
    }

    PAYMENT {
        int PaymentId PK
        int AppointmentId FK
        int PatientId FK
        decimal Amount
        string Status
        string Mode
    }

    MEDICAL_RECORD {
        int RecordId PK
        int AppointmentId FK
        int PatientId FK
        int ProviderId FK
        string Diagnosis
        string Prescription
        string RecordType
    }

    REVIEW {
        int ReviewId PK
        int AppointmentId FK
        int PatientId FK
        int ProviderId FK
        int Rating
        string Comment
    }
```

## 3. Design Diagram
This design diagram outlines the modular structure of the backend application, showing the logical layers (API, Business, Data).

```mermaid
graph LR
    subgraph UI Layer
        A[Angular Components]
        B[Angular Services]
        A --> B
    end

    subgraph API Layer
        C[Controllers]
        D[DTOs / ViewModels]
        C --> D
    end

    subgraph Business Logic Layer
        E[Services / Interfaces]
        F[Entity Models]
        E --> F
    end

    subgraph Data Access Layer
        G[Entity Framework Core]
        H[DbContexts]
        G --> H
    end

    B -.->|HTTP Requests| C
    C --> E
    E --> G
    H --> DB[(PostgreSQL)]
```

## 4. Service Flow Diagram (Appointment Booking)
This sequence diagram shows the interaction between microservices when a patient books an appointment.

```mermaid
sequenceDiagram
    actor Patient
    participant API as Frontend Client
    participant Auth as Auth Service
    participant Appt as Appointment Service
    participant Sched as Schedule Service
    participant Notif as Notification Service

    Patient->>API: 1. Login
    API->>Auth: Authenticate Credentials
    Auth-->>API: JWT Token

    Patient->>API: 2. View Available Slots
    API->>Sched: GET /slots/{providerId}
    Sched-->>API: List of Slots

    Patient->>API: 3. Book Appointment
    API->>Appt: POST /appointments (PatientId, ProviderId, SlotId)
    
    Appt->>Sched: Validate & Mark Slot Booked
    Sched-->>Appt: Slot Confirmed
    
    Appt->>Appt: Save Appointment (Status: Scheduled)
    Appt-->>API: Appointment Details

    Appt->>Notif: Send Booking Confirmation Event
    Notif-->>Patient: Email / SMS Notification
    
    API-->>Patient: Display Success Message
```

## 5. UML Use Case Diagram
This use case diagram represents the actors in the system (Patient, Provider, Admin) and their primary use cases.

```mermaid
flowchart LR
    Patient((Patient))
    Provider((Provider))
    Admin((Admin))

    subgraph MediBook System
        UC1([Register & Login])
        UC2([Search Providers])
        UC3([Book Appointment])
        UC4([Submit Review])
        UC5([View Medical Records])
        
        UC6([Manage Profile])
        UC7([Manage Schedule])
        UC8([Manage Appointments])
        UC9([Add Medical Records])
        
        UC10([Manage Users])
        UC11([Verify Providers])
        UC12([Verify Reviews])
    end

    Patient --> UC1
    Patient --> UC2
    Patient --> UC3
    Patient --> UC4
    Patient --> UC5

    Provider --> UC1
    Provider --> UC6
    Provider --> UC7
    Provider --> UC8
    Provider --> UC9

    Admin --> UC1
    Admin --> UC10
    Admin --> UC11
    Admin --> UC12
```
