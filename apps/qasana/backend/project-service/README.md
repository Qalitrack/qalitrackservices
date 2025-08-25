# Project Service

A microservice for managing projects in the Qasana project management system.

## Overview

The Project Service handles:
- Project creation and management
- Project templates and categorization
- Project member assignments
- Project status tracking and reporting

## Features

### Projects
- Create, update, and delete projects
- Project templates for quick setup
- Custom project fields and metadata
- Project archiving and restoration

### Project Management
- Kanban board configurations
- Project milestones and phases
- Progress tracking and metrics
- Due date management

### Team Management
- Assign team members to projects
- Role-based permissions within projects
- Project visibility settings
- Activity tracking

## Entities

### Project
- **Id**: Unique identifier
- **Name**: Project name
- **Description**: Project description
- **WorkspaceId**: Parent workspace
- **Status**: Planning, Active, On Hold, Completed, Cancelled
- **Priority**: Low, Medium, High, Critical
- **StartDate/EndDate**: Project timeline
- **Progress**: Completion percentage
- **Color**: Project brand color
- **IsPublic**: Visibility setting

### ProjectMember
- **ProjectId**: Associated project
- **UserId**: Team member
- **Role**: Viewer, Member, Admin
- **AssignedAt**: Assignment timestamp

### ProjectTemplate
- **Id**: Template identifier
- **Name**: Template name
- **Category**: Template category
- **Structure**: JSON template structure

## Technology Stack

- **.NET 8** - Framework
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database
- **AutoMapper** - Object mapping

## Integration

Integrates with:
- **Workspace Service** - Project-workspace relationships
- **Task Service** - Project tasks
- **User Service** - Member management
- **Notification Service** - Project updates

## API Endpoints

### Projects
- `GET /api/projects` - List user's projects
- `POST /api/projects` - Create new project
- `GET /api/projects/{id}` - Get project details
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Delete project

### Members
- `GET /api/projects/{id}/members` - List project members
- `POST /api/projects/{id}/members` - Add member to project
- `PUT /api/projects/{id}/members/{userId}` - Update member role
- `DELETE /api/projects/{id}/members/{userId}` - Remove member

## Getting Started

```bash
dotnet build
dotnet run --project src/ProjectService.Api
```