using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

/*DE MARCO: elimine cifrar y descfifrar de la clase. Ahora se ejecutan automaticamente, no con opcion al usuario. 
Esto para menetener la idea de la seguridad de los datos y mantener un proyecto mas limpio :)
*/

namespace GestionEstudiantes
{
    public class MenuPrincipal
    {
        // Punto de entrada de la aplicación. El menú también se encarga de
        // preparar las clases de persistencia y las operaciones del sistema
        //Aqui tambien se encuentra el main
        public static void Main(string[] args)
        {
            try
            {
                Console.Title = "Sistema de Gestion de Estudiantes";

                //Preparamos la carpeta y devolvemos la ruta completa
                string rutaArchivo = ConfiguracionDatos.PrepararRutaXml();

                bool archivoExiste = File.Exists(rutaArchivo);

                bool archivoCifrado = archivoExiste && 
                CifradorAES.EsArchivoCifrado(File.ReadAllBytes(rutaArchivo));

                //Si no existe el archivo
                if (!archivoExiste)
                {
                    Console.WriteLine("Primera ejecucion: se creara un archivo sinn estudiantes.");
                        Console.WriteLine(
                    "Cree una contraseña para proteger sus datos."
                    );
                }
                //Si ya existe un archivo cifrado
                else if (archivoCifrado)
                {
                        Console.WriteLine(
                    "Se encontró un archivo cifrado."
                    );
                    Console.WriteLine(
                    "Ingrese su contraseña para acceder."
                    );
                }
                //Existe un archivo pero no tiene el encabezado de nuestro cifrado.
                else
                {
                    Console.WriteLine(
                        "Se encontró un archivo sin el encabezado de cifrado."
                    );
                    Console.WriteLine(
                        "Si contiene un XML válido, se conservarán sus datos."
                    );
                    Console.WriteLine(
                        "Cree una contraseña para guardarlo cifrado."
                    );
                }

                    ManejadorXML manejador;

                while (true)
                {
                    string contrasena;

                    if (archivoCifrado)
                    {
                        contrasena = LeerContrasena(
                            "Contraseña (ENTER para salir): "
                        );

                        if (string.IsNullOrWhiteSpace(contrasena))
                        {
                            return;
                        }
                    }
                    else
                    {
                        string? nuevaContrasena =
                            SolicitarNuevaContrasena();

                        if (nuevaContrasena == null)
                        {
                            return;
                        }

                        contrasena = nuevaContrasena;
                    }

                    manejador = new ManejadorXML(
                        rutaArchivo,
                        contrasena
                    );

                    if (manejador.InicializarArchivo())
                    {
                        break;
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
                        return;
                    }

                    Console.WriteLine(
                        "Puede intentar otra contraseña o presionar ENTER para salir."
                    );
                    Console.WriteLine();
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

                var operaciones = new OperacionesEstudiante(manejador);

                var menu = new MenuPrincipal(
                    operaciones,
                    manejador
                );

                menu.Ejecutar();
            }
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

        // Clase encargada de realizar las operaciones con los estudiantes.
        private readonly OperacionesEstudiante operaciones;
        private readonly ManejadorXML manejador;

        // Recibimos las operaciones ya preparadas desde Main.
       public MenuPrincipal(OperacionesEstudiante operaciones, ManejadorXML manejador)
        {
            this.operaciones = operaciones
                ?? throw new ArgumentNullException(nameof(operaciones));

            this.manejador = manejador
                ?? throw new ArgumentNullException(nameof(manejador));
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
                string? entrada = Console.ReadLine();

                // Si la consola se cierra o deja de enviar datos, terminamos
                // limpiamente en vez de repetir el menú indefinidamente.
                opcion = entrada == null ? "0" : entrada.Trim();

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
                        CambiarContrasena();
                        break;
                    
                    case "0":
                        Console.WriteLine("Saliendo del sistema. Los cambios guardados permanecen cifrados.");
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
            Console.WriteLine("6. Cambiar contrasena");
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

            Console.Clear();
            Console.WriteLine("\nDatos actuales:");
            MostrarEstudiante(estudianteActual);

            while (true)
            {
                Console.WriteLine("\n1. Nombres\n2. Apellidos\n3. Carrera\n4. Correo\n0. Volver al menú principal");
                Console.Write("Seleccione el campo: ");
                string? opcion = Console.ReadLine()?.Trim();
                if (opcion == null || opcion == "0") return;
                if (opcion != "1" && opcion != "2" && opcion != "3" && opcion != "4")
                {
                    Console.WriteLine("Opción inválida.");
                    continue;
                }
                while (true)
                {
                    // La copia evita cambiar los datos actuales antes de validar y guardar.
                    var candidato = new Estudiante(estudianteActual.Carne, estudianteActual.Nombres,
                        estudianteActual.Apellidos, estudianteActual.Carrera, estudianteActual.Correo);
                    string campo = opcion switch { "1" => "Nombres", "2" => "Apellidos", "3" => "Carrera", _ => "Correo" };
                    Console.Write(campo + " (ENTER para cancelar): ");
                    string? valor = Console.ReadLine()?.Trim();
                    if (valor == null) return;
                    if (valor.Length == 0) break;
                    switch (opcion)
                    {
                        case "1": candidato.Nombres = valor; break;
                        case "2": candidato.Apellidos = valor; break;
                        case "3": candidato.Carrera = valor; break;
                        case "4": candidato.Correo = valor; break;
                    }
                    var errores = Validador.ValidarDatos(candidato);
                    if (errores.Count > 0)
                    {
                        MostrarErrores(errores);
                        Console.WriteLine("Intente nuevamente.");
                        continue;
                    }
                    var resultado = operaciones.ActualizarEstudiante(estudianteActual.Carne, candidato);
                    if (!resultado.exito) { MostrarErrores(resultado.errores); break; }
                    estudianteActual = candidato;
                    Console.WriteLine("Estudiante modificado correctamente.");
                    MostrarEstudiante(estudianteActual);
                    break;
                }
            }
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

            string correo;

            while (true)
            {
                Console.Write("Correo :");
                string? entrada = Console.ReadLine();

                //Si por cualquier cosa se cierra la entrada de la consola, evitamos que haya un ciclo infinito.
                if(entrada == null)
                {
                    correo = string.Empty;
                    break;
                }

                correo = entrada.Trim();

                if (Validador.EsCorreoValido(correo))
                {
                    break;
                }

                Console.WriteLine("Correo invalido. Escriba una direccion de como alumno@universidad.edu");
                Console.WriteLine("Intentelo nuevamente. \n");
            }

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


        //Logica para que podamos pedir una nueva contrasena
        private static string? SolicitarNuevaContrasena()
        {
            while (true)
            {
                string nueva = LeerContrasena(
                    "Nueva contraseña (ENTER para cancelar): "
                );

                if (string.IsNullOrWhiteSpace(nueva))
                {
                    return null;
                }

                string confirmacion = LeerContrasena(
                    "Confirme la nueva contraseña: "
                );

                if (string.Equals(
                    nueva,
                    confirmacion,
                    StringComparison.Ordinal))
                {
                    return nueva;
                }

                Console.WriteLine(
                    "Las contraseñas no coinciden. Intente nuevamente."
                );
                Console.WriteLine();
            }
        }


        private void CambiarContrasena()
        {
            Console.WriteLine("=== CAMBIAR CONTRASEÑA ===");
            Console.WriteLine();

            string actual = LeerContrasena(
                "Contraseña actual (ENTER para cancelar): "
            );

            if (string.IsNullOrWhiteSpace(actual))
            {
                Console.WriteLine("Cambio cancelado.");
                return;
            }

            string? nueva = SolicitarNuevaContrasena();

            if (nueva == null)
            {
                Console.WriteLine("Cambio cancelado.");
                return;
            }

            bool cambioExitoso = manejador.CambiarContrasena(
                actual,
                nueva,
                out string error
            );

            if (!cambioExitoso)
            {
                Console.WriteLine(error);
                return;
            }

            Console.WriteLine(
                "Contraseña cambiada correctamente."
            );
            Console.WriteLine(
                "Los datos se guardaron cifrados con la nueva contraseña."
            );
            Console.WriteLine(
                "Utilice esa contraseña la próxima vez que abra el programa."
            );
        }
    }

}
