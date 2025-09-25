// Re-export the new API client as the main service
export { apiClient, ApiError } from './api/client'
export * from './types/api'

// Import apiClient to use as legacy export
import { apiClient } from './api/client'

// Keep legacy export for backward compatibility during transition
export const apiService = apiClient