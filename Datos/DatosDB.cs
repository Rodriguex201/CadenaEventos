
using System;
using System.Data.Odbc;
using EventoMercantiles.Models;

namespace EventoMercantiles.Datos
{
    public class DatosDB
    {

        // Método para obtener una empresa por clave
        public static EmpresaInfo ObtenerEmpresaPorClave(string clave)
        {
            using (var connection = Conexion.ObtenerConexion())
            {
                connection.Open();
                string query = "SELECT dt_nit, dt_nombre FROM datosempresa WHERE dt_clave = ?";
                using (var command = new OdbcCommand(query, connection))
                {
                    command.Parameters.AddWithValue("?", clave);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new EmpresaInfo
                            {
                                Nit = reader.GetString(0),
                                Nombre = reader.GetString(1)
                            };
                        }
                    }
                }
            }
            return null; // Si no se encuentra una empresa con esa clave
        }


        public static void CreateTablas()
        {
            try
            {
                var connection = Conexion.ObtenerConexion();
                connection.Open();

                // Crear tabla 'datosempresa' si no existe
                new OdbcCommand("CREATE TABLE IF NOT EXISTS `datosempresa` (" +
                    "  `idDtEmp` INT(11) NOT NULL AUTO_INCREMENT," +
                    "  `dt_token` CHAR(128) NULL DEFAULT ''," +
                    "  `dt_url` CHAR(128) NULL DEFAULT ''," +
                    "  `dt_nit` CHAR(14) NULL DEFAULT ''," +
                    "  `dt_nombre` CHAR(128) NULL DEFAULT ''," +
                    "  `dt_nombre2` CHAR(128) NULL DEFAULT ''," +
                    "  `dt_clave` CHAR(128) NULL DEFAULT ''," +
                    "  PRIMARY KEY(`idDtEmp`));", connection).ExecuteNonQuery();

                // Verificar si la columna 'dt_clave' existe
                string verificarColumnaQuery = @"
            SELECT 1
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
            AND TABLE_NAME = 'datosempresa'
            AND COLUMN_NAME = 'dt_clave'";

                using (var verificarColumnaCommand = new OdbcCommand(verificarColumnaQuery, connection))
                {
                    object resultado = verificarColumnaCommand.ExecuteScalar();

                    // Si el resultado es null, significa que la columna no existe
                    if (resultado == null)
                    {
                        // Agregar la columna 'dt_clave' si no existe
                        string agregarColumnaQuery = "ALTER TABLE datosempresa ADD COLUMN dt_clave CHAR(128) NULL DEFAULT ''";
                        new OdbcCommand(agregarColumnaQuery, connection).ExecuteNonQuery();
                        Console.WriteLine("Columna 'dt_clave' agregada correctamente.");
                    }
                }

                // Crear tabla 'eventos' si no existe
                new OdbcCommand("CREATE TABLE IF NOT EXISTS `eventos` (" +
                    "  `id_eventos` INT(11) NOT NULL AUTO_INCREMENT," +
                    "  `even_docum` CHAR(128) NULL DEFAULT ''," +
                    "  `even_receptor` CHAR(128) NULL DEFAULT ''," +
                    "  `even_identif` CHAR(128) NULL DEFAULT ''," +
                    "  `even_fecha` CHAR(128) NULL DEFAULT ''," +
                    "  `even_evento` CHAR(45) NULL DEFAULT ''," +
                    "  `even_cufe` CHAR(254) NULL DEFAULT ''," +
                    "  `even_codigo` CHAR(45) NULL DEFAULT ''," +
                    "  `even_response` TEXT NULL DEFAULT NULL," +
                    "  `even_qrcode` VARCHAR(254) NULL DEFAULT ''," +
                    "  `even_xmlb64` MEDIUMTEXT NULL DEFAULT NULL," +
                    "  PRIMARY KEY(`id_eventos`));", connection).ExecuteNonQuery();

                connection.Close();
            }
            catch (Exception ex)
            {
                // Manejar cualquier error y mostrar un mensaje o log
                Console.WriteLine($"Ocurrió un error: {ex.Message}");
            }
        }




        public static void GuardarClave(string nit, string clave, string empresa)
        {
            // Guardar la clave en el servidor principal
            GuardarClaveEnServidor(Conexion.ObtenerConexion(), nit, clave);

            // Guardar la clave en el servidor secundario (168.232.32.74)
            using (var conexionSecundaria = Conexion.ObtenerConexionServidorSecundario(empresa))
            {
                if (conexionSecundaria != null)
                {
                    GuardarClaveEnServidor(conexionSecundaria, nit, clave);
                }
                else
                {
                    Console.WriteLine($"No se pudo conectar al servidor secundario para la empresa: {empresa}");
                }
            }
        }

        /// <summary>
        /// Método reutilizable para guardar la clave en una conexión dada.
        /// </summary>
        private static void GuardarClaveEnServidor(OdbcConnection connection, string nit, string clave)
        {
            try
            {
                connection.Open();

                // Crear la tabla si no existe
                CreateTablas();

                // Guardar la clave en la tabla `datosempresa`
                string query = "UPDATE datosempresa SET dt_clave = ? WHERE dt_nit = ?";
                using (var command = new OdbcCommand(query, connection))
                {
                    command.Parameters.AddWithValue("?", clave);
                    command.Parameters.AddWithValue("?", nit);
                    int filasAfectadas = command.ExecuteNonQuery();

                    // Si no se actualizó ninguna fila, insertar una nueva entrada
                    if (filasAfectadas == 0)
                    {
                        string insertarQuery = "INSERT INTO datosempresa (dt_nit, dt_clave) VALUES (?, ?)";
                        using (var insertarCommand = new OdbcCommand(insertarQuery, connection))
                        {
                            insertarCommand.Parameters.AddWithValue("?", nit);
                            insertarCommand.Parameters.AddWithValue("?", clave);
                            insertarCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar la clave en el servidor: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }






    }
}
