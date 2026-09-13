using System;
using System.Collections.Generic;
using System.Linq;

//todo: revisar lineas no. 127, 240 :) -MD

namespace GestionEstudiantes
{
    public class MenuPrincipal
    {
        //Todo inicia con los estudiantes disponibles en el sistema
        private readonly List<Estudiante> estudiantes;

        //Para inicializar el menu, el constructor depende de la lista de estudiantes
        public MenuPrincipal(List<Estudiante> estudiantes)
        {
            //Si lalita de estudiantes viene nula
            if(estudiantes == null)
            {
                //Se muestra al usuario porque la accion es invalida
                throw new ArgumentNullException(nameof(estudiantes), "La lista de estudiantes no puede estar vacia.");
            }
            
            //De lo contrario, se asigna la lista de estudiantes al campo privado del menu
            this.estudiantes = estudiantes;
        }

        //Ejecutrar mantiene el programa funcionando hasta que el usuario desee salir
        public void Ejecutar()
        {
            //Variable para almacenar la opcion del usuario
            string opcion;

            do
            {
                //Limpiamos la consola
                Console.Clear();
                //Mostramos el menu principal
                MostrarMenu();

                //Damos las instrucciones de seleccion de opcion
                Console.Write("Seleccione una opcion:");
                //Leemos la opsion del usuario, prevenimos que sea nula y eliminamos espacios en blanco al inicio y al final
                opcion = (Console.ReadLine() ?? string.Empty).Trim();

                //limpiamos la consola
                Console.Clear();

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
                        MoificarEstudiante();
                        break;
                    
                    case "4":
                        EliminarEstudiante();
                        break;
                    
                    case "5":
                        ListarEstudiantes();
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

            } while(opcion != "0");//Detenemos el programa cuando la opcion del usuario sea 0.
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
            Console.WriteLine("0. Salir");
            Console.WriteLine("=============================================");
        }

        //Registrar estudiante permite al usuario ingresar los datos de un nuevo estudiante y agregarlo a la lista
        private void RegistrarEstudiante()
        {
            Console.WriteLine("=== REGISTRO DE ESTUDIANTE ===");

            //Declaramos un nuevo estudiante y leemos sus datos
            Estudiante nuevoEstudiante = LeerDatosEstudiante();

            //Para mejorar la experiencia y el funcionamineto del sistema, listamos cualquier error que pueda ocurrir
            List<string> errores =
                Validador.ValidarRegistro(nuevoEstudiante, estudiantes);

            //Si hay errores, los mostramos al usuario
            if(errores.Count > 0)
            {
                MostrarErrores(errores);
                return;
            }

            //Si no hay errores, agregamos el estudiante a la lista
            estudiantes.Add(nuevoEstudiante);
            Console.WriteLine("\nEstudiante registrado exitosamente.");

            //====== Luego del regitro, mas adelante, llamaremos al metodo que guarde y encripte la informaciomn el archivo XML.
        }

        //Busca de estudiantes por su ID
        private void BuscarEstudiante()
        {
            Console.WriteLine("=== BUSQEUDA DE ESTUDIANTE ===\n");

            //Pedimos al usuario que ingrese el carnet del estudiante (ID)
            Console.Write("Ingrese el no. de carnet del estudiante que desea buscar: ");
            //Guardamos el ingreso del usuario, prevenimos que sea nulo y eliminamos espacios en blanco al inicio y al final
            string carnet = (Console.ReadLine() ?? string.empty).Trim();

            //Si el carne es nulo o vacio (que el usuarion no ingrese nada jaja :D)
            if (string.IsNillOrEmpty(carnet))
            {
                Console.WriteLine("\nDebe ingresar un carnet valido para realiar la busqueda.");
                return;
            }

            //Buscamos el estudiante en la lista
            //Verificamos que el estudiante no sea nulo y que el carnet ingresado por el usuario sea exactamente el mismo que el del estudiante
            Estudiante estudiante = estudiantes.FirstOrDefault(e => e != null && Validador.SonElMismoCarne(e.Carnet, carnet));


            //Si por cualquier razon el estudiante es nulo, indicamos al usuario que no se ha encontrado a ningun estudiante con el carnet especificado
            if(estudiante == null)
            {
                Console.WriteLine($"\nNo se ha encontrado ningun estudiante con el carnet: {carnet}");
            }
            
            //Ya de ultimo, si en efecto existe, le mostramos al usuario la informacion del estudiante
            Console.WriteLine("\nEstudiante encontrado:");
            MostrarEstudiante(estudiante);
        }

        //Modificacion de la informacion de un estudiante existe
        private void ModificarEstudiante()
        {
            Console.WriteLine("=== MODIFICACIÓN DE ESTUDIANTE ===\n");

            //Pedimos al usuario que ingrese el carnet del estudiante que desea modificar
            Console.Write("Ingrese el carné del estudiante: ");
            //Guardamos el ingreso del usuario, prevenimos que sea nulo y eliminamos espacios en blanco al inicio y al final
            string carne = (Console.ReadLine() ?? string.Empty).Trim();

            //Si el carne es nulo o vacio (que el usuarion no ingrese nada jaja :D)
            Estudiante estudianteActual = estudiantes.FirstOrDefault(
                e => e != null &&
                Validador.SonElMismoCarne(e.Carne, carne)
            );

            //Si el estudiante no se encuentra, mostramos un mensaje de error y salimos del metodo
            if (estudianteActual == null)
            {
                Console.WriteLine("\nNo se encontró un estudiante con ese carné.");
                return;
            }

            // De lo contrario, mostramos los datos actuales del estudiante y pedimos al usuario que ingrese los nuevos datos
            Console.WriteLine("\nDatos actuales:");
            MostrarEstudiante(estudianteActual);

            Console.WriteLine("\nIngrese los nuevos datos.");
            Console.WriteLine("Presione ENTER para conservar el valor actual.\n");

            // Leemos los nuevos valores, permitiendo al usuario mantener los valores actuales si lo desea
            string nombres =
                LeerValorActual("Nombres", estudianteActual.Nombres);

            string apellidos =
                LeerValorActual("Apellidos", estudianteActual.Apellidos);

            string carrera =
                LeerValorActual("Carrera", estudianteActual.Carrera);

            string correo =
                LeerValorActual("Correo", estudianteActual.Correo);

            // El carné original se mantiene porque funciona
            // como identificador único del estudiante
            Estudiante estudianteModificado = new Estudiante(
                estudianteActual.Carne,
                nombres,
                apellidos,
                carrera,
                correo
            );

            // Listamos los errores si los hay
            List<string> errores =
                Validador.ValidarActualizacion(
                    estudianteActual.Carne,
                    estudianteModificado,
                    estudiantes
                );
            
            //Si hay errores, los mostramos al usuario y salimos del método -> asi el usuario puede corregirlos y volver a intentarlo.
            if (errores.Count > 0)
            {
                MostrarErrores(errores);
                return;
            }


            //Seteamos los nuevos valores al estudiante actual
            estudianteActual.Nombres = estudianteModificado.Nombres;
            estudianteActual.Apellidos = estudianteModificado.Apellidos;
            estudianteActual.Carrera = estudianteModificado.Carrera;
            estudianteActual.Correo = estudianteModificado.Correo;

            Console.WriteLine("\nEstudiante modificado correctamente.");

            // =========== Aquí se guardarán posteriormente los cambios en el XML.
        }

        // Elimina un estudiante después de comprobar que exista
        private void EliminarEstudiante()
        {
            Console.WriteLine("=== ELIMINACIÓN DE ESTUDIANTE ===\n");

            // Pedimos al usuario que ingrese el carné del estudiante que desea eliminar
            Console.Write("Ingrese el carné del estudiante: ");
            // Guardamos el ingreso del usuario, prevenimos que sea nulo y eliminamos espacios en blanco al inicio y al final
            string carne = (Console.ReadLine() ?? string.Empty).Trim();

            //Listamos los errores si los hay
            List<string> errores =
                Validador.ValidarEliminacion(carne, estudiantes);

            //Si en efecto hay errores, los mostramos al usuario y salimos del método -> asi el usuario puede corregirlos y volver a intentarlo.
            if (errores.Count > 0)
            {
                MostrarErrores(errores);
                return;
            }

            //Ya validada la existencia del estudiante, procedemos a buscarlo en la lista
            Estudiante estudiante = estudiantes.First(
                e => e != null &&
                Validador.SonElMismoCarne(e.Carne, carne)
            );

            //cuando lo encontremos, se lo indicamos al usuario y le pedimos confirmación antes de eliminarlo
            Console.WriteLine("\nEstudiante encontrado:\n");

            MostrarEstudiante(estudiante);

            Console.Write("\n¿Desea eliminar este estudiante? (S/N): ");

            //Leemos la confirmación del usuario, prevenimos que sea nula, eliminamos espacios en blanco al inicio y al final, y convertimos a mayúsculas para facilitar la comparación
            string confirmacion =
                (Console.ReadLine() ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            if (confirmacion != "S")
            {
                Console.WriteLine("\nEliminación cancelada.");
                return;
            }
            
            //Si el usuario confirma, eliminamos el estudiante de la lista
            estudiantes.Remove(estudiante);
             
            //Informamos al usuario que la eliminación fue exitosa
            Console.WriteLine("\nEstudiante eliminado correctamente.");

            // Aquí se actualizará posteriormente el archivo XML.
        }


        // Muestra todos los estudiantes registrados
        private void ListarEstudiantes()
        {
            Console.WriteLine("=== LISTADO DE ESTUDIANTES ===\n");

            //Si la lista de estudiantes está vacía, informamos al usuario y salimos del método -> no habrian estudiantes que mostrar
            if (estudiantes.Count == 0)
            {
                Console.WriteLine("No existen estudiantes registrados.");
                return;
            }
            

            // Mostramos el total de estudiantes registrados
            Console.WriteLine(
                "Total de estudiantes: " + estudiantes.Count
            );

            Console.WriteLine();

            // Recorremos la lista de estudiantes y mostramos la información de cada uno
            foreach (Estudiante estudiante in estudiantes)
            {
                MostrarEstudiante(estudiante);
                Console.WriteLine("------------------------------------------");
            }
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
    }

}