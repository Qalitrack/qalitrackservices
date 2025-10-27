#!/bin/bash
# Reverts all commits by faithzawadi2002@gmail.com after a given date (macOS-safe)

AUTHOR_EMAIL="faithzawadi2002@gmail.com"
SINCE_DATE="2025-10-20"

echo "🔄 Reverting all commits by $AUTHOR_EMAIL since $SINCE_DATE ..."
echo

# Create a safety branch so the main branch isn't modified directly
TMP_BRANCH="revert-${AUTHOR_EMAIL//[^a-zA-Z0-9]/-}-$(date +%s)"
git checkout -b "$TMP_BRANCH"

# Get commits (oldest first), skipping merges — AWK version works on macOS
COMMITS=$(git log --author="$AUTHOR_EMAIL" --since="$SINCE_DATE" --no-merges --pretty=format:"%H" \
  | awk '{lines[NR]=$0} END {for (i=NR; i>0; i--) print lines[i]}')

if [ -z "$COMMITS" ]; then
  echo "✅ No commits found for $AUTHOR_EMAIL since $SINCE_DATE."
  exit 0
fi

# Revert each commit in order
for COMMIT in $COMMITS; do
  echo "Reverting commit $COMMIT ..."
  git revert -n "$COMMIT" || {
    echo "⚠️ Conflict while reverting $COMMIT. Resolve conflicts and run 'git revert --continue' manually."
    exit 1
  }
done

# Commit the combined revert
git commit -m "Revert all commits by $AUTHOR_EMAIL since $SINCE_DATE"

echo
echo "✅ All commits by $AUTHOR_EMAIL since $SINCE_DATE have been reverted."
echo "A new branch '$TMP_BRANCH' has been created containing the revert changes."

