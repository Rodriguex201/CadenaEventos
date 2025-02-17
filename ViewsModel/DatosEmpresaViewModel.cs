using EventoMercantiles.Datos;
using EventoMercantiles.Models;
using System.Collections.Generic;
using System.Data.Odbc;

namespace EventoMercantiles.ViewsModel
{
    public class DatosEmpresaViewModel
    {
        public static void PostDatosEmpresa(string token, string url, string nit, string nombre, string nombre2)
        {
            var connection = Conexion.ObtenerConexion();
            connection.Open();
            var _ = new OdbcCommand("INSERT INTO `datosempresa` (`dt_token`, `dt_url`, `dt_nit`,`dt_nombre`,`dt_nombre2`) " +
                "VALUES ('"+token+"', '"+url+"', '"+nit+"', '"+nombre+"', '"+nombre2+"');", connection).ExecuteNonQuery();
            connection.Close();
        }
        public static void PostUpdateDatosEmpresa(string token, string url, string nit, string nombre, string nombre2)
        {
            var connection = Conexion.ObtenerConexion();
            connection.Open();
            var _ = new OdbcCommand("UPDATE `datosempresa` SET `dt_token` = '" + token + "', `dt_url` = '" + url + "', `dt_nit` = '" + nit + "', `dt_nombre` = '" + nombre + "', `dt_nombre2` = '" + nombre2 + "' " +
                "WHERE (`idDtEmp` = '1');", connection).ExecuteNonQuery();
            connection.Close();
        }



        public static List<DatosEmrpesaModel> GetDatosEmpresa()
        {
            var connection = Conexion.ObtenerConexion();
            List<DatosEmrpesaModel> datos = new();
            connection.Open();
            var _ = new OdbcCommand("SELECT idDtEmp,dt_token,dt_nit,dt_url,dt_nombre,dt_nombre2 FROM datosempresa where idDtEmp= 1;", connection).ExecuteReader();
            if (_.HasRows)
            {
                while (_.Read())
                {
                    datos.Add(new DatosEmrpesaModel
                    {
                        IdDtEmp = _.GetInt32(0),
                        Dt_token = _.GetString(1),
                        Dt_nit = _.GetString(2),
                        Dt_url = _.GetString(3),
                        Dt_nombre = _.GetString(4),
                        Dt_nombre2 = _.GetString(5),
                    });
                }
            }
            connection.Close();
            return datos;
        }


        public static List<DatosEmrpesaModel> GetDatosModulos( string empresa, string mac)
        {
            var connection = Conexion.ObtenerConexion();
            List<DatosEmrpesaModel> datos = new();
            connection.Open();
            var _ = new OdbcCommand("SELECT modulos FROM empresas.llequipo where empresa='"+empresa+"' and nro_mac='"+mac+"';", connection).ExecuteReader();
            if (_.HasRows)
            {
                while (_.Read())
                {                  
                    datos.Add(new DatosEmrpesaModel
                    {
                           Modulos = _.GetString(0)
                    });
                }
            }
            connection.Close();
            return datos;
        }
        public static void PostUpdateDatosModulos(string modulos, string fecha, string mac, string empresa)
        {
            var connection = Conexion.ObtenerConexion();
            connection.Open();
            var _ = new OdbcCommand("UPDATE empresas.llequipo SET `modulos` = '" + modulos + "', `fsuspende` = '" + fecha + "'" +
                " WHERE (`nro_mac` = '"+mac+ "' and `empresa` = '" + empresa + "' );", connection).ExecuteNonQuery();
            connection.Close();
        }
    }
}
