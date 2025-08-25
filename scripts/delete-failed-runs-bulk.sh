#!/bin/bash

echo "🧹 Bulk deleting failed GitHub Actions runs..."
echo ""

# Get failed run IDs
failed_runs=$(gh run list --limit 100 --json databaseId,conclusion | jq -r '.[] | select(.conclusion == "failure") | .databaseId')

if [ -z "$failed_runs" ]; then
    echo "✅ No failed runs found."
    exit 0
fi

count=$(echo "$failed_runs" | wc -l)
echo "📊 Found $count failed runs to delete"
echo ""

# Use GitHub API directly for bulk deletion
for run_id in $failed_runs; do
    echo -n "Deleting run $run_id... "
    if gh api -X DELETE "repos/$(gh repo view --json nameWithOwner -q .nameWithOwner)/actions/runs/$run_id" >/dev/null 2>&1; then
        echo "✅"
    else
        echo "❌"
    fi
    sleep 0.3  # Rate limiting
done

echo ""
echo "✨ Bulk deletion complete!"