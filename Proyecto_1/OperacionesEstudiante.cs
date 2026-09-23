using System;
using System.Collections.Generic;

namespace GestionEstudiantes
{
    // Clase encargada de las operaciones de actualizacion y eliminacion de estudiantes.
    // Depende de Validador (reglas de negocio) y de ManejadorXML (persistencia).
    public class OperacionesEstudiante
    {
        private readonly ManejadorXML manejador;

        // Recibe el manejador de XML ya configurado con la ruta del archivo
        public OperacionesEstudiante(ManejadorXML manejador)
        {
            if (manejador == null)
            {
                throw new ArgumentNullException(nameof(manejador));
            }

            this.manejador = manejador;
        }

        // Registra un estudiante después de validar sus datos y su carné único.
        public (bool exito, List<string> errores) RegistrarEstudiante(Estudiante estudiante)
        {
            List<Estudiante> estudiantes = manejador.LeerEstudiantes();
            if (!manejador.UltimaLecturaExitosa)
            {
                return (false, new List<string> { manejador.UltimoErrorLectura });
            }

            List<string> errores = Validador.ValidarRegistro(estudiante, estudiantes);
            if (errores.Count > 0)
            {
                return (false, errores);
            }

            estudiantes.Add(estudiante);
            if (!manejador.GuardarEstudiantes(estudiantes))
            {
                errores.Add("Ocurrió un error al guardar el estudiante en el archivo XML.");
                return (false, errores);
            }

            return (true, errores);
        }

        // Actualiza los datos de un estudiante existente.
        // carneOriginal: carné del estudiante que se desea modificar
        // estudianteActualizado: objeto con los nuevos datos (el carné debe conservarse igual)
        // Devuelve una tupla: si la operación tuvo éxito y la lista de errores encontrados (vacía si todo salió bien)
        public (bool exito, List<string> errores) ActualizarEstudiante(
            string carneOriginal,
            Estudiante estudianteActualizado)
        {
            // Carga la lista completa, ya que el XML no permite editar un solo registro directamente
            List<Estudiante> estudiantes = manejador.LeerEstudiantes();
            if (!manejador.UltimaLecturaExitosa)
            {
                return (false, new List<string> { manejador.UltimoErrorLectura });
            }

            // Valida los datos nuevos y que el estudiante original exista (regla de Marco)
            List<string> errores = Validador.ValidarActualizacion(carneOriginal, estudianteActualizado, estudiantes);
            if (errores.Count > 0)
            {
                // Si hay errores de validación, no se continúa con la operación
                return (false, errores);
            }

            // Busca la posición del estudiante dentro de la lista para reemplazarlo
            int indice = estudiantes.FindIndex(e => Validador.SonElMismoCarne(e.Carne, carneOriginal));

            // Reemplaza el objeto viejo por el actualizado en la misma posición
            estudiantes[indice] = estudianteActualizado;

            // Guarda la lista completa (con el cambio) de vuelta en el archivo XML
            bool guardadoExitoso = manejador.GuardarEstudiantes(estudiantes);
            if (!guardadoExitoso)
            {
                errores.Add("Ocurrió un error al guardar los cambios en el archivo XML");
                return (false, errores);
            }

            return (true, errores);
        }

        // Elimina un estudiante identificado por su carné.
        // carne: carné del estudiante que se desea eliminar
        // Devuelve una tupla: si la operación tuvo éxito y la lista de errores encontrados
        public (bool exito, List<string> errores) EliminarEstudiante(string carne)
        {
            // Carga la lista completa de estudiantes desde el archivo
            List<Estudiante> estudiantes = manejador.LeerEstudiantes();
            if (!manejador.UltimaLecturaExitosa)
            {
                return (false, new List<string> { manejador.UltimoErrorLectura });
            }

            // Verifica que el carné exista antes de intentar eliminar (regla de Marco)
            List<string> errores = Validador.ValidarEliminacion(carne, estudiantes);
            if (errores.Count > 0)
            {
                return (false, errores);
            }

            // RemoveAll devuelve la cantidad de elementos eliminados;
            // como ya validamos que existe, aquí siempre debería ser al menos 1
            estudiantes.RemoveAll(e => Validador.SonElMismoCarne(e.Carne, carne));

            // Persiste la lista ya sin el estudiante eliminado
            bool guardadoExitoso = manejador.GuardarEstudiantes(estudiantes);
            if (!guardadoExitoso)
            {
                errores.Add("Ocurrió un error al guardar los cambios en el archivo XML");
                return (false, errores);
            }

            return (true, errores);
        }

        // Devuelve la lista completa de estudiantes registrados en el archivo XML.
        // No requiere validación de negocio, solo delega la lectura al manejador.
        public List<Estudiante> ListarEstudiantes()
        {
            return manejador.LeerEstudiantes();
        }

        public bool LecturaValida => manejador.UltimaLecturaExitosa;
        public string ErrorLectura => manejador.UltimoErrorLectura;

        // Busca un estudiante específico a partir de su carné.
        // carne: carné del estudiante que se desea encontrar
        // Devuelve una tupla: si fue encontrado, el estudiante (o null si no existe) y la lista de errores
        public (bool encontrado, Estudiante? estudiante, List<string> errores) BuscarEstudiantePorCarne(string carne)
        {
            // Carga la lista completa, ya que el XML no permite consultar un solo registro directamente
            List<Estudiante> estudiantes = manejador.LeerEstudiantes();
            if (!manejador.UltimaLecturaExitosa)
            {
                return (false, null, new List<string> { manejador.UltimoErrorLectura });
            }

            // Valida que se haya indicado un carné y que el estudiante exista (regla de Marco)
            List<string> errores = Validador.ValidarBusqueda(carne, estudiantes);
            if (errores.Count > 0)
            {
                // Si hay errores de validación, no se continúa con la búsqueda
                return (false, null, errores);
            }

            // Busca el estudiante dentro de la lista usando la misma regla de comparación
            // que se usa en ActualizarEstudiante y EliminarEstudiante
            Estudiante? estudiante = estudiantes.Find(e => Validador.SonElMismoCarne(e.Carne, carne));

            return (true, estudiante, errores);
        }
    }
}