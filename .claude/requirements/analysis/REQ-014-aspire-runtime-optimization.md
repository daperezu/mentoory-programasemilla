# REQ-014: Aspire Runtime Optimization for Production Deployment

## Requirement ID
REQ-014

## Status
ACTIVE

## Priority
HIGH

## Created Date
2025-01-30

## Updated Date
2025-01-30

## Business Context
LinaSys currently deploys to Azure using .NET Aspire AppHost, which adds 210MB of memory overhead to the 45MB web application, resulting in 255MB total memory consumption. This 467% overhead significantly increases Azure hosting costs without providing proportional value for a monolithic application. The monthly cost impact is approximately $15-20 per instance, which scales with the number of instances deployed.

## Current Problems
1. **Memory Overhead**: Aspire AppHost runtime consumes 210MB for orchestrating a single 45MB application
2. **Cost Impact**: Additional $15-20/month per instance in Azure Container Apps hosting costs
3. **Unnecessary Components**: OpenTelemetry (~120MB), Service Discovery (~40MB), and Resilience handlers (~30MB) provide minimal value for a monolithic app
4. **Resource Inefficiency**: 80% of memory consumption is orchestration overhead rather than application functionality
5. **Cold Start Performance**: Larger memory footprint increases container startup times

## Proposed Solution
Implement a dual-mode deployment strategy:
- **Development**: Continue using full Aspire runtime for excellent developer experience
- **Production**: Deploy only the web application container with direct Azure resource connections
- Keep Aspire for infrastructure provisioning (Bicep generation) but not runtime orchestration

## Requirements

### Functional Requirements
1. **Maintain all current application functionality** without Aspire runtime in production
2. **Support environment-based configuration** to toggle between full and slim modes
3. **Preserve health check endpoints** for Azure Container Apps probes
4. **Direct Azure resource connections** without service discovery
5. **Application Insights integration** as lightweight telemetry replacement
6. **Zero-downtime deployment** capability
7. **Rollback mechanism** to restore full Aspire runtime if needed

### Non-Functional Requirements
1. **Target memory footprint**: <55MB in production (down from 255MB)
2. **Startup time**: <5 seconds
3. **Response time P95**: <200ms (no degradation)
4. **Build compliance**: Zero warnings per LinaSys standards
5. **Clean Architecture**: Maintain layer separation
6. **Spanish UI**: All user-facing text remains in Spanish

## Technical Design

### Architecture Changes
1. **Create SlimServiceDefaults**: Minimal service configuration without OpenTelemetry, Service Discovery, or Resilience handlers
2. **Environment detection**: Use `ASPIRE_RUNTIME_ENABLED` flag to switch modes
3. **Direct resource configuration**: Connection strings via Azure Key Vault references
4. **Separate deployment paths**: Different scripts for dev vs production

### Memory Footprint Analysis
| Component | Current Size | Production | Savings |
|-----------|-------------|------------|---------|
| Base Web App | 45MB | 45MB | 0MB |
| OpenTelemetry | 120MB | 0MB | 120MB |
| Service Discovery | 40MB | 0MB | 40MB |
| Resilience Handlers | 30MB | 0MB | 30MB |
| Aspire Runtime | 20MB | 0MB | 20MB |
| Application Insights | 0MB | 5MB | -5MB |
| **TOTAL** | **255MB** | **50MB** | **205MB (80% reduction)** |

### File Structure
```
.claude/
  requirements/
    active/
      REQ-014-aspire-runtime-optimization.md  (this file)
  deployment-optimization.md                  (detailed implementation guide)

Aspire.ServiceDefaults/
  SlimExtensions.cs                           (new - minimal services)

Web/
  appsettings.Production.json                 (new - production config)
  azure.yaml                                   (new - web-only deployment)
  Dockerfile                                   (new - optimized container)
  infra/
    web-container-app.bicep                   (new - standalone deployment)

Aspire.AppHost/
  infra/
    app-insights.bicep                        (new - Application Insights)
    main-optimized.bicep                      (new - optimized deployment)

scripts/
  deploy-dev.ps1                              (new - full Aspire deployment)
  deploy-prod.ps1                             (new - optimized deployment)
  rollback-production.ps1                     (new - emergency rollback)
  test-memory-consumption.ps1                 (new - memory testing)
  test-health-endpoints.ps1                   (new - health validation)
```

## Implementation Tasks

### Stage 1: Analysis and Preparation (2 hours)
- [x] Analyze current Aspire service dependencies
- [x] Document memory footprint of each component
- [x] Identify required vs optional services
- [x] Review package dependencies

### Stage 2: Create Slim Configuration (4 hours)
- [ ] Create SlimExtensions.cs in Aspire.ServiceDefaults
- [ ] Implement minimal service registration
- [ ] Add production health checks
- [ ] Remove OpenTelemetry, Service Discovery, Resilience

### Stage 3: Modify Web Project (3 hours)
- [ ] Update Program.cs with conditional logic
- [ ] Add environment detection for service registration
- [ ] Configure direct Azure Blob Storage connection
- [ ] Update endpoint mapping for production

### Stage 4: Azure Resource Configuration (2 hours)
- [ ] Create appsettings.Production.json
- [ ] Configure direct SQL Server connection
- [ ] Set up connection string management
- [ ] Add Key Vault references

### Stage 5: Application Insights Integration (2 hours)
- [ ] Add Application Insights package to Directory.Packages.props
- [ ] Create app-insights.bicep template
- [ ] Configure telemetry collection
- [ ] Set up monitoring dashboard

### Stage 6: Infrastructure Templates (4 hours)
- [ ] Create web-container-app.bicep
- [ ] Create main-optimized.bicep
- [ ] Configure Container Apps settings
- [ ] Set up managed identity

### Stage 7: Docker Configuration (2 hours)
- [ ] Create optimized Dockerfile
- [ ] Use Alpine base image for size reduction
- [ ] Configure health checks
- [ ] Set up non-root user

### Stage 8: Deployment Scripts (3 hours)
- [ ] Create deploy-dev.ps1 for development
- [ ] Create deploy-prod.ps1 for production
- [ ] Create rollback-production.ps1
- [ ] Add validation and error handling

### Stage 9: Testing Scripts (2 hours)
- [ ] Create test-memory-consumption.ps1
- [ ] Create test-health-endpoints.ps1
- [ ] Add performance benchmarking
- [ ] Document expected results

### Stage 10: Documentation (2 hours)
- [ ] Update CLAUDE.md with deployment modes
- [ ] Create deployment-optimization.md guide
- [ ] Document rollback procedures
- [ ] Add troubleshooting section

## Success Criteria
1. **Memory Usage**: Production deployment uses <55MB (78% reduction)
2. **Cost Reduction**: $15-20/month savings per instance verified in Azure billing
3. **Performance**: No degradation in response times (P95 <200ms)
4. **Functionality**: All features working identically to current deployment
5. **Monitoring**: Application Insights showing telemetry data
6. **Deployment**: Both dev and prod deployment scripts working successfully
7. **Rollback**: Emergency rollback tested and functional

## Risk Analysis

### Risks
| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Missing telemetry | Low | Medium | Application Insights provides sufficient coverage |
| Connection failures | Low | High | Azure retry policies, connection pooling |
| Deployment complexity | Medium | Medium | Detailed scripts, documentation |
| Performance regression | Low | High | Load testing before production |
| Rollback issues | Low | High | Tested rollback script, feature flags |

### Mitigation Strategies
1. **Comprehensive testing** in staging environment before production
2. **Feature flags** to toggle between full and slim modes
3. **Keep both deployment paths** active initially
4. **Monitor closely** for first 48 hours after deployment
5. **Document all changes** thoroughly for team knowledge

## Dependencies
- Azure Container Apps environment (existing)
- Azure SQL Server (existing)
- Azure Blob Storage (existing)
- Azure Key Vault (existing)
- Application Insights resource (to be created)
- Azure Container Registry (existing)

## Estimated Timeline
- **Development Effort**: 26 hours
- **Testing & Validation**: 8 hours
- **Documentation**: 4 hours
- **Total Effort**: 38 hours
- **Calendar Time**: 1 week with parallel work
- **Deployment Window**: 30 minutes per environment

## Testing Plan

### Unit Testing
- Test SlimServiceDefaults registration
- Validate conditional logic in Program.cs
- Test health check endpoints

### Integration Testing
- Validate Azure SQL connectivity without Aspire
- Test Blob Storage operations
- Verify Application Insights telemetry

### Performance Testing
- Memory consumption comparison (Full vs Slim)
- Cold start time measurement
- Response time benchmarking
- Load testing with reduced footprint

### Deployment Testing
- Test deploy-dev.ps1 script
- Test deploy-prod.ps1 script
- Validate rollback-production.ps1
- Container image size verification

## Rollback Plan

### Trigger Conditions
- Memory usage exceeds 70MB consistently
- Response times degrade >50%
- Connection failures >1% error rate
- Missing critical telemetry

### Rollback Procedure
1. Execute `rollback-production.ps1 -Environment prod -Reason "issue description"`
2. Script automatically:
   - Sets ASPIRE_RUNTIME_ENABLED=true
   - Redeploys with full Aspire runtime
   - Logs rollback for audit
3. Monitor application for stability
4. Investigate root cause
5. Fix issues before retry

### Rollback Timeline
- Detection: <5 minutes (health checks)
- Decision: <10 minutes (automated alerts)
- Execution: <15 minutes (script runtime)
- Total RTO: <30 minutes

## Monitoring & Alerting

### Key Metrics
| Metric | Normal | Warning | Critical | Action |
|--------|--------|---------|----------|--------|
| Memory Usage | <50MB | 50-60MB | >70MB | Investigate, consider rollback |
| Startup Time | <5s | 5-8s | >10s | Review configuration |
| Response P95 | <200ms | 200-400ms | >500ms | Performance analysis |
| Error Rate | <0.1% | 0.1-0.5% | >1% | Immediate investigation |
| Health Check | 100% | 99-100% | <99% | Check dependencies |

### Application Insights Queries
```kql
// Memory usage trend
performanceCounters
| where name == "Private Bytes"
| where cloud_RoleName == "linasys-web"
| summarize avg(value/1024/1024) by bin(timestamp, 5m)
| render timechart

// Startup duration
customMetrics
| where name == "AppStartupDuration"
| summarize percentiles(value, 50, 90, 95, 99) by bin(timestamp, 1h)

// Error rate
requests
| where success == false
| summarize errorRate = count() * 100.0 / toscalar(requests | count())
| render gauge
```

## Communication Plan
1. **Pre-deployment**: Notify team 48 hours before
2. **During deployment**: Status updates every 15 minutes
3. **Post-deployment**: Summary report within 2 hours
4. **If rollback needed**: Immediate notification to stakeholders

## Notes
- This optimization is specifically for monolithic deployment
- If microservices are added later, re-evaluate the need for full Aspire runtime
- Cost savings scale linearly with number of instances
- Memory measurements based on actual testing of current application
- All timings are estimates and should be validated in staging

## References
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire)
- [Azure Container Apps Docs](https://learn.microsoft.com/azure/container-apps)
- [Application Insights](https://learn.microsoft.com/azure/azure-monitor/app/app-insights-overview)
- LinaSys Architecture Guide: `.claude/architecture.md`
- LinaSys Coding Standards: `.claude/coding-standards.md`