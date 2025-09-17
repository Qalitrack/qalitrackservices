# QTruck API

A Django REST API for fleet management system with role-based authentication and automatic trip cost calculation.

## Features

- **User Management**: Admin, Driver, and Tester roles with email aliasing
- **Email Aliasing Authentication**: Login with email+role@domain.com format
- **Trip Management**: Complete trip tracking with automatic cost calculation
- **Receipt OCR**: JSON field for storing extracted receipt details
- **Role-Based Permissions**: Different access levels for different user types
- **Auto-Approval**: First user becomes admin, others need approval
- **Tester Controls**: Admin can enable/disable tester registration and login
- **Swagger Documentation**: Complete API documentation

## Setup

1. **Clone and Navigate**
   ```bash
   cd qtruck_api
   ```

2. **Create Virtual Environment**
   ```bash
   python3 -m venv venv
   source venv/bin/activate  # On Windows: venv\Scripts\activate
   ```

3. **Install Dependencies**
   ```bash
   pip install -r requirements.txt
   ```

4. **Environment Configuration**
   Copy `.env.example` to `.env` and update the values:
   ```bash
   cp .env.example .env
   ```

5. **Database Setup**
   ```bash
   python manage.py makemigrations
   python manage.py migrate
   ```

6. **Create Superuser** (Optional - first registered user becomes admin)
   ```bash
   python manage.py createsuperuser
   ```

7. **Run Development Server**
   ```bash
   python manage.py runserver
   ```

## API Documentation

- **Swagger UI**: http://localhost:8000/api/swagger/
- **ReDoc**: http://localhost:8000/api/redoc/
- **API Schema**: http://localhost:8000/api/schema/

## Authentication

### Registration
POST `/auth/register/`
- Use email aliasing: `user+admin@example.com`, `user+tester@example.com`, `user@example.com`
- First user is auto-approved as admin
- Other users need admin approval

### Login
POST `/auth/login/`
- Email aliasing determines user type
- Must be approved to login
- Returns JWT tokens

### User Types
- **Admin**: Full access, can approve users, manage testers
- **Driver**: Can manage own trips, expenses, receipts
- **Tester**: Limited access (when enabled by admin)

## Database Models

All models use UUID primary keys and have created_at/updated_at timestamps:

- **CustomUser**: Extended user with role management
- **Driver**: Driver profiles linked to users
- **Truck**: Vehicle information
- **Trip**: Trip records with automatic cost calculation
- **Material**: Materials transported
- **MaterialCost**: Costs for materials
- **Expense**: Trip expenses
- **Receipt**: Receipt images with OCR data
- **VehicleMileage**: Vehicle mileage tracking

## API Endpoints

### Authentication
- `POST /auth/register/` - User registration
- `POST /auth/login/` - Login with email aliasing
- `POST /auth/refresh/` - Refresh JWT token
- `GET /auth/user-info/` - Current user info

### Admin
- `POST /admin/approve-user/` - Approve/reject users
- `GET /admin/pending-users/` - List pending approvals

### Core Resources
- `/api/drivers/` - Driver management
- `/api/trucks/` - Truck management  
- `/api/trips/` - Trip management
- `/api/materials/` - Material tracking
- `/api/material-costs/` - Material cost tracking
- `/api/expenses/` - Expense management
- `/api/receipts/` - Receipt management
- `/api/vehicle-mileage/` - Mileage tracking
- `/api/system-settings/` - System configuration

### Special Endpoints
- `POST /api/trips/{id}/calculate_cost/` - Recalculate trip cost
- `GET /api/trips/by_status/?status=pending` - Filter trips by status
- `POST /api/receipts/{id}/extract_details/` - Extract receipt details (OCR placeholder)

## Environment Variables

```env
DEBUG=True
SECRET_KEY=your-secret-key
ALLOWED_HOSTS=localhost,127.0.0.1
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://127.0.0.1:3000
DATABASE_URL=sqlite:///db.sqlite3
MEDIA_ROOT=media/
STATIC_ROOT=static/
```

## Development

### Auto Cost Calculation
Trip costs are automatically calculated when expenses or material costs are added/updated using Django signals.

### Tester Management
Admins can enable/disable tester registration and login through the SystemSettings model.

### Role-Based Access
All endpoints implement role-based permissions ensuring users only access their own data (except admins).

## Production Deployment

1. Set `DEBUG=False` in .env
2. Configure proper database (PostgreSQL recommended)
3. Set up proper static/media file serving
4. Configure CORS and ALLOWED_HOSTS for your domain
5. Use proper secret key and secure settings