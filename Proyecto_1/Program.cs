using System;
using System.Collections.Generic;

namespace GestionEstudiantes
{
    class Program
    {
        /// Prueba de lectura de archivo XML de estudiantes (Clase ManejadorXML)
        static void Main(string[] args)
        {
            // Ruta del archivo XML de prueba
            string rutaArchivo = "prueba_XML.xml";

            // Crea una instancia de ManejadorXML asociada a la ruta del archivo configurada.
            ManejadorXML manejador = new ManejadorXML(rutaArchivo);
            Console.WriteLine("PRUEBA DE LECTURA DE ARCHIVO XML\n");

            // Invoca la deserialización del XML para reconstruir los objetos de tipo Estudiante.
            List<Estudiante> estudiantes = manejador.LeerEstudiantes();

            // Evalúa si se recuperaron registros del archivo XML para mostrarlos en la consola.
            if (estudiantes.Count > 0)
            {
                Console.WriteLine($"Se encontraron {estudiantes.Count} estudiantes en el archivo:\n");

                // Recorre y muestra cada estudiante
                foreach (Estudiante est in estudiantes)
                {
                    Console.WriteLine($" Carné:          {est.Carne}");
                    Console.WriteLine($" Nombre Completo: {est.NombreCompleto}");
                    Console.WriteLine($" Carrera:         {est.Carrera}");
                    Console.WriteLine($" Correo:          {est.Correo}");
                    Console.WriteLine(new string('-', 45));
                }
            }
            else
            {
                Console.WriteLine("\nNo se encontraron estudiantes o el archivo no existe.");
            }

            // Bandera que controla si el usuario quiere seguir buscando estudiantes
            bool seguirBuscando = true;

            // Se repite la búsqueda mientras el usuario quiera seguir probando
            while (seguirBuscando)
            {
                // Prueba de búsqueda por carné
                Console.Write("\nIngrese un carné para probar la búsqueda: ");
                string carneBusqueda = Console.ReadLine() ?? string.Empty;

                // Utiliza una expresión Lambda mediante el método Find para buscar el registro coincidente.
                // Se realiza la comparación ignorando mayúsculas, minúsculas y espacios innecesarios.
                Estudiante? encontrado = estudiantes.Find(e => e.Carne.Equals(carneBusqueda.Trim(), StringComparison.OrdinalIgnoreCase));

                if (encontrado != null)
                {
                    Console.WriteLine($"\nEstudiante encontrado exitosamente: {encontrado.NombreCompleto} - {encontrado.Carrera}");
                }
                else
                {
                    Console.WriteLine("\nError. No se encontró ningún estudiante con ese carné.");
                }

                // Pregunta si el usuario desea repetir la búsqueda
                Console.Write("\n¿Desea buscar a otro estudiante? (S/N): ");
                string respuesta = Console.ReadLine() ?? string.Empty;

                // Solo continúa el bucle si la respuesta empieza con "S" (sí)
                seguirBuscando = respuesta.Trim().StartsWith("S", StringComparison.OrdinalIgnoreCase);
            }

            Console.WriteLine("\nFin de las búsquedas.");
        }
    }
}