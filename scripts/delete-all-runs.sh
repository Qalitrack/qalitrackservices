#!/bin/bash

echo "🧹 Deleting ALL GitHub Actions workflow runs..."
echo "Repository: $(gh repo view --json nameWithOwner -q .nameWithOwner)"
echo ""

# Get all run IDs
all_runs=$(gh run list --limit 100 --json databaseId | jq -r '.[].databaseId')

if [ -z "$all_runs" ]; then
    echo "✅ No runs found to delete."
    exit 0
fi

echo "Found runs to delete:"
echo "$all_runs"
echo ""

# Count runs
run_count=$(echo "$all_runs" | wc -l)
echo "📊 Total runs to delete: $run_count"
echo ""

echo "🗑️  Deleting all runs..."
deleted=0
skipped=0

for run_id in $all_runs; do
    echo -n "Deleting run $run_id... "
    if gh api -X DELETE "repos/$(gh repo view --json nameWithOwner -q .nameWithOwner)/actions/runs/$run_id" >/dev/null 2>&1; then
        echo "✅"
        ((deleted++))
    else
        echo "❌"
        ((skipped++))
    fi
    sleep 0.2  # Rate limiting
done

echo ""
echo "📈 Summary:"
echo "   ✅ Successfully deleted: $deleted runs"
echo "   ❌ Failed to delete: $skipped runs"
echo "   🎯 Total processed: $run_count runs"
echo ""
echo "✨ Complete cleanup done!"