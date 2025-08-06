#!/bin/bash

# Bamburi Cement S/4HANA API Setup Script
# This script tests API connectivity and creates sample data

echo "🏭 Bamburi Cement S/4HANA API Setup"
echo "===================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Check if Node.js is installed
if ! command -v node &> /dev/null; then
    echo -e "${RED}❌ Node.js is not installed${NC}"
    echo "Please install Node.js from https://nodejs.org/"
    exit 1
fi

echo -e "${GREEN}✅ Node.js found: $(node --version)${NC}"
echo ""

# Step 1: Test API Connectivity
echo -e "${BLUE}Step 1: Testing S/4HANA API Connectivity...${NC}"
echo "----------------------------------------"

node test-s4hana-api.js

API_TEST_EXIT_CODE=$?

echo ""

if [ $API_TEST_EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}✅ API connectivity test completed${NC}"
    echo ""
    
    # Ask user if they want to proceed with data creation
    echo -e "${YELLOW}Do you want to proceed with creating Bamburi Cement data via API? (y/N)${NC}"
    read -r response
    
    if [[ "$response" =~ ^([yY][eE][sS]|[yY])$ ]]; then
        echo ""
        echo -e "${BLUE}Step 2: Creating Bamburi Cement Data...${NC}"
        echo "--------------------------------------"
        
        node s4hana-api-data-creator.js
        
        DATA_CREATION_EXIT_CODE=$?
        
        if [ $DATA_CREATION_EXIT_CODE -eq 0 ]; then
            echo ""
            echo -e "${GREEN}🎉 Bamburi Cement data creation completed successfully!${NC}"
            echo ""
            echo "Next Steps:"
            echo "1. Log into your S/4HANA trial to verify the created data"
            echo "2. Check the generated 'bamburi-created-data.json' file"
            echo "3. Begin QaliTrack integration development"
        else
            echo ""
            echo -e "${RED}❌ Data creation encountered issues${NC}"
            echo "Check the output above for specific errors"
        fi
    else
        echo ""
        echo -e "${YELLOW}⏸️ Data creation skipped by user${NC}"
        echo "You can run data creation later with:"
        echo "  node s4hana-api-data-creator.js"
    fi
else
    echo -e "${RED}❌ API connectivity test failed${NC}"
    echo ""
    echo -e "${YELLOW}Alternative Options:${NC}"
    echo "1. Check your credentials and trial system status"
    echo "2. Try the Playwright automation approach:"
    echo "   cd /path/to/scripts"
    echo "   npm install"
    echo "   npx playwright install chromium"
    echo "   npm run setup"
    echo ""
    echo "3. Create data manually using the frontend guide in GitHub Issue #6"
fi

echo ""
echo -e "${BLUE}Script completed at $(date)${NC}"