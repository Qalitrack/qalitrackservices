# 🚀 QaliTrack Build System - Quick Reference

## 📍 **Repository Information**
- **URL**: https://github.com/t-qs-builds/qalitrack-microservices-build
- **Organization**: t-qs-builds
- **Visibility**: Private
- **Actions**: https://github.com/t-qs-builds/qalitrack-microservices-build/actions

## 🎯 **Quick Commands**

### Monitor All Builds
```bash
./scripts/monitor-builds.sh
```

### Check Only Failures
```bash
./scripts/monitor-builds.sh --failures
```

### Trigger All Builds
```bash
./scripts/monitor-builds.sh --trigger-all
```

### Push Changes (triggers relevant builds)
```bash
git push qalitrack main
```

## 🐳 **Docker Images**

All successful builds push to:
```
ghcr.io/t-qs-builds/qalitrack-microservices-build/[service-name]:latest
```

### Pull Example
```bash
docker pull ghcr.io/t-qs-builds/qalitrack-microservices-build/customer-service:latest
```

## 🛠️ **First Run Status** (Just Triggered)

### Expected Results:
- ✅ **Some builds will pass**: Services with minimal implementation
- ❌ **Many builds will fail**: Missing entities, repository interface issues
- 🔄 **All workflows triggered**: 14 services + 1 documentation

### Common Issues to Fix:
1. **Missing Entity Classes**: Referenced in DbContext but don't exist
2. **Repository Interface Issues**: Missing methods like `FindAsync`, `FirstOrDefaultAsync`
3. **Docker Configuration**: COPY paths, missing files
4. **Compilation Errors**: Project references, namespaces

## 📋 **Services Being Built**

### Masterdata (10):
- customer-service, driver-service, product-service
- report-service, route-service, sacco-service
- supplier-service, transporter-service
- vehicle-service, weighbridge-service

### Datamanager (4):
- compliance-service, operational-data-service
- transaction-service, weight-data-service

## 🔧 **Fixing Workflow**

1. **Monitor**: Check which builds fail
2. **Analyze**: Use `--failures` flag for detailed errors
3. **Group**: Fix similar issues across multiple services
4. **Test**: Push fixes and watch builds
5. **Iterate**: Repeat until all services build successfully

## 📚 **Documentation**

- **Workflow Guide**: `.github/README.md`
- **Service Setup**: `QALITRACK-BUILD-SETUP.md` 
- **DocFX Configs**: Each service has `docfx.json`
- **API Docs**: Generated automatically on successful builds

---

⚡ **Currently all workflows are running for the first time!** Check the Actions page to see real-time progress.