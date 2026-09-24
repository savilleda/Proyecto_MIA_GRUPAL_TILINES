using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace GestionEstudiantes
{
    public class MenuPrincipal
    {
        // Punto de entrada de la aplicación. El menú también se encarga de
        // preparar las clases de persistencia y las operaciones del sistema
        public static void Main(string[] args)
        {
            string rutaArchivo;
            try
            {
                rutaArchivo = ConfiguracionDatos.PrepararRutaXml();
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is DirectoryNotFoundException ||
                                       ex is InvalidOperationException)
            {
                Console.WriteLine($"No se pudo preparar la ubicación de datos: {ex.Message}");
                return;
            }

            ManejadorXML manejador = new ManejadorXML(rutaArchivo);
            OperacionesEstudiante operaciones = new OperacionesEstudiante(manejador);
            MenuPrincipal menu = new MenuPrincipal(operaciones);
            Console.WriteLine($"Los datos se guardan en: {rutaArchivo}");
            menu.Ejecutar();
        }

        // El menú delega las operaciones del sistema a esta clase
        private readonly OperacionesEstudiante operaciones;

        // Recibimos las operaciones ya configuradas para trabajar con los estudiantes
        public MenuPrincipal(OperacionesEstudiante operaciones)
        {
            // Evitamos iniciar el menú sin la lógica necesaria
            if (operaciones == null)
            {
                throw new ArgumentNullException(
                    nameof(operaciones),
                    "Las operaciones del sistema no pueden ser nulas."
                );
            }

            this.operaciones = operaciones;
        }

        //Ejecutrar mantiene el programa funcionando hasta que el usuario desee salir
        public void Ejecutar()
        {
            //Variable para almacenar la opcion del usuario
            string opcion;

            do
            {
                //Limpiamos la consola
                LimpiarConsola();
                //Mostramos el menu principal
                MostrarMenu();

                //Damos las instrucciones de seleccion de opcion
                Console.Write("Seleccione una opcion:");
                //Leemos la opsion del usuario, prevenimos que sea nula y eliminamos espacios en blanco al inicio y al final
                opcion = (Console.ReadLine() ?? string.Empty).Trim();

                //limpiamos la consola
                LimpiarConsola();

                //Switch para evaluar la accion seleccinada
                switch (opcion)
                {
                    case "1":
                        RegistrarEstudiante();
                        break;

                    case "2":
                        BuscarEstudiante();
                        break;

                    case "3":
                        ModificarEstudiante();
                        break;
                    
                    case "4":
                        EliminarEstudiante();
                        break;
                    
                    case "5":
                        ListarEstudiantes();
                        break;

                    case "6":
                        CifrarXml();
                        break;

                    case "7":
                        DescifrarXml();
                        break;
                    
                    case "0":
                        Console.WriteLine("Saliendo del sistema...");
                        break;
                    
                    default:
                        Console.WriteLine("Opcion invalida. Por favor, seleccione una opcion valida.");
                        break;
                }

                //Si la opcion es diferetne de 0, pausamos la ejecucion para que el usuario pueda ver el resultado de la accion y presionar ENTER para continuar cuando lo desee
                if(opcion != "0")
                {
                    Pausar();
                }

            } while(opcion != "0");//Detenemos el programa cuando la opcion del usuario sea 0
        }
        //Mostramos las opciones principales del sistema
        private void MostrarMenu()
        {
            Console.WriteLine("=============================================");
            Console.WriteLine("     SISTEMA DE GESTION DE ESTUDIANTES");
            Console.WriteLine("=============================================");
            Console.WriteLine("1. Registrar estudiante");
            Console.WriteLine("2. Buscar estudiante por no. de carnet");
            Console.WriteLine("3. Modificar estudiante");
            Console.WriteLine("4. Eliminar estudiante");
            Console.WriteLine("5. Listar estudiantes");
            Console.WriteLine("6. Cifrar XML actual");
            Console.WriteLine("7. Descifrar copia AES");
            Console.WriteLine("0. Salir");
            Console.WriteLine("=============================================");
        }

        //Registrar estudiante permite al usuario ingresar los datos de un nuevo estudiante y agregarlo a la lista
        // Solicita los datos y delega el registro del estudiante
        private void RegistrarEstudiante()
        {
            Console.WriteLine("=== REGISTRO DE ESTUDIANTE ===\n");

            // Obtenemos los datos ingresados por el usuario
            Estudiante nuevoEstudiante = LeerDatosEstudiante();

            // OperacionesEstudiante se encarga de validar y guardar
            var resultado =
                operaciones.RegistrarEstudiante(nuevoEstudiante);

            // Si ocurrió algún problema, mostramos los errores recibidos
            if (!resultado.exito)
            {
                MostrarErrores(resultado.errores);
                return;
            }

            Console.WriteLine("\nEstudiante registrado exitosamente.");
        }

        // Busca un estudiante utilizando su carné como identificador
        private void BuscarEstudiante()
        {
            Console.WriteLine("=== BÚSQUEDA DE ESTUDIANTE ===\n");

            Console.Write("Ingrese el carné del estudiante: ");
            string carne = (Console.ReadLine() ?? string.Empty).Trim();

            // La búsqueda se realiza desde OperacionesEstudiante
            var resultado =
                operaciones.BuscarEstudiantePorCarne(carne);

            // Si no se encontró o hubo un error, mostramos el mensaje recibido
            if (!resultado.encontrado)
            {
                MostrarErrores(resultado.errores);
                return;
            }

            if (resultado.estudiante == null)
            {
                Console.WriteLine("\nNo se encontró el estudiante.");
                return;
            }

            Console.WriteLine("\nEstudiante encontrado:");
            MostrarEstudiante(resultado.estudiante);
        }

        //Modificacion de la informacion de un estudiante existe
        // Permite modificar la información de un estudiante existente
        private void ModificarEstudiante()
        {
            Console.WriteLine("=== MODIFICACIÓN DE ESTUDIANTE ===\n");

            Console.Write("Ingrese el carné del estudiante: ");
            string carne = (Console.ReadLine() ?? string.Empty).Trim();

            // Primero buscamos al estudiante para obtener sus datos actuales
            var busqueda =
                operaciones.BuscarEstudiantePorCarne(carne);

            if (!busqueda.encontrado)
            {
                MostrarErrores(busqueda.errores);
                return;
            }

            if (busqueda.estudiante == null)
            {
                Console.WriteLine("\nNo se encontró el estudiante.");
                return;
            }

            Estudiante estudianteActual = busqueda.estudiante;

            Console.WriteLine("\nDatos actuales:");
            MostrarEstudiante(estudianteActual);

            Console.WriteLine("\nIngrese los nuevos datos.");
            Console.WriteLine(
                "Presione ENTER para conservar el valor actual.\n"
            );

            // Si se deja un campo vacío, mantenemos el valor anterior
            string nombres =
                LeerValorActual("Nombres", estudianteActual.Nombres);

            string apellidos =
                LeerValorActual("Apellidos", estudianteActual.Apellidos);

            string carrera =
                LeerValorActual("Carrera", estudianteActual.Carrera);

            string correo =
                LeerValorActual("Correo", estudianteActual.Correo);

            // El carné se conserva porque funciona como identificador único
            Estudiante estudianteModificado =
                new Estudiante(
                    estudianteActual.Carne,
                    nombres,
                    apellidos,
                    carrera,
                    correo
                );

            // La actualización y persistencia se delegan a OperacionesEstudiante
            var resultado =
                operaciones.ActualizarEstudiante(
                    estudianteActual.Carne,
                    estudianteModificado
                );

            if (!resultado.exito)
            {
                MostrarErrores(resultado.errores);
                return;
            }

            Console.WriteLine(
                "\nEstudiante modificado correctamente."
            );
        }

        // Elimina un estudiante después de comprobar que exista
        // Busca al estudiante y solicita confirmación antes de eliminarlo
        private void EliminarEstudiante()
        {
            Console.WriteLine("=== ELIMINACIÓN DE ESTUDIANTE ===\n");

            Console.Write("Ingrese el carné del estudiante: ");
            string carne = (Console.ReadLine() ?? string.Empty).Trim();

            // Primero buscamos al estudiante para mostrar sus datos
            var busqueda =
                operaciones.BuscarEstudiantePorCarne(carne);

            if (!busqueda.encontrado)
            {
                MostrarErrores(busqueda.errores);
                return;
            }

            if (busqueda.estudiante == null)
            {
                Console.WriteLine("\nNo se encontró el estudiante.");
                return;
            }

            Console.WriteLine("\nEstudiante encontrado:\n");
            MostrarEstudiante(busqueda.estudiante);

            Console.Write(
                "\n¿Desea eliminar este estudiante? (S/N): "
            );

            string confirmacion =
                (Console.ReadLine() ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            if (confirmacion != "S")
            {
                Console.WriteLine("\nEliminación cancelada.");
                return;
            }

            // La eliminación y actualización del archivo se hacen
            // desde OperacionesEstudiante.
            var resultado =
                operaciones.EliminarEstudiante(carne);

            if (!resultado.exito)
            {
                MostrarErrores(resultado.errores);
                return;
            }

            Console.WriteLine(
                "\nEstudiante eliminado correctamente."
            );
        }


        // Obtiene y muestra todos los estudiantes registrados
        private void ListarEstudiantes()
        {
            Console.WriteLine("=== LISTADO DE ESTUDIANTES ===\n");

            // La lista se obtiene directamente desde la persistencia
            List<Estudiante> estudiantes =
                operaciones.ListarEstudiantes();

            if (!operaciones.LecturaValida)
            {
                Console.WriteLine(operaciones.ErrorLectura);
                return;
            }

            if (estudiantes.Count == 0)
            {
                Console.WriteLine(
                    "No existen estudiantes registrados."
                );

                return;
            }

            Console.WriteLine(
                "Total de estudiantes: " + estudiantes.Count
            );

            Console.WriteLine();

            // Mostramos cada estudiante recuperado
            foreach (Estudiante estudiante in estudiantes)
            {
                MostrarEstudiante(estudiante);

                Console.WriteLine(
                    "------------------------------------------"
                );
            }
        }

        // Cifra una copia del XML principal sin modificarlo
        private void CifrarXml()
        {
            Console.WriteLine("=== CIFRAR XML ===\n");
            string rutaXml;
            try
            {
                rutaXml = ObtenerRutaXml();
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is InvalidOperationException)
            {
                Console.WriteLine($"No se pudo ubicar el archivo de datos: {ex.Message}");
                return;
            }

            string rutaCifrada = rutaXml + ".aes";

            if (!File.Exists(rutaXml))
            {
                Console.WriteLine("No existe el archivo XML principal.");
                return;
            }

            string contrasena = LeerContrasena("Ingrese la contraseña: ");
            string confirmacion = LeerContrasena("Confirme la contraseña: ");
            if (!string.Equals(contrasena, confirmacion, StringComparison.Ordinal))
            {
                Console.WriteLine("\nLas contraseñas no coinciden.");
                return;
            }

            if (File.Exists(rutaCifrada) &&
                !Confirmar($"El archivo {rutaCifrada} ya existe. ¿Desea reemplazarlo? (S/N): "))
            {
                Console.WriteLine("\nNo se reemplazó la copia cifrada.");
                return;
            }

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                Console.WriteLine("\nLa contraseña no puede estar vacía.");
                return;
            }

            try
            {
                byte[] xml = File.ReadAllBytes(rutaXml);
                byte[] cifrado = CifradorAES.Encriptar(xml, contrasena);
                GuardarArchivoTemporal(rutaCifrada, cifrado);
                Console.WriteLine($"\nCopia cifrada guardada en: {rutaCifrada}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"\nNo se pudo leer o guardar el archivo: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"\nNo hay permisos para usar el archivo: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"\nNo se pudo cifrar el XML: {ex.Message}");
            }
        }

        // Descifra la copia AES en un archivo nuevo sin reemplazar el XML principal
        private void DescifrarXml()
        {
            Console.WriteLine("=== DESCIFRAR COPIA AES ===\n");
            string rutaCifrada;
            try
            {
                rutaCifrada = ObtenerRutaXml() + ".aes";
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is InvalidOperationException)
            {
                Console.WriteLine($"No se pudo ubicar el archivo de datos: {ex.Message}");
                return;
            }

            string rutaDescifrada = Path.Combine(
                Path.GetDirectoryName(rutaCifrada)!,
                "prueba_XML_descifrado.xml");

            if (!File.Exists(rutaCifrada))
            {
                Console.WriteLine("No existe la copia cifrada prueba_XML.xml.aes.");
                return;
            }

            string contrasena = LeerContrasena("Ingrese la contraseña: ");
            if (string.IsNullOrWhiteSpace(contrasena))
            {
                Console.WriteLine("\nLa contraseña no puede estar vacía.");
                return;
            }

            if (File.Exists(rutaDescifrada) &&
                !Confirmar($"El archivo {rutaDescifrada} ya existe. ¿Desea reemplazarlo? (S/N): "))
            {
                Console.WriteLine("\nNo se reemplazó el XML descifrado.");
                return;
            }

            try
            {
                byte[] cifrado = File.ReadAllBytes(rutaCifrada);
                byte[] xml = CifradorAES.Desencriptar(cifrado, contrasena);
                GuardarArchivoTemporal(rutaDescifrada, xml);
                Console.WriteLine($"\nXML descifrado guardado en: {rutaDescifrada}");
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine($"\nNo se pudo descifrar: {ex.Message}");
            }
            catch (InvalidDataException ex)
            {
                Console.WriteLine($"\nLa copia cifrada está dañada o no tiene un formato válido: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"\nNo se pudo leer o guardar el archivo: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"\nNo hay permisos para usar el archivo: {ex.Message}");
            }
        }

        private static string ObtenerRutaXml() => ConfiguracionDatos.PrepararRutaXml();

        // Reemplaza el archivo de salida solo después de terminar la escritura
        private static void GuardarArchivoTemporal(string rutaDestino, byte[] datos)
        {
            string rutaTemporal = rutaDestino + ".tmp";
            try
            {
                File.WriteAllBytes(rutaTemporal, datos);
                File.Move(rutaTemporal, rutaDestino, true);
            }
            finally
            {
                if (File.Exists(rutaTemporal))
                {
                    File.Delete(rutaTemporal);
                }
            }
        }

        // Oculta la contraseña cuando la consola permite leer teclas
        private static string LeerContrasena(string mensaje)
        {
            Console.Write(mensaje);
            if (Console.IsInputRedirected)
            {
                return Console.ReadLine() ?? string.Empty;
            }

            List<char> caracteres = new List<char>();
            ConsoleKeyInfo tecla;
            do
            {
                tecla = Console.ReadKey(intercept: true);
                if (tecla.Key == ConsoleKey.Backspace && caracteres.Count > 0)
                {
                    caracteres.RemoveAt(caracteres.Count - 1);
                }
                else if (tecla.Key != ConsoleKey.Enter &&
                         !char.IsControl(tecla.KeyChar))
                {
                    caracteres.Add(tecla.KeyChar);
                }
            } while (tecla.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return new string(caracteres.ToArray());
        }

        private static bool Confirmar(string mensaje)
        {
            Console.Write(mensaje);
            string respuesta = (Console.ReadLine() ?? string.Empty).Trim();
            return respuesta.Equals("S", StringComparison.OrdinalIgnoreCase);
        }


        // Solicita la información para crear un nuevo estudiante
        private Estudiante LeerDatosEstudiante()
        {
            Console.Write("Carné: ");
            string carne = Console.ReadLine() ?? string.Empty;

            Console.Write("Nombres: ");
            string nombres = Console.ReadLine() ?? string.Empty;

            Console.Write("Apellidos: ");
            string apellidos = Console.ReadLine() ?? string.Empty;

            Console.Write("Carrera: ");
            string carrera = Console.ReadLine() ?? string.Empty;

            Console.Write("Correo: ");
            string correo = Console.ReadLine() ?? string.Empty;

            return new Estudiante(
                carne,
                nombres,
                apellidos,
                carrera,
                correo
            );
        }


        // Permite conservar el valor anterior durante una modificación
        private string LeerValorActual(string campo, string valorActual)
        {
            Console.Write(campo + " [" + valorActual + "]: ");

            string nuevoValor =
                (Console.ReadLine() ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(nuevoValor))
            {
                return valorActual;
            }

            return nuevoValor;
        }


        // Muestra toda la información de un estudiante
        private void MostrarEstudiante(Estudiante estudiante)
        {
            Console.WriteLine("Carné: " + estudiante.Carne);
            Console.WriteLine("Nombre: " + estudiante.NombreCompleto);
            Console.WriteLine("Carrera: " + estudiante.Carrera);
            Console.WriteLine("Correo: " + estudiante.Correo);
        }


        // Muestra los errores detectados por la clase Validador
        private void MostrarErrores(List<string> errores)
        {
            Console.WriteLine("\nNo se pudo realizar la operación:");

            foreach (string error in errores)
            {
                Console.WriteLine("- " + error);
            }
        }


        // Evita que la consola cambie inmediatamente de pantalla
        private void Pausar()
        {
            Console.WriteLine("\nPresione ENTER para continuar...");
            Console.ReadLine();
        }

        // Console.Clear no está disponible cuando la salida está redirigida
        // (por ejemplo, al ejecutar pruebas automatizadas)
        private void LimpiarConsola()
        {
            if (!Console.IsOutputRedirected)
            {
                Console.Clear();
            }
        }
    }

}