curl -X POST https://dev.qalibrated.co.ke/api/users \
    -H "Content-Type: application/json" \
    -d '{
      "name": "name",
      "email": "email@gmail.com",
      "password": "password*",
      "role": "Admin",
      "status": "Active"
    }'



    Available roles:
  - "Admin"
  - "Client"
  - "Guest"

  Default role: "Client" (if no role is specified)

  Available statuses:
  - "Active"
  - "Pending"
  - "Inactive"