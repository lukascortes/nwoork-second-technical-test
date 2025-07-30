# TimeOff Manager - Nwoork Technical Test

## Description

A web application to manage employee time-off requests. It allows users to register, log in, create time-off requests, view and filter them by status.  
The backend is built with .NET 6.0 and SQLite, while the frontend uses React with TypeScript.

## Technologies Used

**Backend:**
- .NET 6.0
- Entity Framework Core (SQLite)
- JWT for authentication
- Swagger (Swashbuckle) for API documentation
- BCrypt for password hashing

**Frontend:**
- React 19 + TypeScript
- Vite as build tool
- Axios for HTTP requests
- React Router DOM for routing
- Formik + Yup for form validation
- TailwindCSS for styling
- Heroicons and React Icons for icons
- date-fns for date management

**API Communication:** REST  
**CORS:** Configured for local development

## Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- Node.js and npm (or yarn)

## Environment Configuration

The frontend uses a `.env` file to define the backend base URL.

1. Copy `.env.example` to `.env` and adjust the API URL if needed

2. Backend runs by default at `http://localhost:5000`

3. Frontend runs by default at `http://localhost:5173`

## How to Run the Project

### Backend


cd backend/TimeOffManager
dotnet run

### Frontend

cd frontend
npm install
npm run dev

### API Enpoints

Full documentation available at: http://localhost:5000/swagger

## Authentication

POST /api/Auth/register — Register a new user

POST /api/Auth/login — Log in with credentials

## Users
GET /api/User — Get a list of users

GET /api/User/{id} — Get user details

PUT /api/User/{id} — Update user data

DELETE /api/User/{id} — Delete a user

## Time Off Requests

GET /api/TimeOffRequest — List time-off requests

POST /api/TimeOffRequest — Create a new request

PUT /api/TimeOffRequest/{id} — Edit a request

DELETE /api/TimeOffRequest/{id} — Delete a request

GET /api/TimeOffRequest/filter?status=pending|approved|rejected — Filter by status