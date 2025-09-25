const API_URL = process.env.NEXT_PUBLIC_API_URL

class ApiService {
  // Helper function to get user type from current context
  private getCurrentUserType(): string | null {
    if (typeof window === 'undefined') return null
    
    const path = window.location.pathname
    if (path.includes('/dashboard')) return 'admin'
    if (path.includes('/portal')) {
      // Check for driver or tester tokens
      for (const type of ['driver', 'tester']) {
        const token = localStorage.getItem(`access_token_${type}`)
        if (token) return type
      }
    }
    
    // Fallback: check all token types
    for (const type of ['admin', 'driver', 'tester']) {
      const token = localStorage.getItem(`access_token_${type}`)
      if (token) return type
    }
    
    return null
  }

  private getAuthHeaders(skipContentType = false, userType?: string) {
    const currentUserType = userType || this.getCurrentUserType()
    const token = currentUserType ? localStorage.getItem(`access_token_${currentUserType}`) : null
    
    return {
      ...(!skipContentType && { 'Content-Type': 'application/json' }),
      ...(token && { Authorization: `Bearer ${token}` })
    }
  }

  async get(endpoint: string) {
    try {
      const response = await fetch(`${API_URL}${endpoint}`, {
        method: 'GET',
        headers: this.getAuthHeaders(),
      })

      if (!response.ok) {
        const errorData = await response.json().catch(() => null)
        const error = new Error(`API Error: ${response.status}`)
        ;(error as any).response = { data: errorData, status: response.status }
        throw error
      }

      return await response.json()
    } catch (error) {
      if (error instanceof Error && (error as any).response) {
        throw error
      }
      throw new Error(error instanceof Error ? error.message : 'API request failed')
    }
  }

  async post(endpoint: string, data: any) {
    try {
      const response = await fetch(`${API_URL}${endpoint}`, {
        method: 'POST',
        headers: this.getAuthHeaders(),
        body: JSON.stringify(data),
      })

      if (!response.ok) {
        const errorData = await response.json().catch(() => null)
        const error = new Error(`API Error: ${response.status}`)
        ;(error as any).response = { data: errorData, status: response.status }
        throw error
      }

      return await response.json()
    } catch (error) {
      if (error instanceof Error && (error as any).response) {
        throw error
      }
      throw new Error(error instanceof Error ? error.message : 'API request failed')
    }
  }

  async patch(endpoint: string, data: any) {
    try {
      const response = await fetch(`${API_URL}${endpoint}`, {
        method: 'PATCH',
        headers: this.getAuthHeaders(),
        body: JSON.stringify(data),
      })

      if (!response.ok) {
        const errorData = await response.json().catch(() => null)
        const error = new Error(`API Error: ${response.status}`)
        ;(error as any).response = { data: errorData, status: response.status }
        throw error
      }

      return await response.json()
    } catch (error) {
      if (error instanceof Error && (error as any).response) {
        throw error
      }
      throw new Error(error instanceof Error ? error.message : 'API request failed')
    }
  }

  // Authentication methods
  async login(email: string, password: string) {
    const response = await fetch(`${API_URL}/auth/login/`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email, password }),
    })

    if (!response.ok) {
      const errorData = await response.json().catch(() => null)
      const error = new Error(`Login failed: ${response.status}`)
      ;(error as any).response = { data: errorData, status: response.status }
      throw error
    }

    return await response.json()
  }

  async register(userData: any) {
    const response = await fetch(`${API_URL}/auth/register/`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(userData),
    })

    if (!response.ok) {
      const errorData = await response.json().catch(() => null)
      const error = new Error(`Registration failed: ${response.status}`)
      ;(error as any).response = { data: errorData, status: response.status }
      throw error
    }

    return await response.json()
  }

  async getUserInfo(userType?: string) {
    try {
      const response = await fetch(`${API_URL}/auth/user-info/`, {
        method: 'GET',
        headers: this.getAuthHeaders(false, userType),
      })

      if (!response.ok) {
        const errorData = await response.json().catch(() => null)
        const error = new Error(`API Error: ${response.status}`)
        ;(error as any).response = { data: errorData, status: response.status }
        throw error
      }

      return await response.json()
    } catch (error) {
      if (error instanceof Error && (error as any).response) {
        throw error
      }
      throw new Error(error instanceof Error ? error.message : 'API request failed')
    }
  }

  // Admin-specific methods
  async getDrivers() {
    return this.get('/api/drivers/')
  }

  async getTrips() {
    return this.get('/api/trips/')
  }

  async getTripsByStatus(status?: string) {
    const params = status ? `?status=${status}` : ''
    return this.get(`/api/trips/by_status/${params}`)
  }

  async getTrucks() {
    return this.get('/api/trucks/')
  }

  async getVehicleMileage() {
    return this.get('/api/vehicle-mileage/')
  }

  async getPendingUsers() {
    return this.get('/api/admin/pending-users/')
  }

  async getAllUsers() {
    return this.get('/api/admin/users/')
  }

  async approveUser(userId: string) {
    return this.post('/api/admin/approve-user/', { 
      user_id: userId, 
      action: 'approve' 
    })
  }

  async rejectUser(userId: string) {
    return this.post('/api/admin/approve-user/', { 
      user_id: userId, 
      action: 'reject' 
    })
  }

  async deactivateUser(userId: string) {
    return this.post('/api/admin/deactivate-user/', { 
      user_id: userId 
    })
  }

  async reactivateUser(userId: string) {
    return this.post('/api/admin/reactivate-user/', { 
      user_id: userId 
    })
  }

  async deleteUser(userId: string) {
    return this.post('/api/admin/delete-user/', { 
      user_id: userId 
    })
  }

  async createAdmin(adminData: any) {
    return this.post('/api/admin/create-admin/', adminData)
  }

  async getSystemSettings() {
    return this.get('/api/system-settings/')
  }

  async updateSystemSettings(settings: any) {
    // SystemSettings uses singleton pattern, so we update the single instance
    return this.patch('/api/system-settings/1/', settings)
  }

  async createTruck(truckData: any) {
    return this.post('/api/trucks/', truckData)
  }

  async updateTruck(truckId: string, truckData: any) {
    return this.post(`/api/trucks/${truckId}/`, truckData)
  }

  async deleteTruck(truckId: string) {
    return this.get(`/api/trucks/${truckId}/delete/`)
  }

  async getMaterials() {
    return this.get('/api/materials/')
  }

  async createMaterial(materialData: any) {
    // Handle FormData for file uploads
    if (materialData instanceof FormData) {
      try {
        const response = await fetch(`${API_URL}/api/materials/`, {
          method: 'POST',
          headers: this.getAuthHeaders(true), // Skip Content-Type for FormData
          body: materialData,
        })

        if (!response.ok) {
          const errorData = await response.json().catch(() => null)
          const error = new Error(`API Error: ${response.status}`)
          ;(error as any).response = { data: errorData, status: response.status }
          throw error
        }

        return await response.json()
      } catch (error) {
        if (error instanceof Error && (error as any).response) {
          throw error
        }
        throw new Error(error instanceof Error ? error.message : 'API request failed')
      }
    } else {
      return this.post('/api/materials/', materialData)
    }
  }

  async updateMaterial(materialId: string, materialData: any) {
    return this.post(`/api/materials/${materialId}/`, materialData)
  }

  async deleteMaterial(materialId: string) {
    return this.get(`/api/materials/${materialId}/delete/`)
  }

  // Driver-specific methods
  async getMyTrips() {
    return this.get('/api/trips/')
  }

  async createTrip(tripData: any) {
    return this.post('/api/trips/', tripData)
  }

  async updateTrip(tripId: string, tripData: any) {
    return this.post(`/api/trips/${tripId}/`, tripData)
  }

  async getMyExpenses() {
    return this.get('/api/expenses/')
  }

  async createExpense(expenseData: any) {
    return this.post('/api/expenses/', expenseData)
  }

  async uploadReceipt(receiptData: any) {
    return this.post('/api/receipts/', receiptData)
  }

  async getMyProfile() {
    return this.get('/api/drivers/me/')
  }

  async updateMyProfile(profileData: any) {
    return this.post('/api/drivers/me/', profileData)
  }

  // New Enhanced Driver Profile Methods
  async getMyDriverProfile() {
    return this.get('/api/driver-profiles/me/')
  }
  
  async getPendingDriverProfile() {
    return this.get('/api/driver-profiles/pending/')
  }

  async createDriverProfile(profileData: FormData) {
    try {
      const response = await fetch(`${API_URL}/api/driver-profiles/`, {
        method: 'POST',
        headers: this.getAuthHeaders(true), // Skip Content-Type for FormData
        body: profileData,
      })

      if (!response.ok) {
        const errorData = await response.json().catch(() => null)
        const error = new Error(`API Error: ${response.status}`)
        ;(error as any).response = { data: errorData, status: response.status }
        throw error
      }

      return await response.json()
    } catch (error) {
      if (error instanceof Error && (error as any).response) {
        throw error
      }
      throw new Error(error instanceof Error ? error.message : 'API request failed')
    }
  }

  async updateDriverProfile(profileData: FormData) {
    try {
      const response = await fetch(`${API_URL}/api/driver-profiles/me/`, {
        method: 'PATCH',
        headers: this.getAuthHeaders(true), // Skip Content-Type for FormData
        body: profileData,
      })

      if (!response.ok) {
        const errorData = await response.json().catch(() => null)
        const error = new Error(`API Error: ${response.status}`)
        ;(error as any).response = { data: errorData, status: response.status }
        throw error
      }

      return await response.json()
    } catch (error) {
      if (error instanceof Error && (error as any).response) {
        throw error
      }
      throw new Error(error instanceof Error ? error.message : 'API request failed')
    }
  }

  async getDriverProfileChanges() {
    return this.get('/api/driver-profiles/my_changes/')
  }

  async getDriverActivity() {
    return this.get('/api/driver-activities/my_activity/')
  }

  async getDriverActivityHeatmap(year?: number) {
    const yearParam = year ? `?year=${year}` : ''
    return this.get(`/api/driver-activities/my-heatmap/${yearParam}`)
  }

  // Enhanced Driver Profile API Methods
  async getDriverProfiles() {
    return this.get('/api/driver-profiles/')
  }

  async getDriverProfile(profileId: string) {
    return this.get(`/api/driver-profiles/${profileId}/`)
  }

  async updateDriverProfileById(profileId: string, profileData: any) {
    return this.post(`/api/driver-profiles/${profileId}/`, profileData)
  }


  async getDriverProfileHistory(profileId: string) {
    return this.get(`/api/driver-profiles/${profileId}/history/`)
  }

  // Enhanced Driver Management
  async getEnhancedDrivers() {
    return this.get('/api/drivers-enhanced/')
  }

  async getEnhancedDriver(driverId: string) {
    return this.get(`/api/drivers-enhanced/${driverId}/`)
  }

  async getEnhancedDriverActivity(driverId: string, days = 30) {
    return this.get(`/api/drivers-enhanced/${driverId}/activity/?days=${days}`)
  }

  async getDriverHeatmap(driverId: string, year?: number) {
    const params = year ? `?year=${year}` : ''
    return this.get(`/api/drivers-enhanced/${driverId}/heatmap/${params}`)
  }

  async getExpiringLicenses() {
    return this.get('/api/drivers-enhanced/expiring_licenses/')
  }

  // Driver Activity
  async getDriverActivities() {
    return this.get('/api/driver-activities/')
  }

  // Driver Profile Management
  async getMyDriverProfile() {
    return this.get('/api/driver-profiles/me/')
  }

  async submitDriverProfile(profileId: string) {
    return this.post(`/api/driver-profiles/${profileId}/submit/`, {})
  }

  async getDriverProfileChanges() {
    return this.get('/api/driver-profiles/my_changes/')
  }

  async approveDriverProfile(profileId: string, action: string, notes: string = '') {
    return this.post(`/api/driver-profiles/${profileId}/approve/`, { action, notes })
  }

  async getPendingDriverProfiles() {
    return this.get('/api/driver-profiles/pending_profiles/')
  }

  async getLicenseClasses() {
    return this.get('/api/license-classes/')
  }

  async createLicenseClass(licenseClassData: any) {
    return this.post('/api/license-classes/', licenseClassData)
  }

  async updateLicenseClass(licenseClassId: string, licenseClassData: any) {
    return this.post(`/api/license-classes/${licenseClassId}/`, licenseClassData)
  }

  async deleteLicenseClass(licenseClassId: string) {
    try {
      const response = await fetch(`${API_URL}/api/license-classes/${licenseClassId}/`, {
        method: 'DELETE',
        headers: this.getAuthHeaders(),
      })

      if (!response.ok) {
        const errorData = await response.json().catch(() => null)
        const error = new Error(`API Error: ${response.status}`)
        ;(error as any).response = { data: errorData, status: response.status }
        throw error
      }

      return { message: 'License class deleted successfully' }
    } catch (error) {
      if (error instanceof Error && (error as any).response) {
        throw error
      }
      throw new Error(error instanceof Error ? error.message : 'API request failed')
    }
  }
}

export const apiService = new ApiService()