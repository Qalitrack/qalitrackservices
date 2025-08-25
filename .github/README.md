# QaliTrack GitHub Actions Workflows

This directory contains automated CI/CD workflows for all QaliTrack microservices.

## 🚀 Workflows Overview

### Individual Service Builds
We use **individual workflows per service** for better isolation and faster feedback:

#### Masterdata Services (10)
- `build-customer-service.yml` - Customer management service
- `build-driver-service.yml` - Driver management service  
- `build-product-service.yml` - Product catalog service
- `build-report-service.yml` - Reporting service
- `build-route-service.yml` - Route management service
- `build-sacco-service.yml` - SACCO integration service
- `build-supplier-service.yml` - Supplier management service
- `build-transporter-service.yml` - Transporter management service
- `build-vehicle-service.yml` - Vehicle management service
- `build-weighbridge-service.yml` - Weighbridge management service

#### Datamanager Services (4)
- `build-compliance-service.yml` - Compliance monitoring service
- `build-operational-data-service.yml` - Operational data processing service
- `build-transaction-service.yml` - Transaction processing service
- `build-weight-data-service.yml` - Weight data processing service

### Documentation Generation
- `generate-docs.yml` - DocFX documentation generation for all services

## 🔧 Workflow Features

### Each service workflow includes:
- ✅ .NET 8 build and compilation
- ✅ Unit test execution  
- ✅ Docker image build and optimization
- ✅ Push to GitHub Container Registry (GHCR)
- ✅ Path-based triggering (only builds when service changes)
- ✅ Build caching for faster execution
- ✅ Multi-platform support (linux/amd64)

### Triggering Conditions:
- **Push to main**: When files in the service directory change
- **Pull Request**: When PR affects the service
- **Manual**: Can be triggered manually via GitHub Actions UI

## 📦 Docker Images

All images are pushed to GitHub Container Registry:
```
ghcr.io/[owner]/qalitrackservices/[service-name]
```

### Image Tags:
- `latest` - Latest main branch build
- `main` - Main branch builds  
- `pr-[number]` - Pull request builds
- `sha-[commit]` - Specific commit builds

## 🔍 Monitoring Builds

Use the provided monitoring script:

```bash
# View all build statuses
./scripts/monitor-builds.sh

# View detailed failure information
./scripts/monitor-builds.sh --failures

# Trigger all service builds
./scripts/monitor-builds.sh --trigger-all
```

### Prerequisites for monitoring:
- [GitHub CLI](https://cli.github.com/) installed
- Authenticated with `gh auth login`

## 🛠️ Development Workflow

1. **Make changes** to any service in `packages/microservices/`
2. **Commit and push** to main or create PR
3. **Watch builds** automatically trigger for changed services
4. **Monitor progress** using the monitoring script or GitHub Actions UI
5. **Debug failures** using detailed logs and failure analysis

## 📚 Documentation Generation

The `generate-docs.yml` workflow:
- Generates API documentation using DocFX
- Combines service documentation from all services
- Publishes to GitHub Pages (if configured)
- Creates downloadable documentation artifacts

## 🔒 Security Features

- **GHCR Authentication**: Uses GitHub token for secure image pushing
- **Secrets Management**: No secrets exposed in logs
- **Dependency Scanning**: Built into .NET toolchain
- **Vulnerability Scanning**: Container scanning via GitHub

## 🚨 Troubleshooting

### Common Issues:

1. **Build Failures**: 
   - Check service-specific logs in GitHub Actions
   - Use `./scripts/monitor-builds.sh --failures`
   - Verify .NET project references and dependencies

2. **Docker Build Issues**:
   - Ensure Dockerfile exists in service directory
   - Check COPY paths in Dockerfile
   - Verify all required files are included in Docker context

3. **Missing Dependencies**:
   - Check `dotnet restore` step output
   - Verify NuGet package references
   - Ensure solution file exists and is valid

4. **DocFX Generation Issues**:
   - Check `docfx.json` configuration
   - Ensure API documentation exists
   - Verify project builds successfully first

### Getting Help:
- Check GitHub Actions logs for detailed error messages
- Use the monitoring script for quick status overview
- Review service-specific README files
- Check DocFX configuration in each service directory

## 🔄 Workflow Updates

To modify workflows:
1. Edit YAML files in `.github/workflows/`
2. Test changes in feature branch first
3. Commit and push to main
4. Workflows automatically pick up changes

## 📊 Performance Optimization

- **Parallel Execution**: Services build independently
- **Smart Triggering**: Only builds changed services  
- **Caching**: Docker layer and build caching enabled
- **Optimization**: Multi-stage Docker builds for smaller images