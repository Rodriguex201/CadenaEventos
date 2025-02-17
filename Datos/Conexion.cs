using System;
using System.Collections.Generic;
using System.Data.Odbc;

namespace EventoMercantiles.Datos
{
    public class Conexion
    {
        private static List<string> datosconexion;

        /// <summary>
        /// Obtiene la conexión al servidor principal.
        /// </summary>
        public static OdbcConnection ObtenerConexion()
        {
            datosconexion = CadenaConexion.ReadConexion();

            string MyConString = "DRIVER={MySQL ODBC 5.1 Driver};" +
                                 "SERVER=" + datosconexion[0] + ";" +
                                 "DATABASE=" + datosconexion[1] + ";" +
                                 "UID=RmSoft20X;" +
                                 "PASSWORD=*LiLo89*;" +
                                 "Option=3;";
            return new OdbcConnection(MyConString);
        }

        /// <summary>
        /// Obtiene la conexión al servidor secundario (168.232.32.74) usando el nombre de la empresa como base de datos.
        /// </summary>
        public static OdbcConnection ObtenerConexionServidorSecundario(string empresa)
        {
            try
            {
                string connectionString = "DRIVER={MySQL ODBC 5.1 Driver};" +
                                          "SERVER=168.232.32.74;PORT=3306;" +
                                          "DATABASE=" + empresa + ";" +
                                          "UID=RMDC;" +
                                          "PASSWORD=Adict057;" +
                                          "Option=3;";

                return new OdbcConnection(connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al conectar con el servidor secundario: {ex.Message}");
                return null;
            }
        }

        public static OdbcConnection ObtenerConexionEquipos()
        {
            string MyConString = "DRIVER={MySQL ODBC 5.1 Driver};" +
                                 "SERVER=200.118.190.213;" +
                                 "DATABASE=empresas;" +
                                 "UID=RmSoft20X;" +
                                 "PASSWORD=*LiLo89*;" +
                                 "Option=3;";

            return new OdbcConnection(MyConString);
        }

    }
}
