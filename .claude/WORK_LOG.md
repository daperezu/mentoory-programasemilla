# Work Log

## 2025-01-30 - Aspire Runtime Optimization Planning (REQ-014)

### Problem
Production deployment using Aspire AppHost consumes 255MB (45MB app + 210MB overhead), resulting in 467% memory overhead and $15-20/month extra cost per instance.

### Analysis Performed
1. **Memory Profiling**:
   - OpenTelemetry: 120MB (distributed tracing, metrics, logging)
   - Service Discovery: 40MB (unnecessary for monolith)
   - Resilience Handlers: 30MB (retry policies, circuit breakers)
   - Aspire Runtime: 20MB (orchestration overhead)
   - Base App: 45MB (actual application)

2. **Cost Analysis**:
   - Current: 255MB per instance
   - Target: 50MB per instance (80% reduction)
   - Savings: $15-20/month per instance

### Solutions Designed

**1. Dual-Mode Deployment Strategy**:
```csharp
// Development: Full Aspire
if (builder.Environment.IsDevelopment()) {
    builder.AddServiceDefaults(); // 255MB total
}
// Production: Slim configuration
else {
    builder.AddSlimServiceDefaults(); // 50MB total
}
```

**2. Created Documentation Structure**:
- `.claude/requirements/active/REQ-014-aspire-runtime-optimization.md` - Full specification
- `.claude/deployment-optimization.md` - Implementation guide with code snippets

**3. Key Architecture Decisions**:
- Keep Aspire for infrastructure provisioning (Bicep generation)
- Remove Aspire runtime from production containers
- Replace OpenTelemetry with Application Insights (120MB → 5MB)
- Use Alpine Docker base for additional size reduction

### Files Created
- ✅ `REQ-014-aspire-runtime-optimization.md` - Complete requirement specification
- ✅ `deployment-optimization.md` - Detailed implementation guide
- ✅ Updated `CURRENT_SESSION.md` - Track implementation progress

### Key Patterns Documented
1. **SlimServiceDefaults Pattern**: Minimal service registration without telemetry
2. **Environment Detection**: `ASPIRE_RUNTIME_ENABLED` flag for mode switching
3. **Direct Azure Connections**: Skip service discovery, use connection strings
4. **Health Check Preservation**: Keep endpoints for Container Apps probes

### Next Steps
1. Implement `SlimExtensions.cs` with minimal services
2. Update `Program.cs` with conditional registration
3. Create production configuration files
4. Test memory consumption locally
5. Deploy and validate savings

### Important Notes
- All code snippets ready in implementation guide
- Exact line numbers documented for Program.cs changes
- PowerShell scripts prepared for both deployment modes
- Rollback procedure documented for safety

---