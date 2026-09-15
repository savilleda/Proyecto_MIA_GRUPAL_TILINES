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
            this.manejador = manejador;
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
    }
}