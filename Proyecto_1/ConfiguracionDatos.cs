using System;
using System.IO;
using System.Text;


/*El cambio en esta clase nace de la curiosidad de poder ejecutar el .exe 
para que el mismo cree sus necesidades en cualquier entorno. Sin la necesidad de estar 
en la carpeta(s) del programa.
*/
namespace GestionEstudiantes
{
    public static class ConfiguracionDatos
    {
        // Nombre del archivo XML principal del sistema.
        private const string NOMBRE_XML = "prueba_XML.xml";

        // Carpeta donde la aplicación almacenará sus archivos.
        public static string CarpetaDatos { get; private set; } = string.Empty;


        // Prepara la carpeta de datos del programa.
        public static void PrepararUbicacionDatos()
        {
            // El script de inicio puede indicar una carpeta de datos concreta.
            // Si la aplicación se abre directamente, usamos AppData como respaldo.
            string? carpetaConfigurada = Environment.GetEnvironmentVariable(
                "GESTION_ESTUDIANTES_DATOS"
            );

            if (!string.IsNullOrWhiteSpace(carpetaConfigurada))
            {
                CarpetaDatos = Path.GetFullPath(carpetaConfigurada);
            }
            else
            {
                string appData = Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData
                );

                CarpetaDatos = Path.Combine(
                    appData,
                    "GestionEstudiantes",
                    "Datos"
                );
            }

            // Si la carpeta todavía no existe, la creamos y la indicamos.
            //Si ya existe, lo indicamos.
            if (Directory.Exists(CarpetaDatos))
            {
                Console.WriteLine("La carpeta de datos ya existe:");
            }
            else
            {
                Directory.CreateDirectory(CarpetaDatos);
                Console.WriteLine("Carpeta de datos creada:");
            }

            Console.WriteLine(CarpetaDatos);
            Console.WriteLine();

            // Conservamos la variable para las clases que ya la utilizan.
            Environment.SetEnvironmentVariable(
                "GESTION_ESTUDIANTES_DATOS",
                CarpetaDatos,
                EnvironmentVariableTarget.Process
            );
        }

                // Devuelve la ruta completa del XML principal.
        public static string PrepararRutaXml()
        {
            // Si todavía no hemos preparado la carpeta, lo hacemos ahora.
            if (string.IsNullOrWhiteSpace(CarpetaDatos))
            {
                PrepararUbicacionDatos();
            }

            return Path.Combine(
                CarpetaDatos,
                NOMBRE_XML
            );
        }
    }
}
