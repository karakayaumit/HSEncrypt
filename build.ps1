param(
    [string]$Configuration = "Debug"
)

$project = "BankServiceViewer/BankServiceViewer.csproj"

Write-Host "Restoring NuGet packages for $project..." -ForegroundColor Cyan
dotnet restore $project

Write-Host "Building $project in $Configuration configuration..." -ForegroundColor Cyan
dotnet build $project -c $Configuration
