const express = require('express');
const app = express();
const port = process.env.PORT || 3001;
const serviceName = process.env.SERVICE_NAME || 'sample-service';

app.use(express.json());

// Health check endpoint
app.get('/health', (req, res) => {
  res.status(200).json({
    status: 'healthy',
    service: serviceName,
    timestamp: new Date().toISOString(),
    uptime: process.uptime()
  });
});

// Sample API endpoints
app.get('/api/items', (req, res) => {
  res.json([
    { id: 1, name: 'Item 1', description: 'Sample item 1' },
    { id: 2, name: 'Item 2', description: 'Sample item 2' }
  ]);
});

app.get('/api/items/:id', (req, res) => {
  const id = parseInt(req.params.id);
  res.json({
    id: id,
    name: `Item ${id}`,
    description: `Sample item ${id}`
  });
});

app.post('/api/items', (req, res) => {
  const newItem = {
    id: Math.floor(Math.random() * 1000),
    ...req.body
  };
  res.status(201).json(newItem);
});

// Swagger specification endpoint
app.get('/swagger/v1/swagger.json', (req, res) => {
  const swaggerSpec = {
    "openapi": "3.0.1",
    "info": {
      "title": `${serviceName.charAt(0).toUpperCase() + serviceName.slice(1)} API`,
      "description": `API for ${serviceName}`,
      "version": "1.0.0"
    },
    "paths": {
      "/health": {
        "get": {
          "tags": [`${serviceName}`],
          "summary": "Health check",
          "responses": {
            "200": {
              "description": "Service is healthy",
              "content": {
                "application/json": {
                  "schema": {
                    "$ref": "#/components/schemas/HealthResponse"
                  }
                }
              }
            }
          }
        }
      },
      "/api/items": {
        "get": {
          "tags": [`${serviceName}`],
          "summary": "Get all items",
          "responses": {
            "200": {
              "description": "List of items",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "array",
                    "items": {
                      "$ref": "#/components/schemas/Item"
                    }
                  }
                }
              }
            }
          }
        },
        "post": {
          "tags": [`${serviceName}`],
          "summary": "Create a new item",
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/CreateItemRequest"
                }
              }
            }
          },
          "responses": {
            "201": {
              "description": "Item created",
              "content": {
                "application/json": {
                  "schema": {
                    "$ref": "#/components/schemas/Item"
                  }
                }
              }
            }
          }
        }
      },
      "/api/items/{id}": {
        "get": {
          "tags": [`${serviceName}`],
          "summary": "Get item by ID",
          "parameters": [
            {
              "name": "id",
              "in": "path",
              "required": true,
              "schema": {
                "type": "integer"
              }
            }
          ],
          "responses": {
            "200": {
              "description": "Item details",
              "content": {
                "application/json": {
                  "schema": {
                    "$ref": "#/components/schemas/Item"
                  }
                }
              }
            }
          }
        }
      }
    },
    "components": {
      "schemas": {
        "HealthResponse": {
          "type": "object",
          "properties": {
            "status": {
              "type": "string",
              "example": "healthy"
            },
            "service": {
              "type": "string",
              "example": serviceName
            },
            "timestamp": {
              "type": "string",
              "format": "date-time"
            },
            "uptime": {
              "type": "number",
              "example": 123.45
            }
          }
        },
        "Item": {
          "type": "object",
          "properties": {
            "id": {
              "type": "integer",
              "example": 1
            },
            "name": {
              "type": "string",
              "example": "Item 1"
            },
            "description": {
              "type": "string",
              "example": "Sample item 1"
            }
          }
        },
        "CreateItemRequest": {
          "type": "object",
          "properties": {
            "name": {
              "type": "string",
              "example": "New Item"
            },
            "description": {
              "type": "string",
              "example": "New item description"
            }
          },
          "required": ["name"]
        }
      }
    }
  };
  
  res.json(swaggerSpec);
});

app.listen(port, '0.0.0.0', () => {
  console.log(`${serviceName} running on port ${port}`);
});