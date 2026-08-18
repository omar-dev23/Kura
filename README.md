## 🏥 Kura — AI-Powered Medical History Management Platform

> **All your medical information, anytime, anywhere.**
> 

Kura is a comprehensive medical history management platform that connects patients, doctors, hospitals, clinics, pharmacies, and laboratories in one unified ecosystem. Built with ASP.NET Core 8 and Flutter, powered by AI for intelligent document analysis and health pattern detection.

---

### 📱 Overview

Kura solves a critical problem in healthcare — fragmented medical records. Patients carry their full medical history in their pocket, doctors access patient data securely, and healthcare organizations manage their connections digitally.

---

### ✨ Key Features

#### 👤 For Patients

- 📋 Complete digital medical profile (blood type, allergies, chronic diseases, medications, surgeries)
- 📄 Upload and manage medical documents (lab results, X-rays, prescriptions, radiology)
- 🤖 AI-powered document analysis and health pattern detection
- 👨‍⚕️ Connect with doctors and healthcare organizations
- 💊 View prescriptions written by doctors
- 📅 Book and manage appointments
- 💬 Real-time chat with doctors and organizations
- 🔔 Smart notifications for all activities

#### 👨‍⚕️ For Doctors

- 🏥 Professional profile with specialization, certificates, and services
- 👥 View and manage assigned patients
- 📋 Access full patient medical history and documents
- 💊 Write digital prescriptions with multiple medicines
- 📅 Manage appointments and daily schedule
- 🏢 Link workplaces to hospitals and clinics
- 💬 Chat with patients directly
- ⭐ Patient rating system

#### 🏢 For Organizations (Hospital / Clinic / Pharmacy / Lab)

- 🏥 Rich organization profile with departments, services, and specialties
- 👥 Manage connected patients
- 💬 Chat with patients
- ⭐ Patient rating system
- 📸 Profile photo and working hours management

---

### 🛠️ Tech Stack

| Layer | Technology |
| --- | --- |
| **Backend** | ASP.NET Core 8, Entity Framework Core, SQL Server |
| **Frontend** | Flutter (iOS & Android) |
| **Authentication** | JWT Bearer Tokens |
| **AI Service** | Python FastAPI + Grok AI (document analysis, pattern detection, medical summaries) |
| **Email** | Gmail SMTP via MailKit (OTP verification) |
| **Networking** | Tailscale VPN (AI service connectivity) |
| **Deployment** | Railway |

---

### 🏗️ Architecture

```
Flutter App
     │
     ▼
ASP.NET Core 8 API
     │
     ├── SQL Server (main database)
     │
     └── Python FastAPI AI Service
              │
              ├── Document Analysis (PDF, X-Ray, Lab Results)
              ├── Health Pattern Detection
              └── Medical Summary Generation (RAG)
```

---

### 📡 API Overview

| Controller | Endpoints | Description |
| --- | --- | --- |
| **Auth** | 8 endpoints | Register, Login, OTP, Reset Password |
| **Patient** | 3 endpoints | Profile management, photo upload |
| **Doctor** | 10 endpoints | Profile, certificates, services, patients |
| **Connection** | 6 endpoints | Doctor-Patient connection system |
| **OrgConnection** | 6 endpoints | Patient-Organization connection system |
| **Document** | 4 endpoints | Upload, view, delete medical records |
| **Prescription** | 5 endpoints | Write and manage prescriptions |
| **Appointment** | 5 endpoints | Book and manage appointments |
| **Organization** | 14 endpoints | Full organization management |
| **Workplace** | 3 endpoints | Doctor workplace management |
| **Chat** | 5 endpoints | Messaging system |
| **Notification** | 6 endpoints | Push notifications |
| **AI** | 5 endpoints | Document analysis, pattern detection |

---

### 🗄️ Database Schema

```
Users
 ├── Patients (medical profile + documents)
 ├── Doctors (profile + certificates + services + workplaces)
 └── Organizations (Hospital / Clinic / Pharmacy / Lab)

Connections
 ├── DoctorPatientConnections
 └── PatientOrganizationConnections

Medical
 ├── Documents
 ├── Prescriptions + PrescriptionMedicines
 └── Appointments

Communication
 ├── Messages (Chat)
 └── Notifications
```

---

### 🔐 Security

- JWT Bearer Token authentication with role-based authorization
- BCrypt password hashing
- OTP email verification for password reset (5-minute expiry)
- Role-based access control (Patient / Doctor / Organization)
- Secure file upload with type and size validation (max 10MB)

---

### 🚀 Getting Started

#### Prerequisites

- .NET 8 SDK
- SQL Server
- Visual Studio 2022

#### Installation

bash

```bash
# 1. Clone the repository
git clone https://github.com/your-username/kura-api.git

# 2. Navigate to the project
cd kura-api/Kura.API

# 3. Update appsettings.json with your settings
# - ConnectionStrings:DefaultConnection → your SQL Server
# - JwtSettings:SecretKey → your secret key
# - EmailSettings → your Gmail credentials
# - KuraAI:BaseUrl → your AI service URL

# 4. Apply migrations
dotnet ef database update

# 5. Run the project
dotnet run
```

#### Configuration (`appsettings.json`)

json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=KuraDB;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_SECRET_KEY",
    "Issuer": "KuraAPI",
    "Audience": "KuraApp",
    "ExpiryDays": 7
  },
  "EmailSettings": {
    "FromEmail": "your-email@gmail.com",
    "FromName": "Kura Medical",
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  },
  "KuraAI": {
    "BaseUrl": "http://your-ai-service-url:8005"
  }
}
```

---

### 👥 Team

| Role | Responsibility |
| --- | --- |
| **Backend** | ASP.NET Core 8 API |
| **Frontend** | Flutter Mobile App |
| **AI** | Python FastAPI + Medical AI Models |

---

### 📄 License

This project is a graduation project developed for academic purposes.

---

> Built with ❤️ by the Kura Team
>
