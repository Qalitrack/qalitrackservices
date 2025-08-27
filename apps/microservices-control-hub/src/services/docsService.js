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
        const pathCount = docs?.paths ? Object.keys(docs.paths).length : 0;
        
        let modules = null;
        if (service.modules && Array.isArray(service.modules) && service.modules.length > 0) {
          const moduleEndpointCounts = this.countModuleEndpoints(docs?.paths, service.modules);
          modules = service.modules.map(module => ({
            name: module.name,
            description: module.description,
            path: module.path,
            endpointCount: moduleEndpointCounts[module.name] || 0
          }));
        }
        
        return {
          name: service.name,
          title: this.formatServiceTitle(service.name),
          available: !!docs,
          description: service.description,
          group: service.group,
          apiRoot: service.apiRoot,
          pathCount,
          modules,
          isModular: !!modules
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
          modules: null,
          isModular: false,
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

    const pathCount = serviceDocs.paths ? Object.keys(serviceDocs.paths).length : 0;

    // Check if service should use modular grouping (either has modules config OR has multiple tags in swagger)
    const serviceTags = this.extractServiceTags(serviceDocs);
    const shouldUseModularGrouping = service.modules || serviceTags.length > 1;

    if (shouldUseModularGrouping && serviceTags.length > 1) {
      // Use existing swagger tags for modular grouping
      const tagEndpointCounts = this.countEndpointsByTag(serviceDocs.paths);
      
      serviceTags.forEach(tagName => {
        // Skip generic API tags like "QaliTrack.MasterData.Api"
        if (this.isGenericTag(tagName)) {
          return;
        }
        
        const endpointCount = tagEndpointCounts[tagName] || 0;
        if (endpointCount > 0) {
          const moduleTag = {
            name: `${service.name}-${this.normalizeTagName(tagName)}`,
            description: `${tagName} - ${endpointCount} endpoints`
          };
          aggregatedSwagger.tags.push(moduleTag);
        }
      });
    } else {
      // Add traditional single service tag
      const serviceTag = {
        name: service.name,
        description: `${service.description || this.formatServiceTitle(service.name)} - ${pathCount} endpoints`
      };
      aggregatedSwagger.tags.push(serviceTag);
    }

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
            // Use existing tags or determine appropriate tag
            const originalTags = methodObject.tags || [];
            const serviceTags = this.extractServiceTags(serviceDocs);
            
            if (serviceTags.length > 1 && originalTags.length > 0) {
              // Use modular tags: replace original tag with service-prefixed version
              const originalTag = originalTags[0];
              if (!this.isGenericTag(originalTag)) {
                methodObject.tags = [`${service.name}-${this.normalizeTagName(originalTag)}`];
              } else {
                methodObject.tags = [service.name];
              }
            } else {
              // Use service name as tag
              methodObject.tags = [service.name];
            }
            
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

  /**
   * Count endpoints for each module based on path patterns
   */
  countModuleEndpoints(paths, modules) {
    const counts = {};
    
    if (!paths || !modules) {
      return counts;
    }

    // Initialize counts
    modules.forEach(module => {
      counts[module.name] = 0;
    });

    // Count endpoints for each path
    Object.keys(paths).forEach(path => {
      const matchedModule = this.findMatchingModule(path, modules);
      if (matchedModule) {
        counts[matchedModule.name]++;
      }
    });

    return counts;
  }

  /**
   * Find which module a path belongs to based on module path patterns
   */
  findMatchingModule(path, modules) {
    if (!modules || !Array.isArray(modules)) {
      return null;
    }

    // Sort modules by path specificity (longer paths first for better matching)
    const sortedModules = modules.slice().sort((a, b) => b.path.length - a.path.length);

    // Find the first module whose path matches the beginning of the endpoint path
    return sortedModules.find(module => {
      if (module.path) {
        return path.startsWith(module.path);
      }
      return false;
    });
  }

  /**
   * Get the appropriate tag name for an endpoint (either module tag or service tag)
   */
  getEndpointTag(path, service) {
    // If service has modules, try to match endpoint to a module
    if (service.modules && Array.isArray(service.modules) && service.modules.length > 0) {
      const matchedModule = this.findMatchingModule(path, service.modules);
      if (matchedModule) {
        return `${service.name}-${matchedModule.name}`;
      }
    }
    
    // Fall back to service name if no module match
    return service.name;
  }

  /**
   * Extract unique tags from swagger documentation
   */
  extractServiceTags(serviceDocs) {
    if (!serviceDocs?.paths) {
      return [];
    }

    const tags = new Set();
    
    // Extract tags from all endpoints
    Object.values(serviceDocs.paths).forEach(pathObject => {
      Object.values(pathObject).forEach(methodObject => {
        if (methodObject?.tags && Array.isArray(methodObject.tags)) {
          methodObject.tags.forEach(tag => tags.add(tag));
        }
      });
    });

    return Array.from(tags);
  }

  /**
   * Count endpoints for each tag
   */
  countEndpointsByTag(paths) {
    const counts = {};
    
    if (!paths) {
      return counts;
    }

    Object.values(paths).forEach(pathObject => {
      Object.values(pathObject).forEach(methodObject => {
        if (methodObject?.tags && Array.isArray(methodObject.tags)) {
          methodObject.tags.forEach(tag => {
            counts[tag] = (counts[tag] || 0) + 1;
          });
        }
      });
    });

    return counts;
  }

  /**
   * Check if a tag is generic (should be skipped for modular grouping)
   */
  isGenericTag(tagName) {
    if (!tagName || typeof tagName !== 'string') {
      return true;
    }

    // Skip generic patterns like "ServiceName.Api", "Api", etc.
    const genericPatterns = [
      /\.Api$/i,
      /^Api$/i,
      /^[A-Za-z]+\.[A-Za-z]+\.Api$/i
    ];

    return genericPatterns.some(pattern => pattern.test(tagName));
  }

  /**
   * Normalize tag name for use in service-tag combination
   */
  normalizeTagName(tagName) {
    if (!tagName || typeof tagName !== 'string') {
      return 'unknown';
    }

    return tagName
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '');
  }

  formatServiceTitle(serviceName) {
    return serviceName
      .split('-')
      .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
      .join(' ');
  }
}

module.exports = DocsService;