# 🚚 QTruck API Endpoints

## 📊 **Swagger Documentation**
- **Swagger UI**: http://localhost:8000/api/swagger/
- **ReDoc**: http://localhost:8000/api/redoc/
- **Schema**: http://localhost:8000/api/schema/

## 🔐 **Authentication Endpoints**
| Method | Endpoint | Description | Tags |
|--------|----------|-------------|------|
| POST | `/auth/register/` | User registration with email aliasing | Authentication |
| POST | `/auth/login/` | Login with email aliasing | Authentication |
| POST | `/auth/refresh/` | Refresh JWT token | Authentication |
| GET | `/auth/user-info/` | Get current user info | Authentication |

## 👥 **User Management (Admin Only)**
| Method | Endpoint | Description | Tags |
|--------|----------|-------------|------|
| POST | `/admin/approve-user/` | Approve/reject users | User Management |
| GET | `/admin/pending-users/` | List pending approvals | User Management |
| GET | `/admin/users/` | List all users | User Management |
| GET | `/admin/users/?user_type=tester` | List all testers | User Management |
| GET | `/admin/users/?user_type=driver` | List all drivers | User Management |
| GET | `/admin/users/?user_type=admin` | List all admins | User Management |

## ⚙️ **System Settings (Admin Only)**
| Method | Endpoint | Description | Tags |
|--------|----------|-------------|------|
| GET | `/api/system-settings/` | Get system settings | System Settings |
| PUT | `/api/system-settings/` | Update system settings | System Settings |

## 👤 **User Profiles**
| Method | Endpoint | Description | Access | Tags |
|--------|----------|-------------|--------|------|
| GET | `/api/drivers/` | List driver profiles | Admin: all, User: own | User Profiles |
| POST | `/api/drivers/` | Create driver profile | Drivers/Testers | User Profiles |
| GET | `/api/drivers/{id}/` | Get driver profile | Admin/Owner | User Profiles |
| PUT | `/api/drivers/{id}/` | Update driver profile | Admin/Owner | User Profiles |
| DELETE | `/api/drivers/{id}/` | Delete driver profile | Admin/Owner | User Profiles |

## 🚛 **Fleet Management (Admin: Full, Others: Read-Only)**
| Method | Endpoint | Description | Access | Tags |
|--------|----------|-------------|--------|------|
| GET | `/api/trucks/` | List trucks | All approved users | Fleet Management |
| POST | `/api/trucks/` | Create truck | Admin only | Fleet Management |
| GET | `/api/trucks/{id}/` | Get truck details | All approved users | Fleet Management |
| PUT | `/api/trucks/{id}/` | Update truck | Admin only | Fleet Management |
| DELETE | `/api/trucks/{id}/` | Delete truck | Admin only | Fleet Management |
| GET | `/api/materials/` | List materials | All approved users | Fleet Management |
| POST | `/api/materials/` | Create material | Admin only | Fleet Management |
| GET | `/api/materials/{id}/` | Get material details | All approved users | Fleet Management |
| PUT | `/api/materials/{id}/` | Update material | Admin only | Fleet Management |
| DELETE | `/api/materials/{id}/` | Delete material | Admin only | Fleet Management |

## 🛣️ **Trip Management**
| Method | Endpoint | Description | Access | Tags |
|--------|----------|-------------|--------|------|
| GET | `/api/trips/` | List trips | Admin: all, User: own | Trip Management |
| POST | `/api/trips/` | Create trip | Drivers/Testers | Trip Management |
| GET | `/api/trips/{id}/` | Get trip details | Admin/Owner | Trip Management |
| PUT | `/api/trips/{id}/` | Update trip | Admin/Owner | Trip Management |
| DELETE | `/api/trips/{id}/` | Delete trip | Admin/Owner | Trip Management |
| POST | `/api/trips/{id}/calculate_cost/` | Recalculate trip cost | Admin/Owner | Trip Management |
| GET | `/api/trips/by_status/?status=pending` | Filter trips by status | Admin/Owner | Trip Management |

## 💰 **Trip Expenses**
| Method | Endpoint | Description | Access | Tags |
|--------|----------|-------------|--------|------|
| GET | `/api/expenses/` | List expenses | Admin: all, User: own | Trip Expenses |
| POST | `/api/expenses/` | Create expense | Drivers/Testers | Trip Expenses |
| GET | `/api/expenses/{id}/` | Get expense details | Admin/Owner | Trip Expenses |
| PUT | `/api/expenses/{id}/` | Update expense | Admin/Owner | Trip Expenses |
| DELETE | `/api/expenses/{id}/` | Delete expense | Admin/Owner | Trip Expenses |
| GET | `/api/material-costs/` | List material costs | Admin: all, User: own | Trip Expenses |
| POST | `/api/material-costs/` | Create material cost | Drivers/Testers | Trip Expenses |
| GET | `/api/receipts/` | List receipts | Admin: all, User: own | Trip Expenses |
| POST | `/api/receipts/` | Create receipt | Drivers/Testers | Trip Expenses |
| POST | `/api/receipts/{id}/extract_details/` | Extract receipt details (OCR) | Admin/Owner | Trip Expenses |

## 📍 **Vehicle Tracking**
| Method | Endpoint | Description | Access | Tags |
|--------|----------|-------------|--------|------|
| GET | `/api/vehicle-mileage/` | List mileage records | Admin: all, User: own | Vehicle Tracking |
| POST | `/api/vehicle-mileage/` | Create mileage record | Drivers/Testers | Vehicle Tracking |
| GET | `/api/vehicle-mileage/{id}/` | Get mileage details | Admin/Owner | Vehicle Tracking |
| PUT | `/api/vehicle-mileage/{id}/` | Update mileage | Admin/Owner | Vehicle Tracking |
| DELETE | `/api/vehicle-mileage/{id}/` | Delete mileage record | Admin/Owner | Vehicle Tracking |

## 💬 **Feedback System**
| Method | Endpoint | Description | Access | Tags |
|--------|----------|-------------|--------|------|
| GET | `/api/feedback/` | List feedback | Admin: all, User: own | Feedback System |
| POST | `/api/feedback/` | Submit feedback | Drivers/Testers | Feedback System |
| GET | `/api/feedback/{id}/` | Get feedback details | Admin/Owner | Feedback System |
| PUT | `/api/feedback/{id}/` | Update feedback | Admin/Owner | Feedback System |
| POST | `/api/feedback/{id}/respond/` | Respond to feedback | Admin only | Feedback System |
| GET | `/api/feedback/pending/` | List pending feedback | Admin only | Feedback System |

## 🔒 **Access Control Rules**

### **Admin**
- ✅ Full CRUD on all resources
- ✅ User approval/rejection
- ✅ System settings management
- ✅ View all data across users
- ✅ Respond to feedback

### **Drivers**
- ✅ Create/edit own profile
- ✅ Read-only access to trucks/materials
- ✅ Full CRUD on own trips/expenses/receipts
- ✅ Submit feedback
- ❌ Cannot modify fleet data
- ❌ Cannot access other users' data

### **Testers**
- ✅ Same permissions as drivers
- ✅ Can be enabled/disabled by admin
- ✅ Create driver profiles (act as drivers)
- ✅ Submit feedback

## 📧 **Email Aliasing System**

### **Registration**
- `user@domain.com` → **Admin** (no alias)
- `user+admin@domain.com` → **Admin** 
- `user+driver@domain.com` → **Driver**
- `user+tester@domain.com` → **Tester**

### **Login**
- Use same email format as registration
- First user is auto-approved as admin
- All others need admin approval

## 🔄 **Automatic Features**

### **Trip Cost Calculation**
- Automatically calculated when expenses are added/updated
- Uses Django signals for real-time updates
- Includes material costs and regular expenses

### **User Approval Workflow**
- First user: Auto-admin + auto-approved
- Subsequent users: Need admin approval
- Pending users listed in admin panel

### **Tester Controls**
- Admin can enable/disable tester registration
- Admin can enable/disable tester login
- Independent controls for registration vs login

## 🧪 **Testing with Playwright**

```bash
# Setup test environment
pnpm run test:setup

# Run all tests
pnpm run test

# Run with UI
pnpm run test:ui

# View test report
pnpm run test:report
```

**Test Coverage**: 40+ test cases covering all user flows and edge cases.