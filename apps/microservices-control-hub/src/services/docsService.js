const fetch = require('node-fetch');
const ConfigService = require('./configService');

class DocsService {
  constructor() {
    this.configService = new ConfigService();
    this.timeout = 10000; // 10 second timeout for docs
  }

  async getAggregatedDocs() {
    const servers = this.configService.getServers();
    const applicationName = this.configService.getApplicationName();
    
    const aggregatedSwagger = {
      openapi: '3.0.1',
      info: {
        title: `${applicationName} API Documentation`,
        description: `Complete API documentation for all ${applicationName.toLowerCase()} microservices`,
        version: '1.0.0',
        contact: {
          name: `${applicationName} Control Hub`,
          url: 'https://github.com/microservices-control-hub'
        }
      },
      servers: servers,
      paths: {},
      components: {
        schemas: {},
        securitySchemes: {
          Bearer: {
            type: 'http',
            scheme: 'bearer',
            bearerFormat: 'JWT',
            description: 'JWT Authorization header using the Bearer scheme'
          }
        }
      },
      tags: []
    };

    const services = this.configService.getServicesWithDocumentation();
    
    // Process each service's swagger docs
    for (const service of services) {
      try {
        await this.mergeServiceSwagger(service, aggregatedSwagger);
      } catch (error) {
        console.warn(`⚠️  Failed to fetch docs for ${service.name}: ${error.message}`);
      }
    }

    return aggregatedSwagger;
  }

  async getServiceDocs(serviceName) {
    const service = this.configService.getService(serviceName);
    if (!service || !service.swaggerPath) {
      return null;
    }

    try {
      const baseUrl = this.configService.getServiceUrl(service);
      const url = `${baseUrl}${service.swaggerPath}`;
      
      // Validate URL before making request
      try {
        new URL(url);
      } catch (urlError) {
        throw new Error(`Invalid URL constructed: "${url}" - ${urlError.message}`);
      }
      
      const response = await fetch(url, {
        method: 'GET',
        timeout: this.timeout,
        headers: {
          'User-Agent': 'Microservices-Control-Hub/1.0',
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      return await response.json();
    } catch (error) {
      console.error(`Failed to fetch docs for ${serviceName}:`, error.message);
      return null;
    }
  }

  async getDocumentedServices() {
    const services = this.configService.getServicesWithDocumentation();
    const servicePromises = services.map(async (service) => {
      try {
        const docs = await this.getServiceDocs(service.name);
        return {
          name: service.name,
          title: this.formatServiceTitle(service.name),
          available: !!docs,
          description: service.description,
          group: service.group,
          apiRoot: service.apiRoot,
          pathCount: docs?.paths ? Object.keys(docs.paths).length : 0
        };
      } catch (error) {
        return {
          name: service.name,
          title: this.formatServiceTitle(service.name),
          available: false,
          description: service.description,
          group: service.group,
          apiRoot: service.apiRoot,
          pathCount: 0,
          error: error.message
        };
      }
    });

    return await Promise.all(servicePromises);
  }

  async getDocumentationSummary() {
    const services = await this.getDocumentedServices();
    const totalServices = services.length;
    const availableServices = services.filter(s => s.available).length;
    const totalPaths = services.reduce((sum, s) => sum + (s.pathCount || 0), 0);
    
    const groups = services.reduce((acc, service) => {
      const group = service.group || 'application';
      if (!acc[group]) {
        acc[group] = { services: 0, available: 0, paths: 0 };
      }
      acc[group].services++;
      if (service.available) {
        acc[group].available++;
      }
      acc[group].paths += service.pathCount || 0;
      return acc;
    }, {});

    return {
      total: {
        services: totalServices,
        available: availableServices,
        paths: totalPaths,
        coverage: totalServices > 0 ? Math.round((availableServices / totalServices) * 100) : 0
      },
      groups,
      services: services
    };
  }

  async mergeServiceSwagger(service, aggregatedSwagger) {
    const serviceDocs = await this.getServiceDocs(service.name);
    if (!serviceDocs) {
      return;
    }

    // Add service tag
    const pathCount = serviceDocs.paths ? Object.keys(serviceDocs.paths).length : 0;
    const serviceTag = {
      name: service.name,
      description: `${service.description || this.formatServiceTitle(service.name)} - ${pathCount} endpoints`
    };
    aggregatedSwagger.tags.push(serviceTag);

    // Create schema mapping for reference updates
    const schemaMapping = {};
    if (serviceDocs.components?.schemas) {
      for (const schemaName of Object.keys(serviceDocs.components.schemas)) {
        const prefixedSchemaName = `${service.name.replace(/-/g, '')}${schemaName}`;
        schemaMapping[schemaName] = prefixedSchemaName;
      }
    }

    // Process paths
    if (serviceDocs.paths) {
      for (const [path, pathObject] of Object.entries(serviceDocs.paths)) {
        // Apply path transformations (gateway routing adjustments)
        let finalPath = this.applyPathTransformations(path, service);
        
        // Apply API root prefix if configured (legacy support)
        if (service.apiRoot && !service.pathTransformations) {
          finalPath = service.apiRoot + path.replace(/^\/api/, '');
        }

        // Process each HTTP method in this path
        const processedPathObject = { ...pathObject };
        for (const [method, methodObject] of Object.entries(processedPathObject)) {
          if (typeof methodObject === 'object' && methodObject !== null) {
            // Replace tags with service name
            methodObject.tags = [service.name];
            
            // Add service info to operation
            if (!methodObject.description) {
              methodObject.description = `${this.formatServiceTitle(service.name)} endpoint`;
            }

            // Update schema references in the method object
            processedPathObject[method] = this.updateSchemaReferences(methodObject, schemaMapping);
          }
        }

        aggregatedSwagger.paths[finalPath] = processedPathObject;
      }
    }

    // Merge schemas with service prefix to avoid conflicts
    if (serviceDocs.components?.schemas) {
      for (const [schemaName, schemaObject] of Object.entries(serviceDocs.components.schemas)) {
        const prefixedSchemaName = `${service.name.replace(/-/g, '')}${schemaName}`;
        // Also update internal schema references within the schema object
        const updatedSchemaObject = this.updateSchemaReferences(schemaObject, schemaMapping);
        aggregatedSwagger.components.schemas[prefixedSchemaName] = updatedSchemaObject;
      }
    }
  }

  updateSchemaReferences(obj, schemaMapping) {
    if (!obj || typeof obj !== 'object') {
      return obj;
    }

    // Handle arrays
    if (Array.isArray(obj)) {
      return obj.map(item => this.updateSchemaReferences(item, schemaMapping));
    }

    // Handle objects - create deep copy
    const updated = JSON.parse(JSON.stringify(obj));
    this.updateSchemaReferencesInPlace(updated, schemaMapping);
    return updated;
  }

  updateSchemaReferencesInPlace(obj, schemaMapping) {
    if (!obj || typeof obj !== 'object') {
      return;
    }

    // Handle arrays
    if (Array.isArray(obj)) {
      obj.forEach(item => this.updateSchemaReferencesInPlace(item, schemaMapping));
      return;
    }

    // Handle objects
    for (const [key, value] of Object.entries(obj)) {
      if (key === '$ref' && typeof value === 'string' && value.startsWith('#/components/schemas/')) {
        // Extract schema name from reference
        const schemaName = value.replace('#/components/schemas/', '');
        if (schemaMapping[schemaName]) {
          obj[key] = `#/components/schemas/${schemaMapping[schemaName]}`;
        }
      } else if (typeof value === 'object') {
        // Recursively update nested objects
        this.updateSchemaReferencesInPlace(value, schemaMapping);
      }
    }
  }

  /**
   * Apply path transformations based on service configuration
   * Handles gateway routing transformations (strip/add path segments)
   */
  applyPathTransformations(originalPath, service) {
    if (!service.pathTransformations || !Array.isArray(service.pathTransformations)) {
      return originalPath;
    }

    let transformedPath = originalPath;

    // Apply each transformation in order
    for (const transformation of service.pathTransformations) {
      const { pattern, operation, replacement, description, except } = transformation;
      
      if (!pattern || !operation) {
        console.warn(`⚠️ Invalid path transformation for ${service.name}: missing pattern or operation`);
        continue;
      }

      // Check if path should be excluded from this transformation
      if (except && this.matchesExceptPatterns(transformedPath, except)) {
        continue;
      }

      try {
        switch (operation.toLowerCase()) {
          case 'strip':
            // Remove the pattern from the beginning of the path
            if (transformedPath.startsWith(pattern)) {
              transformedPath = transformedPath.substring(pattern.length);
              // Ensure path starts with /
              if (!transformedPath.startsWith('/')) {
                transformedPath = '/' + transformedPath;
              }
            }
            break;

          case 'add':
            // Add replacement pattern to the beginning of matching paths
            if (transformedPath.startsWith(pattern)) {
              const replacementPrefix = replacement || '';
              // Remove the original pattern and add the replacement
              const remainingPath = transformedPath.substring(pattern.length);
              transformedPath = replacementPrefix + (remainingPath.startsWith('/') ? remainingPath : '/' + remainingPath);
            }
            break;

          case 'replace':
            // Replace pattern with replacement
            const replacementValue = replacement || '';
            if (transformedPath.startsWith(pattern)) {
              transformedPath = transformedPath.replace(pattern, replacementValue);
            }
            break;

          case 'strip-all-except':
            // Strip pattern from all paths except those matching 'except' patterns
            // This is handled by the except logic above combined with strip
            if (transformedPath.startsWith(pattern)) {
              transformedPath = transformedPath.substring(pattern.length);
              if (!transformedPath.startsWith('/')) {
                transformedPath = '/' + transformedPath;
              }
            }
            break;

          case 'add-all-except':
            // Add replacement to all paths except those matching 'except' patterns
            if (!transformedPath.startsWith(replacement || '')) {
              const replacementPrefix = replacement || '';
              transformedPath = replacementPrefix + transformedPath;
            }
            break;

          default:
            console.warn(`⚠️ Unknown path transformation operation: ${operation} for service ${service.name}`);
        }
      } catch (error) {
        console.error(`❌ Error applying path transformation for ${service.name}:`, error.message);
      }
    }

    return transformedPath;
  }

  /**
   * Check if a path matches any of the except patterns
   * Supports both string patterns and arrays of patterns
   */
  matchesExceptPatterns(path, except) {
    if (!except) {
      return false;
    }

    // Handle array of patterns
    if (Array.isArray(except)) {
      return except.some(pattern => path.startsWith(pattern));
    }

    // Handle single pattern string
    if (typeof except === 'string') {
      return path.startsWith(except);
    }

    return false;
  }

  formatServiceTitle(serviceName) {
    return serviceName
      .split('-')
      .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
      .join(' ');
  }
}

module.exports = DocsService;