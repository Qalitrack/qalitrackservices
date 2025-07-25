### `security.md`
Explaining JWT and security measures:

```markdown
# Security

The API uses JWT-based authentication to secure access to protected resources. All endpoints, except for **/login** and **/update-password**, require the user to provide a valid JWT token in the `Authorization` header.

Example:
```plaintext
Authorization: Bearer <JWT_Token>

