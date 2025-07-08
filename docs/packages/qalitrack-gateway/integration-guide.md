# QaliTrack API Gateway - Integration Guide

Comprehensive guide for integrating with the QaliTrack API Gateway, including service-to-service communication patterns, client integration examples, and best practices.

## 📋 Table of Contents

- [Integration Overview](#integration-overview)
- [Authentication Integration](#authentication-integration)
- [Service-to-Service Communication](#service-to-service-communication)
- [Client SDK Examples](#client-sdk-examples)
- [Error Handling Patterns](#error-handling-patterns)
- [Testing Integration](#testing-integration)
- [Production Deployment](#production-deployment)

## Integration Overview

### Integration Patterns

The QaliTrack API Gateway supports multiple integration patterns:

1. **Direct Client Integration** - Web applications, mobile apps
2. **Service-to-Service Communication** - Internal microservice calls
3. **Third-Party Integration** - External system connections
4. **Webhook Integration** - Event-driven notifications

### Architecture Flow

```
┌─────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Client    │────│   API Gateway   │────│  Microservices  │
│             │    │                 │    │                 │
│ • Web App   │    │ • Authentication│    │ • User Service  │
│ • Mobile    │    │ • Authorization │    │ • Product Svc   │
│ • External  │    │ • Rate Limiting │    │ • Customer Svc  │
│ • Services  │    │ • Load Balancing│    │ • Analytics Svc │
└─────────────┘    └─────────────────┘    └─────────────────┘
```

## Authentication Integration

### 1. Initial Authentication Setup

#### Step 1: Register Application
```bash
# Contact system administrator to register your application
# Receive client credentials and gateway endpoint
GATEWAY_URL="https://localhost:7000"
CLIENT_ID="your-app-id"  
CLIENT_SECRET="your-secret"
```

#### Step 2: Implement Login Flow
```typescript
// TypeScript/JavaScript example
class QaliTrackAuth {
  private baseUrl: string;
  private token: string | null = null;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  async login(username: string, password: string): Promise<AuthResult> {
    const response = await fetch(`${this.baseUrl}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    });

    if (!response.ok) {
      throw new Error(`Login failed: ${response.statusText}`);
    }

    const data = await response.json();
    this.token = data.token;
    
    return {
      token: data.token,
      user: data.user,
      expiresAt: new Date(data.expiresAt)
    };
  }

  getAuthHeaders(): Record<string, string> {
    if (!this.token) {
      throw new Error('Not authenticated');
    }
    return { 'Authorization': `Bearer ${this.token}` };
  }
}
```

#### Step 3: Token Management
```typescript
class TokenManager {
  private token: string | null = null;
  private expiresAt: Date | null = null;

  setToken(token: string, expiresAt: Date) {
    this.token = token;
    this.expiresAt = expiresAt;
    localStorage.setItem('qalitrack_token', token);
    localStorage.setItem('qalitrack_expires', expiresAt.toISOString());
  }

  getToken(): string | null {
    if (this.isExpired()) {
      this.clearToken();
      return null;
    }
    return this.token;
  }

  isExpired(): boolean {
    if (!this.expiresAt) return true;
    return new Date() >= this.expiresAt;
  }

  clearToken() {
    this.token = null;
    this.expiresAt = null;
    localStorage.removeItem('qalitrack_token');
    localStorage.removeItem('qalitrack_expires');
  }
}
```

### 2. Mock Authentication (Testing)

For development and testing environments:

```typescript
class MockAuthService {
  async mockLogin(username: string, role: string): Promise<AuthResult> {
    const response = await fetch(`${this.baseUrl}/api/MockAuth/mock-login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, role })
    });

    return response.json();
  }

  async testRole(role: 'Guest' | 'User' | 'Operator' | 'Admin' | 'SuperAdmin'): Promise<AuthResult> {
    return this.mockLogin(`test-${role.toLowerCase()}`, role);
  }
}
```

## Service-to-Service Communication

### 1. Internal Service Integration

#### Service Registration Pattern
```csharp
// .NET service integration example
public class QaliTrackServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public QaliTrackServiceClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        
        // Configure base URL for gateway
        _httpClient.BaseAddress = new Uri(_configuration["QaliTrack:GatewayUrl"]);
    }

    // Service-to-service authentication using service account
    public async Task AuthenticateServiceAsync()
    {
        var serviceCredentials = new
        {
            username = _configuration["QaliTrack:ServiceAccount:Username"],
            password = _configuration["QaliTrack:ServiceAccount:Password"]
        };

        var response = await _httpClient.PostAsJsonAsync("/api/auth/service-login", serviceCredentials);
        var authResult = await response.Content.ReadFromJsonAsync<AuthResult>();
        
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", authResult.Token);
    }

    // Service call with automatic retry
    public async Task<T> CallServiceAsync<T>(string endpoint, object data = null)
    {
        var maxRetries = 3;
        var delay = TimeSpan.FromSeconds(1);

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                HttpResponseMessage response;
                
                if (data != null)
                {
                    response = await _httpClient.PostAsJsonAsync(endpoint, data);
                }
                else
                {
                    response = await _httpClient.GetAsync(endpoint);
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await AuthenticateServiceAsync();
                    continue;
                }

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (HttpRequestException) when (i < maxRetries - 1)
            {
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); // Exponential backoff
            }
        }

        throw new InvalidOperationException($"Failed to call service endpoint {endpoint} after {maxRetries} retries");
    }
}
```

#### Dependency Injection Setup
```csharp
// Startup.cs or Program.cs
services.AddHttpClient<QaliTrackServiceClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => !msg.IsSuccessStatusCode)
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
```

### 2. User Context Forwarding

When the gateway forwards requests to downstream services, it adds user context headers:

```csharp
// Downstream service receives these headers
public class UserContextMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.Request.Headers["X-User-ID"].FirstOrDefault();
        var userName = context.Request.Headers["X-User-Name"].FirstOrDefault();
        var userEmail = context.Request.Headers["X-User-Email"].FirstOrDefault();
        var userRoles = context.Request.Headers["X-User-Roles"].FirstOrDefault()?.Split(',');
        var serviceName = context.Request.Headers["X-Service-Name"].FirstOrDefault();
        var isGatewayAuthorized = context.Request.Headers["X-Gateway-Authorized"].FirstOrDefault();

        if (isGatewayAuthorized == "true")
        {
            // Create user context for downstream service
            var userContext = new UserContext
            {
                UserId = userId,
                UserName = userName,
                Email = userEmail,
                Roles = userRoles?.ToList() ?? new List<string>(),
                ServiceName = serviceName
            };

            // Set user context for the request
            context.Items["UserContext"] = userContext;
        }

        await _next(context);
    }
}
```

## Client SDK Examples

### 1. JavaScript/TypeScript SDK

```typescript
// QaliTrack JavaScript SDK
export class QaliTrackSDK {
  private auth: QaliTrackAuth;
  private baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
    this.auth = new QaliTrackAuth(baseUrl);
  }

  // Authentication
  async login(username: string, password: string) {
    return this.auth.login(username, password);
  }

  async logout() {
    return this.auth.logout();
  }

  // Generic API call with automatic authentication
  private async apiCall<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    const headers = {
      'Content-Type': 'application/json',
      ...this.auth.getAuthHeaders(),
      ...options.headers
    };

    const response = await fetch(`${this.baseUrl}${endpoint}`, {
      ...options,
      headers
    });

    if (response.status === 401) {
      throw new Error('Authentication required');
    }

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'API call failed');
    }

    return response.json();
  }

  // Product management
  async getProducts(filters?: ProductFilters): Promise<Product[]> {
    const query = filters ? '?' + new URLSearchParams(filters).toString() : '';
    return this.apiCall<Product[]>(`/api/products${query}`);
  }

  async createProduct(product: CreateProductRequest): Promise<Product> {
    return this.apiCall<Product>('/api/products', {
      method: 'POST',
      body: JSON.stringify(product)
    });
  }

  async updateProduct(id: string, product: UpdateProductRequest): Promise<Product> {
    return this.apiCall<Product>(`/api/products/${id}`, {
      method: 'PUT',
      body: JSON.stringify(product)
    });
  }

  // Customer management
  async getCustomers(): Promise<Customer[]> {
    return this.apiCall<Customer[]>('/api/customers');
  }

  async createCustomer(customer: CreateCustomerRequest): Promise<Customer> {
    return this.apiCall<Customer>('/api/customers', {
      method: 'POST',
      body: JSON.stringify(customer)
    });
  }

  // Analytics
  async getSalesAnalytics(period: 'daily' | 'weekly' | 'monthly'): Promise<SalesAnalytics> {
    return this.apiCall<SalesAnalytics>(`/api/analytics/sales?period=${period}`);
  }

  // Health monitoring
  async getSystemHealth(): Promise<HealthStatus> {
    return this.apiCall<HealthStatus>('/health');
  }
}

// Usage example
const sdk = new QaliTrackSDK('https://localhost:7000');

// Login
await sdk.login('operator1', 'password123');

// Use API
const products = await sdk.getProducts({ category: 'cement' });
const newProduct = await sdk.createProduct({
  name: 'Premium Cement',
  category: 'Building Materials',
  unitPrice: 850.00
});
```

### 2. Python SDK

```python
import requests
from typing import Dict, List, Optional, Any
from datetime import datetime, timedelta
import json

class QaliTrackSDK:
    def __init__(self, base_url: str):
        self.base_url = base_url.rstrip('/')
        self.session = requests.Session()
        self.token = None
        self.token_expires_at = None
    
    def login(self, username: str, password: str) -> Dict[str, Any]:
        """Authenticate and store token"""
        response = self.session.post(
            f"{self.base_url}/api/auth/login",
            json={"username": username, "password": password}
        )
        response.raise_for_status()
        
        data = response.json()
        self.token = data["token"]
        self.token_expires_at = datetime.fromisoformat(data["expiresAt"].replace('Z', '+00:00'))
        
        # Set authorization header for future requests
        self.session.headers.update({
            'Authorization': f'Bearer {self.token}'
        })
        
        return data
    
    def mock_login(self, username: str, role: str) -> Dict[str, Any]:
        """Mock authentication for testing"""
        response = self.session.post(
            f"{self.base_url}/api/MockAuth/mock-login",
            json={"username": username, "role": role}
        )
        response.raise_for_status()
        
        data = response.json()
        self.token = data["token"]
        self.session.headers.update({
            'Authorization': f'Bearer {self.token}'
        })
        
        return data
    
    def _api_call(self, method: str, endpoint: str, **kwargs) -> Any:
        """Make authenticated API call with error handling"""
        url = f"{self.base_url}{endpoint}"
        
        try:
            response = self.session.request(method, url, **kwargs)
            
            if response.status_code == 401:
                raise AuthenticationError("Authentication required or token expired")
            
            response.raise_for_status()
            
            if response.headers.get('content-type', '').startswith('application/json'):
                return response.json()
            return response.text
            
        except requests.RequestException as e:
            raise APIError(f"API call failed: {str(e)}")
    
    # Product management
    def get_products(self, filters: Optional[Dict] = None) -> List[Dict]:
        params = filters or {}
        return self._api_call('GET', '/api/products', params=params)
    
    def create_product(self, product_data: Dict) -> Dict:
        return self._api_call('POST', '/api/products', json=product_data)
    
    def update_product(self, product_id: str, product_data: Dict) -> Dict:
        return self._api_call('PUT', f'/api/products/{product_id}', json=product_data)
    
    def delete_product(self, product_id: str) -> None:
        self._api_call('DELETE', f'/api/products/{product_id}')
    
    # Customer management
    def get_customers(self) -> List[Dict]:
        return self._api_call('GET', '/api/customers')
    
    def create_customer(self, customer_data: Dict) -> Dict:
        return self._api_call('POST', '/api/customers', json=customer_data)
    
    # Weight data
    def create_weight_entry(self, weight_data: Dict) -> Dict:
        return self._api_call('POST', '/api/weight-data', json=weight_data)
    
    def get_weight_data(self, filters: Optional[Dict] = None) -> List[Dict]:
        params = filters or {}
        return self._api_call('GET', '/api/weight-data', params=params)
    
    # Analytics
    def get_sales_analytics(self, period: str = 'monthly') -> Dict:
        return self._api_call('GET', f'/api/analytics/sales?period={period}')
    
    def get_compliance_report(self, start_date: str, end_date: str) -> Dict:
        params = {'startDate': start_date, 'endDate': end_date}
        return self._api_call('GET', '/api/compliance/reports', params=params)
    
    # System monitoring
    def get_health_status(self) -> Dict:
        return self._api_call('GET', '/health')
    
    def get_service_info(self) -> Dict:
        return self._api_call('GET', '/api/gateway/info')

class AuthenticationError(Exception):
    pass

class APIError(Exception):
    pass

# Usage example
sdk = QaliTrackSDK('https://localhost:7000')

# Authentication
sdk.login('operator1', 'password123')

# Or mock authentication for testing
# sdk.mock_login('testuser', 'Operator')

# Use API
products = sdk.get_products({'category': 'cement'})
new_product = sdk.create_product({
    'name': 'Premium Cement',
    'category': 'Building Materials',
    'unitPrice': 850.00
})

# Analytics
sales_data = sdk.get_sales_analytics('monthly')
```

### 3. C# SDK

```csharp
// QaliTrack C# SDK
public class QaliTrackSDK : IDisposable
{
    private readonly HttpClient _httpClient;
    private string _token;
    private DateTime _tokenExpiresAt;

    public QaliTrackSDK(string baseUrl)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "QaliTrack-SDK-CSharp/1.0");
    }

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        var loginRequest = new { username, password };
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        _token = result.Token;
        _tokenExpiresAt = result.ExpiresAt;

        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _token);

        return result;
    }

    public async Task<AuthResult> MockLoginAsync(string username, string role)
    {
        var mockRequest = new { username, role };
        var response = await _httpClient.PostAsJsonAsync("/api/MockAuth/mock-login", mockRequest);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        _token = result.Token;

        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _token);

        return result;
    }

    private async Task<T> ApiCallAsync<T>(string endpoint, HttpMethod method, object data = null)
    {
        using var request = new HttpRequestMessage(method, endpoint);
        
        if (data != null)
        {
            request.Content = JsonContent.Create(data);
        }

        var response = await _httpClient.SendAsync(request);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Authentication required or token expired");
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    // Product management
    public async Task<List<Product>> GetProductsAsync(ProductFilters filters = null)
    {
        var query = filters?.ToQueryString() ?? "";
        return await ApiCallAsync<List<Product>>($"/api/products{query}", HttpMethod.Get);
    }

    public async Task<Product> CreateProductAsync(CreateProductRequest product)
    {
        return await ApiCallAsync<Product>("/api/products", HttpMethod.Post, product);
    }

    public async Task<Product> UpdateProductAsync(string id, UpdateProductRequest product)
    {
        return await ApiCallAsync<Product>($"/api/products/{id}", HttpMethod.Put, product);
    }

    public async Task DeleteProductAsync(string id)
    {
        await ApiCallAsync<object>($"/api/products/{id}", HttpMethod.Delete);
    }

    // Customer management
    public async Task<List<Customer>> GetCustomersAsync()
    {
        return await ApiCallAsync<List<Customer>>("/api/customers", HttpMethod.Get);
    }

    public async Task<Customer> CreateCustomerAsync(CreateCustomerRequest customer)
    {
        return await ApiCallAsync<Customer>("/api/customers", HttpMethod.Post, customer);
    }

    // Weight data
    public async Task<WeightEntry> CreateWeightEntryAsync(CreateWeightEntryRequest weightData)
    {
        return await ApiCallAsync<WeightEntry>("/api/weight-data", HttpMethod.Post, weightData);
    }

    // Analytics
    public async Task<SalesAnalytics> GetSalesAnalyticsAsync(string period = "monthly")
    {
        return await ApiCallAsync<SalesAnalytics>($"/api/analytics/sales?period={period}", HttpMethod.Get);
    }

    // System monitoring
    public async Task<HealthStatus> GetHealthStatusAsync()
    {
        return await ApiCallAsync<HealthStatus>("/health", HttpMethod.Get);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

// Usage example
using var sdk = new QaliTrackSDK("https://localhost:7000");

// Authentication
await sdk.LoginAsync("operator1", "password123");

// Use API
var products = await sdk.GetProductsAsync(new ProductFilters { Category = "cement" });
var newProduct = await sdk.CreateProductAsync(new CreateProductRequest
{
    Name = "Premium Cement",
    Category = "Building Materials",
    UnitPrice = 850.00m
});
```

## Error Handling Patterns

### 1. Standard Error Response Format

All APIs return errors in a consistent format:

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error message",
    "details": {
      "field": "Additional error details"
    },
    "timestamp": "2025-07-08T10:30:00Z",
    "traceId": "unique-trace-id"
  }
}
```

### 2. Client Error Handling

```typescript
class APIErrorHandler {
  static handle(error: any): never {
    if (error.response) {
      const { status, data } = error.response;
      
      switch (status) {
        case 400:
          throw new ValidationError(data.error.message, data.error.details);
        case 401:
          throw new AuthenticationError(data.error.message);
        case 403:
          throw new AuthorizationError(data.error.message);
        case 404:
          throw new NotFoundError(data.error.message);
        case 429:
          throw new RateLimitError(data.error.message, data.error.details.retryAfter);
        case 500:
          throw new ServerError(data.error.message);
        case 502:
          throw new ServiceUnavailableError(data.error.message);
        default:
          throw new APIError(`Unexpected error: ${data.error.message}`);
      }
    }
    
    throw new NetworkError('Network error occurred');
  }
}

// Usage with retry logic
class ResilientAPIClient {
  async callWithRetry<T>(apiCall: () => Promise<T>, maxRetries = 3): Promise<T> {
    let lastError: Error;
    
    for (let attempt = 1; attempt <= maxRetries; attempt++) {
      try {
        return await apiCall();
      } catch (error) {
        lastError = error;
        
        // Don't retry client errors (4xx) except 429
        if (error instanceof ValidationError || 
            error instanceof AuthorizationError || 
            error instanceof NotFoundError) {
          throw error;
        }
        
        // Retry server errors and rate limits
        if (attempt < maxRetries) {
          const delay = this.calculateBackoff(attempt);
          await this.sleep(delay);
        }
      }
    }
    
    throw lastError;
  }
  
  private calculateBackoff(attempt: number): number {
    return Math.min(1000 * Math.pow(2, attempt - 1), 10000); // Max 10 seconds
  }
  
  private sleep(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }
}
```

## Testing Integration

### 1. Mock Service Testing

```typescript
// Test configuration for mock services
const testConfig = {
  gatewayUrl: 'http://localhost:7000',
  useMockServices: true,
  mockRoles: ['Guest', 'User', 'Operator', 'Admin', 'SuperAdmin']
};

describe('QaliTrack Gateway Integration Tests', () => {
  let sdk: QaliTrackSDK;
  
  beforeEach(() => {
    sdk = new QaliTrackSDK(testConfig.gatewayUrl);
  });

  describe('Authentication', () => {
    test('should authenticate with mock service', async () => {
      const result = await sdk.mockLogin('testuser', 'Operator');
      
      expect(result.token).toBeDefined();
      expect(result.user.role).toBe('Operator');
    });

    test('should handle all role types', async () => {
      for (const role of testConfig.mockRoles) {
        const result = await sdk.mockLogin(`test-${role}`, role);
        expect(result.user.role).toBe(role);
      }
    });
  });

  describe('Authorization Matrix', () => {
    test('User role should access products', async () => {
      await sdk.mockLogin('testuser', 'User');
      const products = await sdk.getProducts();
      expect(Array.isArray(products)).toBe(true);
    });

    test('Guest role should not access customers', async () => {
      await sdk.mockLogin('testguest', 'Guest');
      await expect(sdk.getCustomers()).rejects.toThrow('403');
    });

    test('Operator role should access products and customers', async () => {
      await sdk.mockLogin('testoperator', 'Operator');
      
      const products = await sdk.getProducts();
      const customers = await sdk.getCustomers();
      
      expect(Array.isArray(products)).toBe(true);
      expect(Array.isArray(customers)).toBe(true);
    });
  });
});
```

### 2. Integration Test Framework

```csharp
// C# integration test framework
[TestClass]
public class GatewayIntegrationTests
{
    private QaliTrackSDK _sdk;
    private string _gatewayUrl = "http://localhost:7000";

    [TestInitialize]
    public void Setup()
    {
        _sdk = new QaliTrackSDK(_gatewayUrl);
    }

    [TestMethod]
    public async Task TestRoleBasedAccess()
    {
        // Test different roles and their access patterns
        var testCases = new[]
        {
            new { Role = "User", CanAccessProducts = true, CanAccessCustomers = false },
            new { Role = "Operator", CanAccessProducts = true, CanAccessCustomers = true },
            new { Role = "Admin", CanAccessProducts = true, CanAccessCustomers = true }
        };

        foreach (var testCase in testCases)
        {
            await _sdk.MockLoginAsync($"test-{testCase.Role}", testCase.Role);

            // Test product access
            if (testCase.CanAccessProducts)
            {
                var products = await _sdk.GetProductsAsync();
                Assert.IsNotNull(products);
            }

            // Test customer access
            if (testCase.CanAccessCustomers)
            {
                var customers = await _sdk.GetCustomersAsync();
                Assert.IsNotNull(customers);
            }
            else
            {
                await Assert.ThrowsExceptionAsync<HttpRequestException>(() => 
                    _sdk.GetCustomersAsync());
            }
        }
    }

    [TestMethod]
    public async Task TestServiceHealthMonitoring()
    {
        var health = await _sdk.GetHealthStatusAsync();
        
        Assert.AreEqual("Healthy", health.Status);
        Assert.IsTrue(health.Entries.ContainsKey("gateway"));
    }

    [TestCleanup]
    public void Cleanup()
    {
        _sdk?.Dispose();
    }
}
```

## Production Deployment

### 1. Environment Configuration

```yaml
# Production deployment configuration
production:
  gateway:
    url: "https://api.qalitrack.com"
    timeout: 30
    retries: 3
    
  authentication:
    issuer: "UserService"
    audience: "UserService"
    
  rate_limiting:
    requests_per_minute: 1000
    burst_limit: 200
    
  monitoring:
    health_check_interval: 30
    log_level: "Information"
```

### 2. Client Configuration Best Practices

```typescript
// Production client configuration
class ProductionQaliTrackSDK extends QaliTrackSDK {
  constructor() {
    super(process.env.QALITRACK_GATEWAY_URL);
    
    // Production-specific configuration
    this.configure({
      timeout: 30000,
      retries: 3,
      rateLimiting: {
        maxRequestsPerMinute: 100,
        respectRetryAfter: true
      },
      logging: {
        level: 'info',
        enableMetrics: true
      }
    });
  }

  // Override for production error handling
  protected handleError(error: any): void {
    // Log to monitoring service
    this.logger.error('API Error', {
      error: error.message,
      endpoint: error.endpoint,
      statusCode: error.statusCode,
      timestamp: new Date().toISOString()
    });

    // Send metrics to monitoring
    this.metrics.increment('api.errors', {
      service: 'qalitrack-gateway',
      error_type: error.type
    });

    super.handleError(error);
  }
}
```

### 3. Security Best Practices

```typescript
// Secure token storage and management
class SecureTokenManager {
  private static readonly TOKEN_KEY = 'qalitrack_token';
  private static readonly REFRESH_THRESHOLD = 5 * 60 * 1000; // 5 minutes

  static storeToken(token: string, expiresAt: Date): void {
    // Use secure storage in production
    if (typeof window !== 'undefined') {
      // Browser environment - use secure cookie or sessionStorage
      sessionStorage.setItem(this.TOKEN_KEY, token);
    } else {
      // Node.js environment - use secure key store
      process.env.QALITRACK_TOKEN = token;
    }
  }

  static getToken(): string | null {
    if (this.isTokenExpiringSoon()) {
      // Proactively refresh token
      this.refreshToken();
    }

    if (typeof window !== 'undefined') {
      return sessionStorage.getItem(this.TOKEN_KEY);
    } else {
      return process.env.QALITRACK_TOKEN || null;
    }
  }

  private static isTokenExpiringSoon(): boolean {
    // Implementation to check token expiration
    // Return true if token expires within REFRESH_THRESHOLD
    return false;
  }

  private static async refreshToken(): Promise<void> {
    // Implementation to refresh token before expiration
  }
}
```

---

*This integration guide provides comprehensive patterns and examples for successfully integrating with the QaliTrack API Gateway across different technology stacks and deployment scenarios.*