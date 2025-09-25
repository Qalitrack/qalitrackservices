// Modern API Client for Django REST API
// Replaces the old api.ts with proper TypeScript types and error handling

import {
  ApiResponse,
  User,
  LoginRequest,
  LoginResponse,
  RefreshTokenRequest,
  RefreshTokenResponse,
  RegisterRequest,
  RegisterResponse,
  CreateUserRequest,
  UpdateUserRequest,
  UserListParams,
  SystemSettings,
  UpdateSystemSettingsRequest,
  LicenseClass,
  CreateLicenseClassRequest,
  UpdateLicenseClassRequest,
  ErrorResponse,
  ValidationError,
  UserType,
  UserStatus,
  TokenStorage,
  // Fleet & Materials types
  Truck,
  CreateTruckRequest,
  UpdateTruckRequest,
  Material,
  CreateMaterialRequest,
  UpdateMaterialRequest,
  MaterialVariant,
  CreateMaterialVariantRequest,
  UpdateMaterialVariantRequest,
  MaterialPhoto,
  MaterialVariantPhoto,
  MaterialCost,
  CreateMaterialCostRequest,
  MaterialListParams,
  MaterialVariantListParams,
  MaterialPhotoListParams,
  MaterialVariantPhotoListParams,
  // Driver Profile types
  DriverProfile,
  DriverProfileStatus,
  CreateDriverProfileRequest,
  UpdateDriverProfileRequest,
  DriverProfileApprovalRequest,
  DriverProfileListParams,
  Driver,
  DriverActivity,
  DriverActivityStats,
  DriverProfileChange,
  // Trips types
  Trip,
  TripListParams,
  CreateTripRequest,
  UpdateTripRequest,
  Expense,
  ExpenseListParams,
  CreateExpenseRequest,
  UpdateExpenseRequest,
  Receipt,
  ReceiptListParams,
  CreateReceiptRequest,
  UpdateReceiptRequest,
  VehicleMileage,
  VehicleMileageListParams,
  // Feedback types
  Feedback,
  FeedbackType,
  FeedbackStatus,
  CreateFeedbackRequest,
  UpdateFeedbackRequest,
  RespondToFeedbackRequest,
  ResolveFeedbackRequest,
  RejectFeedbackRequest,
  FeedbackListParams,
  CreateVehicleMileageRequest,
  UpdateVehicleMileageRequest
} from '../types/api'

export class ApiError extends Error {
  public status: number
  public errors: Record<string, string[]> = {}
  public detail?: string

  constructor(status: number, response: any) {
    super(`API Error: ${status}`)
    this.status = status
    this.name = 'ApiError'

    // Handle Django REST framework error format
    if (typeof response === 'object') {
      this.detail = response.detail
      
      // Parse field-specific errors
      Object.keys(response).forEach(key => {
        if (key !== 'detail' && Array.isArray(response[key])) {
          this.errors[key] = response[key]
        } else if (key !== 'detail' && typeof response[key] === 'string') {
          this.errors[key] = [response[key]]
        }
      })
    }
  }

  getFieldErrors(field: string): string[] {
    return this.errors[field] || []
  }

  getAllErrors(): ValidationError[] {
    const errors: ValidationError[] = []
    
    // Add general detail error
    if (this.detail) {
      errors.push({ field: 'general', message: this.detail })
    }
    
    // Add field-specific errors
    Object.entries(this.errors).forEach(([field, messages]) => {
      messages.forEach(message => {
        errors.push({ field, message })
      })
    })
    
    return errors
  }
}

export class ApiClient {
  private baseUrl: string
  private tokenStorage: TokenStorage = {
    access_token: null,
    refresh_token: null,
    expires_at: null,
    user_type: null,
    user_id: null
  }

  constructor(baseUrl?: string) {
    if (baseUrl) {
      this.baseUrl = baseUrl.replace(/\/$/, '') // Remove trailing slash
    } else if (process.env.NEXT_PUBLIC_API_URL && !process.env.NEXT_PUBLIC_API_URL.startsWith('/')) {
      // Use NEXT_PUBLIC_API_URL only if it's a full URL
      this.baseUrl = process.env.NEXT_PUBLIC_API_URL.replace(/\/$/, '')
    } else {
      // Will be set by initializeConfig on first request
      this.baseUrl = ''
    }
    this.loadTokensFromStorage()
  }

  // Initialize configuration from runtime config API
  private async initializeConfig(): Promise<string> {
    if (this.baseUrl) return this.baseUrl

    try {
      // Fetch config from our API route
      const response = await fetch('/api/config')
      const config = await response.json()
      
      if (config.apiUrl) {
        this.baseUrl = config.apiUrl.replace(/\/$/, '')
        return this.baseUrl
      }
    } catch (error) {
      console.warn('Failed to fetch runtime config, falling back to localhost:', error)
    }
    
    this.baseUrl = 'http://localhost:8000'
    return this.baseUrl
  }

  // Token Management
  private loadTokensFromStorage() {
    if (typeof window !== 'undefined') {
      // Try to load from user-type-specific keys first
      const userType = this.tokenStorage.user_type || this.detectUserType()
      if (userType) {
        this.tokenStorage.access_token = localStorage.getItem(`access_token_${userType}`)
        this.tokenStorage.refresh_token = localStorage.getItem(`refresh_token_${userType}`)
        this.tokenStorage.user_type = userType
      }
      
      this.tokenStorage.user_id = localStorage.getItem('user_id')
      
      const expiresAt = localStorage.getItem('token_expires_at')
      this.tokenStorage.expires_at = expiresAt ? parseInt(expiresAt) : null
    }
  }
  
  private detectUserType(): UserType | null {
    if (typeof window !== 'undefined') {
      const path = window.location.pathname
      if (path.includes('/dashboard')) return 'admin'
      if (path.includes('/portal')) return 'driver' // Could also be tester
    }
    return null
  }

  private saveTokensToStorage() {
    if (typeof window !== 'undefined') {
      const userType = this.tokenStorage.user_type
      if (userType) {
        if (this.tokenStorage.access_token) {
          localStorage.setItem(`access_token_${userType}`, this.tokenStorage.access_token)
        }
        if (this.tokenStorage.refresh_token) {
          localStorage.setItem(`refresh_token_${userType}`, this.tokenStorage.refresh_token)
        }
      }
      if (this.tokenStorage.user_id) {
        localStorage.setItem('user_id', this.tokenStorage.user_id)
      }
      if (this.tokenStorage.expires_at) {
        localStorage.setItem('token_expires_at', this.tokenStorage.expires_at.toString())
      }
    }
  }

  private clearTokensFromStorage() {
    if (typeof window !== 'undefined') {
      const userType = this.tokenStorage.user_type
      if (userType) {
        localStorage.removeItem(`access_token_${userType}`)
        localStorage.removeItem(`refresh_token_${userType}`)
      }
      localStorage.removeItem('user_id')
      localStorage.removeItem('token_expires_at')
    }
    
    this.tokenStorage = {
      access_token: null,
      refresh_token: null,
      expires_at: null,
      user_type: null,
      user_id: null
    }
  }

  private isTokenExpired(): boolean {
    if (!this.tokenStorage.expires_at) return true
    return Date.now() / 1000 > this.tokenStorage.expires_at - 60 // Refresh 1 minute before expiry
  }

  private async refreshTokenIfNeeded(): Promise<void> {
    if (!this.tokenStorage.refresh_token || !this.isTokenExpired()) {
      return
    }

    try {
      const response = await this.refreshToken({ refresh: this.tokenStorage.refresh_token })
      this.tokenStorage.access_token = response.access
      // Decode token to get expiry (simplified - in production use proper JWT library)
      const tokenParts = response.access.split('.')
      if (tokenParts.length === 3) {
        const payload = JSON.parse(atob(tokenParts[1]))
        this.tokenStorage.expires_at = payload.exp
      }
      this.saveTokensToStorage()
    } catch (error) {
      // If refresh fails, clear tokens and force re-login
      this.clearTokensFromStorage()
      throw error
    }
  }

  // HTTP Methods with Authentication
  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    await this.initializeConfig()
    await this.refreshTokenIfNeeded()

    const url = `${this.baseUrl}${endpoint}`
    const headers: HeadersInit = { ...options.headers }

    // Don't set Content-Type for FormData - browser handles it
    const isFormData = options.body instanceof FormData
    if (!isFormData) {
      headers['Content-Type'] = 'application/json'
    }

    // Add auth header if token exists
    if (this.tokenStorage.access_token) {
      headers.Authorization = `Bearer ${this.tokenStorage.access_token}`
    }

    const config: RequestInit = {
      ...options,
      headers
    }

    try {
      const response = await fetch(url, config)
      
      if (!response.ok) {
        let errorData: any
        try {
          errorData = await response.json()
        } catch {
          errorData = { detail: `HTTP ${response.status}: ${response.statusText}` }
        }
        throw new ApiError(response.status, errorData)
      }

      // Handle 204 No Content responses
      if (response.status === 204) {
        return {} as T
      }

      return await response.json()
    } catch (error) {
      if (error instanceof ApiError) {
        throw error
      }
      throw new ApiError(500, { detail: `Network error: ${error instanceof Error ? error.message : 'Unknown error'}` })
    }
  }

  private async get<T>(endpoint: string): Promise<T> {
    return this.request<T>(endpoint, { method: 'GET' })
  }

  private async post<T>(endpoint: string, data?: any, customHeaders?: Record<string, string>): Promise<T> {
    const isFormData = data instanceof FormData
    return this.request<T>(endpoint, {
      method: 'POST',
      body: isFormData ? data : (data ? JSON.stringify(data) : undefined),
      headers: customHeaders
    })
  }

  private async patch<T>(endpoint: string, data: any, customHeaders?: Record<string, string>): Promise<T> {
    const isFormData = data instanceof FormData
    return this.request<T>(endpoint, {
      method: 'PATCH',
      body: isFormData ? data : JSON.stringify(data),
      headers: customHeaders
    })
  }

  private async put<T>(endpoint: string, data: any): Promise<T> {
    return this.request<T>(endpoint, {
      method: 'PUT',
      body: JSON.stringify(data)
    })
  }

  private async delete<T>(endpoint: string): Promise<T> {
    return this.request<T>(endpoint, { method: 'DELETE' })
  }

  // Authentication Endpoints
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response = await this.post<LoginResponse>('/auth/login/', credentials)
    
    // Store tokens
    this.tokenStorage.access_token = response.access
    this.tokenStorage.refresh_token = response.refresh
    this.tokenStorage.user_type = response.user_type
    this.tokenStorage.user_id = response.user_id
    
    // Decode token to get expiry
    const tokenParts = response.access.split('.')
    if (tokenParts.length === 3) {
      const payload = JSON.parse(atob(tokenParts[1]))
      this.tokenStorage.expires_at = payload.exp
    }
    
    this.saveTokensToStorage()
    return response
  }

  async register(data: RegisterRequest): Promise<RegisterResponse> {
    // Automatically add the appropriate alias based on user_type (if not already present)
    let emailWithAlias = data.email
    if (!data.email.includes('+')) {
      const alias = data.user_type || 'driver' // default to driver if no user_type specified
      emailWithAlias = data.email.replace('@', `+${alias}@`)
    }

    const requestData = {
      ...data,
      email: emailWithAlias
    }

    return this.post<RegisterResponse>('/auth/register/', requestData)
  }

  async refreshToken(data: RefreshTokenRequest): Promise<RefreshTokenResponse> {
    // Direct fetch call to avoid authentication loop
    const response = await fetch(`${this.baseUrl}/auth/refresh/`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    })

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }

    return response.json()
  }

  async logout(): Promise<void> {
    if (this.tokenStorage.refresh_token) {
      try {
        await this.post('/auth/logout/', { refresh: this.tokenStorage.refresh_token })
      } catch {
        // Ignore logout errors - still clear local tokens
      }
    }
    this.clearTokensFromStorage()
  }

  // User Management Endpoints
  async getCurrentUser(): Promise<User> {
    return this.get<User>('/api/users/me/')
  }

  async getUsers(params?: UserListParams): Promise<ApiResponse<User>> {
    const searchParams = new URLSearchParams()
    
    if (params?.status) searchParams.set('status', params.status)
    if (params?.user_type) searchParams.set('user_type', params.user_type)
    if (params?.is_active !== undefined) searchParams.set('is_active', params.is_active.toString())
    if (params?.search) searchParams.set('search', params.search)
    if (params?.page) searchParams.set('page', params.page.toString())
    if (params?.page_size) searchParams.set('page_size', params.page_size.toString())

    const queryString = searchParams.toString()
    return this.get<ApiResponse<User>>(`/api/users/${queryString ? '?' + queryString : ''}`)
  }

  async getUser(id: string): Promise<User> {
    return this.get<User>(`/api/users/${id}/`)
  }

  async createUser(data: CreateUserRequest): Promise<User> {
    // Automatically add the appropriate alias based on user_type (if not already present)
    let emailWithAlias = data.email
    if (!data.email.includes('+')) {
      const alias = data.user_type || 'driver' // default to driver if no user_type specified
      emailWithAlias = data.email.replace('@', `+${alias}@`)
    }

    const requestData = {
      ...data,
      email: emailWithAlias
    }

    return this.post<User>('/api/users/', requestData)
  }

  async updateUser(id: string, data: UpdateUserRequest): Promise<User> {
    return this.patch<User>(`/api/users/${id}/`, data)
  }

  async deleteUser(id: string): Promise<void> {
    return this.delete<void>(`/api/users/${id}/`)
  }

  // Convenience methods for common user operations
  async approveUser(id: string): Promise<User> {
    return this.updateUser(id, { status: 'approved' })
  }

  async rejectUser(id: string): Promise<User> {
    return this.updateUser(id, { status: 'rejected' })
  }

  async moveToPreapproval(id: string): Promise<User> {
    return this.updateUser(id, { status: 'preapproval' })
  }

  async deactivateUser(id: string): Promise<User> {
    return this.updateUser(id, { is_active: false })
  }

  async reactivateUser(id: string): Promise<User> {
    return this.updateUser(id, { is_active: true })
  }

  // System Settings Endpoints
  async getSystemSettings(): Promise<ApiResponse<SystemSettings>> {
    return this.get<ApiResponse<SystemSettings>>('/api/settings/system-settings/')
  }

  async updateSystemSettings(id: string, data: UpdateSystemSettingsRequest): Promise<SystemSettings> {
    return this.patch<SystemSettings>(`/api/settings/system-settings/${id}/`, data)
  }

  // License Class Endpoints
  async getLicenseClasses(): Promise<ApiResponse<LicenseClass>> {
    return this.get<ApiResponse<LicenseClass>>('/api/settings/license-classes/')
  }

  async createLicenseClass(data: CreateLicenseClassRequest): Promise<LicenseClass> {
    return this.post<LicenseClass>('/api/settings/license-classes/', data)
  }

  async updateLicenseClass(id: string, data: UpdateLicenseClassRequest): Promise<LicenseClass> {
    return this.patch<LicenseClass>(`/api/settings/license-classes/${id}/`, data)
  }

  async deleteLicenseClass(id: string): Promise<void> {
    return this.delete<void>(`/api/settings/license-classes/${id}/`)
  }

  // Fleet Endpoints (Trucks)
  async getTrucks(): Promise<ApiResponse<Truck>> {
    return this.get<ApiResponse<Truck>>('/api/fleet/trucks/')
  }

  async getTruck(id: string): Promise<Truck> {
    return this.get<Truck>(`/api/fleet/trucks/${id}/`)
  }

  async createTruck(data: CreateTruckRequest): Promise<Truck> {
    return this.post<Truck>('/api/fleet/trucks/', data)
  }

  async updateTruck(id: string, data: UpdateTruckRequest): Promise<Truck> {
    return this.patch<Truck>(`/api/fleet/trucks/${id}/`, data)
  }

  async deleteTruck(id: string): Promise<void> {
    return this.delete<void>(`/api/fleet/trucks/${id}/`)
  }

  // Materials Endpoints
  async getMaterials(params?: MaterialListParams): Promise<ApiResponse<Material>> {
    const searchParams = new URLSearchParams()
    
    if (params?.page) searchParams.set('page', params.page.toString())
    if (params?.page_size) searchParams.set('page_size', params.page_size.toString())
    if (params?.search) searchParams.set('search', params.search)

    const queryString = searchParams.toString()
    return this.get<ApiResponse<Material>>(`/api/materials/${queryString ? '?' + queryString : ''}`)
  }

  async getMaterial(id: string): Promise<Material> {
    return this.get<Material>(`/api/materials/${id}/`)
  }

  async createMaterial(data: CreateMaterialRequest | FormData): Promise<Material> {
    // FormData handling is done automatically in the request method
    return this.post<Material>('/api/materials/', data)
  }

  async updateMaterial(id: string, data: UpdateMaterialRequest | FormData): Promise<Material> {
    // FormData handling is done automatically in the request method
    return this.patch<Material>(`/api/materials/${id}/`, data)
  }

  async deleteMaterial(id: string): Promise<void> {
    return this.delete<void>(`/api/materials/${id}/`)
  }

  // Material Variants Endpoints
  async getMaterialVariants(params?: MaterialVariantListParams): Promise<ApiResponse<MaterialVariant>> {
    const searchParams = new URLSearchParams()
    
    if (params?.material) searchParams.set('material', params.material)
    if (params?.page) searchParams.set('page', params.page.toString())
    if (params?.page_size) searchParams.set('page_size', params.page_size.toString())

    const queryString = searchParams.toString()
    return this.get<ApiResponse<MaterialVariant>>(`/api/materials/variants/${queryString ? '?' + queryString : ''}`)
  }

  async getMaterialVariant(id: string): Promise<MaterialVariant> {
    return this.get<MaterialVariant>(`/api/materials/variants/${id}/`)
  }

  async createMaterialVariant(data: CreateMaterialVariantRequest | FormData): Promise<MaterialVariant> {
    // FormData handling is done automatically in the request method
    return this.post<MaterialVariant>('/api/materials/variants/', data)
  }

  async updateMaterialVariant(id: string, data: UpdateMaterialVariantRequest | FormData): Promise<MaterialVariant> {
    // FormData handling is done automatically in the request method
    return this.patch<MaterialVariant>(`/api/materials/variants/${id}/`, data)
  }

  async deleteMaterialVariant(id: string): Promise<void> {
    return this.delete<void>(`/api/materials/variants/${id}/`)
  }

  // Get variants for a specific material
  async getMaterialVariantsForMaterial(materialId: string): Promise<MaterialVariant[]> {
    return this.get<MaterialVariant[]>(`/api/materials/${materialId}/variants/`)
  }

  // Material Photos Endpoints
  async getMaterialPhotos(params?: MaterialPhotoListParams): Promise<ApiResponse<MaterialPhoto>> {
    const searchParams = new URLSearchParams()
    
    if (params?.material) searchParams.set('material', params.material)
    if (params?.page) searchParams.set('page', params.page.toString())
    if (params?.page_size) searchParams.set('page_size', params.page_size.toString())

    const queryString = searchParams.toString()
    return this.get<ApiResponse<MaterialPhoto>>(`/api/materials/photos/${queryString ? '?' + queryString : ''}`)
  }

  async deleteMaterialPhoto(id: string): Promise<void> {
    return this.delete<void>(`/api/materials/photos/${id}/`)
  }

  // Material Variant Photos Endpoints
  async getMaterialVariantPhotos(params?: MaterialVariantPhotoListParams): Promise<ApiResponse<MaterialVariantPhoto>> {
    const searchParams = new URLSearchParams()
    
    if (params?.material_variant) searchParams.set('material_variant', params.material_variant)
    if (params?.page) searchParams.set('page', params.page.toString())
    if (params?.page_size) searchParams.set('page_size', params.page_size.toString())

    const queryString = searchParams.toString()
    return this.get<ApiResponse<MaterialVariantPhoto>>(`/api/materials/variant-photos/${queryString ? '?' + queryString : ''}`)
  }

  async deleteMaterialVariantPhoto(id: string): Promise<void> {
    return this.delete<void>(`/api/materials/variant-photos/${id}/`)
  }

  // ===== TRIPS API METHODS =====
  
  // Trips Endpoints
  async getTrips(params?: TripListParams): Promise<ApiResponse<Trip>> {
    const searchParams = new URLSearchParams()
    
    if (params?.status) searchParams.set('status', params.status)
    if (params?.driver_id) searchParams.set('driver_id', params.driver_id)
    if (params?.truck_id) searchParams.set('truck_id', params.truck_id)
    if (params?.material_id) searchParams.set('material_id', params.material_id)
    if (params?.date_from) searchParams.set('date_from', params.date_from)
    if (params?.date_to) searchParams.set('date_to', params.date_to)
    if (params?.search) searchParams.set('search', params.search)
    if (params?.ordering) searchParams.set('ordering', params.ordering)
    if (params?.page) searchParams.set('page', params.page.toString())
    if (params?.page_size) searchParams.set('page_size', params.page_size.toString())

    const queryString = searchParams.toString()
    return this.get<ApiResponse<Trip>>(`/api/trips/${queryString ? '?' + queryString : ''}`)
  }

  async getTrip(id: string): Promise<Trip> {
    return this.get<Trip>(`/api/trips/${id}/`)
  }

  async createTrip(data: CreateTripRequest | FormData): Promise<Trip> {
    // FormData handling is done automatically in the request method
    return this.post<Trip>('/api/trips/', data)
  }

  async updateTrip(id: string, data: UpdateTripRequest | FormData): Promise<Trip> {
    // FormData handling is done automatically in the request method
    return this.patch<Trip>(`/api/trips/${id}/`, data)
  }

  async deleteTrip(id: string): Promise<void> {
    return this.delete<void>(`/api/trips/${id}/`)
  }

  async uploadTripPhoto(tripId: string, photoType: 'start_mileage' | 'material_loading' | 'end_mileage', photoFile: File): Promise<{ photo_url: string; message: string }> {
    const formData = new FormData()
    formData.append('photo_type', photoType)
    formData.append('photo', photoFile)
    
    return this.post<{ photo_url: string; message: string }>(`/api/trips/${tripId}/upload_photos/`, formData)
  }

  // Start a trip
  async startTrip(tripId: string, data: {
    start_mileage: number
    proof_image?: File
    material_loading_photos?: File[]
    current_location_coords?: string
  }): Promise<{message: string, trip_id: string, status: string}> {
    const formData = new FormData()
    formData.append('start_mileage', data.start_mileage.toString())
    
    if (data.proof_image) {
      formData.append('proof_image', data.proof_image)
    }
    
    if (data.material_loading_photos) {
      data.material_loading_photos.forEach(photo => {
        formData.append('material_loading_photos', photo)
      })
    }
    
    if (data.current_location_coords) {
      formData.append('current_location_coords', data.current_location_coords)
    }
    
    return this.post(`/api/trips/${tripId}/start_trip/`, formData)
  }

  // End a trip
  async endTrip(tripId: string, data: {
    end_mileage: number
    proof_end_image?: File
  }): Promise<{message: string, trip_id: string, status: string, total_mileage?: number}> {
    const formData = new FormData()
    formData.append('end_mileage', data.end_mileage.toString())
    
    if (data.proof_end_image) {
      formData.append('proof_end_image', data.proof_end_image)
    }
    
    return this.post(`/api/trips/${tripId}/end_trip/`, formData)
  }

  // Trip Actions
  async calculateTripCost(id: string): Promise<{ total_cost: string }> {
    return this.post<{ total_cost: string }>(`/api/trips/${id}/calculate_cost/`, {})
  }

  async getTripsByStatus(status?: string): Promise<Record<string, Trip[]> | Trip[]> {
    const params = status ? `?status=${status}` : ''
    return this.get<Record<string, Trip[]> | Trip[]>(`/api/trips/by_status/${params}`)
  }

  // Expenses Endpoints
  async getExpenses(params?: ExpenseListParams): Promise<ApiResponse<Expense>> {
    const searchParams = new URLSearchParams()
    
    if (params?.trip_id) searchParams.set('trip_id', params.trip_id)
    if (params?.driver_id) searchParams.set('driver_id', params.driver_id)
    if (params?.truck_id) searchParams.set('truck_id', params.truck_id)
    if (params?.search) searchParams.set('search', params.search)
    if (params?.ordering) searchParams.set('ordering', params.ordering)
    if (params?.page) searchParams.set('page', params.page.toString())
    if (params?.page_size) searchParams.set('page_size', params.page_size.toString())

    const queryString = searchParams.toString()
    return this.get<ApiResponse<Expense>>(`/api/trips/expenses/${queryString ? '?' + queryString : ''}`)
  }

  async getExpense(id: string): Promise<Expense> {
    return this.get<Expense>(`/api/trips/expenses/${id}/`)
  }

  async createExpense(data: CreateExpenseRequest): Promise<Expense> {
    return this.post<Expense>('/api/trips/expenses/', data)
  }

  async updateExpense(id: string, data: UpdateExpenseRequest): Promise<Expense> {
    return this.patch<Expense>(`/api/trips/expenses/${id}/`, data)
  }

  async deleteExpense(id: string): Promise<void> {
    return this.delete<void>(`/api/trips/expenses/${id}/`)
  }

  // Expense Actions
  async getExpensesByDriver(driverId: string): Promise<Expense[]> {
    return this.get<Expense[]>(`/api/trips/expenses/by_driver/?driver_id=${driverId}`)
  }

  async getExpensesByTruck(truckId: string): Promise<Expense[]> {
    return this.get<Expense[]>(`/api/trips/expenses/by_truck/?truck_id=${truckId}`)
  }

  // Receipts Endpoints
  async getReceipts(params?: ReceiptListParams): Promise<ApiResponse<Receipt>> {
    const searchParams = new URLSearchParams()
    
    if (params?.expense_id) searchParams.set('expense_id', params.expense_id)
    if (params?.page) searchParams.set('page', params.page.toString())
    if (params?.page_size) searchParams.set('page_size', params.page_size.toString())

    const queryString = searchParams.toString()
    return this.get<ApiResponse<Receipt>>(`/api/trips/receipts/${queryString ? '?' + queryString : ''}`)
  }

  async getReceipt(id: string): Promise<Receipt> {
    return this.get<Receipt>(`/api/trips/receipts/${id}/`)
  }

  async createReceipt(data: CreateReceiptRequest | FormData): Promise<Receipt> {
    // FormData handling is done automatically in the request method
    return this.post<Receipt>('/api/trips/receipts/', data)
  }

  async updateReceipt(id: string, data: UpdateReceiptRequest | FormData): Promise<Receipt> {
    // FormData handling is done automatically in the request method
    return this.patch<Receipt>(`/api/trips/receipts/${id}/`, data)
  }

  async deleteReceipt(id: string): Promise<void> {
    return this.delete<void>(`/api/trips/receipts/${id}/`)
  }

  // Receipt Actions
  async extractReceiptDetails(id: string): Promise<Record<string, any>> {
    return this.post<Record<string, any>>(`/api/trips/receipts/${id}/extract_details/`, {})
  }

  // Vehicle Mileage Endpoints
  async getVehicleMileages(params?: VehicleMileageListParams): Promise<ApiResponse<VehicleMileage>> {
    const searchParams = new URLSearchParams()
    
    if (params?.driver_id) searchParams.set('driver_id', params.driver_id)
    if (params?.truck_id) searchParams.set('truck_id', params.truck_id)
    if (params?.date_from) searchParams.set('date_from', params.date_from)
    if (params?.date_to) searchParams.set('date_to', params.date_to)
    if (params?.ordering) searchParams.set('ordering', params.ordering)
    if (params?.page) searchParams.set('page', params.page.toString())
    if (params?.page_size) searchParams.set('page_size', params.page_size.toString())

    const queryString = searchParams.toString()
    return this.get<ApiResponse<VehicleMileage>>(`/api/trips/vehicle-mileage/${queryString ? '?' + queryString : ''}`)
  }

  async getVehicleMileage(id: string): Promise<VehicleMileage> {
    return this.get<VehicleMileage>(`/api/trips/vehicle-mileage/${id}/`)
  }

  async createVehicleMileage(data: CreateVehicleMileageRequest | FormData): Promise<VehicleMileage> {
    // FormData handling is done automatically in the request method
    return this.post<VehicleMileage>('/api/trips/vehicle-mileage/', data)
  }

  async updateVehicleMileage(id: string, data: UpdateVehicleMileageRequest | FormData): Promise<VehicleMileage> {
    // FormData handling is done automatically in the request method
    return this.patch<VehicleMileage>(`/api/trips/vehicle-mileage/${id}/`, data)
  }

  async deleteVehicleMileage(id: string): Promise<void> {
    return this.delete<void>(`/api/trips/vehicle-mileage/${id}/`)
  }

  // ===== DRIVER PROFILE API METHODS =====
  
  // List driver profiles with filters and enhanced data
  async getDriverProfiles(params?: DriverProfileListParams): Promise<ApiResponse<DriverProfile>> {
    const searchParams = new URLSearchParams()
    
    if (params?.status) searchParams.set('status', params.status)
    if (params?.license_expiring) searchParams.set('license_expiring', params.license_expiring.toString())
    if (params?.license_expires_before) searchParams.set('license_expires_before', params.license_expires_before)
    if (params?.has_pending_version !== undefined) searchParams.set('has_pending_version', params.has_pending_version.toString())
    if (params?.include) searchParams.set('include', params.include)
    if (params?.days) searchParams.set('days', params.days.toString())
    if (params?.search) searchParams.set('search', params.search)
    if (params?.ordering) searchParams.set('ordering', params.ordering)
    if (params?.page) searchParams.set('page', params.page.toString())
    if (params?.page_size) searchParams.set('page_size', params.page_size.toString())

    const queryString = searchParams.toString()
    return this.get<ApiResponse<DriverProfile>>(`/api/drivers/profiles/${queryString ? '?' + queryString : ''}`)
  }

  // Get specific driver profile
  async getDriverProfile(id: string, include?: string): Promise<DriverProfile> {
    const searchParams = new URLSearchParams()
    if (include) searchParams.set('include', include)
    const queryString = searchParams.toString()
    return this.get<DriverProfile>(`/api/drivers/profiles/${id}/${queryString ? '?' + queryString : ''}`)
  }

  // Create new driver profile
  async createDriverProfile(data: CreateDriverProfileRequest, files?: { [key: string]: File }): Promise<DriverProfile> {
    if (files && Object.keys(files).length > 0) {
      // Handle multipart upload for images
      const formData = new FormData()
      
      // Add text fields
      Object.entries(data).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          if (Array.isArray(value)) {
            value.forEach((item, index) => {
              formData.append(`${key}[${index}]`, item)
            })
          } else {
            formData.append(key, value.toString())
          }
        }
      })
      
      // Add files
      Object.entries(files).forEach(([key, file]) => {
        formData.append(key, file)
      })
      
      return this.request<DriverProfile>('/api/drivers/profiles/', {
        method: 'POST',
        body: formData,
        // Don't set Content-Type header for FormData
      })
    } else {
      return this.post<DriverProfile>('/api/drivers/profiles/', data)
    }
  }

  // Update driver profile
  async updateDriverProfile(id: string, data: UpdateDriverProfileRequest, files?: { [key: string]: File }): Promise<DriverProfile> {
    if (files && Object.keys(files).length > 0) {
      // Handle multipart upload for images
      const formData = new FormData()
      
      // Add text fields
      Object.entries(data).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          if (Array.isArray(value)) {
            value.forEach((item, index) => {
              formData.append(`${key}[${index}]`, item)
            })
          } else {
            formData.append(key, value.toString())
          }
        }
      })
      
      // Add files
      Object.entries(files).forEach(([key, file]) => {
        formData.append(key, file)
      })
      
      return this.request<DriverProfile>(`/api/drivers/profiles/${id}/`, {
        method: 'PATCH',
        body: formData,
      })
    } else {
      return this.patch<DriverProfile>(`/api/drivers/profiles/${id}/`, data)
    }
  }

  // Delete driver profile
  async deleteDriverProfile(id: string): Promise<void> {
    return this.delete<void>(`/api/drivers/profiles/${id}/`)
  }

  // Get current driver's profile (/me endpoint)
  async getMyDriverProfile(include?: string): Promise<DriverProfile> {
    const searchParams = new URLSearchParams()
    if (include) searchParams.set('include', include)
    const queryString = searchParams.toString()
    return this.get<DriverProfile>(`/api/drivers/profiles/me/${queryString ? '?' + queryString : ''}`)
  }

  // Update current driver's profile
  async updateMyDriverProfile(data: UpdateDriverProfileRequest, files?: { [key: string]: File }): Promise<DriverProfile> {
    if (files && Object.keys(files).length > 0) {
      const formData = new FormData()
      
      Object.entries(data).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          if (Array.isArray(value)) {
            value.forEach((item, index) => {
              formData.append(`${key}[${index}]`, item)
            })
          } else {
            formData.append(key, value.toString())
          }
        }
      })
      
      Object.entries(files).forEach(([key, file]) => {
        formData.append(key, file)
      })
      
      return this.request<DriverProfile>('/api/drivers/profiles/me/', {
        method: 'PATCH',
        body: formData,
      })
    } else {
      return this.patch<DriverProfile>('/api/drivers/profiles/me/', data)
    }
  }

  // Driver: Submit own profile for approval
  async submitDriverProfile(id: string): Promise<{ message: string }> {
    return this.post<{ message: string }>(`/api/drivers/profiles/${id}/submit/`, {})
  }

  // Admin: Approve/reject/request changes for driver profile
  async approveDriverProfile(id: string, data: DriverProfileApprovalRequest): Promise<{ message: string }> {
    return this.post<{ message: string }>(`/api/drivers/profiles/${id}/approve/`, data)
  }

  // Get driver profile change history
  async getDriverProfileHistory(id: string): Promise<DriverProfileChange[]> {
    return this.get<DriverProfileChange[]>(`/api/drivers/profiles/${id}/history/`)
  }

  // ============================================================================
  // FEEDBACK MANAGEMENT
  // ============================================================================

  // Get feedback list with filtering
  async getFeedback(params: FeedbackListParams = {}): Promise<ApiResponse<Feedback>> {
    const searchParams = new URLSearchParams()
    
    // Admin-only filters
    if (params.status) {
      const statuses = Array.isArray(params.status) ? params.status : [params.status]
      statuses.forEach(status => searchParams.append('status', status))
    }
    if (params.has_response !== undefined) {
      searchParams.append('has_response', params.has_response.toString())
    }
    if (params.responded_by) {
      searchParams.append('responded_by', params.responded_by)
    }
    
    // Content filters
    if (params.feedback_type) {
      const types = Array.isArray(params.feedback_type) ? params.feedback_type : [params.feedback_type]
      types.forEach(type => searchParams.append('feedback_type', type))
    }
    if (params.search) {
      searchParams.append('search', params.search)
    }
    
    // Date filters
    if (params.created_after) searchParams.append('created_after', params.created_after)
    if (params.created_before) searchParams.append('created_before', params.created_before)
    if (params.responded_after) searchParams.append('responded_after', params.responded_after)
    if (params.responded_before) searchParams.append('responded_before', params.responded_before)
    
    // Include parameter
    if (params.include) searchParams.append('include', params.include)
    
    // Pagination
    if (params.page) searchParams.append('page', params.page.toString())
    if (params.page_size) searchParams.append('page_size', params.page_size.toString())
    if (params.ordering) searchParams.append('ordering', params.ordering)
    
    const queryString = searchParams.toString()
    const url = queryString ? `/api/feedback/?${queryString}` : '/api/feedback/'
    
    return this.get<ApiResponse<Feedback>>(url)
  }

  // Get single feedback by ID
  async getFeedbackById(id: string, include?: string): Promise<Feedback> {
    const url = include ? `/api/feedback/${id}/?include=${include}` : `/api/feedback/${id}/`
    return this.get<Feedback>(url)
  }

  // Create new feedback
  async createFeedback(data: CreateFeedbackRequest): Promise<Feedback> {
    return this.post<Feedback>('/api/feedback/', data)
  }

  // Update feedback (user can only update their own)
  async updateFeedback(id: string, data: UpdateFeedbackRequest): Promise<Feedback> {
    return this.patch<Feedback>(`/api/feedback/${id}/`, data)
  }

  // Delete feedback (user can only delete their own)
  async deleteFeedback(id: string): Promise<void> {
    return this.delete(`/api/feedback/${id}/`)
  }

  // Get current user's feedback (shortcut endpoint)
  async getMyFeedback(params: Omit<FeedbackListParams, 'status' | 'has_response' | 'responded_by'> = {}): Promise<ApiResponse<Feedback>> {
    const searchParams = new URLSearchParams()
    
    // Content filters
    if (params.feedback_type) {
      const types = Array.isArray(params.feedback_type) ? params.feedback_type : [params.feedback_type]
      types.forEach(type => searchParams.append('feedback_type', type))
    }
    if (params.search) searchParams.append('search', params.search)
    
    // Date filters
    if (params.created_after) searchParams.append('created_after', params.created_after)
    if (params.created_before) searchParams.append('created_before', params.created_before)
    if (params.responded_after) searchParams.append('responded_after', params.responded_after)
    if (params.responded_before) searchParams.append('responded_before', params.responded_before)
    
    // Include parameter
    if (params.include) searchParams.append('include', params.include)
    
    // Pagination
    if (params.page) searchParams.append('page', params.page.toString())
    if (params.page_size) searchParams.append('page_size', params.page_size.toString())
    if (params.ordering) searchParams.append('ordering', params.ordering)
    
    const queryString = searchParams.toString()
    const url = queryString ? `/api/feedback/me/?${queryString}` : '/api/feedback/me/'
    
    return this.get<ApiResponse<Feedback>>(url)
  }

  // ============================================================================
  // ADMIN FEEDBACK ACTIONS
  // ============================================================================

  // Admin: Respond to feedback
  async respondToFeedback(id: string, data: RespondToFeedbackRequest): Promise<{ message: string, feedback: Feedback }> {
    return this.post<{ message: string, feedback: Feedback }>(`/api/feedback/${id}/respond/`, data)
  }

  // Admin: Resolve feedback
  async resolveFeedback(id: string, data: ResolveFeedbackRequest = {}): Promise<{ message: string, feedback: Feedback }> {
    return this.post<{ message: string, feedback: Feedback }>(`/api/feedback/${id}/resolve/`, data)
  }

  // Admin: Reject feedback
  async rejectFeedback(id: string, data: RejectFeedbackRequest = {}): Promise<{ message: string, feedback: Feedback }> {
    return this.post<{ message: string, feedback: Feedback }>(`/api/feedback/${id}/reject/`, data)
  }

  // Auth State Helpers
  isAuthenticated(): boolean {
    return !!this.tokenStorage.access_token && !this.isTokenExpired()
  }

  getCurrentUserType(): UserType | null {
    return this.tokenStorage.user_type
  }

  getCurrentUserId(): string | null {
    return this.tokenStorage.user_id
  }
}

// Create default instance
export const apiClient = new ApiClient()