#Requires -Version 5.1
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$raiz = 'C:\mia_proyecto_I'
$url = 'https://github.com/savilleda/Proyecto_MIA_GRUPAL_TILINES.git'
$repositorio = Join-Path $raiz 'Proyecto_MIA_GRUPAL_TILINES'
$proyecto = Join-Path $repositorio 'Proyecto_1\Proyecto_1.csproj'

try {
    foreach ($comando in @('git', 'gh', 'dotnet')) {
        if (-not (Get-Command $comando -CommandType Application -ErrorAction SilentlyContinue)) {
            throw "Falta $comando. Instala Git, GitHub CLI y el SDK de .NET 8 o posterior."
        }
    }

    $sdks = & dotnet --list-sdks
    if ($LASTEXITCODE -ne 0 -or -not ($sdks -match '^(8|9|[1-9][0-9]+)\.')) {
        throw 'Se necesita el SDK de .NET 8 o posterior para compilar.'
    }

    New-Item -ItemType Directory -Path $raiz -Force | Out-Null

    # Reutiliza la sesion de GitHub o solicita acceso mediante el navegador.
    & gh auth status --hostname github.com
    if ($LASTEXITCODE -ne 0) {
        & gh auth login --hostname github.com --git-protocol https --web
        if ($LASTEXITCODE -ne 0) { throw 'No se pudo autenticar en GitHub.' }
    }
    & gh auth setup-git --hostname github.com
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo configurar la autenticacion de Git.' }

    if (Test-Path -LiteralPath $repositorio) {
        # Permite repetir la compilacion sin borrar ni sobrescribir el codigo.
        if (-not (Test-Path -LiteralPath (Join-Path $repositorio '.git'))) {
            throw "La carpeta ya existe y no es un clon Git: $repositorio"
        }
        $origen = & git -C $repositorio remote get-url origin
        if ($LASTEXITCODE -ne 0 -or $origen -ne $url) {
            throw "La carpeta existente no corresponde al repositorio esperado: $repositorio"
        }
        Write-Host 'Se utilizara el repositorio local existente.'
    }
    else {
        & git clone --branch main --single-branch $url $repositorio
        if ($LASTEXITCODE -ne 0) { throw 'No se pudo clonar el repositorio.' }
    }

    if (-not (Test-Path -LiteralPath $proyecto -PathType Leaf)) {
        throw "No se encontro el proyecto: $proyecto"
    }
    # Guarda los datos en la misma carpeta base y abre el programa.
    $env:GESTION_ESTUDIANTES_DATOS = Join-Path $raiz 'Datos'
    New-Item -ItemType Directory -Path $env:GESTION_ESTUDIANTES_DATOS -Force | Out-Null
    Push-Location -LiteralPath (Split-Path -Parent $proyecto)
    try {
        & dotnet run --project $proyecto --configuration Release
        if ($LASTEXITCODE -ne 0) { throw "El programa fallo con el codigo $LASTEXITCODE." }
    }
    finally {
        Pop-Location
    }

    exit 0
}
catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
