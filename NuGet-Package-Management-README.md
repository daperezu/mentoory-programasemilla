# Managing NuGet Package Updates with Central Package Management

This project uses **Central Package Management (CPM)** with `Directory.Packages.props` to manage NuGet package versions centrally. All `.csproj` files have `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>` enabled.

---

## Checking for Outdated Packages

### 1. Per Project (Safe with CPM)
Run inside any project folder (with a `.csproj` file):
```powershell
dotnet list package --outdated
```

### 2. Across All Projects, Skipping SQL Project
From the solution root, run:
```powershell
Get-ChildItem -Recurse -Filter *.csproj |
  ForEach-Object {
    Write-Host "\n==== $($_.FullName) ===="
    dotnet list $_.FullName package --outdated
  }
```

> ⚠️ This skips `.sqlproj` (e.g., `Db\LinaDb.sqlproj`) which is not supported by the `dotnet` CLI.

Options:
- Add `--include-transitive` to also check transitive dependencies.
- Add `--verbosity quiet` for cleaner output.

### 3. Using dotnet-outdated Tool (Recommended)
Install once:
```powershell
dotnet tool install --global dotnet-outdated-tool
```

Run from solution root:
```powershell
# Show outdated packages
dotnet outdated

# Upgrade centrally in Directory.Packages.props
dotnet outdated --upgrade
```

Useful flags:
- `--pre-release` → include pre-releases
- `--include-transitive` → include transitive dependencies
- `--version-lock Major|Minor|Patch` → control upgrade scope
- `--prompt` → confirm before each update

---

## Updating Packages

Because packages are managed centrally:

1. Open `Directory.Packages.props`
2. Update the version:
   ```xml
   <ItemGroup>
     <PackageVersion Include="FluentValidation" Version="11.7.1" />
     <PackageVersion Include="MediatR" Version="12.1.0" />
   </ItemGroup>
   ```
3. Run:
   ```powershell
   dotnet restore
   ```

All projects using those packages will pick up the new versions.

---

## Bonus: Vulnerabilities & Deprecated Packages

Check for known issues:
```powershell
dotnet list package --vulnerable
```

Check for deprecated packages:
```powershell
dotnet list package --deprecated
```

---

## Best Practices
- Run `dotnet list package --outdated` regularly on projects.
- Use `dotnet outdated` to automate and manage central updates.
- Always commit updates to `Directory.Packages.props` to keep consistency across projects.
