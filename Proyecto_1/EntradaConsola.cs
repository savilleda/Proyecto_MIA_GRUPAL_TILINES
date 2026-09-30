using System;
using System.Collections.Generic;

namespace GestionEstudiantes
{
    // Métodos de entrada por consola que comparten el arranque (Proyecto_1)
    // y el menú (CambiarContrasena). Están aparte para que ambos puedan usarlos.
    public static class EntradaConsola
    {
        // Lee una contraseña ocultando lo que se escribe.
        // Si la entrada está redirigida (por ejemplo, en pruebas automáticas),
        // no se pueden leer teclas y se usa ReadLine normal.
        public static string LeerContrasena(string mensaje)
        {
            Console.Write(mensaje);
            if (Console.IsInputRedirected)
            {
                return Console.ReadLine() ?? string.Empty;
            }

            // Guardamos cada carácter escrito sin mostrarlo en pantalla
            List<char> caracteres = new List<char>();
            ConsoleKeyInfo tecla;
            do
            {
                // intercept: true evita que la tecla se muestre en la consola
                tecla = Console.ReadKey(intercept: true);
                if (tecla.Key == ConsoleKey.Backspace && caracteres.Count > 0)
                {
                    // Borra el último carácter escrito
                    caracteres.RemoveAt(caracteres.Count - 1);
                }
                else if (tecla.Key != ConsoleKey.Enter &&
                         !char.IsControl(tecla.KeyChar))
                {
                    // Ignora ENTER y teclas de control; agrega el resto
                    caracteres.Add(tecla.KeyChar);
                }
            } while (tecla.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return new string(caracteres.ToArray());
        }

        // Pide una contraseña nueva y su confirmación.
        // Se repite hasta que coincidan. Devuelve null si el usuario cancela con ENTER.
        public static string? SolicitarNuevaContrasena()
        {
            while (true)
            {
                string nueva = LeerContrasena(
                    "Nueva contraseña (ENTER para cancelar): "
                );

                if (string.IsNullOrWhiteSpace(nueva))
                {
                    return null;
                }

                string confirmacion = LeerContrasena(
                    "Confirme la nueva contraseña: "
                );

                // Ordinal distingue mayúsculas y minúsculas: las contraseñas deben ser idénticas
                if (string.Equals(
                    nueva,
                    confirmacion,
                    StringComparison.Ordinal))
                {
                    return nueva;
                }

                Console.WriteLine(
                    "Las contraseñas no coinciden. Intente nuevamente."
                );
                Console.WriteLine();
            }
        }
    }
}