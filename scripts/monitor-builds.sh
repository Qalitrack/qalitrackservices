#!/bin/bash

# QaliTrack Build Monitor
# Monitors GitHub Actions builds for all 14 services

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🚀 QaliTrack Services Build Monitor${NC}"
echo "================================================="

# Check if gh CLI is installed
if ! command -v gh &> /dev/null; then
    echo -e "${RED}❌ GitHub CLI (gh) is not installed${NC}"
    echo "Install it with: https://cli.github.com/"
    exit 1
fi

# Check if user is authenticated
if ! gh auth status &> /dev/null; then
    echo -e "${RED}❌ Not authenticated with GitHub CLI${NC}"
    echo "Run: gh auth login"
    exit 1
fi

# Define all services
MASTERDATA_SERVICES=(
    "customer-service"
    "driver-service" 
    "product-service"
    "report-service"
    "route-service"
    "sacco-service"
    "supplier-service"
    "transporter-service"
    "vehicle-service"
    "weighbridge-service"
)

DATAMANAGER_SERVICES=(
    "compliance-service"
    "operational-data-service"
    "transaction-service"
    "weight-data-service"
)

ALL_SERVICES=("${MASTERDATA_SERVICES[@]}" "${DATAMANAGER_SERVICES[@]}")

# Function to check workflow status
check_workflow_status() {
    local service=$1
    local workflow_name="Build $(echo $service | sed 's/-/ /g' | sed 's/\b\w/\U&/g')"
    
    echo -n "  $service: "
    
    # Get latest workflow run status
    status=$(gh run list --workflow="build-${service}.yml" --limit=1 --json status --jq '.[0].status' 2>/dev/null || echo "none")
    conclusion=$(gh run list --workflow="build-${service}.yml" --limit=1 --json conclusion --jq '.[0].conclusion' 2>/dev/null || echo "none")
    
    case "$status" in
        "completed")
            case "$conclusion" in
                "success")
                    echo -e "${GREEN}✅ SUCCESS${NC}"
                    ;;
                "failure")
                    echo -e "${RED}❌ FAILED${NC}"
                    ;;
                "cancelled")
                    echo -e "${YELLOW}⚠️  CANCELLED${NC}"
                    ;;
                *)
                    echo -e "${YELLOW}⚠️  $conclusion${NC}"
                    ;;
            esac
            ;;
        "in_progress")
            echo -e "${BLUE}🔄 RUNNING${NC}"
            ;;
        "queued")
            echo -e "${YELLOW}⏳ QUEUED${NC}"
            ;;
        "none")
            echo -e "${YELLOW}⚪ NOT RUN${NC}"
            ;;
        *)
            echo -e "${YELLOW}⚠️  $status${NC}"
            ;;
    esac
}

# Function to get failed workflow details
get_failure_details() {
    local service=$1
    echo -e "\n${RED}Failure details for $service:${NC}"
    
    run_id=$(gh run list --workflow="build-${service}.yml" --limit=1 --json databaseId --jq '.[0].databaseId' 2>/dev/null)
    if [ "$run_id" != "null" ] && [ -n "$run_id" ]; then
        gh run view $run_id --log-failed || echo "Could not fetch failure logs"
    else
        echo "No run found for $service"
    fi
}

# Function to trigger workflow
trigger_workflow() {
    local service=$1
    echo -e "Triggering workflow for ${BLUE}$service${NC}..."
    gh workflow run "build-${service}.yml" || echo -e "${RED}Failed to trigger $service${NC}"
}

# Main monitoring function
monitor_builds() {
    echo -e "\n${BLUE}📊 Current Build Status:${NC}"
    echo "------------------------"
    
    local failed_services=()
    local success_count=0
    local failed_count=0
    local running_count=0
    
    echo -e "\n${YELLOW}Masterdata Services:${NC}"
    for service in "${MASTERDATA_SERVICES[@]}"; do
        check_workflow_status "$service"
        
        status=$(gh run list --workflow="build-${service}.yml" --limit=1 --json status,conclusion --jq '.[0] | .status + ":" + (.conclusion // "none")' 2>/dev/null || echo "none:none")
        
        case "$status" in
            "completed:success")
                ((success_count++))
                ;;
            "completed:failure"|"completed:timed_out")
                ((failed_count++))
                failed_services+=("$service")
                ;;
            "in_progress:"*|"queued:"*)
                ((running_count++))
                ;;
        esac
    done
    
    echo -e "\n${YELLOW}Datamanager Services:${NC}"
    for service in "${DATAMANAGER_SERVICES[@]}"; do
        check_workflow_status "$service"
        
        status=$(gh run list --workflow="build-${service}.yml" --limit=1 --json status,conclusion --jq '.[0] | .status + ":" + (.conclusion // "none")' 2>/dev/null || echo "none:none")
        
        case "$status" in
            "completed:success")
                ((success_count++))
                ;;
            "completed:failure"|"completed:timed_out")
                ((failed_count++))
                failed_services+=("$service")
                ;;
            "in_progress:"*|"queued:"*)
                ((running_count++))
                ;;
        esac
    done
    
    # Summary
    echo -e "\n${BLUE}📈 Summary:${NC}"
    echo "==========="
    echo -e "${GREEN}✅ Successful: $success_count${NC}"
    echo -e "${RED}❌ Failed: $failed_count${NC}"
    echo -e "${BLUE}🔄 Running: $running_count${NC}"
    echo -e "Total Services: ${#ALL_SERVICES[@]}"
    
    # Show failed services
    if [ ${#failed_services[@]} -gt 0 ]; then
        echo -e "\n${RED}❌ Failed Services:${NC}"
        for service in "${failed_services[@]}"; do
            echo "  - $service"
        done
        
        echo -e "\n${YELLOW}Get detailed failure info with:${NC}"
        echo "./scripts/monitor-builds.sh --failures"
    fi
}

# Function to show failures
show_failures() {
    echo -e "${RED}🔍 Detailed Failure Analysis:${NC}"
    echo "============================="
    
    for service in "${ALL_SERVICES[@]}"; do
        status=$(gh run list --workflow="build-${service}.yml" --limit=1 --json conclusion --jq '.[0].conclusion' 2>/dev/null || echo "none")
        if [ "$status" = "failure" ]; then
            get_failure_details "$service"
            echo -e "\n---\n"
        fi
    done
}

# Function to trigger all builds
trigger_all() {
    echo -e "${BLUE}🚀 Triggering all service builds...${NC}"
    echo "==================================="
    
    for service in "${ALL_SERVICES[@]}"; do
        trigger_workflow "$service"
        sleep 2  # Rate limiting
    done
    
    echo -e "\n${GREEN}✅ All workflows triggered!${NC}"
    echo "Monitor progress with: ./scripts/monitor-builds.sh"
}

# Command line options
case "${1:-}" in
    "--failures"|"-f")
        show_failures
        ;;
    "--trigger-all"|"-t")
        trigger_all
        ;;
    "--help"|"-h")
        echo "QaliTrack Build Monitor"
        echo ""
        echo "Usage: $0 [OPTION]"
        echo ""
        echo "Options:"
        echo "  (none)         Monitor current build status"
        echo "  -f, --failures Show detailed failure information"
        echo "  -t, --trigger-all Trigger all service builds"
        echo "  -h, --help     Show this help message"
        ;;
    *)
        monitor_builds
        ;;
esac