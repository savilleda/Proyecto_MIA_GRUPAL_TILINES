using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Buffers.Binary;

namespace GestionEstudiantes
{
    public static class CifradorAES
    {
        private static readonly byte[] Encabezado = Encoding.ASCII.GetBytes("MIA-AES-GCM-PBKDF2-1");
        private const int TamanoSal = 16;
        private const int TamanoNonce = 12;
        private const int TamanoEtiqueta = 16;
        private const int Iteraciones = 150_000;

        // Cifra los datos con AES-GCM y agrega una etiqueta para detectar cambios
        public static byte[] Encriptar(byte[] datos, string contrasena)
        {
            ValidarParametros(datos, contrasena);

            byte[] sal = RandomNumberGenerator.GetBytes(TamanoSal);
            byte[] nonce = RandomNumberGenerator.GetBytes(TamanoNonce);
            byte[] textoCifrado = new byte[datos.Length];
            byte[] etiqueta = new byte[TamanoEtiqueta];
            byte[] parametros = CrearParametros(sal);
            byte[] clave = CrearClave(contrasena, sal);

            using (AesGcm aes = new AesGcm(clave, TamanoEtiqueta))
            {
                aes.Encrypt(nonce, datos, textoCifrado, etiqueta, parametros);
            }

            using MemoryStream salida = new MemoryStream();
            salida.Write(parametros);
            salida.Write(nonce);
            salida.Write(etiqueta);
            salida.Write(textoCifrado);
            return salida.ToArray();
        }

        // Descifra y autentica los datos antes de devolverlos
        public static byte[] Desencriptar(byte[] datos, string contrasena)
        {
            if (datos is null)
            {
                throw new ArgumentNullException(nameof(datos));
            }

            if (datos.Length == 0)
            {
                throw new InvalidDataException(
                    "El archivo cifrado está vacío y no tiene un formato válido");
            }

            ValidarParametros(datos, contrasena);

            int tamanoParametros = Encabezado.Length + sizeof(int) + TamanoSal;
            int posicionTexto = tamanoParametros + TamanoNonce + TamanoEtiqueta;
            if (datos.Length <= posicionTexto ||
                !datos.AsSpan(0, Encabezado.Length).SequenceEqual(Encabezado))
            {
                throw new InvalidDataException("El archivo no tiene un formato AES-GCM válido");
            }

            int iteraciones = BinaryPrimitives.ReadInt32LittleEndian(
                datos.AsSpan(Encabezado.Length, sizeof(int)));
            if (iteraciones < 10_000 || iteraciones > 1_000_000)
            {
                throw new InvalidDataException("Los parámetros PBKDF2 del archivo no son válidos");
            }

            byte[] sal = datos[(Encabezado.Length + sizeof(int))..tamanoParametros];
            byte[] nonce = datos[tamanoParametros..(tamanoParametros + TamanoNonce)];
            byte[] etiqueta = datos[(tamanoParametros + TamanoNonce)..posicionTexto];
            byte[] textoCifrado = datos[posicionTexto..];
            byte[] textoOriginal = new byte[textoCifrado.Length];
            byte[] parametros = datos[..tamanoParametros];
            byte[] clave = CrearClave(contrasena, sal, iteraciones);

            try
            {
                using AesGcm aes = new AesGcm(clave, TamanoEtiqueta);
                aes.Decrypt(nonce, textoCifrado, etiqueta, textoOriginal, parametros);
            }
            catch (CryptographicException)
            {
                throw new CryptographicException(
                    "La contraseña es incorrecta o el archivo cifrado fue modificado");
            }

            return textoOriginal;
        }

        private static byte[] CrearClave(
            string contrasena,
            byte[] sal,
            int iteraciones = Iteraciones)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                contrasena,
                sal,
                iteraciones,
                HashAlgorithmName.SHA256,
                32);
        }

        private static byte[] CrearParametros(byte[] sal)
        {
            byte[] parametros = new byte[Encabezado.Length + sizeof(int) + sal.Length];
            Encabezado.CopyTo(parametros, 0);
            BinaryPrimitives.WriteInt32LittleEndian(
                parametros.AsSpan(Encabezado.Length, sizeof(int)),
                Iteraciones);
            sal.CopyTo(parametros, Encabezado.Length + sizeof(int));
            return parametros;
        }

        private static void ValidarParametros(byte[]? datos, string contrasena)
        {
            if (datos == null || datos.Length == 0)
            {
                throw new ArgumentException("No hay datos para procesar", nameof(datos));
            }

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                throw new ArgumentException("La contraseña no puede estar vacía", nameof(contrasena));
            }
        }
    }
}
