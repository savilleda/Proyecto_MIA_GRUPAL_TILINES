using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace GestionEstudiantes
{
    public class ManejadorXML
    {
        private readonly string rutaArchivo;

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
                    return contenedor?.Estudiantes ?? new List<Estudiante>();
                }
            }
            catch (Exception ex)
            {
                // En caso de que el archivo XML esté corrupto, mal formado o con permisos restringidos,
                // se captura la excepción, se informa en consola y se retorna una lista vacía de forma segura.
                Console.WriteLine($"Error al leer el archivo XML: {ex.Message}");
                return new List<Estudiante>();
            }
        }

        /// Serializa y guarda la lista completa de estudiantes en el archivo XML.
        public bool GuardarEstudiantes(List<Estudiante> lista)
        {
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
                using (StreamWriter writer = new StreamWriter(rutaArchivo))
                {
                    serializer.Serialize(writer, contenedor);
                }

                // Si no ocurrió ninguna excepción, se confirma que la persistencia fue exitosa.
                return true;
            }
            catch (Exception ex)
            {
                // Manejo de errores requerido por el proyecto
                Console.WriteLine($"Error al guardar el archivo XML: {ex.Message}");
                return false;
            }
        }
    }
}