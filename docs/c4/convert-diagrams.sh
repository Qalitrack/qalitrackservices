#!/bin/bash

# Script to convert Mermaid diagrams to PNG
# Prerequisites: npm install -g @mermaid-js/mermaid-cli

echo "🔄 Converting C4 Mermaid diagrams to PNG..."

# Create assets directory if it doesn't exist
mkdir -p assets

# Check if mermaid CLI is installed
if ! command -v mmdc &> /dev/null; then
    echo "❌ Mermaid CLI not found. Install with: npm install -g @mermaid-js/mermaid-cli"
    exit 1
fi

# Convert system context diagrams
echo "📊 Converting System Context diagrams..."
mmdc -i system-context-c4.mmd -o assets/system-context-c4.png -t default -b white
mmdc -i system-context-flowchart.mmd -o assets/system-context-flowchart.png -t default -b white -s 3

# Convert container architecture diagrams
echo "📦 Converting Container Architecture diagrams..."
mmdc -i container-architecture.mmd -o assets/container-architecture.png -t default -b white

# Convert component flows diagrams
echo "🔄 Converting Component Flows diagrams..."
mmdc -i auth-authorization-flow.mmd -o assets/auth-authorization-flow.png -t default -b white
mmdc -i complete-weighing-transaction-flow.mmd -o assets/complete-weighing-transaction-flow.png -t default -b white
mmdc -i customer-order-processing-flow.mmd -o assets/customer-order-processing-flow.png -t default -b white
mmdc -i vehicle-driver-assignment-flow.mmd -o assets/vehicle-driver-assignment-flow.png -t default -b white
mmdc -i weight-data-processing-pipeline.mmd -o assets/weight-data-processing-pipeline.png -t default -b white
mmdc -i event-driven-architecture-components.mmd -o assets/event-driven-architecture-components.png -t default -b white
mmdc -i rbac-flow.mmd -o assets/rbac-flow.png -t default -b white
mmdc -i multi-service-transaction-pattern.mmd -o assets/multi-service-transaction-pattern.png -t default -b white
mmdc -i event-sourcing-pattern.mmd -o assets/event-sourcing-pattern.png -t default -b white
mmdc -i service-discovery-health-monitoring.mmd -o assets/service-discovery-health-monitoring.png -t default -b white
mmdc -i circuit-breaker-pattern.mmd -o assets/circuit-breaker-pattern.png -t default -b white
mmdc -i retry-timeout-strategy.mmd -o assets/retry-timeout-strategy.png -t default -b white

# Convert service architectures diagrams
echo "⚙️ Converting Service Architectures diagrams..."
mmdc -i user-service-architecture.mmd -o assets/user-service-architecture.png -t default -b white
mmdc -i customer-service-architecture.mmd -o assets/customer-service-architecture.png -t default -b white
mmdc -i weight-data-service-architecture.mmd -o assets/weight-data-service-architecture.png -t default -b white
mmdc -i transaction-service-architecture.mmd -o assets/transaction-service-architecture.png -t default -b white
mmdc -i repository-pattern-implementation.mmd -o assets/repository-pattern-implementation.png -t default -b white
mmdc -i event-publishing-architecture.mmd -o assets/event-publishing-architecture.png -t default -b white
mmdc -i service-client-pattern.mmd -o assets/service-client-pattern.png -t default -b white
mmdc -i jwt-token-service-architecture.mmd -o assets/jwt-token-service-architecture.png -t default -b white
mmdc -i authorization-middleware-architecture.mmd -o assets/authorization-middleware-architecture.png -t default -b white
mmdc -i database-context-architecture.mmd -o assets/database-context-architecture.png -t default -b white

# Convert business processes diagrams
echo "📈 Converting Business Processes diagrams..."
mmdc -i complete-weighing-transaction-process.mmd -o assets/complete-weighing-transaction-process.png -t default -b white
mmdc -i vehicle-registration-validation-workflow.mmd -o assets/vehicle-registration-validation-workflow.png -t default -b white
mmdc -i customer-order-processing-workflow.mmd -o assets/customer-order-processing-workflow.png -t default -b white
mmdc -i real-time-weight-processing-workflow.mmd -o assets/real-time-weight-processing-workflow.png -t default -b white
mmdc -i organization-specific-workflows.mmd -o assets/organization-specific-workflows.png -t default -b white
mmdc -i cross-organization-collaboration-flow.mmd -o assets/cross-organization-collaboration-flow.png -t default -b white
mmdc -i real-time-performance-monitoring.mmd -o assets/real-time-performance-monitoring.png -t default -b white
mmdc -i compliance-monitoring-reporting-process.mmd -o assets/compliance-monitoring-reporting-process.png -t default -b white
mmdc -i failover-recovery-process.mmd -o assets/failover-recovery-process.png -t default -b white
mmdc -i daily-operations-checklist.mmd -o assets/daily-operations-checklist.png -t default -b white
mmdc -i monthly-business-review-process.mmd -o assets/monthly-business-review-process.png -t default -b white

# Convert onboarding guide diagrams
echo "🚀 Converting Onboarding Guide diagrams..."
mmdc -i system-at-a-glance.mmd -o assets/system-at-a-glance.png -t default -b white
mmdc -i daily-development-process.mmd -o assets/daily-development-process.png -t default -b white
mmdc -i service-discovery-pattern.mmd -o assets/service-discovery-pattern.png -t default -b white

echo "✅ Conversion complete!"
echo "📁 PNG files saved in assets/ directory:"
ls -la assets/*.png

echo ""
echo "🌐 Alternative: Use https://mermaid.live/ for online conversion"