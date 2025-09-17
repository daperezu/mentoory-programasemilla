param(
    [string]$Configuration = "Debug",
    [switch]$Publish,
    [ValidateSet("Development", "Production")]
    [string]$Profile = "Development",
    [switch]$Force,
    [switch]$GenerateScript,
    [string]$MSBuildPath,
    [string]$SqlPackagePath
)

function Resolve-MSBuild {
    param([string]$HintPath)
    if ($HintPath -and (Test-Path $HintPath)) { return $HintPath }

    $cmd = Get-Command msbuild -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Path }

    $pf86 = ${env:ProgramFiles(x86)}
    if ($pf86) {
        $vswhere = Join-Path $pf86 "Microsoft Visual Studio\Installer\vswhere.exe"
        if (Test-Path $vswhere) {
            $found = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" 2>$null
            if ($found) {
                if ($found -is [array]) { return $found[0] } else { return $found }
            }
        }
    }

    $candidates = @(
        "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    )
    foreach ($p in $candidates) { if (Test-Path $p) { return $p } }
    return $null
}

function Resolve-SqlPackage {
    param([string]$HintPath)
    if ($HintPath -and (Test-Path $HintPath)) { return $HintPath }

    $cmd = Get-Command SqlPackage.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Path }

    $vsGlob = "C:\Program Files\Microsoft Visual Studio\2022\*\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\SqlPackage.exe"
    $sqlGlob = Join-Path ${env:ProgramFiles} "Microsoft SQL Server\*\DAC\bin\SqlPackage.exe"

    $candidates = @()
    $candidates += Get-ChildItem -Path $vsGlob -ErrorAction SilentlyContinue
    $candidates += Get-ChildItem -Path $sqlGlob -ErrorAction SilentlyContinue

    if ($candidates.Count -gt 0) {
        $picked = $candidates | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        return $picked.FullName
    }
    return $null
}

# Resolve script directory (same folder as .sqlproj)
$ScriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Definition }

# Paths relative to the script folder
$SqlProjPath = Join-Path $ScriptDir "LinaDb.sqlproj"
$OutputDir   = Join-Path $ScriptDir ("bin\" + $Configuration)

# Pick publish profile
if ($Profile -eq "Production") {
    $PublishProfile = Join-Path $ScriptDir "LinaDb.publish.xml"
} else {
    $PublishProfile = Join-Path $ScriptDir "LinaDb.Development.publish.xml"
}

# Resolve tools
$MSBuildExe = Resolve-MSBuild -HintPath $MSBuildPath
if (-not $MSBuildExe) {
    Write-Error "MSBuild.exe not found. Install Visual Studio Build Tools (MSBuild) or pass -MSBuildPath 'C:\Path\To\MSBuild.exe'."
    exit 1
}
$MSBuildExe = ($MSBuildExe.ToString()).Trim().Trim('"')

if ($Publish -or $GenerateScript) {
    $SqlPackageExe = Resolve-SqlPackage -HintPath $SqlPackagePath
    if (-not $SqlPackageExe) {
        Write-Error "SqlPackage.exe not found. Install SQL Server Data Tools / SqlPackage or pass -SqlPackagePath 'C:\Path\To\SqlPackage.exe'."
        exit 1
    }
    $SqlPackageExe = ($SqlPackageExe.ToString()).Trim().Trim('"')
}

Write-Host "Using MSBuild: $MSBuildExe"
if ($Publish -or $GenerateScript) { Write-Host "Using SqlPackage: $SqlPackageExe" }

# Step 1: Build (use Start-Process to avoid quoting issues)
Write-Host "Building SQL project: $SqlProjPath" -ForegroundColor Cyan
$buildArgs = @("$SqlProjPath", "/t:Build", "/p:Configuration=$Configuration")
$proc = Start-Process -FilePath $MSBuildExe -ArgumentList $buildArgs -NoNewWindow -PassThru -Wait
if ($proc.ExitCode -ne 0) {
    Write-Error "Build failed with exit code $($proc.ExitCode)"
    exit $proc.ExitCode
}

# Step 2: Locate DACPAC
$dacpac = Get-ChildItem -Path $OutputDir -Filter *.dacpac -Recurse -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $dacpac) {
    Write-Error "Could not find a DACPAC in $OutputDir (did the build succeed and produce a .dacpac?)"
    exit 1
}
Write-Host "Found DACPAC: $($dacpac.FullName)" -ForegroundColor Green

# Step 3: Generate script (optional)
if ($GenerateScript) {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $scriptFileName = "LinaDb_${Profile}_${timestamp}.sql"
    $scriptPath = Join-Path $ScriptDir $scriptFileName

    Write-Host "Generating deployment script: $scriptFileName" -ForegroundColor Cyan

    $scriptArgs = @(
        "/Action:Script",
        "/SourceFile:$($dacpac.FullName)",
        "/Profile:$PublishProfile",
        "/OutputPath:$scriptPath"
    )

    $procScript = Start-Process -FilePath $SqlPackageExe -ArgumentList $scriptArgs -NoNewWindow -PassThru -Wait
    if ($procScript.ExitCode -eq 0) {
        Write-Host "Script generated successfully: $scriptPath" -ForegroundColor Green
    } else {
        Write-Error "Script generation failed with exit code $($procScript.ExitCode)"
        exit $procScript.ExitCode
    }
}

# Step 4: Publish (optional)
if ($Publish) {
    if ($Profile -eq "Production" -and -not $Force) {
        $answer = Read-Host "You are about to publish to PRODUCTION using profile: $PublishProfile. Continue? (y/N)"
        if ($answer -notin @("y","Y","yes","YES")) {
            Write-Host "Publish canceled by user."
            exit 0
        }
    }

    Write-Host "Publishing using profile: $PublishProfile" -ForegroundColor Cyan
    $pubArgs = @("/Action:Publish", "/SourceFile:$($dacpac.FullName)", "/Profile:$PublishProfile")
    $proc2 = Start-Process -FilePath $SqlPackageExe -ArgumentList $pubArgs -NoNewWindow -PassThru -Wait
    if ($proc2.ExitCode -eq 0) {
        Write-Host "Publish completed successfully." -ForegroundColor Green
    } else {
        Write-Error "Publish failed with exit code $($proc2.ExitCode)"
        exit $proc2.ExitCode
    }
} else {
    if (-not $GenerateScript) {
        Write-Host "Skipping publish step (use -Publish to enable)." -ForegroundColor Yellow
    }
}
