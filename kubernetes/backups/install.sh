#!/bin/bash
set -e

echo "💾 Setting up automated PostgreSQL backups..."

# Apply CronJob for automated backups
echo "📅 Creating backup CronJob (runs daily at 2:00 AM UTC)..."
kubectl apply -f cronjob-postgres-backup.yaml

echo "✅ Backup CronJob created successfully"

# Verify
echo ""
echo "📋 Checking CronJob status..."
kubectl get cronjob -n qalitrack-prod

echo ""
echo "🎉 Automated backups configured!"
echo ""
echo "Backup schedule: Daily at 2:00 AM UTC"
echo "Retention: 30 days"
echo "Storage: 50Gi persistent volume"
echo ""
echo "Useful commands:"
echo ""
echo "  # Trigger manual backup now:"
echo "  kubectl create job --from=cronjob/postgres-backup manual-backup-\$(date +%s) -n qalitrack-prod"
echo ""
echo "  # View backup job logs:"
echo "  kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=postgres-backup --tail=100"
echo ""
echo "  # List backup files:"
echo "  kubectl exec -it -n qalitrack-prod deployment/backup-service -- ls -lh /backups"
echo ""
echo "  # Check CronJob history:"
echo "  kubectl get jobs -n qalitrack-prod -l app.kubernetes.io/name=postgres-backup"
