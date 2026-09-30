using System;
using System.IO;
using System.Security.Cryptography;

namespace GestionEstudiantes
{
    // Clase de arranque del programa. Solo contiene Main y lo necesario para
    // pedir la contraseña y abrir el archivo cifrado. El menú está en MenuPrincipal.
    internal static class Proyecto_1
    {
        // Punto de entrada de la aplicación. Prepara la contraseña, el archivo
        // cifrado y las clases de persistencia, y luego abre el menú.
        public static void Main(string[] args)
        {
            try
            {
                Console.Title = "Sistema de Gestión de Estudiantes";

                // Prepara la carpeta de datos y devuelve la ruta completa del XML
                string rutaArchivo = ConfiguracionDatos.PrepararRutaXml();

                // Pide la contraseña (o la crea) y abre el archivo.
                // Devuelve null si el usuario sale o si no se pudo continuar
                // (el motivo ya se mostró en pantalla).
                ManejadorXML? manejador = AbrirManejador(rutaArchivo);

                if (manejador == null)
                {
                    return;
                }

                // Comprueba que el archivo guardado puede descifrarse y leerse.
                manejador.LeerEstudiantes();

                if (!manejador.UltimaLecturaExitosa)
                {
                    Console.WriteLine(manejador.UltimoErrorLectura);
                    Console.WriteLine("Presione ENTER para cerrar.");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine();
                Console.WriteLine(
                    "Datos descifrados en memoria correctamente."
                );
                Console.WriteLine(
                    "El archivo permanece cifrado en la carpeta de datos."
                );
                Console.WriteLine(
                    "Presione ENTER para abrir el menú principal."
                );
                Console.ReadLine();

                // Todo está listo: creamos las clases y abrimos el menú
                var operaciones = new OperacionesEstudiante(manejador);

                var menu = new MenuPrincipal(
                    operaciones,
                    manejador
                );

                menu.Ejecutar();
            }
            // Solo se capturan errores esperables (archivos, permisos, cifrado).
            // Cualquier otro error inesperado no se oculta.
            catch (Exception ex) when (
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is InvalidOperationException ||
                ex is ArgumentException ||
                ex is System.Security.SecurityException ||
                ex is CryptographicException)
            {
                Console.WriteLine();
                Console.WriteLine("No se pudo iniciar el sistema:");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Presione ENTER para cerrar.");
                Console.ReadLine();
            }
        }

        // Pide la contraseña (o crea una nueva) hasta lograr abrir el archivo.
        // Devuelve null si el usuario decide salir o si el problema no se puede corregir.
        private static ManejadorXML? AbrirManejador(string rutaArchivo)
        {
            // Revisamos en qué estado está el archivo para saber qué mensaje mostrar
            // y si debemos pedir una contraseña existente o crear una nueva
            bool archivoExiste = File.Exists(rutaArchivo);

            bool archivoCifrado = archivoExiste &&
                CifradorAES.EsArchivoCifrado(File.ReadAllBytes(rutaArchivo));

            MostrarMensajeInicial(archivoExiste, archivoCifrado);

            // Se repite hasta lograr abrir el archivo o hasta que el usuario decida salir
            while (true)
            {
                string contrasena;

                if (archivoCifrado)
                {
                    // Archivo cifrado: pedimos la contraseña existente.
                    // ENTER sin escribir nada cierra el programa.
                    contrasena = EntradaConsola.LeerContrasena(
                        "Contraseña (ENTER para salir): "
                    );

                    if (string.IsNullOrWhiteSpace(contrasena))
                    {
                        Console.WriteLine("\nSaliendo del sistema...");
                        Console.WriteLine("Presione ENTER para cerrar.");
                        Console.ReadLine();
                        return null;
                    }
                }
                else
                {
                    // Archivo nuevo o sin cifrar: pedimos crear una contraseña nueva.
                    // Devuelve null si el usuario cancela.
                    string? nuevaContrasena =
                        EntradaConsola.SolicitarNuevaContrasena();

                    if (nuevaContrasena == null)
                    {
                        return null;
                    }

                    contrasena = nuevaContrasena;
                }

                var manejador = new ManejadorXML(
                    rutaArchivo,
                    contrasena
                );

                // InicializarArchivo crea el archivo, lo migra o comprueba la contraseña.
                // Si tiene éxito, devolvemos el manejador ya listo.
                if (manejador.InicializarArchivo())
                {
                    return manejador;
                }

                Console.WriteLine();
                Console.WriteLine(manejador.UltimoErrorLectura);

                // Un archivo nuevo o un XML legible no falla por
                // una contraseña anterior: el problema debe corregirse.
                if (!archivoCifrado)
                {
                    Console.WriteLine(
                        "No se abrirá el menú porque no se pudo preparar el archivo."
                    );
                    Console.WriteLine("Presione ENTER para cerrar.");
                    Console.ReadLine();
                    return null;
                }

                // Si el archivo estaba cifrado, lo más probable es que la contraseña
                // sea incorrecta, así que se permite intentar de nuevo.
                Console.WriteLine(
                    "Puede intentar otra contraseña o presionar ENTER para salir."
                );
                Console.WriteLine();
            }
        }

        // Muestra el mensaje inicial según el estado del archivo de datos
        private static void MostrarMensajeInicial(bool archivoExiste, bool archivoCifrado)
        {
            // Caso 1: no existe el archivo (primera ejecución)
            if (!archivoExiste)
            {
                Console.WriteLine("Primera ejecución: se creará un archivo sin estudiantes.");
                Console.WriteLine("Cree una contraseña para proteger sus datos.");
            }
            // Caso 2: ya existe un archivo cifrado con nuestro formato
            else if (archivoCifrado)
            {
                Console.WriteLine("Se encontró un archivo cifrado.");
                Console.WriteLine("Ingrese su contraseña para acceder.");
            }
            // Caso 3: existe un archivo pero no tiene el encabezado de nuestro cifrado
            // (por ejemplo, un XML antiguo sin cifrar). Se migra al formato cifrado.
            else
            {
                Console.WriteLine("Se encontró un archivo sin el encabezado de cifrado.");
                Console.WriteLine("Si contiene un XML válido, se conservarán sus datos.");
                Console.WriteLine("Cree una contraseña para guardarlo cifrado.");
            }
        }
    }
}