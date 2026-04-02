#!/bin/bash

echo "🧹 Deleting failed GitHub Actions workflow runs..."
echo "Repository: $(gh repo view --json nameWithOwner -q .nameWithOwner)"
echo ""

# Get all failed run IDs
failed_runs=$(gh run list --limit 100 --json databaseId,status,conclusion | jq -r '.[] | select(.conclusion == "failure") | .databaseId')

if [ -z "$failed_runs" ]; then
    echo "✅ No failed runs found to delete."
    exit 0
fi

echo "Found failed runs to delete:"
echo "$failed_runs"
echo ""

# Count runs
run_count=$(echo "$failed_runs" | wc -l)
echo "📊 Total failed runs to delete: $run_count"
echo ""

# Ask for confirmation
read -p "❓ Do you want to delete all these failed runs? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "❌ Operation cancelled."
    exit 0
fi

echo ""
echo "🗑️  Deleting failed runs..."
deleted=0
skipped=0

for run_id in $failed_runs; do
    echo -n "Deleting run $run_id... "
    if gh run delete "$run_id" --confirm 2>/dev/null; then
        echo "✅ deleted"
        ((deleted++))
    else
        echo "❌ failed"
        ((skipped++))
    fi
    sleep 0.5  # Rate limiting
done

echo ""
echo "📈 Summary:"
echo "   ✅ Successfully deleted: $deleted runs"
echo "   ❌ Failed to delete: $skipped runs"
echo "   🎯 Total processed: $run_count runs"
echo ""
echo "✨ Cleanup complete!"