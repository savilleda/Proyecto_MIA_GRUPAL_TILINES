# Nos ubicamos en la carpeta donde está guardado este script.
Set-Location -LiteralPath $PSScriptRoot

# Usamos Datos junto al script para que el XML visible en VS Code sea el activo.
$env:GESTION_ESTUDIANTES_DATOS = Join-Path $PSScriptRoot "Datos"

# Creamos la carpeta si todavía no existe.
New-Item -ItemType Directory -Path $env:GESTION_ESTUDIANTES_DATOS -Force | Out-Null

# Abrimos la carpeta para ver el XML y las copias cifradas.
explorer.exe $env:GESTION_ESTUDIANTES_DATOS

# Ejecutamos el proyecto.
dotnet run --project (Join-Path $PSScriptRoot "Proyecto_1.csproj")

# Mantenemos la ventana abierta para poder leer cualquier mensaje.
Read-Host "Presioná ENTER para cerrar"
