using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/*DE MARCO: elimine cifrar y descfifrar de la clase. Ahora se ejecutan automaticamente, no con opcion al usuario. 
Esto para mantener la idea de la seguridad de los datos y mantener un proyecto mas limpio :)
*/

namespace GestionEstudiantes
{
    // Muestra el menú y pide los datos al usuario por consola.
    // El arranque del programa (Main) está en Proyecto_1.cs.
    // La lógica de negocio está en OperacionesEstudiante y Validador,
    // y la lectura/escritura del archivo en ManejadorXML.
    public class MenuPrincipal
    {
        // ------------------------------------------------------------
        // CAMPOS Y CONSTRUCTOR
        // ------------------------------------------------------------

        // Clase encargada de realizar las operaciones con los estudiantes
        private readonly OperacionesEstudiante operaciones;
        // Clase encargada de leer y guardar el archivo cifrado.
        // Aquí se usa directamente solo para cambiar la contraseña.
        private readonly ManejadorXML manejador;

        // Recibimos las clases ya preparadas desde Proyecto_1 (Main)
        public MenuPrincipal(OperacionesEstudiante operaciones, ManejadorXML manejador)
        {
            this.operaciones = operaciones
                ?? throw new ArgumentNullException(nameof(operaciones));

            this.manejador = manejador
                ?? throw new ArgumentNullException(nameof(manejador));
        }

        // ------------------------------------------------------------
        // MENÚ PRINCIPAL
        // ------------------------------------------------------------

        // Mantiene el programa funcionando hasta que el usuario elija salir (opción 0)
        public void Ejecutar()
        {
            // Variable para almacenar la opción del usuario
            string opcion;

            do
            {
                // Limpiamos la consola y mostramos el menú
                LimpiarConsola();
                MostrarMenu();

                Console.Write("Seleccione una opción: ");
                // Leemos la opción; ReadLine puede devolver null, por eso el "?"
                string? entrada = Console.ReadLine();

                // Si la consola se cierra o deja de enviar datos, terminamos
                // limpiamente en vez de repetir el menú indefinidamente.
                // Además se eliminan los espacios al inicio y al final.
                opcion = entrada == null ? "0" : entrada.Trim();

                LimpiarConsola();

                // Ejecuta la acción que corresponde a la opción elegida
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
                        Console.WriteLine("Opción inválida. Por favor, seleccione una opción válida.");
                        break;
                }

                // Si la opción no es 0, pausamos para que el usuario pueda ver el
                // resultado de la acción y presione ENTER cuando desee continuar
                if(opcion != "0")
                {
                    Pausar();
                }

            } while(opcion != "0"); // El programa termina cuando la opción es 0
        }

        // Muestra las opciones principales del sistema
        private void MostrarMenu()
        {
            Console.WriteLine("=============================================");
            Console.WriteLine("     SISTEMA DE GESTIÓN DE ESTUDIANTES");
            Console.WriteLine("=============================================");
            Console.WriteLine("1. Registrar estudiante");
            Console.WriteLine("2. Buscar estudiante por No. de carnet");
            Console.WriteLine("3. Modificar estudiante");
            Console.WriteLine("4. Eliminar estudiante");
            Console.WriteLine("5. Listar estudiantes");
            Console.WriteLine("6. Cambiar contraseña");
            Console.WriteLine("0. Salir");
            Console.WriteLine("=============================================");
        }

        // ------------------------------------------------------------
        // OPCIONES DEL MENÚ
        // ------------------------------------------------------------

        // Opción 1: pide los datos de un nuevo estudiante y lo registra
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

        // Opción 2: busca un estudiante usando su carné como identificador
        private void BuscarEstudiante()
        {
            Console.WriteLine("=== BÚSQUEDA DE ESTUDIANTE ===\n");

            Estudiante? estudianteActual = null;

            // Pide el carné hasta encontrar un estudiante o hasta que el usuario cancele
            while (true)
            {
                Console.Write("Ingrese el carné del estudiante (ENTER para cancelar): ");
                string carne = (Console.ReadLine() ?? string.Empty).Trim();

                // ENTER sin escribir nada cancela la operación y vuelve al menú
                if (string.IsNullOrEmpty(carne))
                {
                    Console.WriteLine("\nOperación cancelada.");
                    return;
                }

                // La búsqueda la realiza OperacionesEstudiante
                var busqueda = operaciones.BuscarEstudiantePorCarne(carne);

                if (!busqueda.encontrado || busqueda.estudiante == null)
                {
                    MostrarErrores(busqueda.errores);
                    Console.WriteLine("Intente nuevamente con otro carné.\n");
                    continue; // Vuelve a pedir el carné
                }

                // Si se encontró, guardamos la referencia y salimos del ciclo
                estudianteActual = busqueda.estudiante;
                break;
            }

            Console.WriteLine("\nEstudiante encontrado:");
            MostrarEstudiante(estudianteActual);
        }

        // Opción 3: modifica un campo de un estudiante existente.
        // El carné no se puede modificar porque es el identificador.
        private void ModificarEstudiante()
        {
            Console.WriteLine("=== MODIFICACIÓN DE ESTUDIANTE ===\n");
            
            Estudiante? estudianteActual = null;

            // Pide el carné hasta encontrar un estudiante o hasta que el usuario cancele
            while (true)
            {
                Console.Write("Ingrese el carné del estudiante (ENTER para cancelar): ");
                string carne = (Console.ReadLine() ?? string.Empty).Trim();

                // ENTER sin escribir nada cancela la operación y vuelve al menú
                if (string.IsNullOrEmpty(carne))
                {
                    Console.WriteLine("\nOperación cancelada.");
                    return;
                }

                // La búsqueda la realiza OperacionesEstudiante
                var busqueda = operaciones.BuscarEstudiantePorCarne(carne);

                if (!busqueda.encontrado || busqueda.estudiante == null)
                {
                    MostrarErrores(busqueda.errores);
                    Console.WriteLine("Intente nuevamente con otro carné.\n");
                    continue; // Vuelve a pedir el carné
                }

                // Si se encontró, guardamos la referencia y salimos del ciclo
                estudianteActual = busqueda.estudiante;
                break;
            }

            Console.Clear();
            Console.WriteLine("\nDatos actuales:");
            MostrarEstudiante(estudianteActual);

            // Ciclo externo: permite modificar varios campos seguidos
            // hasta que el usuario elija volver al menú
            while (true)
            {
                Console.WriteLine("\n1. Nombre(s)\n2. Apellidos\n3. Carrera\n4. Correo\n0. Volver al menú principal");
                Console.Write("Seleccione el campo: ");
                string? opcion = Console.ReadLine()?.Trim();
                if (opcion == null || opcion == "0") return;
                if (opcion != "1" && opcion != "2" && opcion != "3" && opcion != "4")
                {
                    Console.WriteLine("Opción inválida.");
                    continue;
                }

                // Ciclo interno: pide el nuevo valor hasta que sea válido o el usuario cancele
                while (true)
                {
                    // Se trabaja sobre una copia para no cambiar los datos actuales
                    // antes de validar y guardar.
                    var candidato = new Estudiante(estudianteActual.Carne, estudianteActual.Nombres,
                        estudianteActual.Apellidos, estudianteActual.Carrera, estudianteActual.Correo);
                    string campo = opcion switch { "1" => "Nombre(s)", "2" => "Apellidos", "3" => "Carrera", _ => "Correo" };
                    Console.Write(campo + " (ENTER para cancelar): ");
                    string? valor = Console.ReadLine()?.Trim();
                    if (valor == null) return;

                    // ENTER sin escribir nada: cancela este campo y vuelve a elegir otro
                    if (valor.Length == 0) break;

                    // Aplica el nuevo valor solo a la copia
                    switch (opcion)
                    {
                        case "1": candidato.Nombres = valor; break;
                        case "2": candidato.Apellidos = valor; break;
                        case "3": candidato.Carrera = valor; break;
                        case "4": candidato.Correo = valor; break;
                    }

                    // Si se cambia el correo, comprueba que no pertenezca a otro estudiante
                    if (opcion == "4")
                    {
                        List<Estudiante> estudiantesActuales = operaciones.ListarEstudiantes();

                        // Busca el correo en los demás estudiantes (se excluye al que se está editando)
                        bool correoDuplicado = estudiantesActuales.Any(e => 
                            e != null && 
                            !Validador.SonElMismoCarne(e.Carne, estudianteActual.Carne) && 
                            Validador.SonElMismoCorreo(e.Correo, valor)
                        );

                        if (correoDuplicado)
                        {
                            Console.WriteLine("\nEl correo ingresado ya pertenece a otro estudiante.");
                            Console.WriteLine("Intente nuevamente con un correo diferente.\n");
                            continue; // Vuelve a pedir el valor
                        }
                    }

                    // Valida el formato de todos los campos de la copia
                    var errores = Validador.ValidarDatos(candidato);
                    if (errores.Count > 0)
                    {
                        MostrarErrores(errores);
                        Console.WriteLine("Intente nuevamente.");
                        continue;
                    }

                    // Guarda el cambio. Si falla, se muestran los errores y se vuelve a elegir campo.
                    var resultado = operaciones.ActualizarEstudiante(estudianteActual.Carne, candidato);
                    if (!resultado.exito) { MostrarErrores(resultado.errores); break; }

                    // El cambio se guardó: la copia pasa a ser el estudiante actual
                    estudianteActual = candidato;
                    Console.WriteLine("\nEstudiante modificado correctamente.\n");
                    MostrarEstudiante(estudianteActual);
                    break;
                }
            }
        }

        // Opción 4: busca al estudiante y pide confirmación antes de eliminarlo
        private void EliminarEstudiante()
        {
            Console.WriteLine("=== ELIMINACIÓN DE ESTUDIANTE ===\n");

            Estudiante? estudianteEncontrado = null;
            string carneValido;

            // Pide el carné hasta encontrar un estudiante o hasta que el usuario cancele
            while (true)
            {
                Console.Write("Ingrese el carné del estudiante (ENTER para cancelar): ");
                string carne = (Console.ReadLine() ?? string.Empty).Trim();

                // ENTER sin escribir nada cancela la operación y vuelve al menú
                if (string.IsNullOrEmpty(carne))
                {
                    Console.WriteLine("\nOperación cancelada.");
                    return;
                }

                // La búsqueda la realiza OperacionesEstudiante
                var busqueda = operaciones.BuscarEstudiantePorCarne(carne);

                if (!busqueda.encontrado || busqueda.estudiante == null)
                {
                    MostrarErrores(busqueda.errores);
                    Console.WriteLine("Intente nuevamente con otro carné.\n");
                    continue; // Vuelve a pedir el carné
                }

                // Si se encontró, guardamos la información y salimos del ciclo
                estudianteEncontrado = busqueda.estudiante;
                carneValido = carne;
                break;
            }

            // Mostramos al estudiante encontrado y pedimos confirmación
            Console.WriteLine("\nEstudiante encontrado:\n");
            MostrarEstudiante(estudianteEncontrado);

            Console.Write(
                "\n¿Desea eliminar este estudiante? (S/N): "
            );

            string confirmacion =
                (Console.ReadLine() ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            // Cualquier respuesta distinta de "S" cancela la eliminación
            if (confirmacion != "S")
            {
                Console.WriteLine("\nEliminación cancelada.");
                return;
            }

            // La eliminación y la actualización del archivo las hace
            // OperacionesEstudiante
            var resultado =
                operaciones.EliminarEstudiante(carneValido);

            if (!resultado.exito)
            {
                MostrarErrores(resultado.errores);
                return;
            }

            Console.WriteLine(
                "\nEstudiante eliminado correctamente."
            );
        }


        // Opción 5: obtiene y muestra todos los estudiantes registrados
        private void ListarEstudiantes()
        {
            Console.WriteLine("=== LISTADO DE ESTUDIANTES ===\n");

            // La lista se obtiene directamente desde el archivo
            List<Estudiante> estudiantes =
                operaciones.ListarEstudiantes();

            // Si la lectura falló, se muestra el error en lugar de una lista vacía
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

            // Mostramos cada estudiante recuperado, separados por una línea
            foreach (Estudiante estudiante in estudiantes)
            {
                MostrarEstudiante(estudiante);

                Console.WriteLine(
                    "------------------------------------------"
                );
            }
        }

        // Opción 6: cambia la contraseña con la que se cifran los datos
        private void CambiarContrasena()
        {
            Console.WriteLine("=== CAMBIAR CONTRASEÑA ===");
            Console.WriteLine();

            // Se pide la contraseña actual para confirmar que es el usuario legítimo
            string actual = EntradaConsola.LeerContrasena(
                "Contraseña actual (ENTER para cancelar): "
            );

            if (string.IsNullOrWhiteSpace(actual))
            {
                Console.WriteLine("Cambio cancelado.");
                return;
            }

            // Pide la nueva contraseña con confirmación (null si cancela)
            string? nueva = EntradaConsola.SolicitarNuevaContrasena();

            if (nueva == null)
            {
                Console.WriteLine("Cambio cancelado.");
                return;
            }

            // ManejadorXML comprueba la contraseña actual y vuelve a cifrar el archivo
            // con la nueva. Si falla, devuelve el motivo en "error".
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

        // ------------------------------------------------------------
        // MÉTODOS DE APOYO
        // (Nota: ObtenerRutaXml, GuardarArchivoTemporal, Confirmar y
        // LeerValorActual no se llaman desde ningún otro lugar en este
        // archivo. Se dejan por si se necesitan más adelante.)
        // ------------------------------------------------------------

        // Devuelve la ruta del XML principal (sin uso actualmente)
        private static string ObtenerRutaXml() => ConfiguracionDatos.PrepararRutaXml();

        // Reemplaza el archivo de salida solo después de terminar la escritura,
        // para no dejar un archivo incompleto si algo falla (sin uso actualmente)
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
                // Se elimina el temporal si quedó en disco
                if (File.Exists(rutaTemporal))
                {
                    File.Delete(rutaTemporal);
                }
            }
        }

        // Pregunta al usuario y devuelve true solo si responde "S" (sin uso actualmente)
        private static bool Confirmar(string mensaje)
        {
            Console.Write(mensaje);
            string respuesta = (Console.ReadLine() ?? string.Empty).Trim();
            return respuesta.Equals("S", StringComparison.OrdinalIgnoreCase);
        }


        // Pide los datos para crear un nuevo estudiante.
        // El carné y el correo se validan aquí (formato y duplicados) para que
        // el usuario pueda corregirlos de inmediato sin volver a escribir todo.
        private Estudiante LeerDatosEstudiante()
        {
            string carne;

            // Pide el carné hasta que sea válido y único
            while (true)
            {
                Console.Write("Carné (entero de 6 dígitos): ");
                string? entrada = Console.ReadLine();

                // Si por cualquier razón se cierra la entrada de la consola,
                // evitamos que haya un ciclo infinito.
                if(entrada == null)
                {
                    carne = string.Empty;
                    break;
                }

                carne = entrada.Trim();

                // 1. Valida el formato de 6 dígitos
                if (!Validador.EsCarneValido(carne, out string mensajeError))
                {
                    Console.WriteLine($"Error: {mensajeError}");
                    Console.WriteLine("Inténtelo nuevamente. \n");
                    continue;
                }

                // 2. Valida si el carné ya está en uso
                List<Estudiante> estudiantesActuales = operaciones.ListarEstudiantes();
                if (Validador.ExisteCarne(carne, estudiantesActuales))
                {
                    Console.WriteLine($"El carné '{carne}' ya está registrado.");
                    Console.WriteLine("Intentelo nuevamente. \n");
                    continue;
                }

                break; // Carné válido y único
            }

            // Estos campos se leen tal cual; la validación final
            // (campos vacíos, caracteres inválidos) la hace Validador al registrar
            Console.Write("Nombre(s): ");
            string nombres = Console.ReadLine() ?? string.Empty;

            Console.Write("Apellidos: ");
            string apellidos = Console.ReadLine() ?? string.Empty;

            Console.Write("Carrera: ");
            string carrera = Console.ReadLine() ?? string.Empty;

            string correo;

            // Pide el correo hasta que sea válido y único
            while (true)
            {
                Console.Write("Correo: ");
                string? entrada = Console.ReadLine();

                // Si por cualquier razón se cierra la entrada de la consola,
                // evitamos que haya un ciclo infinito.
                if(entrada == null)
                {
                    correo = string.Empty;
                    break;
                }

                correo = entrada.Trim();
                
                // 1. Valida el formato del correo
                if (!Validador.EsCorreoValido(correo))
                {
                    Console.WriteLine("Correo inválido. Escriba una dirección como alumno@universidad.edu.");
                    Console.WriteLine("Intentelo nuevamente. \n");
                    continue;
                }

                // 2. Valida si el correo ya está en uso
                List<Estudiante> estudiantesActuales = operaciones.ListarEstudiantes();
                if (Validador.ExisteCorreo(correo, estudiantesActuales))
                {
                    Console.WriteLine($"El correo '{correo}' ya está registrado. Ingrese otro diferente.");
                    Console.WriteLine("Intentelo nuevamente. \n");
                    continue; // Repite el ciclo sin salir
                }

                break; // Correo válido y único
            }

            return new Estudiante(
                carne,
                nombres,
                apellidos,
                carrera,
                correo
            );
        }


        // Muestra el valor actual entre corchetes y devuelve el valor nuevo,
        // o el anterior si el usuario deja el campo vacío (sin uso actualmente)
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


        // Muestra una lista de errores (por ejemplo, los que devuelve Validador)
        private void MostrarErrores(List<string> errores)
        {
            Console.WriteLine("\nNo se pudo realizar la operación:");

            foreach (string error in errores)
            {
                Console.WriteLine("- " + error);
            }
        }


        // Evita que la consola cambie inmediatamente de pantalla
        // para que el usuario alcance a leer el resultado
        private void Pausar()
        {
            Console.WriteLine("\nPresione ENTER para continuar...");
            Console.ReadLine();
        }

        // Limpia la pantalla. Console.Clear no está disponible cuando la salida
        // está redirigida (por ejemplo, al ejecutar pruebas automatizadas).
        private void LimpiarConsola()
        {
            try
            {
                Console.Clear();
                // Secuencia ANSI que limpia el buffer de desplazamiento y mueve el cursor al inicio
                Console.Write("\x1b[3J\x1b[H");
            }
            catch (System.IO.IOException)
            {
                // Ignora si el entorno no lo soporta
            }
        }
    }

}