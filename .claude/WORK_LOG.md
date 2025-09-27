# Work Log

## 2025-09-27 - Database Build Fix (Final) ✅ COMPLETED

### Problem
Database build failing with 144 SQL syntax errors after restoring from stash. Build was completely broken.

### Root Causes
1. **UTF-8 BOM characters** in all SQL files (0xEF 0xBB 0xBF) causing MSBuild.Sdk.SqlProj parser to fail
2. **Index syntax order**: INCLUDE clause after WHERE clause (must be before)
3. **Duplicate schema file**: `notification/notification.sql` and `notification/Schema.sql` both existed
4. **Missing line terminators**: Some files ended without newlines
5. **Geohash indexes**: User requested deletion due to complexity

### Solutions Applied

**1. Removed UTF-8 BOMs from all SQL files**:
```bash
find Db -name "*.sql" -type f -exec sed -i '1s/^\xEF\xBB\xBF//' {} \;
find PostDeployment -name "*.sql" -type f -exec sed -i '1s/^\xEF\xBB\xBF//' {} \;
```

**2. Fixed filtered index syntax order**:
```sql
-- BEFORE (WRONG)
CREATE INDEX [name] ON [table] ([cols])
WHERE [condition]
INCLUDE ([included_cols]);

-- AFTER (CORRECT)
CREATE INDEX [name] ON [table] ([cols])
INCLUDE ([included_cols])
WHERE [condition];
```
- Fixed: `IX_FormSubmissions.sql`
- Fixed: `IX_Projects_Latitude_Longitude.sql`

**3. Deleted problematic geohash indexes** (per user request):
- Removed: `IX_Projects_GeohashPrefix5.sql`
- Removed: `IX_Projects_GeohashPrefix6.sql`

**4. Removed duplicate schema file**:
- Deleted: `notification/notification.sql`
- Kept: `notification/Schema.sql`

**5. Added missing line terminators**:
```bash
echo "" >> file.sql  # For files flagged by `file` command
```

### Result
- ✅ Build successful: 0 errors, 0 warnings
- ✅ DACPAC generated: 275K
- ✅ Publish script working: `./publish-linadb.sh`

### Key Learnings
1. **MSBuild.Sdk.SqlProj is sensitive to UTF-8 BOMs** - causes cryptic "Incorrect syntax" errors
2. **SQL Server filtered index syntax is strict** - INCLUDE must precede WHERE
3. **Line terminators matter** - files without newlines can cause parser issues
4. **Project uses MSBuild.Sdk.SqlProj 3.2.0** for cross-platform SQL builds

---

## 2025-09-27 - Database Build Fix (Earlier Attempt)

### Problem
Running `./publish-linadb.sh -p` failed with 143 SQL syntax errors:
- "Incorrect syntax near 'INCLUDE'" across multiple schema files
- "Incorrect syntax near '('" errors
- "Filtered index cannot be created on computed column" errors

### Root Causes Identified
1. **UTF-8 BOM Characters**: Files had byte order marks (0xEF 0xBB 0xBF) causing parser errors
2. **Wrong Index Syntax Order**: WHERE clause before INCLUDE clause (SQL Server requires INCLUDE first)
3. **Computed Column Filtering**: Original indexes tried to filter directly on computed columns in WHERE clause
4. **File Naming**: `notification/notification.sql` should be `notification/Schema.sql`
5. **Project File Ordering**: MSBuild.Sdk.SqlProj was including indexes before tables alphabetically

### Investigation Process
1. Checked for BOM with `od -An -tx1 -N3 file.sql` - confirmed BOMs present
2. Removed BOMs with `find . -name "*.sql" -exec sed -i '1s/^\xEF\xBB\xBF//' {} \;`
3. Discovered errors persisted - not BOM related
4. Read actual error file `IX_FormSubmissions.sql` - found INCLUDE after WHERE
5. Fixed syntax order: moved INCLUDE before WHERE
6. Attempted file ordering in sqlproj - MSBuild sorts alphabetically regardless
7. Discovered DacFx handles dependencies automatically - simplified to wildcards
8. Reverted computed column filter change - persisted computed columns can be filtered

### Solutions Applied

**1. Fixed Index Syntax Order** (`IX_FormSubmissions.sql`, `IX_Projects_Latitude_Longitude.sql`):
```sql
-- BEFORE (WRONG)
CREATE INDEX [name] ON [table] ([cols])
WHERE [condition]
INCLUDE ([included_cols]);

-- AFTER (CORRECT)
CREATE INDEX [name] ON [table] ([cols])
INCLUDE ([included_cols])
WHERE [condition];
```

**2. Reverted Geohash Filter Logic** (`IX_Projects_GeohashPrefix5.sql`, `IX_Projects_GeohashPrefix6.sql`):
```sql
-- CORRECT - Persisted computed columns CAN be filtered
WHERE [GeohashPrefix5] IS NOT NULL AND [IsDeleted] = 0

-- Initially tried (unnecessary):
WHERE LEN([Geohash]) >= 5 AND [IsDeleted] = 0
```

**3. Renamed Schema File**:
```bash
mv notification/notification.sql notification/Schema.sql
```

**4. Simplified SqlProj** (`LinaDb.sqlproj`):
```xml
<!-- BEFORE - Explicit ordering attempt -->
<Content Include="businessincubators/Schema.sql" />
<Content Include="businessincubators/Tables/*.sql" />
<Content Include="businessincubators/Indexes/*.sql" />
<!-- ... repeated for all schemas -->

<!-- AFTER - Let DacFx handle dependencies -->
<ItemGroup>
  <Content Include="**/*.sql" Exclude="obj/**;bin/**" />
</ItemGroup>
```

**5. Fixed PostDeployment Path** (already done in previous session):
```sql
-- Script.PostDeployment.sql uses Unix paths
:r ./000.SeedRolesAndUsers.sql  -- Not .\filename
```

### Files Modified
- `Db/LinaDb.sqlproj` - Simplified to wildcard includes
- `Db/businessincubators/Indexes/IX_FormSubmissions.sql` - Fixed INCLUDE/WHERE order
- `Db/businessincubators/Indexes/IX_Projects_Latitude_Longitude.sql` - Fixed INCLUDE/WHERE order
- `Db/businessincubators/Indexes/IX_Projects_GeohashPrefix5.sql` - Reverted to original WHERE logic
- `Db/businessincubators/Indexes/IX_Projects_GeohashPrefix6.sql` - Reverted to original WHERE logic
- `Db/notification/notification.sql` → `Db/notification/Schema.sql` - Renamed

### Key Learnings

**SQL Server Filtered Index Rules**:
1. INCLUDE must come before WHERE in CREATE INDEX syntax
2. Persisted computed columns CAN be referenced in WHERE clause
3. Non-persisted computed columns CANNOT be in WHERE clause

**MSBuild.Sdk.SqlProj Behavior**:
1. Includes are sorted alphabetically regardless of declaration order
2. DacFx automatically resolves dependencies (schemas → tables → indexes)
3. Explicit ordering not needed with `**/*.sql` wildcard
4. PostDeployment scripts excluded from validation

**Debugging Approach**:
1. Check file encoding first (`od -An -tx1`)
2. Read actual file content causing error
3. Compare against SQL Server syntax documentation
4. Test simple wildcard approach before complex ordering

### Results
- ✅ `dotnet build` succeeds with 0 errors, 0 warnings
- ✅ DACPAC generated: `Db/bin/Debug/net8.0/LinaDb.dacpac` (275K)
- ⚠️ `./publish-linadb.sh -p` hangs (database connection issue, not build issue)

### Next Investigation Needed
- Check `LinaDb.Development.publish.xml` connection string
- Verify SQL Server is running
- Test `sqlpackage` connectivity separately

---