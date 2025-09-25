// Generated TypeScript types for Django REST API
// Based on the qtruck_api endpoints we just tested

export type UserStatus = 'preapproval' | 'approved' | 'rejected';
export type UserType = 'admin' | 'driver' | 'tester';

// Core API Response Types
export interface ApiResponse<T = any> {
  count?: number;
  next?: string | null;
  previous?: string | null;
  results?: T[];
  // For single object responses, data is directly at root level
}

export interface ApiError {
  detail?: string;
  [field: string]: string | string[] | undefined;
}

// User Management Types
export interface UserProfile {
  approval_date: string | null;
  approved_by: string | null; // UUID of approving admin
}

export interface User {
  id: string; // UUID
  email: string;
  base_email: string; // Email without alias
  first_name: string;
  last_name: string;
  full_name: string; // Computed field
  user_type: UserType;
  status: UserStatus;
  is_active: boolean;
  profile: UserProfile;
  created_at: string; // ISO datetime
  updated_at: string; // ISO datetime
  last_login?: string | null; // ISO datetime
}

export interface CreateUserRequest {
  email: string;
  password: string;
  first_name: string;
  last_name: string;
  user_type?: UserType; // Optional, defaults to 'driver'
}

export interface UpdateUserRequest {
  first_name?: string;
  last_name?: string;
  status?: UserStatus;
  is_active?: boolean;
}

// Authentication Types
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  access: string; // JWT access token
  refresh: string; // JWT refresh token
  user_type: UserType;
  user_id: string; // UUID
  status: UserStatus;
}

export interface RefreshTokenRequest {
  refresh: string;
}

export interface RefreshTokenResponse {
  access: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  password_confirm: string;
  first_name: string;
  last_name: string;
  user_type?: UserType; // Optional, auto-detected from email alias
}

export interface RegisterResponse {
  id: string;
  email: string;
  base_email: string;
  first_name: string;
  last_name: string;
  full_name: string;
  user_type: UserType;
  status: UserStatus;
  is_active: boolean;
  profile: UserProfile;
  created_at: string;
  updated_at: string;
}

// System Settings Types
export interface SystemSettings {
  id: string;
  tester_registration_enabled: boolean;
  license_expiry_warning_days: number;
  auto_approve_user_types: UserType[];
  created_at: string;
  updated_at: string;
}

export interface UpdateSystemSettingsRequest {
  tester_registration_enabled?: boolean;
  license_expiry_warning_days?: number;
  auto_approve_user_types?: UserType[];
}

// License Class Types
export interface LicenseClass {
  id: string;
  name: string;
  description: string;
  created_at: string;
  updated_at: string;
}

export interface CreateLicenseClassRequest {
  name: string;
  description: string;
}

export interface UpdateLicenseClassRequest {
  name?: string;
  description?: string;
}

// API Query Parameters
export interface UserListParams {
  status?: UserStatus;
  user_type?: UserType;
  is_active?: boolean;
  search?: string; // Search in first_name, last_name, email
  page?: number;
  page_size?: number;
}

// Common HTTP Status Types
export type HttpStatus = 200 | 201 | 204 | 400 | 401 | 403 | 404 | 422 | 500;

// Error Response Format
export interface ErrorResponse {
  detail?: string;
  [field: string]: string | string[] | undefined;
}

// Form Validation Error Types
export interface ValidationError {
  field: string;
  message: string;
}

export interface FormErrors<T = Record<string, string>> {
  field_errors: T;
  non_field_errors: string[];
}

// API Client Configuration
export interface ApiConfig {
  baseUrl: string;
  timeout?: number;
  retries?: number;
}

// Token Storage Interface
export interface TokenStorage {
  access_token: string | null;
  refresh_token: string | null;
  expires_at: number | null;
  user_type: UserType | null;
  user_id: string | null;
}

// Fleet & Materials Types
export interface Truck {
  id: string; // UUID
  license_plate: string;
  model?: string;
  driver?: string; // UUID of driver
  created_at: string; // ISO datetime
  updated_at: string; // ISO datetime
}

export interface CreateTruckRequest {
  license_plate: string;
  model?: string;
  driver?: string; // UUID
}

export interface UpdateTruckRequest {
  license_plate?: string;
  model?: string;
  driver?: string; // UUID
}

export interface MaterialPhoto {
  id: string; // UUID
  material: string; // UUID
  photo: string; // URL to image
  caption?: string;
  created_at: string; // ISO datetime
  updated_at: string; // ISO datetime
}

export interface MaterialVariantPhoto {
  id: string; // UUID
  material_variant: string; // UUID
  photo: string; // URL to image
  caption?: string;
  created_at: string; // ISO datetime
  updated_at: string; // ISO datetime
}

export interface MaterialVariant {
  id: string; // UUID
  material: string; // UUID
  material_name: string; // Read-only computed field
  name: string; // Variant name (e.g., Darugo, Kajido, River Sand)
  description?: string;
  photos: MaterialVariantPhoto[];
  created_at: string; // ISO datetime
  updated_at: string; // ISO datetime
}

export interface Material {
  id: string; // UUID
  name: string; // Base material name (e.g., Sand, Stone, Cement)
  description?: string;
  variants: MaterialVariant[]; // Read-only computed field
  photos: MaterialPhoto[];
  created_at: string; // ISO datetime
  updated_at: string; // ISO datetime
}

export interface CreateMaterialRequest {
  name: string;
  description?: string;
  // photos handled via FormData multipart upload
}

export interface UpdateMaterialRequest {
  name?: string;
  description?: string;
  // photos handled via FormData multipart upload
}

export interface CreateMaterialVariantRequest {
  material: string; // UUID
  name: string;
  description?: string;
  // photos handled via FormData multipart upload
}

export interface UpdateMaterialVariantRequest {
  material?: string; // UUID
  name?: string;
  description?: string;
  // photos handled via FormData multipart upload
}

export interface MaterialCost {
  id: string; // UUID
  material: string; // UUID
  cost: number; // Decimal
  location?: string;
  user_id?: string;
  synced: boolean;
  created_at: string; // ISO datetime
  updated_at: string; // ISO datetime
}

export interface CreateMaterialCostRequest {
  material: string; // UUID
  cost: number;
  location?: string;
  user_id?: string;
}

// Trip Types (comprehensive implementation)
export interface Trip {
  id: string; // UUID
  truck: Truck; // Read-only populated truck object
  driver: Driver; // Read-only populated driver object
  material?: Material | null; // Optional material being transported
  material_variant?: MaterialVariant | null; // Optional specific variant
  start_location?: string | null;
  end_location?: string | null;
  start_location_coords?: [number, number] | null; // [longitude, latitude]
  end_location_coords?: [number, number] | null; // [longitude, latitude]
  start_mileage?: number | null;
  end_mileage?: number | null;
  total_mileage?: number | null; // Auto-calculated
  proof_image?: string | null; // URL to image
  proof_end_image?: string | null; // URL to image
  status: 'pending' | 'in_progress' | 'completed' | 'cancelled';
  date: string; // ISO datetime
  total_cost: string; // Decimal field as string (auto-calculated)
  material_cost?: string | null; // Decimal field as string
  expenses: Expense[]; // Read-only list of expenses
  created_at: string; // ISO datetime
  updated_at: string; // ISO datetime
  
  // Write-only fields for creation/update
  truck_id?: string; // UUID - write-only
  driver_id?: string; // UUID - write-only
  material_id?: string | null; // UUID - write-only
  material_variant_id?: string | null; // UUID - write-only
}

export interface CreateTripRequest {
  truck_id: string; // UUID
  driver_id: string; // UUID
  date?: string; // ISO datetime - Optional, auto-set by backend
  start_location?: string;
  end_location?: string;
  start_location_coords?: [number, number] | null;
  end_location_coords?: [number, number] | null;
  start_mileage?: number;
  end_mileage?: number;
  material_id?: string | null;
  material_variant_id?: string | null;
  material_cost?: string | null; // Decimal as string
  current_location_coords?: [number, number] | null; // Current GPS coordinates [longitude, latitude]
  status?: 'pending' | 'in_progress' | 'completed' | 'cancelled';
  // Images handled via FormData multipart upload
}

export interface UpdateTripRequest {
  truck_id?: string; // UUID
  driver_id?: string; // UUID
  date?: string; // ISO datetime
  start_location?: string;
  end_location?: string;
  start_location_coords?: [number, number] | null;
  end_location_coords?: [number, number] | null;
  start_mileage?: number;
  end_mileage?: number;
  material_id?: string | null;
  material_variant_id?: string | null;
  material_cost?: string | null; // Decimal as string
  status?: 'pending' | 'in_progress' | 'completed' | 'cancelled';
  // Images handled via FormData multipart upload
}

export interface Expense {
  id: string; // UUID
  trip?: Trip; // Read-only populated trip object (when fetched standalone)
  description: string;
  amount: string; // Decimal field as string
  receipts: Receipt[]; // Read-only list of receipts
  driver?: Driver | null; // Read-only computed field
  user_id?: string | null;
  synced: boolean;
  created_at: string; // ISO datetime
  updated_at: string; // ISO datetime
  
  // Write-only fields for creation
  trip_id?: string; // UUID - write-only
}

export interface CreateExpenseRequest {
  trip_id: string; // UUID
  description: string;
  amount: string; // Decimal as string
  user_id?: string;
}

export interface UpdateExpenseRequest {
  description?: string;
  amount?: string; // Decimal as string
  user_id?: string;
}

export interface Receipt {
  id: string; // UUID
  expense: string; // UUID reference to expense
  image: string; // URL to receipt image
  note?: string | null;
  receipt_details: Record<string, any>; // JSON field for OCR extracted data
  user_id?: string | null;
  synced: boolean;
  created_at: string; // ISO datetime
  updated_at: string; // ISO datetime
}

export interface CreateReceiptRequest {
  expense: string; // UUID
  note?: string;
  receipt_details?: Record<string, any>;
  user_id?: string;
  // image handled via FormData multipart upload
}

export interface UpdateReceiptRequest {
  note?: string;
  receipt_details?: Record<string, any>;
  user_id?: string;
  // image handled via FormData multipart upload
}

export interface VehicleMileage {
  id: string; // UUID
  truck: Truck; // Read-only populated truck object
  driver: Driver; // Read-only populated driver object
  start_mileage: number;
  end_mileage: number;
  mileage?: number | null; // Auto-calculated
  proof_image?: string | null; // URL to image
  proof_end_image?: string | null; // URL to image
  date: string; // ISO datetime
  user_id?: string | null;
  synced: boolean;
  created_at: string; // ISO datetime
  updated_at: string; // ISO datetime
  
  // Write-only fields for creation/update
  truck_id?: string; // UUID - write-only
  driver_id?: string; // UUID - write-only
}

export interface CreateVehicleMileageRequest {
  truck_id: string; // UUID
  driver_id: string; // UUID
  start_mileage: number;
  end_mileage: number;
  date: string; // ISO datetime
  user_id?: string;
  // Images handled via FormData multipart upload
}

export interface UpdateVehicleMileageRequest {
  truck_id?: string; // UUID
  driver_id?: string; // UUID
  start_mileage?: number;
  end_mileage?: number;
  date?: string; // ISO datetime
  user_id?: string;
  // Images handled via FormData multipart upload
}

// Driver Profile Types
export type DriverProfileStatus = 'draft' | 'pending' | 'approved' | 'rejected';

export interface LicenseClass {
  id: string; // UUID
  name: string;
  description?: string;
  created_at: string; // ISO datetime
  updated_at: string; // ISO datetime
}

export interface Driver {
  id: string; // UUID
  user: string; // UUID reference to User
  name: string;
  phone: string;
  license_number: string;
  created_at: string; // ISO datetime
  updated_at: string; // ISO datetime
}

export interface DriverProfile {
  id: string; // UUID
  driver: string; // UUID reference to Driver
  full_name: string;
  phone_number: string;
  id_number: string;
  license_number: string;
  license_expiry_date: string; // Date
  profile_photo?: string; // URL
  license_front_image?: string; // URL
  license_back_image?: string; // URL
  id_front_image?: string; // URL
  id_back_image?: string; // URL
  license_classes: string[]; // Array of LicenseClass UUIDs
  status: DriverProfileStatus;
  version_number: number;
  is_current: boolean;
  reviewed_by?: string; // UUID of admin who reviewed
  review_notes?: string;
  review_date?: string; // ISO datetime
  created_at: string; // ISO datetime
  updated_at: string; // ISO datetime
  
  // License status fields from backend
  is_license_expired?: boolean;
  days_until_license_expiry?: number;
  license_status?: 'valid' | 'expiring_soon' | 'expired';
  
  // Additional API fields
  driver_name?: string;
  status_display?: string;
  reviewed_at?: string;
  approval_notes?: string;
  rejection_reason?: string;
  submitted_at?: string;
  is_first_profile?: boolean;
  has_pending_version?: boolean;
  pending_version_id?: string;
  pending_status?: DriverProfileStatus;
  changes_to?: string[];
  
  // Enhanced data fields (optional based on include params)
  stats?: DriverActivityStats;
  recent_activity?: {
    activities: DriverActivity[];
    stats: DriverActivityStats;
  };
  heatmap_data?: any; // Heatmap data structure
  has_pending_version?: boolean;
  pending_version_id?: string;
  pending_status?: DriverProfileStatus;
  is_first_profile?: boolean;
}

export interface DriverActivity {
  id: string; // UUID
  driver: string; // UUID
  activity_type: string;
  activity_data: any; // JSON data
  ip_address: string;
  user_agent: string;
  created_at: string; // ISO datetime
}

export interface DriverActivityStats {
  total_activities: number;
  profile_updates: number;
  recent_activity_count: number;
  days_since_last_activity?: number;
}

export interface DriverProfileChange {
  id: string; // UUID
  old_profile?: string; // UUID
  new_profile: string; // UUID
  change_type: string;
  change_data: any; // JSON data
  created_at: string; // ISO datetime
}

// Driver API Request Types
export interface CreateDriverProfileRequest {
  full_name: string;
  phone_number: string;
  id_number: string;
  license_number: string;
  license_expiry_date: string; // Date string
  license_classes?: string[]; // Array of LicenseClass UUIDs
  // Images handled via FormData multipart upload
}

export interface UpdateDriverProfileRequest {
  full_name?: string;
  phone_number?: string;
  id_number?: string;
  license_number?: string;
  license_expiry_date?: string; // Date string
  license_classes?: string[]; // Array of LicenseClass UUIDs
  // Images handled via FormData multipart upload
}

export interface DriverProfileApprovalRequest {
  action: 'approve' | 'reject' | 'request_changes';
  notes?: string;
}

export interface DriverProfileListParams {
  status?: DriverProfileStatus;
  license_expiring?: number; // Days until expiry
  license_expires_before?: string; // Date string
  has_pending_version?: boolean;
  include?: string; // Comma-separated: 'stats', 'activity', 'heatmap'
  days?: number; // For activity/heatmap data
  search?: string;
  ordering?: string;
  page?: number;
  page_size?: number;
}

// Query Parameters
export interface MaterialListParams {
  page?: number;
  page_size?: number;
  search?: string;
}

export interface MaterialVariantListParams {
  material?: string; // UUID - filter by material
  page?: number;
  page_size?: number;
}

export interface MaterialPhotoListParams {
  material?: string; // UUID - filter by material
  page?: number;
  page_size?: number;
}

export interface MaterialVariantPhotoListParams {
  material_variant?: string; // UUID - filter by material variant
  page?: number;
  page_size?: number;
}

// Trip Query Parameters
export interface TripListParams {
  status?: 'pending' | 'in_progress' | 'completed' | 'cancelled';
  driver_id?: string; // UUID - filter by driver
  truck_id?: string; // UUID - filter by truck
  material_id?: string; // UUID - filter by material
  date_from?: string; // ISO date
  date_to?: string; // ISO date
  page?: number;
  page_size?: number;
  search?: string; // Search in start_location, end_location
  ordering?: string; // e.g., 'date', '-date', 'total_cost'
}

export interface ExpenseListParams {
  trip_id?: string; // UUID - filter by trip
  driver_id?: string; // UUID - filter by driver
  truck_id?: string; // UUID - filter by truck
  page?: number;
  page_size?: number;
  search?: string; // Search in description
  ordering?: string; // e.g., 'amount', '-amount', 'created_at'
}

export interface ReceiptListParams {
  expense_id?: string; // UUID - filter by expense
  page?: number;
  page_size?: number;
}

export interface VehicleMileageListParams {
  driver_id?: string; // UUID - filter by driver
  truck_id?: string; // UUID - filter by truck
  date_from?: string; // ISO date
  date_to?: string; // ISO date
  page?: number;
  page_size?: number;
  ordering?: string; // e.g., 'date', '-date', 'mileage'
}

// Feedback System Types
export type FeedbackType = 'bug_report' | 'feature_request' | 'general' | 'complaint' | 'suggestion';
export type FeedbackStatus = 'pending' | 'reviewed' | 'in_progress' | 'resolved' | 'rejected';

export interface Feedback {
  id: string; // UUID
  user: User;
  responded_by: User | null;
  created_at: string; // ISO datetime
  updated_at: string; // ISO datetime
  feedback_type: FeedbackType;
  subject: string;
  description: string;
  status: FeedbackStatus;
  admin_response: string | null;
  response_date: string | null; // ISO datetime
  
  // Enhanced data (available with include parameter)
  response_metadata?: {
    response_length: number;
    response_date_formatted: string | null;
    days_to_respond: number | null;
  };
  user_profile?: {
    full_name: string;
    user_type: UserType;
    date_joined: string; // ISO date
  };
}

export interface CreateFeedbackRequest {
  feedback_type: FeedbackType;
  subject: string;
  description: string;
}

export interface UpdateFeedbackRequest {
  feedback_type?: FeedbackType;
  subject?: string;
  description?: string;
}

export interface RespondToFeedbackRequest {
  admin_response: string;
  status?: FeedbackStatus; // defaults to 'reviewed'
}

export interface ResolveFeedbackRequest {
  admin_response?: string;
}

export interface RejectFeedbackRequest {
  admin_response?: string;
}

export interface FeedbackListParams {
  // Status filters (admin only)
  status?: FeedbackStatus | FeedbackStatus[];
  has_response?: boolean; // admin only
  responded_by?: string; // UUID - admin only
  
  // Type and content filters (all users)
  feedback_type?: FeedbackType | FeedbackType[];
  search?: string; // search subject and description
  
  // Date filters (all users)
  created_after?: string; // ISO date
  created_before?: string; // ISO date
  responded_after?: string; // ISO date
  responded_before?: string; // ISO date
  
  // Include enhanced data (all users)
  include?: string; // 'response_details', 'user_profile', or 'response_details,user_profile'
  
  // Standard pagination
  page?: number;
  page_size?: number;
  ordering?: string; // e.g., 'created_at', '-created_at', 'status', 'response_date'
}