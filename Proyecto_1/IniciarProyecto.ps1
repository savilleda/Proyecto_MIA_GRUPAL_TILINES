[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$proyecto = Join-Path $PSScriptRoot "Proyecto_1.csproj"
$carpetaDatos = Join-Path "C:\" "GestionEstudiantes\Datos"

try {
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw "No se encontró .NET. Instalá el SDK de .NET 8 o una versión posterior."
    }

    if (-not (Test-Path -LiteralPath $proyecto -PathType Leaf)) {
        throw "No se encontró el archivo del proyecto: $proyecto"
    }

    New-Item -ItemType Directory -Path $carpetaDatos -Force | Out-Null

    # El programa detecta la carpeta personal y crea el XML al iniciar.
    $env:GESTION_ESTUDIANTES_DATOS = $carpetaDatos

    Push-Location -LiteralPath $PSScriptRoot
    try {
        & dotnet run --project $proyecto

        if ($LASTEXITCODE -ne 0) {
            throw "El programa terminó con el código de error $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}
catch {
    Write-Host ""
    Write-Host "No se pudo iniciar el programa:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Read-Host "Presioná ENTER para cerrar"
    exit 1
}
