using System;
// Deja usar list e Inumerable
using System.Collections.Generic;
//Deja utilizar Any para buscar coincidencias en listas
using System.Linq;
//Permite revisar el formato de correo electronico
using System.Net.Mail;
//Permite comprobar si un texto contiene caracteres validos para XML
using System.Xml;
// El namespace agrupa las clases del proyecto para mantener el código organizado
// y evitar conflictos si existen clases con nombres parecidos en otros lugares
namespace GestionEstudiantes
{
    
    public static class Validador
    {
        //Este método revisa que el correo tenga un formato válido
        public static List<string> ValidarDatos(Estudiante estudiante)
        {
            //Crea una lista vacía para guardar errores
            var errores = new List<string>();
            //Comprueba si no se recibió un objeto estudiante
            if(estudiante == null)
            {
                //Agrega un mensaje de error a la lista y la devuelve
                errores.Add("No se recibió un estudiante para validar");
                return errores;
            }
            //Revisa que el carné tenga contenido y caracteres válidos para XML
            //El segundo parametro es el nombre que aparecerá en el mensaje de error si la validación falla
            //El tercero es la lista donde el método agrega los errores
            ValidarTextoObligatorio(estudiante.Carne, "El carné", errores);
            //Revisa que los nombres tengan contenido valido
            ValidarTextoObligatorio(estudiante.Nombres, "Los nombres", errores);
            //Revisa que los apellidos tengan contenido valido
            ValidarTextoObligatorio(estudiante.Apellidos, "Los apellidos", errores);
            //Revisa que el correo tenga contenido valido
            ValidarTextoObligatorio(estudiante.Correo, "El correo", errores);
            //Este if revisa que el correo tenga contenido y que su formato sea válido
            if(!string.IsNullOrWhiteSpace(estudiante.Correo) && !EsCorreoValido(estudiante.Correo))
            {
               //Agrega el error si el correo tiene contenido pero su formato no es válido 
               errores.Add("El correo debe de tener un formato válido, por ejemplo: alumno@universidad.edu.");
            }
            return errores;
        }
        //Revisa los datos antes de registrar un estudiante
        // Debe recibir TODOS los estudiantes almacenados para detectar duplicados
          public static List<string> ValidarRegistro(
            Estudiante estudiante,
            IEnumerable<Estudiante> estudiantes)
        {
            //Comprueba que no sea nulo
            ComprobarColeccion(estudiantes);
            //Revisa los campos del estudiante y guarda los errores
            List<string> errores = ValidarDatos(estudiante);
            if(estudiante != null && !string.IsNullOrWhiteSpace(estudiante.Carne) && 
            ExisteCarne(estudiante.Carne, estudiantes))
            {
                //No deja registrar un estudiante con un carné que ya exista
                Console.WriteLine("El carné encontrado fue: " + estudiante.Carne +" pero no es único");
                errores.Add("Ya existe un estudiante con ese carne");
            }
            //Devuelve los errores de los campos y posible carné duplicado
            return errores;
        }

        public static List<string> ValidarActualizacion(
            string carneOriginal,
            Estudiante estudiante,
            IEnumerable<Estudiante> estudiantes)
        {
            //Comprueba que se haya proporcionado una colección
            ComprobarColeccion(estudiantes);
            //Revisa que los nuevos datos cumplen las reglas de los campos
            List<string> errores = ValidarDatos(estudiante);
            //Comprueba que el estudiante original exista
            //Si detecta un problema lo manda a la lista de errores
            ValidarExistencia(carneOriginal, estudiantes, errores);
            //Comprueba que el carné no se haya modificado
           if (estudiante != null &&
            !string.IsNullOrWhiteSpace(carneOriginal) &&
            !string.IsNullOrWhiteSpace(estudiante.Carne) &&
            !SonElMismoCarne(carneOriginal, estudiante.Carne))
            {
                errores.Add("El carné no se puede modificar, conserve el id original");
            }
            return errores;
        }

         //Valida que el estudiante exista antes de intentar eliminarlo
         public static List<string> ValidarEliminacion(
            string carne,
            IEnumerable<Estudiante> estudiantes)
        {
            //Comprueba que la colección no sea nula
            ComprobarColeccion(estudiantes);
            //Crea la lista donde se guardarán los errores
            var errores = new List<string>();
            //revisa que se haya indicado un carné y que esté registrado
            ValidarExistencia(carne, estudiantes, errores);
            //Devuelve la lista con los resultados de la validación
            return errores;
        }

        //Revisa si son el mismo carné, ignorando mayúsculas y espacios al inicio o final
        public static bool SonElMismoCarne(string primero, string segundo)
        {
            //Comprueba si ambos valores son nulos o vacíos
            if(string.IsNullOrWhiteSpace(primero) ||
            string.IsNullOrWhiteSpace(segundo))
            {
                //Los valores vacios no se consideran carnes iguales
                return false;
            }

            //Compara los valores ignorando mayúsculas y espacios al inicio o final
            return string.Equals(
                primero.Trim(),
                segundo.Trim(),
                StringComparison.OrdinalIgnoreCase
            );
        }
         //Comprueba si un carné ya existe en la colección de estudiantes
         public static bool ExisteCarne(string carne, 
         IEnumerable<Estudiante> estudiantes)
        {
            //Comprueba que la colección no sea nulo
            ComprobarColeccion(estudiantes);
            //Comprueba si algún estudiante tiene el mismo carné que el proporcionado
            return estudiantes.Any(e => e != null && SonElMismoCarne(e.Carne, carne));     
        }

        //Comprueba que la colección no sea nula
        private static void ValidarExistencia(
            string carne,
            IEnumerable<Estudiante> estudiantes,
            List<string> errores)
        {
            //Comprueba que la colección no sea nula
            if(string.IsNullOrWhiteSpace(carne))
            {
                //Agrega un error si no se proporcionó un carné
                errores.Add("Debe indicar el carné del estudiante que desea modificar o eliminar");
            }
            //Solo lo verifica si tiene contenido adentro  
            else if(!ExisteCarne(carne, estudiantes))
            {
                //Agrega un error si el carné no se encuentra en la colección
                errores.Add("No se encontró un estudiante con ese carné");
            }
        }
        
        //El metodo comrpeuba que el campo tenga contenido válido para XML
          private static void ValidarTextoObligatorio(
            string valor,
            string campo,
            List<string> errores)
        {
            // Detecta si falta el contenido del campo
            if (string.IsNullOrWhiteSpace(valor))
            {
                // Une el nombre del campo con el mensaje
                // Ejemplo: "Los nombres: es un dato obligatorio"
                errores.Add(campo + ": es un dato obligatorio.");
                // Termina este método porque no hay texto que revisar
                // El método que lo llamó puede continuar con los demás campos
                return;
            }

            try
            {
                // Comprueba que todos los caracteres puedan guardarse en XML
                // Si encuentra uno no permitido, lanza una XmlException
                // Símbolos normales como & y < se permiten:
                // el serializador se encarga de representarlos correctamente
                XmlConvert.VerifyXmlChars(valor);
            }
            // Captura específicamente el error de caracteres inválidos para XML
            catch (XmlException)
            {
                // Convierte ese problema en un mensaje de validación
                errores.Add(campo + ": contiene caracteres no permitidos en XML");
            }
        }
        //Verifica si el correo tiene un formato válido
        private static bool EsCorreoValido(string correo)
        {
            //Quita espacios al inicio y final del correo
            string texto = correo.Trim();
            //Si el correo está vacío, no es válido
            try
            {
                var direccion = new MailAddress(texto);
                //Retorna true si el correo tiene un formato válido, ignorando mayúsculas y minúsculas
                return string.Equals(
                    direccion.Address,
                    texto,
                    StringComparison.OrdinalIgnoreCase
                );
            }catch (FormatException)
            {
                //Si el correo no tiene un formato válido, retorna false
                return false;
            }
        }
        //Compruba que el codigo que llama al validador entregue una coleccion
        private static void ComprobarColeccion(IEnumerable<Estudiante> estudiantes)
        {
            //Si la colección es nula, lanza una excepción para que el código que llama al validador sepa que debe proporcionar la colección
          if(estudiantes == null)
            {

                throw new ArgumentNullException(
                    nameof(estudiantes),
                    "Debe de dar la coleccion completa de estudiantes cargados"
                );
            }
        }
    }
}