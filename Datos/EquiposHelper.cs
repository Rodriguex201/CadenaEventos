using System;
using System.Collections.Generic;
using System.Data.Odbc;

namespace EventoMercantiles.Datos
{
    public static class EquiposHelper
    {
        public static List<EquipoInfo> ObtenerEquipos()
        {
            List<EquipoInfo> equipos = new List<EquipoInfo>();

            try
            {
                using (var connection = Conexion.ObtenerConexion())
                {
                    Console.WriteLine("🔄 Intentando conectar a la base de datos de equipos...");
                    connection.Open();
                    Console.WriteLine("✅ Conexión exitosa.");

                    // Depurar qué base de datos se está consultando
                    using (var commandCheck = new OdbcCommand("SELECT DATABASE()", connection))
                    {
                        object dbName = commandCheck.ExecuteScalar();
                        Console.WriteLine($"📌 Base de datos actual: {dbName}");
                    }

                    // Ejecutar consulta
                    string query = @"
                        SELECT empresa, nro_mac, maquina, factivar, facceso, fsuspende 
                        FROM empresas.llequipo";

                    using (var command = new OdbcCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            Console.WriteLine("⚠ La consulta no devolvió registros.");
                            return equipos;
                        }

                        while (reader.Read())
                        {
                            try
                            {
                                // Imprimir los valores antes de asignarlos
                                Console.WriteLine($"Leyendo fila: {reader[0]} | {reader[1]} | {reader[2]} | {reader[3]} | {reader[4]} | {reader[5]}");

                                equipos.Add(new EquipoInfo
                                {
                                    Empresa = ObtenerTextoSeguro(reader, 0, "Sin empresa"),
                                    NroMac = ObtenerTextoSeguro(reader, 1, "Sin MAC"),
                                    Maquina = ObtenerTextoSeguro(reader, 2, "Sin máquina"),
                                    FechaActivacion = ObtenerFechaSegura(reader, 3, "Fecha no registrada"),
                                    FechaAcceso = ObtenerFechaSegura(reader, 4, "Sin acceso"),
                                    FechaSuspension = ObtenerFechaSegura(reader, 5, "No suspendido")
                                });
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"❌ Error al leer fila: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en la conexión: {ex.Message}");
            }

            return equipos;
        }

        /// <summary>
        /// Método seguro para obtener valores de texto.
        /// </summary>
        private static string ObtenerTextoSeguro(OdbcDataReader reader, int columnIndex, string valorPredeterminado)
        {
            try
            {
                if (!reader.IsDBNull(columnIndex))
                {
                    return reader[columnIndex].ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer columna {columnIndex}: {ex.Message}");
            }
            return valorPredeterminado;
        }

        /// <summary>
        /// Método seguro para obtener fechas sin errores de conversión.
        /// </summary>
        private static string ObtenerFechaSegura(OdbcDataReader reader, int columnIndex, string valorPredeterminado)
        {
            try
            {
                if (!reader.IsDBNull(columnIndex))
                {
                    string fechaTexto = reader[columnIndex].ToString().Trim();

                    // Depuración: Imprimir el valor antes de convertir
                    Console.WriteLine($"📌 Valor crudo de fecha en columna {columnIndex}: {fechaTexto}");

                    // Si la fecha es "0000-00-00" o algo inválido, retorna un valor predeterminado
                    if (fechaTexto == "0000-00-00" || !DateTime.TryParse(fechaTexto, out DateTime fecha))
                    {
                        return valorPredeterminado;
                    }

                    return fecha.ToString("yyyy-MM-dd");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al obtener fecha en columna {columnIndex}: {ex.Message}");
            }

            return valorPredeterminado;
        }

        /// <summary>
        /// Método para actualizar las fechas de activación y suspensión de un equipo en la base de datos.
        /// </summary>
        public static void ActualizarFechasEquipo(string empresa, string nroMac, string nuevaFechaActivacion, string nuevaFechaSuspension)
        {
            try
            {
                using (var connection = Conexion.ObtenerConexion())
                {
                    connection.Open();
                    Console.WriteLine($"🔄 Actualizando fechas para empresa: {empresa}, MAC: {nroMac}");

                    string query = "UPDATE empresas.llequipo SET factivar = ?, fsuspende = ? WHERE empresa = ? AND nro_mac = ?";

                    using (var command = new OdbcCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("?", nuevaFechaActivacion);
                        command.Parameters.AddWithValue("?", string.IsNullOrEmpty(nuevaFechaSuspension) ? (object)DBNull.Value : nuevaFechaSuspension);
                        command.Parameters.AddWithValue("?", empresa);
                        command.Parameters.AddWithValue("?", nroMac);

                        int filasAfectadas = command.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                        {
                            Console.WriteLine("✅ Fechas actualizadas correctamente.");
                        }
                        else
                        {
                            Console.WriteLine("⚠ No se encontró el registro para actualizar.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al actualizar fechas: {ex.Message}");
            }
        }




    }

    public class EquipoInfo
    {
        public string Empresa { get; set; }
        public string NroMac { get; set; }
        public string Maquina { get; set; }
        public string FechaActivacion { get; set; }
        public string FechaAcceso { get; set; }
        public string FechaSuspension { get; set; }
    }
}
