#!/bin/bash
set -e

echo "📊 Importing Qalitrack Custom Grafana Dashboards..."

# Get Grafana admin password
GRAFANA_PASSWORD=$(kubectl get secret prometheus-grafana -n qalitrack-monitoring \
  -o jsonpath="{.data.admin-password}" | base64 -d)

# Port-forward to Grafana
echo "🌐 Setting up port-forward to Grafana..."
kubectl port-forward -n qalitrack-monitoring svc/prometheus-grafana 3000:80 > /dev/null 2>&1 &
PF_PID=$!

# Wait for port-forward
sleep 3

# Import dashboards
echo "📥 Importing dashboards..."

for dashboard_file in *.json; do
    if [ -f "$dashboard_file" ]; then
        echo "  - Importing $dashboard_file..."

        curl -X POST http://admin:$GRAFANA_PASSWORD@localhost:3000/api/dashboards/db \
          -H "Content-Type: application/json" \
          -d @"$dashboard_file" \
          --silent --show-error

        echo "    ✅ Imported $dashboard_file"
    fi
done

# Cleanup
kill $PF_PID 2>/dev/null || true

echo ""
echo "🎉 Dashboards imported successfully!"
echo ""
echo "Access Grafana:"
echo "  kubectl port-forward -n qalitrack-monitoring svc/prometheus-grafana 3000:80"
echo "  Then open: http://localhost:3000"
echo "  Username: admin"
echo "  Password: $GRAFANA_PASSWORD"
echo ""
echo "Imported dashboards:"
echo "  - Qalitrack Platform Overview"
echo "  - Qalitrack Transactions Dashboard"
