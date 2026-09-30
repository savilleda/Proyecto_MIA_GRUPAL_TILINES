@echo off
setlocal

rem Descarga y ejecuta la version guardada en la rama main de GitHub.
set "MIA_SCRIPT_URL=https://raw.githubusercontent.com/savilleda/Proyecto_MIA_GRUPAL_TILINES/main/Proyecto_1/IniciarProyecto.ps1"

echo Descargando IniciarProyecto.ps1 desde GitHub...
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -Command "$ErrorActionPreference = 'Stop'; $resultado = 1; $temporal = Join-Path ([IO.Path]::GetTempPath()) ('MIA-' + [guid]::NewGuid().ToString('N')); try { [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; New-Item -ItemType Directory -Path $temporal | Out-Null; $script = Join-Path $temporal 'IniciarProyecto.ps1'; Invoke-WebRequest -UseBasicParsing -Uri $env:MIA_SCRIPT_URL -OutFile $script -TimeoutSec 60; if ((Get-Item -LiteralPath $script).Length -eq 0) { throw 'El archivo descargado esta vacio.' }; & powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File $script; $resultado = $LASTEXITCODE } catch { Write-Host ('Error: ' + $_.Exception.Message) -ForegroundColor Red } finally { if (Test-Path -LiteralPath $temporal) { Remove-Item -LiteralPath $temporal -Recurse -Force -ErrorAction SilentlyContinue } }; exit $resultado"
set "resultado=%ERRORLEVEL%"

echo.
if "%resultado%"=="0" (
    echo Proceso completado correctamente.
) else (
    echo El proceso fallo. Codigo de error: %resultado%
)

pause
exit /b %resultado%
