using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GestionEstudiantes
{
    // Se encarga de encriptar y desencriptar datos usando AES.
    public static class CifradorAES
    {
        // Convierte la contraseña en una clave de 256 bits para AES.
        private static byte[] CrearClave(string contrasena)
        {
            return SHA256.HashData(
                Encoding.UTF8.GetBytes(contrasena)
            );
        }


        // Encripta los datos recibidos usando AES.
        public static byte[] Encriptar(
            byte[] datos,
            string contrasena)
        {
            // Creamos una instancia de AES.
            using Aes aes = Aes.Create();

            // Generamos la clave a partir de la contraseña.
            aes.Key = CrearClave(contrasena);

            // Generamos un IV aleatorio para el cifrado.
            // El IV se necesita después para desencriptar.
            aes.GenerateIV();


            // Aquí se guardará el IV y luego la información cifrada.
            using MemoryStream memoria =
                new MemoryStream();


            // Guardamos primero el IV.
            memoria.Write(
                aes.IV,
                0,
                aes.IV.Length
            );


            // CryptoStream aplica el cifrado mientras escribimos los datos.
            using CryptoStream crypto =
                new CryptoStream(
                    memoria,
                    aes.CreateEncryptor(),
                    CryptoStreamMode.Write
                );


            // Escribimos los datos originales para que sean cifrados.
            crypto.Write(
                datos,
                0,
                datos.Length
            );

            // Finaliza el proceso de cifrado.
            crypto.FlushFinalBlock();


            // Devuelve el IV seguido de los datos cifrados.
            return memoria.ToArray();
        }


        // Desencripta datos que fueron cifrados con el método anterior.
        public static byte[] Desencriptar(
            byte[] datos,
            string contrasena)
        {
            // Creamos una nueva instancia de AES.
            using Aes aes = Aes.Create();

            // Generamos nuevamente la misma clave.
            aes.Key = CrearClave(contrasena);


            // Cargamos en memoria los datos cifrados.
            using MemoryStream memoria =
                new MemoryStream(datos);


            // Reservamos espacio para recuperar el IV.
            // AES utiliza un IV de 16 bytes.
            byte[] iv =
                new byte[aes.BlockSize / 8];


            // Leemos el IV que está al inicio de los datos.
            memoria.Read(
                iv,
                0,
                iv.Length
            );

            // Asignamos el IV recuperado a AES.
            aes.IV = iv;


            // CryptoStream se encargará de desencriptar durante la lectura.
            using CryptoStream crypto =
                new CryptoStream(
                    memoria,
                    aes.CreateDecryptor(),
                    CryptoStreamMode.Read
                );


            // Aquí se almacenarán los datos ya desencriptados.
            using MemoryStream resultado =
                new MemoryStream();


            // Copiamos y desencriptamos la información.
            crypto.CopyTo(resultado);


            // Devolvemos los datos originales.
            return resultado.ToArray();
        }
    }
}

//IV o Vector de Inicialización (Initialization Vector) es un número aleatorio o pseudoaleatorio de tamaño fijo que se usa junto con una clave secreta para cifrar
//  datos. Su propósito principal es introducir aleatoriedad