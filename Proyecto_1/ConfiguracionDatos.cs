using System;
using System.IO;
using System.Text;

namespace GestionEstudiantes
{
    public static class ConfiguracionDatos
    {
        private const string NombreArchivoXml = "prueba_XML.xml";
        private const string VariableRutaPruebas = "GESTION_ESTUDIANTES_DATOS";

        // Obtiene la carpeta Datos o la carpeta indicada explícitamente para pruebas
        public static string ObtenerDirectorioDatos()
        {
            string? rutaPruebas = Environment.GetEnvironmentVariable(VariableRutaPruebas);
            string directorio = string.IsNullOrWhiteSpace(rutaPruebas)
                ? Path.Combine(EncontrarCarpetaProyecto(), "Datos")
                : rutaPruebas;

            Directory.CreateDirectory(directorio);
            return directorio;
        }

        // Devuelve el XML activo sin migrar ni reemplazar datos automáticamente
        public static string PrepararRutaXml()
        {
            string rutaXml = Path.Combine(ObtenerDirectorioDatos(), NombreArchivoXml);
            if (!File.Exists(rutaXml))
            {
                File.WriteAllText(
                    rutaXml,
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<estudiantes />",
                    new UTF8Encoding(false));
            }

            return rutaXml;
        }

        // Busca Proyecto_1.csproj desde la carpeta del ejecutable hacia arriba
        private static string EncontrarCarpetaProyecto()
        {
            DirectoryInfo? carpeta = new DirectoryInfo(AppContext.BaseDirectory);
            while (carpeta != null)
            {
                if (File.Exists(Path.Combine(carpeta.FullName, "Proyecto_1.csproj")))
                {
                    return carpeta.FullName;
                }

                carpeta = carpeta.Parent;
            }

            throw new DirectoryNotFoundException(
                "No se encontró Proyecto_1.csproj desde la ubicación del ejecutable");
        }
    }
}
