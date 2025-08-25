# Workspace Service

A microservice for managing workspaces in the Qasana project management system.

## Overview

The Workspace Service handles:
- Workspace creation and management
- Team member management
- Workspace invitations
- Role-based permissions within workspaces

## Features

### Workspaces
- Create, update, and delete workspaces
- Customize workspace appearance (icons, colors)
- Set member limits
- Archive/deactivate workspaces

### Members
- Add/remove team members
- Manage member roles (Owner, Admin, Member)
- Track member activity and status
- Member permissions management

### Invitations
- Send email invitations to join workspaces
- Token-based invitation acceptance
- Invitation expiration and management
- Resend and cancel invitations

## Entities

### Workspace
- **Id**: Unique identifier
- **Name**: Workspace name
- **Description**: Workspace description
- **Status**: Active, Inactive, Suspended, Archived
- **IconUrl**: Custom workspace icon
- **Color**: Brand color for the workspace
- **OwnerId/OwnerName**: Workspace owner details
- **MaxMembers**: Maximum allowed members

### WorkspaceMember
- **WorkspaceId**: Associated workspace
- **UserId/UserName/UserEmail**: Member details
- **Role**: Member, Admin, Owner
- **Status**: Active, Inactive, Suspended
- **JoinedAt**: Join timestamp

### WorkspaceInvitation
- **WorkspaceId**: Target workspace
- **Email**: Invitee email address
- **Role**: Intended role for invitee
- **Status**: Pending, Accepted, Declined, Expired, Cancelled
- **InvitedBy**: Who sent the invitation
- **ExpiresAt**: Invitation expiration
- **AcceptToken**: Secure token for acceptance

## API Endpoints

### Workspaces
- `GET /api/workspaces` - List user's workspaces
- `POST /api/workspaces` - Create new workspace
- `GET /api/workspaces/{id}` - Get workspace details
- `PUT /api/workspaces/{id}` - Update workspace
- `DELETE /api/workspaces/{id}` - Delete workspace

### Members
- `GET /api/workspaces/{id}/members` - List workspace members
- `POST /api/workspaces/{id}/members/invite` - Invite new member
- `PUT /api/workspaces/{id}/members/{userId}` - Update member role
- `DELETE /api/workspaces/{id}/members/{userId}` - Remove member

### Invitations
- `GET /api/workspaces/{id}/invitations` - List pending invitations
- `POST /api/invitations/{token}/accept` - Accept invitation
- `DELETE /api/invitations/{id}` - Cancel invitation
- `POST /api/invitations/{id}/resend` - Resend invitation

## Technology Stack

- **.NET 8** - Framework
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **Serilog** - Logging

## Database Schema

The service uses PostgreSQL with the following main tables:
- `workspaces` - Main workspace data
- `workspace_members` - Member associations
- `workspace_invitations` - Pending invitations

## Integration

This service integrates with:
- **User Service** - User authentication and profile data
- **Project Service** - Workspace-project relationships
- **Notification Service** - Invitation and activity notifications

## Getting Started

1. **Build the service**:
   ```bash
   dotnet build
   ```

2. **Run migrations**:
   ```bash
   dotnet ef database update
   ```

3. **Start the service**:
   ```bash
   dotnet run --project src/WorkspaceService.Api
   ```

The service will be available at `https://localhost:7001` (HTTPS) or `http://localhost:5001` (HTTP).

## Configuration

Key configuration settings in `appsettings.json`:
- `ConnectionStrings:DefaultConnection` - Database connection
- `JwtSettings` - JWT authentication configuration
- `EmailSettings` - Email service configuration for invitations

## Testing

Run tests with:
```bash
dotnet test
```

## Docker Support

The service includes Docker support:
```bash
docker build -t qasana-workspace-service .
docker run -p 5001:80 qasana-workspace-service
```