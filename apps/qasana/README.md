# Qasana - Asana Clone

A full-stack project management application inspired by Asana, built with Next.js frontend and .NET microservices backend.

## Project Structure

```
apps/qasana/
├── wireframes/          # HTML wireframes and mockups
├── frontend/           # Next.js application
└── backend/            # .NET microservices
    ├── workspace-service/      # Workspace/team management
    ├── project-service/        # Project management
    ├── task-service/          # Task operations
    ├── comment-service/       # Comments and activity feeds
    └── notification-service/  # Real-time notifications
```

## Technology Stack

### Frontend
- **Next.js 14** - React framework with App Router
- **TypeScript** - Type safety
- **Tailwind CSS** - Utility-first CSS
- **Shadcn/ui** - Component library
- **Redux Toolkit** - State management

### Backend
- **.NET 8** - Microservices architecture
- **PostgreSQL** - Database per service
- **SignalR** - Real-time communication
- **Entity Framework Core** - ORM
- **JWT Authentication** - Via existing user-service

### Infrastructure
- **Ocelot API Gateway** - Request routing
- **Docker** - Containerization
- **pnpm** - Package manager (monorepo)

## Getting Started

1. **Wireframes**: Start with HTML wireframes in `wireframes/`
2. **Backend**: Initialize microservices following existing patterns
3. **Frontend**: Next.js application with modern tooling
4. **Integration**: Connect via API Gateway

## Related Services

This project integrates with existing QaliTrack services:
- **user-service** - Authentication and user management
- **qalitrack-gateway** - API routing and authentication
- **service-discovery** - Service registration