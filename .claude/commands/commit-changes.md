# Commit Changes

Commit changes to git repository with intelligent commit message generation and flexible file selection.

## Target: $ARGUMENTS

## Commit Process

1. **Load Commit Context**
   - Parse the commit target: $ARGUMENTS (can be "all", directory path, or file path)
   - Analyze current git status to understand what files have changed
   - Examine the scope and nature of changes to be committed
   - Review recent commit history to understand commit message patterns
   - Identify the type of changes (features, fixes, documentation, tests, etc.)
   - Understand the business impact and technical scope of the changes

2. **ULTRATHINK**
   - Think hard before committing. Create a comprehensive commit strategy
   - Break down commit analysis into manageable steps using your TodoWrite tool
   - Use the TodoWrite tool to create and track your commit analysis plan
   - Analyze the changes to determine appropriate commit message structure
   - Consider the scope of changes and their business/technical impact
   - Plan for meaningful commit message that follows conventional commit patterns
   - Ensure commit scope is appropriate (not too large, not too granular)

3. **Execute Commit Process**
   - Stage the appropriate files based on $ARGUMENTS:
     - "all" or empty: Stage all modified/new files (git add -A)
     - Directory path: Stage all files in specified directory (git add {directory})
     - File path: Stage specific file (git add {file})
   - Generate intelligent commit message based on changes:
     - Analyze file changes to determine commit type (feat, fix, docs, test, etc.)
     - Create concise but descriptive summary line (50 chars max)
     - Add detailed description for complex changes
     - Include business context where relevant
   - Execute git commit with generated message
   - Add standard Claude Code attribution footer

4. **Validate**
   - Verify that intended files were staged correctly
   - Check that commit message accurately reflects the changes
   - Validate that commit doesn't include unintended files or sensitive data
   - Ensure commit message follows conventional commit format
   - Check that commit size is appropriate (not too large)
   - Fix any issues with staging or commit message
   - Re-stage and re-commit if necessary

5. **Complete**
   - Confirm successful commit with commit hash
   - Display summary of what was committed
   - Show commit message for verification
   - Provide next steps if applicable (push, PR creation, etc.)
   - Update any related documentation if needed
   - Report commit completion status with details

6. **Reference Change Context**
   - You can always reference the actual file changes if needed for commit message
   - Cross-reference changes with business requirements or technical goals
   - Ensure commit message provides sufficient context for future developers
   - Verify changes align with project conventions and standards

## Commit Message Generation Strategy

### Conventional Commit Format
```
<type>[optional scope]: <description>

[optional body]

[optional footer]
```

### Commit Types
- **feat**: New feature implementation
- **fix**: Bug fix or issue resolution
- **docs**: Documentation changes
- **test**: Adding or updating tests
- **refactor**: Code refactoring without functional changes
- **style**: Code style changes (formatting, etc.)
- **chore**: Maintenance tasks, build changes
- **perf**: Performance improvements
- **ci**: CI/CD pipeline changes

### Message Examples
```bash
# Feature addition
feat(auth): add JWT token refresh functionality

# Bug fix
fix(gateway): resolve routing issue for protected endpoints

# Documentation
docs(testing): add comprehensive testing guide and standards

# Test addition
test(user-service): add authentication and authorization test suite

# Refactoring
refactor(database): optimize query performance for user lookups
```

## Usage Examples

### Commit All Changes
```bash
# Commit all modified files with auto-generated message
/commit-changes all

# Commit all changes (same as above)
/commit-changes
```

### Commit Specific Directory
```bash
# Commit all changes in testing directory
/commit-changes testing-unified/

# Commit all changes in specific service
/commit-changes packages/microservices/masterdata/user-service/

# Commit configuration changes
/commit-changes configs/

# Commit documentation changes
/commit-changes docs/
```

### Commit Specific File
```bash
# Commit specific configuration file
/commit-changes configs/clients/testing.yml

# Commit specific documentation file
/commit-changes TESTING.md

# Commit specific service file
/commit-changes packages/qalitrack-gateway/src/Program.cs

# Commit makefile changes
/commit-changes Makefile
```

## Intelligent Message Generation

The command analyzes changes and generates appropriate commit messages:

### For Service Implementation
```bash
# Input: /commit-changes packages/microservices/masterdata/product-service/
# Generated: feat(product-service): implement product management with HAZMAT classification
```

### For Testing Changes
```bash
# Input: /commit-changes testing-unified/
# Generated: test: consolidate testing infrastructure with comprehensive documentation
```

### For Configuration Updates
```bash
# Input: /commit-changes configs/clients/testing.yml
# Generated: config(testing): enable product service in testing environment
```

### For Documentation
```bash
# Input: /commit-changes docs/
# Generated: docs: add comprehensive API documentation and user guides
```

## Change Analysis Process

The command analyzes changes by:

1. **File Scope Analysis**
   - Identify modified, added, and deleted files
   - Categorize changes by type (code, config, docs, tests)
   - Determine primary impact area

2. **Change Content Analysis**
   - Examine git diff output for change patterns
   - Identify new features, bug fixes, or improvements
   - Understand business context of changes

3. **Commit History Context**
   - Review recent commits for message patterns
   - Ensure consistency with project conventions
   - Avoid duplicate or redundant commits

## Pre-Commit Validation

Before committing, the command validates:

- **File Safety**: No sensitive data (passwords, keys, tokens)
- **Code Quality**: Basic syntax and formatting checks
- **Commit Size**: Reasonable number of changes
- **Message Quality**: Clear, concise, and informative
- **Scope Appropriateness**: Logical grouping of related changes

## Error Handling

### Common Issues and Solutions
- **No Changes to Commit**: Inform user and suggest checking git status
- **Merge Conflicts**: Guide user to resolve conflicts before committing
- **Large Commit Size**: Suggest breaking into smaller, focused commits
- **Unclear Changes**: Request user input for commit message clarification
- **Sensitive Data**: Block commit and warn about potential security issues

## Interactive Mode

For complex changes, the command may prompt for:
```bash
# Multiple change types detected
"Multiple types of changes found. Commit separately? (y/n)"

# Large changeset
"Large number of changes detected. Review files to commit? (y/n)"

# Custom message
"Auto-generated message: 'feat(auth): add JWT functionality'
Use this message? (y/n/edit)"
```

## Integration with Development Workflow

### Standard Development Cycle
```bash
# 1. Make changes to code/config/docs
# 2. Commit specific areas as you work
/commit-changes packages/microservices/masterdata/user-service/

# 3. Commit tests separately
/commit-changes testing-unified/

# 4. Final commit of any remaining changes
/commit-changes all
```

### Feature Development Workflow
```bash
# Commit feature implementation
/commit-changes packages/microservices/masterdata/product-service/

# Commit related tests
/commit-changes testing-unified/scripts/

# Commit documentation updates
/commit-changes docs/packages/product/

# Commit configuration changes
/commit-changes configs/clients/
```

## Completion Criteria

Commit operation is complete when:
- [ ] Appropriate files are staged based on $ARGUMENTS specification
- [ ] Intelligent commit message is generated reflecting actual changes
- [ ] Commit message follows conventional commit format
- [ ] No sensitive data or unintended files are included
- [ ] Commit is successfully created with proper attribution
- [ ] User is informed of commit hash and summary
- [ ] Any follow-up actions are suggested (push, PR, etc.)

This command streamlines the commit process while ensuring high-quality, meaningful commit messages that provide proper context for future development and maintenance.