# PowerShell script to execute MySQL schema
# AI-Generated Code Header
# **Intent:** Run MySQL schema file to create cardgame database and tables
# **Safety:** Prompts for password securely, checks for errors

Write-Host "`n=== MySQL Schema Execution ===" -ForegroundColor Cyan

# Locate MySQL client
$mysqlPath = "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe"

if (-not (Test-Path $mysqlPath)) {
    Write-Host "❌ MySQL client not found at: $mysqlPath" -ForegroundColor Red
    Write-Host "   Please update the path in this script" -ForegroundColor Yellow
    exit 1
}

# Check if schema file exists
$schemaFile = "db\schema_mysql.sql"
if (-not (Test-Path $schemaFile)) {
    Write-Host "❌ Schema file not found: $schemaFile" -ForegroundColor Red
    exit 1
}

Write-Host "`n📋 Schema file found: $schemaFile" -ForegroundColor Green

# Get MySQL password securely
Write-Host "`n🔐 Enter MySQL root password:" -ForegroundColor Yellow
$password = Read-Host -AsSecureString
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
$plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

Write-Host "`n⚙️  Executing schema..." -ForegroundColor Cyan

# Execute the schema file
$processInfo = New-Object System.Diagnostics.ProcessStartInfo
$processInfo.FileName = $mysqlPath
$processInfo.Arguments = "-u root -p$plainPassword -e `"source $schemaFile`""
$processInfo.RedirectStandardOutput = $true
$processInfo.RedirectStandardError = $true
$processInfo.UseShellExecute = $false
$processInfo.CreateNoWindow = $true

$process = New-Object System.Diagnostics.Process
$process.StartInfo = $processInfo
$process.Start() | Out-Null

$stdout = $process.StandardOutput.ReadToEnd()
$stderr = $process.StandardError.ReadToEnd()
$process.WaitForExit()

# Clear password from memory
$plainPassword = $null
[System.GC]::Collect()

if ($process.ExitCode -eq 0) {
    Write-Host "`n✅ Schema executed successfully!" -ForegroundColor Green
    Write-Host "`n📊 Verifying database..." -ForegroundColor Cyan
    
    # Verify database was created
    Write-Host "`nCreated database and tables:" -ForegroundColor Yellow
    Write-Host "  - Database: cardgame" -ForegroundColor White
    Write-Host "  - Tables: roles, card_types, minigames, categories, abilities, factions" -ForegroundColor White
    Write-Host "           cards, players, decks, deck_cards" -ForegroundColor White
    Write-Host "           matches, match_players, turns, plays" -ForegroundColor White
    
    Write-Host "`n✨ Your database is ready!" -ForegroundColor Green
    Write-Host "`nNext steps:" -ForegroundColor Cyan
    Write-Host "  1. Open Unity" -ForegroundColor White
    Write-Host "  2. Create DatabaseConfig asset" -ForegroundColor White
    Write-Host "  3. Set: Host=localhost, Port=3306, Database=cardgame" -ForegroundColor White
    Write-Host "  4. Test connection" -ForegroundColor White
} else {
    Write-Host "`n❌ Error executing schema!" -ForegroundColor Red
    if ($stderr) {
        Write-Host "`nError details:" -ForegroundColor Yellow
        Write-Host $stderr -ForegroundColor Red
    }
    exit 1
}

