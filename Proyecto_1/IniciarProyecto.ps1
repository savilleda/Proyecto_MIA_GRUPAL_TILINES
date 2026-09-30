#Requires -Version 5.1
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$raiz = 'C:\mia_proyecto_I'
$url = 'https://github.com/savilleda/Proyecto_MIA_GRUPAL_TILINES.git'
$repositorio = Join-Path $raiz 'Proyecto_MIA_GRUPAL_TILINES'
$proyecto = Join-Path $repositorio 'Proyecto_1\Proyecto_1.csproj'
$salida = Join-Path $raiz 'ejecutable'

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
    New-Item -ItemType Directory -Path $salida -Force | Out-Null

    # Compila el .csproj directamente y conserva todos los archivos de salida.
    Push-Location -LiteralPath (Split-Path -Parent $proyecto)
    try {
        & dotnet build $proyecto --configuration Release --output $salida -p:UseAppHost=true
        if ($LASTEXITCODE -ne 0) { throw 'La compilacion fallo.' }
    }
    finally {
        Pop-Location
    }

    $exe = Join-Path $salida 'Proyecto_1.exe'
    if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
        throw "La compilacion no genero el ejecutable esperado: $exe"
    }
    Write-Host "Ejecutable generado: $exe" -ForegroundColor Green
    Write-Host 'Conserva los demas archivos de la carpeta ejecutable junto al EXE.'
    exit 0
}
catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
