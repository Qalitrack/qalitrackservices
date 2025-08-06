#!/bin/bash

# S/4HANA Mock Server Startup Script
# QaliTrack Systems - Bamburi Cement Integration Development

set -e

echo "🚀 Starting S/4HANA Mock Server for Bamburi Cement Integration..."
echo "=================================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}❌ Docker is not running. Please start Docker and try again.${NC}"
    exit 1
fi

# Create network if it doesn't exist
echo -e "${BLUE}📡 Setting up Docker network...${NC}"
docker network create qalitrack-network 2>/dev/null || echo -e "${YELLOW}⚠️  Network already exists${NC}"

# Check if we're in development or production mode
ENVIRONMENT=${ENVIRONMENT:-development}
echo -e "${BLUE}🔧 Environment: ${ENVIRONMENT}${NC}"

# Install npm dependencies if node_modules doesn't exist
if [ ! -d "node_modules" ]; then
    echo -e "${YELLOW}📦 Installing npm dependencies...${NC}"
    npm install
fi

# Build Docker image
echo -e "${BLUE}🏗️  Building S/4HANA Mock Server Docker image...${NC}"
docker build -t qalitrack/s4hana-mock-server:latest .

# Start services
echo -e "${BLUE}🚀 Starting S/4HANA Mock Server services...${NC}"
docker-compose -f docker-compose.s4hana-mock.yml up -d

# Wait for services to be ready
echo -e "${YELLOW}⏳ Waiting for services to be ready...${NC}"
sleep 10

# Health check
echo -e "${BLUE}🏥 Checking service health...${NC}"
for i in {1..30}; do
    if curl -f http://localhost:8080/health > /dev/null 2>&1; then
        echo -e "${GREEN}✅ S/4HANA Mock Server is healthy!${NC}"
        break
    elif [ $i -eq 30 ]; then
        echo -e "${RED}❌ Health check failed after 30 attempts${NC}"
        echo -e "${YELLOW}📋 Checking container logs:${NC}"
        docker-compose -f docker-compose.s4hana-mock.yml logs --tail=20
        exit 1
    else
        echo -e "${YELLOW}⏳ Attempt $i/30 - waiting for health check...${NC}"
        sleep 2
    fi
done

# Display service information
echo ""
echo -e "${GREEN}🎉 S/4HANA Mock Server is now running!${NC}"
echo "=================================================="
echo -e "${BLUE}📊 Service Status:${NC}"
echo -e "   Mock Server: ${GREEN}http://localhost:8080${NC}"
echo -e "   Redis Cache: ${GREEN}http://localhost:6379${NC}"
echo ""
echo -e "${BLUE}🔗 API Endpoints:${NC}"
echo -e "   Sales Order API: ${BLUE}http://localhost:8080/sap/opu/odata4/sap/api_salesorder/srvd_a2x/sap/salesorder/0001${NC}"
echo -e "   Business Partner API: ${BLUE}http://localhost:8080/sap/opu/odata/sap/API_BUSINESS_PARTNER${NC}"
echo -e "   Material API: ${BLUE}http://localhost:8080/sap/opu/odata/sap/API_MATERIAL${NC}"
echo -e "   Plant API: ${BLUE}http://localhost:8080/sap/opu/odata/sap/API_PLANT${NC}"
echo ""
echo -e "${BLUE}📋 Management Commands:${NC}"
echo -e "   View logs: ${YELLOW}docker-compose -f docker-compose.s4hana-mock.yml logs -f${NC}"
echo -e "   Stop services: ${YELLOW}docker-compose -f docker-compose.s4hana-mock.yml down${NC}"
echo -e "   Restart services: ${YELLOW}docker-compose -f docker-compose.s4hana-mock.yml restart${NC}"
echo ""
echo -e "${BLUE}🏢 Bamburi Cement Mock Data:${NC}"
echo -e "   Plants: ${GREEN}Mombasa (1000), Athi River (1100), Nairobi Grinding (1200)${NC}"
echo -e "   Customers: ${GREEN}CRBC, NHWD, John Mwangi Construction${NC}"
echo -e "   Products: ${GREEN}PowerMax, Nguvu, Fundi, BamburiBlox, Readymix${NC}"
echo -e "   Sample Orders: ${GREEN}16.675M KES, 2.56M KES, 75K KES${NC}"
echo ""
echo -e "${GREEN}✨ Ready for Bamburi Cement integration development!${NC}"