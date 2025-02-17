using System;
using System.Security.Cryptography;
using System.Text;

namespace EventoMercantiles.Datos
{
    public static class ActivacionHelper
    {
        /// <summary>
        /// Genera una clave única basada en el NIT, nombre de la empresa, mes y año.
        /// </summary>
        /// <param name="nit">NIT de la empresa.</param>
        /// <param name="nombreEmpresa">Nombre de la empresa.</param>
        /// <param name="mes">Mes actual (formato numérico).</param>
        /// <param name="anio">Año actual.</param>
        /// <returns>Una clave única de activación.</returns>
        public static string GenerarClave(string nit, string nombreEmpresa, int mes, int anio)
        {
            // Concatenar los valores para formar la base de la clave
            string baseClave = $"{nit}|{nombreEmpresa}|{mes:D2}|{anio}";

            // Aplicar codificación para mayor seguridad (utilizando SHA-256)
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(baseClave));

                // Convertir a Base64 y devolver los primeros 16 caracteres
                return Convert.ToBase64String(hash).Substring(0, 16);
            }
        }
    }
}
