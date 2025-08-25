# Task Service

A microservice for managing tasks in the Qasana project management system.

## Overview

The Task Service handles:
- Task creation and management
- Task assignments and status tracking
- Subtask management
- Task dependencies and relationships

## Features

### Tasks
- Create, update, and delete tasks
- Task priority and status management
- Due date and deadline tracking
- Task descriptions and attachments

### Task Management
- Subtask creation and hierarchy
- Task dependencies and blocking relationships
- Task assignment to team members
- Progress tracking and completion

### Task Organization
- Task lists and sections
- Task labeling and categorization
- Search and filtering capabilities
- Task templates

## Entities

### Task
- **Id**: Unique identifier
- **Title**: Task title
- **Description**: Detailed description
- **ProjectId**: Parent project
- **AssigneeId**: Assigned user
- **Status**: To Do, In Progress, In Review, Done
- **Priority**: Low, Medium, High, Critical
- **DueDate**: Task deadline
- **CreatedBy**: Task creator
- **CompletedAt**: Completion timestamp

### Subtask
- **Id**: Unique identifier
- **ParentTaskId**: Parent task
- **Title**: Subtask title
- **IsCompleted**: Completion status
- **CreatedBy**: Creator
- **DueDate**: Subtask deadline

### TaskDependency
- **Id**: Unique identifier
- **TaskId**: Dependent task
- **DependsOnTaskId**: Blocking task
- **DependencyType**: Blocks, Duplicates, Related

## Technology Stack

- **.NET 8** - Framework
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database
- **AutoMapper** - Object mapping

## Integration

Integrates with:
- **Project Service** - Task-project relationships
- **User Service** - Task assignments
- **Comment Service** - Task comments
- **Notification Service** - Task updates

## API Endpoints

### Tasks
- `GET /api/tasks` - List user's tasks
- `POST /api/tasks` - Create new task
- `GET /api/tasks/{id}` - Get task details
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task

### Subtasks
- `GET /api/tasks/{id}/subtasks` - List task subtasks
- `POST /api/tasks/{id}/subtasks` - Create subtask
- `PUT /api/subtasks/{id}` - Update subtask
- `DELETE /api/subtasks/{id}` - Delete subtask

## Getting Started

```bash
dotnet build
dotnet run --project src/TaskService.Api
```