using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace GestionEstudiantes
{
    public class ManejadorXML
    {
        private readonly string rutaArchivo;
        public bool UltimaLecturaExitosa { get; private set; } = true;
        public string UltimoErrorLectura { get; private set; } = string.Empty;
        public List<string> UltimosErroresLectura { get; } = new List<string>();

        // Constructor que recibe la ruta donde se guardará y leerá el XML
        public ManejadorXML(string rutaArchivo)
        {
            this.rutaArchivo = rutaArchivo;
        }

        // CLASE ANIDADA: Define la estructura raíz del documento XML
        // La hacemos pública para que el XmlSerializer pueda acceder a ella
        [XmlRoot("estudiantes")]
        public class ContenedorEstudiantes
        {
            // El atributo XmlElement indica que cada elemento de esta lista se representará 
            // en el archivo XML bajo la etiqueta individual <estudiante>
            [XmlElement("estudiante")]
            public List<Estudiante> Estudiantes { get; set; }

            // Constructor sin parámetros requerido obligatoriamente por XmlSerializer.
            // Inicializa una lista vacía para evitar referencias nulas al agregar elementos.
            public ContenedorEstudiantes()
            {
                Estudiantes = new List<Estudiante>();
            }
        }

        /// Lee y deserializa la lista de estudiantes desde el archivo XML.
        public List<Estudiante> LeerEstudiantes()
        {
            // Validamos la existencia del archivo antes de intentar leer
            if (!File.Exists(rutaArchivo))
            {
                UltimaLecturaExitosa = true;
                UltimoErrorLectura = string.Empty;
                UltimosErroresLectura.Clear();
                return new List<Estudiante>();
            }
            
            try
            {
                // Se configura el XmlSerializer especificando la clase raíz (ContenedorEstudiantes) que representa la estructura XML.
                XmlSerializer serializer = new XmlSerializer(typeof(ContenedorEstudiantes));
    
                using (StreamReader reader = new StreamReader(rutaArchivo))
                {
                    // Lee el documento XML y lo convierte (deserializa) nuevamente a objetos en C#.
                    ContenedorEstudiantes? contenedor = (ContenedorEstudiantes?)serializer.Deserialize(reader);

                    // Retorna la lista de estudiantes contenida en el XML.
                    // Si el contenedor o la lista fueran nulos, se retorna una lista vacía por seguridad (null-coalescing).
                    List<Estudiante> estudiantes = contenedor?.Estudiantes ?? new List<Estudiante>();
                    List<string> errores = ValidarEstudiantesCargados(estudiantes);

                    UltimosErroresLectura.Clear();
                    UltimosErroresLectura.AddRange(errores);
                    UltimaLecturaExitosa = errores.Count == 0;
                    UltimoErrorLectura = errores.Count == 0
                        ? string.Empty
                        : "El XML contiene datos inválidos:\n- " + string.Join("\n- ", errores);

                    if (!UltimaLecturaExitosa)
                    {
                        Console.WriteLine(UltimoErrorLectura);
                    }

                    return estudiantes;
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is IOException ||
                                       ex is UnauthorizedAccessException || ex is XmlException)
            {
                // Un XML dañado no se interpreta como una lista vacía: así se evita
                // que una operación posterior sobrescriba los datos originales.
                UltimaLecturaExitosa = false;
                UltimoErrorLectura = $"No se pudo leer el archivo XML: {ex.Message}";
                UltimosErroresLectura.Clear();
                UltimosErroresLectura.Add(UltimoErrorLectura);
                Console.WriteLine(UltimoErrorLectura);
                return new List<Estudiante>();
            }
        }

        private static List<string> ValidarEstudiantesCargados(List<Estudiante> estudiantes)
        {
            List<string> errores = new List<string>();

            for (int i = 0; i < estudiantes.Count; i++)
            {
                Estudiante? estudiante = estudiantes[i];
                if (estudiante == null)
                {
                    errores.Add($"El registro {i + 1} está vacío");
                    continue;
                }

                List<string> erroresDatos = Validador.ValidarDatos(estudiante);
                foreach (string error in erroresDatos)
                {
                    errores.Add($"Registro {i + 1}: {error}");
                }

                if (estudiantes.Take(i).Any(anterior =>
                    anterior != null &&
                    Validador.SonElMismoCarne(anterior.Carne, estudiante.Carne)))
                {
                    errores.Add($"Registro {i + 1}: el carné '{estudiante.Carne}' está duplicado");
                }
            }

            return errores;
        }

        /// Serializa y guarda la lista completa de estudiantes en el archivo XML.
        public bool GuardarEstudiantes(List<Estudiante> lista)
        {
            if (lista == null)
            {
                Console.WriteLine("No se puede guardar una lista nula de estudiantes.");
                return false;
            }

            try
            {
                // 1. Manejo de directorios y rutas:
                // Se obtiene la ruta de la carpeta donde se desea guardar el archivo XML.
                string?  directorio = Path.GetDirectoryName(rutaArchivo);
                if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
                {
                    Directory.CreateDirectory(directorio);
                }

                // 2. Configuración del serializador XML:
                // Se le indica al XmlSerializer que trabajará con el nodo raíz de la clase ContenedorEstudiantes.
                XmlSerializer serializer = new XmlSerializer(typeof(ContenedorEstudiantes));
                
                // 3. Preparación de la estructura XML:
                // Se asigna la lista de estudiantes recibida como parámetro al atributo interno del nodo raíz.
                ContenedorEstudiantes contenedor = new ContenedorEstudiantes 
                { 
                    Estudiantes = lista 
                };

                // 4. Escritura en el archivo:
                // La instrucción 'using' garantiza que el recurso (StreamWriter) se cierre y libere correctamente 
                // incluso si ocurre un fallo durante la escritura.
                // Se escribe primero en un archivo temporal y se reemplaza el original
                // solo cuando la serialización termina correctamente.
                string rutaTemporal = rutaArchivo + ".tmp";
                try
                {
                    using (StreamWriter writer = new StreamWriter(rutaTemporal, false))
                    {
                        serializer.Serialize(writer, contenedor);
                    }

                    File.Move(rutaTemporal, rutaArchivo, true);
                }
                finally
                {
                    if (File.Exists(rutaTemporal))
                    {
                        File.Delete(rutaTemporal);
                    }
                }

                // Si no ocurrió ninguna excepción, se confirma que la persistencia fue exitosa.
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar el archivo XML: {ex.Message}");
                return false;
            }
        }
    }
}