# Comment Service

A microservice for managing comments and activity feeds in the Qasana project management system.

## Overview

The Comment Service handles:
- Comments on tasks and projects
- Activity feed generation
- File attachments to comments
- Mention system and notifications

## Features

### Comments
- Create, update, and delete comments
- Rich text formatting support
- Reply threading and conversation management
- Comment editing history

### Attachments
- File upload and attachment to comments
- Image preview and document linking
- File size and type validation
- Secure file storage

### Activity Feeds
- Real-time activity tracking
- Filtered activity streams
- Activity aggregation and summarization
- Activity export and reporting

### Mentions
- @mention functionality for users
- Mention notifications and alerts
- Team and group mentions
- Mention search and discovery

## Entities

### Comment
- **Id**: Unique identifier
- **Content**: Comment text content
- **AuthorId**: Comment author
- **TaskId/ProjectId**: Parent entity
- **ParentCommentId**: Reply threading
- **CreatedAt/UpdatedAt**: Timestamps
- **IsEdited**: Edit status
- **IsDeleted**: Soft delete flag

### CommentAttachment
- **Id**: Unique identifier
- **CommentId**: Parent comment
- **FileName**: Original file name
- **FileUrl**: Storage URL
- **FileSize**: File size in bytes
- **ContentType**: MIME type
- **UploadedBy**: Uploader user ID

### ActivityEvent
- **Id**: Unique identifier
- **EventType**: Type of activity
- **EntityId**: Related entity ID
- **EntityType**: Entity type (Task, Project, etc.)
- **UserId**: Actor user ID
- **Description**: Activity description
- **Timestamp**: Event timestamp
- **Metadata**: Additional event data

## Technology Stack

- **.NET 8** - Framework
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database
- **SignalR** - Real-time updates
- **File Storage** - Cloud storage integration

## Integration

Integrates with:
- **Task Service** - Task comments and activity
- **Project Service** - Project comments
- **User Service** - Comment authors and mentions
- **Notification Service** - Comment notifications

## API Endpoints

### Comments
- `GET /api/comments` - List comments for entity
- `POST /api/comments` - Create new comment
- `GET /api/comments/{id}` - Get comment details
- `PUT /api/comments/{id}` - Update comment
- `DELETE /api/comments/{id}` - Delete comment

### Attachments
- `POST /api/comments/{id}/attachments` - Upload attachment
- `GET /api/attachments/{id}` - Download attachment
- `DELETE /api/attachments/{id}` - Delete attachment

### Activity
- `GET /api/activity` - Get activity feed
- `GET /api/activity/project/{id}` - Project activity
- `GET /api/activity/task/{id}` - Task activity

## Getting Started

```bash
dotnet build
dotnet run --project src/CommentService.Api
```