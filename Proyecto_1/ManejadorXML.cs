using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace GestionEstudiantes
{
    public class ManejadorXML
    {
        // Ruta absoluta del archivo donde se almacenan los estudiantes.
        // Es readonly para impedir que cambie después de construir el objeto.
        private readonly string rutaArchivo;

        // Se mantiene durante esta sesión. No se guarda en disco.
        // Contraseña utilizada durante la sesión para cifrar y descifrar.
        // Nunca se escribe dentro del archivo.
        private string contrasena;

        // Indica si la última lectura o inicialización fue correcta.
        // Una lista vacía válida también produce true.
        public bool UltimaLecturaExitosa { get; private set; } = true;

        // Mensaje correspondiente al último error de lectura detectado.
        public string UltimoErrorLectura { get; private set; }
            = string.Empty;

        // Lista pública de errores para que la interfaz pueda mostrarlos.
        public List<string> UltimosErroresLectura { get; }
            = new List<string>();

        // Valida los datos esenciales y normaliza la ruta recibida.
        public ManejadorXML(string rutaArchivo, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo))
            {
                throw new ArgumentException(
                    "Debe indicar la ruta del archivo."
                );
            }

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                throw new ArgumentException(
                    "La contraseña no puede estar vacía."
                );
            }

            // Usar una ruta absoluta evita depender del directorio actual.
            this.rutaArchivo = Path.GetFullPath(rutaArchivo);
            this.contrasena = contrasena;
        }

        // Representa el elemento raíz <estudiantes> del documento XML.
        [XmlRoot("estudiantes")]
        public class ContenedorEstudiantes
        {
            // Cada objeto de la lista se serializa como <estudiante>.
            [XmlElement("estudiante")]
            public List<Estudiante> Estudiantes { get; set; }
                = new List<Estudiante>();
        }

        // Se llama al iniciar, después de solicitar la contraseña.
        public bool InicializarArchivo()
        {
            try
            {
                // En la primera ejecución se crea una lista vacía cifrada.
                if (!File.Exists(rutaArchivo))
                {
                    PrepararDirectorio();

                    // Primero se genera y cifra el XML vacío en memoria.
                    byte[] xml = Serializar(new List<Estudiante>());
                    byte[] cifrado = CifradorAES.Encriptar(
                        xml,
                        contrasena
                    );

                    // CreateNew evita sobrescribir otro archivo por accidente.
                    using (var archivo = new FileStream(
                        rutaArchivo,
                        FileMode.CreateNew,
                        FileAccess.Write))
                    {
                        archivo.Write(cifrado, 0, cifrado.Length);
                        archivo.Flush(true);
                    }

                    LimpiarErroresLectura();
                    return true;
                }

                byte[] contenido = File.ReadAllBytes(rutaArchivo);

                // También acepta un XML legible existente y valida sus datos
                // antes de convertirlo al formato cifrado.
                List<Estudiante> estudiantes =
                    DecodificarYValidar(contenido);

                if (!CifradorAES.EsArchivoCifrado(contenido))
                {
                    // Migra automáticamente un XML antiguo sin cifrar.
                    GuardarCifrado(estudiantes, contrasena);
                }

                LimpiarErroresLectura();
                return true;
            }
            catch (Exception ex) when (EsErrorDeArchivo(ex))
            {
                RegistrarErrorLectura(
                    "No se pudo preparar el archivo: " + ex.Message
                );

                return false;
            }
        }

        // Lee el archivo cifrado y reconstruye los estudiantes en memoria.
        public List<Estudiante> LeerEstudiantes()
        {
            try
            {
                // El contenido se carga en memoria para descifrarlo y validar
                // su estructura sin crear XML temporales en el disco.
                byte[] contenido = File.ReadAllBytes(rutaArchivo);

                List<Estudiante> estudiantes =
                    DecodificarYValidar(contenido);

                LimpiarErroresLectura();
                return estudiantes;
            }
            catch (Exception ex) when (EsErrorDeArchivo(ex))
            {
                RegistrarErrorLectura(
                    "No se pudo leer el archivo: " + ex.Message
                );

                // UltimaLecturaExitosa permite distinguir este error
                // de un archivo válido que no contiene estudiantes.
                return new List<Estudiante>();
            }
        }

        // Guarda todos los estudiantes cifrados en la misma ruta.
        public bool GuardarEstudiantes(List<Estudiante> estudiantes)
        {
            try
            {
                // Este método valida, serializa, cifra y reemplaza el archivo.
                GuardarCifrado(estudiantes, contrasena);
                return true;
            }
            catch (Exception ex) when (EsErrorDeArchivo(ex))
            {
                Console.WriteLine(
                    "No se pudieron guardar los datos: " + ex.Message
                );

                return false;
            }
        }

        // Comprueba la contraseña actual y vuelve a cifrar el archivo
        // con la nueva. La contraseña de sesión cambia solo al guardar.
        public bool CambiarContrasena(
            string contrasenaActual,
            string nuevaContrasena,
            out string error)
        {
            // El error se devuelve al llamador para que pueda mostrarlo en la
            // interfaz sin tener que manejar una excepción.
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(nuevaContrasena))
            {
                error = "La nueva contraseña no puede estar vacía.";
                return false;
            }

            try
            {
                byte[] contenido = File.ReadAllBytes(rutaArchivo);

                // Desencriptar comprueba que la contraseña actual sea válida.
                byte[] xml = CifradorAES.Desencriptar(
                    contenido,
                    contrasenaActual
                );

                List<Estudiante> estudiantes =
                    DeserializarYValidar(xml);

                // Se guarda primero con la nueva clave y solo después se
                // actualiza la contraseña activa de esta sesión.
                GuardarCifrado(estudiantes, nuevaContrasena);

                contrasena = nuevaContrasena;
                return true;
            }
            catch (Exception ex) when (EsErrorDeArchivo(ex))
            {
                error = "No se pudo cambiar la contraseña: "
                    + ex.Message;

                return false;
            }
        }

        // Reconoce el formato y obtiene el XML únicamente en memoria.
        private List<Estudiante> DecodificarYValidar(byte[] contenido)
        {
            // Determina si debe descifrar el contenido o tratarlo como XML.
            byte[] xml;

            if (CifradorAES.EsArchivoCifrado(contenido))
            {
                xml = CifradorAES.Desencriptar(
                    contenido,
                    contrasena
                );
            }
            else
            {
                // No tener encabezado de cifrado no significa que esté vacío:
                // todavía debe ser un XML válido y pasar todas las validaciones.
                xml = contenido;
            }

            return DeserializarYValidar(xml);
        }

        private static List<Estudiante> DeserializarYValidar(
            byte[] xml)
        {
            // XmlSerializer convierte los bytes XML en objetos de la clase
            // contenedora siguiendo sus atributos XmlRoot y XmlElement.
            var serializer = new XmlSerializer(
                typeof(ContenedorEstudiantes)
            );

            using var memoria = new MemoryStream(xml);

            var contenedor =
                serializer.Deserialize(memoria)
                as ContenedorEstudiantes;

            if (contenedor == null)
            {
                throw new InvalidDataException(
                    "No se encontró la estructura de estudiantes."
                );
            }

            List<Estudiante> estudiantes =
                contenedor.Estudiantes;

            // La deserialización valida la forma, pero no todas las reglas de
            // negocio; por eso se comprueba también el contenido.
            ComprobarEstudiantes(estudiantes);

            return estudiantes;
        }

        // Convierte objetos a XML sin crear una copia legible en disco.
        private static byte[] Serializar(
            List<Estudiante> estudiantes)
        {
            // Serializa en memoria para que ninguna copia XML sin cifrar quede
            // almacenada en el disco.
            var serializer = new XmlSerializer(
                typeof(ContenedorEstudiantes)
            );

            var contenedor = new ContenedorEstudiantes
            {
                Estudiantes = estudiantes
            };

            using var memoria = new MemoryStream();

            serializer.Serialize(memoria, contenedor);

            return memoria.ToArray();
        }

        private void GuardarCifrado(
            List<Estudiante> estudiantes,
            string clave)
        {
            // Nunca se guardan registros inválidos, aunque el método sea
            // llamado directamente desde otra operación de esta clase.
            ComprobarEstudiantes(estudiantes);

            byte[] xml = Serializar(estudiantes);
            byte[] cifrado = CifradorAES.Encriptar(xml, clave);

            PrepararDirectorio();

            // El temporal contiene exclusivamente datos cifrados.
            string rutaTemporal = rutaArchivo
                + "."
                + Guid.NewGuid().ToString("N")
                + ".tmp";

            try
            {
                // Se escribe completamente un archivo temporal antes de tocar
                // el principal, reduciendo el riesgo de dejarlo incompleto.
                using (var archivo = new FileStream(
                    rutaTemporal,
                    FileMode.CreateNew,
                    FileAccess.Write))
                {
                    archivo.Write(cifrado, 0, cifrado.Length);
                    archivo.Flush(true);
                }

                // El reemplazo ocurre solo después de terminar la escritura.
                File.Move(rutaTemporal, rutaArchivo, true);
            }
            finally
            {
                // Un fallo de limpieza no debe ocultar el resultado
                // del guardado. El temporal, si queda, está cifrado.
                try
                {
                    if (File.Exists(rutaTemporal))
                    {
                        File.Delete(rutaTemporal);
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine(
                        "No se pudo limpiar un archivo temporal cifrado."
                    );
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine(
                        "No hay permiso para limpiar el temporal cifrado."
                    );
                }
            }
        }

        private void PrepararDirectorio()
        {
            // Crea la carpeta padre si todavía no existe.
            string? directorio =
                Path.GetDirectoryName(rutaArchivo);

            if (!string.IsNullOrWhiteSpace(directorio))
            {
                Directory.CreateDirectory(directorio);
            }
        }

        // Valida campos, registros nulos y carnés duplicados en toda la lista.
        private static void ComprobarEstudiantes(
            List<Estudiante> estudiantes)
        {
            // Se acumulan todos los problemas para informar varios errores de
            // una vez, en lugar de detenerse en el primer registro incorrecto.
            if (estudiantes == null)
            {
                throw new InvalidDataException(
                    "No se recibió una lista de estudiantes."
                );
            }

            var errores = new List<string>();

            for (int i = 0; i < estudiantes.Count; i++)
            {
                // El índice permite indicar exactamente qué registro falló.
                Estudiante? estudiante = estudiantes[i];

                if (estudiante == null)
                {
                    errores.Add(
                        $"El registro {i + 1} está vacío."
                    );

                    continue;
                }

                foreach (string error in
                    Validador.ValidarDatos(estudiante))
                {
                    errores.Add(
                        $"Registro {i + 1}: {error}"
                    );
                }

                bool duplicado = estudiantes
                    .Take(i)
                    .Any(anterior =>
                        anterior != null &&
                        Validador.SonElMismoCarne(
                            anterior.Carne,
                            estudiante.Carne
                        ));

                if (duplicado)
                {
                    errores.Add(
                        $"El carné '{estudiante.Carne}' está duplicado."
                    );
                }
            }

            if (errores.Count > 0)
            {
                throw new InvalidDataException(
                    string.Join(Environment.NewLine, errores)
                );
            }
        }

        private void LimpiarErroresLectura()
        {
            // Elimina el estado de error anterior tras una operación exitosa.
            UltimaLecturaExitosa = true;
            UltimoErrorLectura = string.Empty;
            UltimosErroresLectura.Clear();
        }

        private void RegistrarErrorLectura(string mensaje)
        {
            // Guarda el error actual y marca la lectura como fallida.
            UltimaLecturaExitosa = false;
            UltimoErrorLectura = mensaje;

            UltimosErroresLectura.Clear();
            UltimosErroresLectura.Add(mensaje);
        }

        private static bool EsErrorDeArchivo(Exception ex)
        {
            // Limita el catch a errores esperables de archivo, XML, argumentos
            // y criptografía; otros errores no deben ocultarse.
            return ex is IOException
                || ex is UnauthorizedAccessException
                || ex is InvalidOperationException
                || ex is CryptographicException
                || ex is ArgumentException
                || ex is System.Xml.XmlException;
        }
    }
}