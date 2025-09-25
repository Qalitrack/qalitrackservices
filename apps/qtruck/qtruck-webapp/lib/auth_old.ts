const API_URL = process.env.NEXT_PUBLIC_API_URL

interface LoginData {
  email: string
  password: string
}

interface RegisterData {
  email: string
  password: string
  password_confirm: string
  first_name?: string
  last_name?: string
  phone?: string
  companyName?: string
  licenseNumber?: string
  licenseExpiry?: string
}

export type UserRole = 'driver' | 'admin'

class AuthService {
  private addRoleAlias(email: string, role: UserRole): string {
    if (role === 'driver' && !email.includes('+driver@')) {
      const [localPart, domain] = email.split('@')
      if (localPart && domain) {
        return `${localPart}+driver@${domain}`
      }
    }
    return email
  }

  async login(data: LoginData, role: UserRole) {
    const processedData = {
      ...data,
      email: this.addRoleAlias(data.email, role)
    }

    try {
      const response = await fetch(`${API_URL}/auth/login/`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(processedData),
      })

      if (response.ok) {
        const result = await response.json()
        // Store token if provided
        if (result.access) {
          localStorage.setItem('auth_token', result.access)
        }
        if (result.refresh) {
          localStorage.setItem('refresh_token', result.refresh)
        }
        return result
      } else {
        const errorData = await response.json().catch(() => ({}))
        throw new Error(errorData.detail || errorData.message || 'Login failed')
      }
    } catch (error) {
      throw new Error(error instanceof Error ? error.message : 'Login failed')
    }
  }

  async register(data: RegisterData, role: UserRole) {
    const processedData = {
      ...data,
      email: this.addRoleAlias(data.email, role)
    }

    try {
      const response = await fetch(`${API_URL}/auth/register/`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(processedData),
      })

      if (response.ok) {
        return await response.json()
      } else {
        const errorData = await response.json().catch(() => ({}))
        throw new Error(errorData.detail || errorData.message || 'Registration failed')
      }
    } catch (error) {
      throw new Error(error instanceof Error ? error.message : 'Registration failed')
    }
  }

  logout() {
    localStorage.removeItem('auth_token')
  }

  getToken() {
    return localStorage.getItem('auth_token')
  }

  isAuthenticated() {
    return !!this.getToken()
  }
}

export const authService = new AuthService()