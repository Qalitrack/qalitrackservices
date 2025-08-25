# Notification Service

A microservice for managing notifications and real-time updates in the Qasana project management system.

## Overview

The Notification Service handles:
- Real-time notifications via WebSocket/SignalR
- Email notifications and digests
- Push notifications for mobile apps
- Notification preferences and settings

## Features

### Real-time Notifications
- Instant notifications via SignalR
- WebSocket connections for live updates
- Connection management and scaling
- Notification queuing and delivery

### Email Notifications
- Automated email notifications
- Daily/weekly digest emails
- Email templates and customization
- Email delivery tracking

### Push Notifications
- Mobile push notifications
- Web push notifications
- Device token management
- Notification targeting

### Notification Management
- Notification history and archive
- Read/unread status tracking
- Notification preferences
- Bulk notification operations

## Entities

### Notification
- **Id**: Unique identifier
- **UserId**: Target user
- **Title**: Notification title
- **Message**: Notification content
- **Type**: Task, Project, Comment, Mention, etc.
- **EntityId**: Related entity ID
- **EntityType**: Entity type
- **IsRead**: Read status
- **CreatedAt**: Timestamp
- **ReadAt**: Read timestamp

### NotificationPreference
- **Id**: Unique identifier
- **UserId**: User ID
- **NotificationType**: Type of notification
- **Channel**: Email, Push, InApp
- **IsEnabled**: Preference enabled
- **Settings**: Additional settings JSON

### EmailDigest
- **Id**: Unique identifier
- **UserId**: Target user
- **DigestType**: Daily, Weekly
- **Content**: Digest content
- **SentAt**: Send timestamp
- **Status**: Pending, Sent, Failed

### DeviceToken
- **Id**: Unique identifier
- **UserId**: User ID
- **Token**: Device/browser token
- **Platform**: iOS, Android, Web
- **IsActive**: Token status
- **LastUsed**: Last usage timestamp

## Technology Stack

- **.NET 8** - Framework
- **SignalR** - Real-time communication
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database
- **Email Service** - SMTP/SendGrid
- **Push Service** - Firebase/APNS/Web Push

## Integration

Integrates with:
- **All Services** - Receives notification events
- **User Service** - User preferences
- **Email Service** - Email delivery
- **Push Service** - Mobile notifications

## API Endpoints

### Notifications
- `GET /api/notifications` - List user notifications
- `POST /api/notifications/{id}/read` - Mark as read
- `POST /api/notifications/read-all` - Mark all as read
- `DELETE /api/notifications/{id}` - Delete notification

### Preferences
- `GET /api/preferences` - Get user preferences
- `PUT /api/preferences` - Update preferences
- `POST /api/preferences/reset` - Reset to defaults

### Real-time
- `/notificationHub` - SignalR hub endpoint
- Connection-based real-time updates
- Group-based notifications

## Getting Started

```bash
dotnet build
dotnet run --project src/NotificationService.Api
```

## Real-time Features

The service provides real-time updates through SignalR:
- Task assignments and updates
- Project changes
- New comments and mentions
- Team activity notifications