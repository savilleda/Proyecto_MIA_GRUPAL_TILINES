@echo off
setlocal
set "MIA_SCRIPT_URL=https://raw.githubusercontent.com/savilleda/Proyecto_MIA_GRUPAL_TILINES/main/Proyecto_1/IniciarProyecto.ps1"
echo Descargando IniciarProyecto.ps1 en C:\mia_proyecto_I...
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -Command "$ErrorActionPreference = 'Stop'; try { [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; $raiz = 'C:\mia_proyecto_I'; New-Item -ItemType Directory -Path $raiz -Force | Out-Null; $script = Join-Path $raiz 'IniciarProyecto.ps1'; $respuesta = Invoke-WebRequest -UseBasicParsing -Uri $env:MIA_SCRIPT_URL -TimeoutSec 60; if ([string]::IsNullOrWhiteSpace($respuesta.Content)) { throw 'El PS1 descargado esta vacio.' }; [IO.File]::WriteAllText($script, [string]$respuesta.Content, [Text.Encoding]::UTF8); & powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File $script; exit $LASTEXITCODE } catch { Write-Host ('Error: ' + $_.Exception.Message) -ForegroundColor Red; exit 1 }"
set "resultado=%ERRORLEVEL%"
echo.
if not "%resultado%"=="0" echo El proceso fallo. Revisa el mensaje anterior.
pause
exit /b %resultado%