using System;
// Permite usar List e IEnumerable
using System.Collections.Generic;
// Permite usar Any para buscar coincidencias en listas
using System.Linq;
// Permite revisar el formato del correo electrónico
using System.Net.Mail;
// Permite comprobar si un texto contiene caracteres válidos para XML
using System.Xml;

// El namespace agrupa las clases del proyecto para mantener el código organizado
// y evitar conflictos con clases de nombres parecidos
namespace GestionEstudiantes
{
    // Reúne todas las reglas de negocio de los estudiantes.
    // Solo revisa y devuelve errores: no imprime nada ni guarda datos.
    public static class Validador
    {
        // Revisa que todos los campos del estudiante tengan contenido válido
        // y que el correo tenga un formato correcto.
        public static List<string> ValidarDatos(Estudiante estudiante)
        {
            // Lista donde se guardan los errores encontrados
            var errores = new List<string>();

            // Si no se recibió un estudiante, no hay nada más que revisar
            if (estudiante == null)
            {
                errores.Add("No se recibió un estudiante para validar");
                return errores;
            }

            // Cada llamada revisa que el campo no esté vacío y que sus caracteres
            // sean válidos para XML. El segundo parámetro es el nombre que aparece
            // en el mensaje de error y el tercero es la lista donde se agregan.
            ValidarTextoObligatorio(estudiante.Carne, "El carné", errores);
            ValidarTextoObligatorio(estudiante.Nombres, "Los nombres", errores);
            ValidarTextoObligatorio(estudiante.Apellidos, "Los apellidos", errores);
            ValidarTextoObligatorio(estudiante.Carrera, "La carrera", errores);
            ValidarTextoObligatorio(estudiante.Correo, "El correo", errores);

            // Si el correo tiene contenido pero su formato no es válido, se agrega el error.
            // Si estaba vacío, ValidarTextoObligatorio ya lo reportó.
            if (!string.IsNullOrWhiteSpace(estudiante.Correo) && !EsCorreoValido(estudiante.Correo))
            {
                errores.Add("El correo debe de tener un formato válido, por ejemplo: alumno@universidad.edu.");
            }

            return errores;
        }

        // Revisa los datos antes de registrar un estudiante.
        // Debe recibir TODOS los estudiantes almacenados para detectar duplicados.
        public static List<string> ValidarRegistro(
            Estudiante estudiante,
            IEnumerable<Estudiante> estudiantes)
        {
            // La colección no puede ser nula
            ComprobarColeccion(estudiantes);

            // Primero se revisan los campos del estudiante
            List<string> errores = ValidarDatos(estudiante);

            // No se permite registrar un carné que ya exista
            if (estudiante != null && !string.IsNullOrWhiteSpace(estudiante.Carne) &&
                ExisteCarne(estudiante.Carne, estudiantes))
            {
                errores.Add("Ya existe un estudiante con ese carné");
            }

            // No se permite registrar un correo que ya exista
            if (estudiante != null && !string.IsNullOrWhiteSpace(estudiante.Correo) &&
                ExisteCorreo(estudiante.Correo, estudiantes))
            {
                errores.Add("Ya existe un estudiante con ese correo");
            }

            // Devuelve los errores de los campos y los posibles duplicados
            return errores;
        }

        // Revisa los datos antes de actualizar un estudiante.
        // carneOriginal es el carné del estudiante que se quiere modificar.
        public static List<string> ValidarActualizacion(
            string carneOriginal,
            Estudiante estudiante,
            IEnumerable<Estudiante> estudiantes)
        {
            // La colección no puede ser nula
            ComprobarColeccion(estudiantes);

            // Revisa que los nuevos datos cumplan las reglas de cada campo
            List<string> errores = ValidarDatos(estudiante);

            // Revisa que el estudiante original exista
            ValidarExistencia(carneOriginal, estudiantes, errores);

            // El carné es el identificador, por lo que no puede cambiar
            if (estudiante != null &&
                !string.IsNullOrWhiteSpace(carneOriginal) &&
                !string.IsNullOrWhiteSpace(estudiante.Carne) &&
                !SonElMismoCarne(carneOriginal, estudiante.Carne))
            {
                errores.Add("El carné no se puede modificar, conserve el id original");
            }

            return errores;
        }

        // Valida que el estudiante exista antes de intentar eliminarlo
        public static List<string> ValidarEliminacion(
            string carne,
            IEnumerable<Estudiante> estudiantes)
        {
            // La colección no puede ser nula
            ComprobarColeccion(estudiantes);

            // Lista donde se guardan los errores
            var errores = new List<string>();

            // Revisa que se haya indicado un carné y que esté registrado
            ValidarExistencia(carne, estudiantes, errores);

            return errores;
        }

        // Valida que el estudiante exista antes de intentar buscarlo.
        // Sigue la misma lógica que ValidarEliminacion, ya que buscar y
        // eliminar comparten la misma regla: el carné debe existir.
        public static List<string> ValidarBusqueda(
            string carne,
            IEnumerable<Estudiante> estudiantes)
        {
            // La colección no puede ser nula
            ComprobarColeccion(estudiantes);

            // Lista donde se guardan los errores
            var errores = new List<string>();

            // Revisa que se haya indicado un carné y que esté registrado
            ValidarExistencia(carne, estudiantes, errores);

            return errores;
        }

        // Revisa si son el mismo carné, ignorando mayúsculas y espacios al inicio o al final
        public static bool SonElMismoCarne(string primero, string segundo)
        {
            // Los valores vacíos no se consideran carnés iguales
            if (string.IsNullOrWhiteSpace(primero) ||
                string.IsNullOrWhiteSpace(segundo))
            {
                return false;
            }

            // Compara los valores ignorando espacios al inicio o al final
            return string.Equals(
                primero.Trim(),
                segundo.Trim(),
                StringComparison.OrdinalIgnoreCase
            );
        }

        // Comprueba si un carné ya existe en la colección de estudiantes
        public static bool ExisteCarne(string carne,
            IEnumerable<Estudiante> estudiantes)
        {
            // La colección no puede ser nula
            ComprobarColeccion(estudiantes);

            // Devuelve true si algún estudiante tiene el mismo carné
            return estudiantes.Any(e => e != null && SonElMismoCarne(e.Carne, carne));
        }

        // Revisa si son el mismo correo, ignorando mayúsculas y espacios al inicio o al final
        public static bool SonElMismoCorreo(string primero, string segundo)
        {
            // Los valores vacíos no se consideran correos iguales
            if (string.IsNullOrWhiteSpace(primero) ||
                string.IsNullOrWhiteSpace(segundo))
            {
                return false;
            }

            // Compara ignorando mayúsculas, así Ana@x.com y ana@x.com son el mismo correo
            return string.Equals(
                primero.Trim(),
                segundo.Trim(),
                StringComparison.OrdinalIgnoreCase
            );
        }

        // Comprueba si un correo ya existe en la colección de estudiantes
        public static bool ExisteCorreo(string correo,
            IEnumerable<Estudiante> estudiantes)
        {
            // La colección no puede ser nula
            ComprobarColeccion(estudiantes);

            // Devuelve true si algún estudiante tiene el mismo correo
            return estudiantes.Any(e => e != null && SonElMismoCorreo(e.Correo, correo));
        }

        // Comprueba que se haya indicado un carné y que exista en la colección.
        // Agrega el error correspondiente a la lista si algo falla.
        private static void ValidarExistencia(
            string carne,
            IEnumerable<Estudiante> estudiantes,
            List<string> errores)
        {
            // Si no se indicó un carné, se agrega el error
            if (string.IsNullOrWhiteSpace(carne))
            {
                errores.Add("Debe indicar el carné del estudiante.");
            }
            // Solo busca en la colección si el carné tiene contenido
            else if (!ExisteCarne(carne, estudiantes))
            {
                errores.Add("No se encontró un estudiante con ese carné");
            }
        }

        // Comprueba que el campo no esté vacío y que solo tenga caracteres válidos para XML
        private static void ValidarTextoObligatorio(
            string valor,
            string campo,
            List<string> errores)
        {
            // Detecta si falta el contenido del campo
            if (string.IsNullOrWhiteSpace(valor))
            {
                // Une el nombre del campo con el mensaje
                // Ejemplo: "Los nombres: es un dato obligatorio."
                errores.Add(campo + ": es un dato obligatorio.");

                // No hay texto que revisar. El método que lo llamó continúa con los demás campos.
                return;
            }

            try
            {
                // Lanza una XmlException si encuentra un carácter que XML no permite.
                // Símbolos normales como & y < sí se permiten, porque el serializador
                // se encarga de representarlos correctamente.
                XmlConvert.VerifyXmlChars(valor);
            }
            catch (XmlException)
            {
                // Convierte ese problema en un mensaje de validación
                errores.Add(campo + ": contiene caracteres no permitidos en XML");
            }
        }

        // Verifica si el correo tiene un formato válido
        public static bool EsCorreoValido(string correo)
        {
            // Un correo vacío no es válido
            if (string.IsNullOrWhiteSpace(correo))
            {
                return false;
            }

            // Quita espacios al inicio y al final
            string texto = correo.Trim();

            try
            {
                // MailAddress lanza FormatException si el formato no es válido
                var direccion = new MailAddress(texto);

                // Comprueba que la dirección resultante sea igual al texto escrito.
                // Así se rechazan entradas como "Nombre <a@b.com>", que MailAddress acepta.
                return string.Equals(
                    direccion.Address,
                    texto,
                    StringComparison.OrdinalIgnoreCase
                );
            }
            catch (FormatException)
            {
                // El formato no es válido
                return false;
            }
        }

        // Verifica si el carné tiene un formato válido: 6 caracteres numéricos.
        // Versión simple que solo devuelve true o false.
        public static bool EsCarneValido(string carne)
        {
            // Un carné vacío no es válido
            if (string.IsNullOrWhiteSpace(carne))
            {
                return false;
            }

            // Quita espacios al inicio y al final
            string texto = carne.Trim();

            try
            {
                // Intenta convertir el carné a un número entero
                int numero = int.Parse(texto);
            }
            catch (FormatException)
            {
                // No es un número válido
                return false;
            }

            // Debe tener 6 caracteres y no contener signos
            if (carne.Length != 6 || carne.Contains('+') || carne.Contains('-'))
            {
                return false;
            }
            else
            {
                // Comprueba que no haya espacios sobrantes en el carné original
                return string.Equals(
                    texto,
                    carne,
                    StringComparison.OrdinalIgnoreCase
                );
            }
        }

        // Verifica si el carné tiene un formato válido: 6 dígitos numéricos.
        // Además devuelve en mensajeError el motivo del rechazo, para mostrarlo al usuario.
        public static bool EsCarneValido(string carne, out string mensajeError)
        {
            // 1. Validar que no esté vacío
            if (string.IsNullOrWhiteSpace(carne))
            {
                mensajeError = "El carné no puede estar vacío.";
                return false;
            }

            string texto = carne.Trim();

            // 2. Validar que tenga exactamente 6 caracteres
            if (texto.Length != 6)
            {
                mensajeError = "El carné debe contener exactamente 6 dígitos.";
                return false;
            }

            // 3. Validar que no contenga signos (+ o -)
            if (texto.Contains('+') || texto.Contains('-'))
            {
                mensajeError = "El carné no debe contener signos (+ o -).";
                return false;
            }

            // 4. Validar que sea numérico
            if (!int.TryParse(texto, out _))
            {
                mensajeError = "El carné debe ser un número entero de 6 dígitos.";
                return false;
            }

            // Pasó todas las validaciones
            mensajeError = string.Empty;
            return true;
        }

        // Comprueba que quien llama al validador entregue una colección.
        // Si es nula, lanza una excepción para avisar que es un error de programación.
        private static void ComprobarColeccion(IEnumerable<Estudiante> estudiantes)
        {
            if (estudiantes == null)
            {
                throw new ArgumentNullException(
                    nameof(estudiantes),
                    "Debe de dar la colección completa de estudiantes cargados"
                );
            }
        }
    }
}