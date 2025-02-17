using EventoMercantiles.Datos;
using EventoMercantiles.Models;

using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.IO;
using System.Net;
using System.Text;

namespace EventoMercantiles.ViewsModel
{
    public class EventoViewModel
    {
        public static void PostEvento(string prefijoFactura, string codigo,  string fecha, string xMLBase64, string evento, List<string> datos, string emisor, string identificacion, string qRCode, string cufe)
        {
           var connection = Conexion.ObtenerConexion();
           connection.Open();
             new OdbcCommand("INSERT INTO eventos (`even_docum`, `even_receptor`, `even_identif`, `even_fecha`, `even_evento`, `even_xmlb64`,`even_codigo`,`even_response`,`even_qrcode`,`even_cufe`) " +
             "VALUES ('"+prefijoFactura+"', '"+emisor+"', '"+identificacion+"', '"+fecha+"', '"+evento+"', '"+xMLBase64+"','"+codigo+"','"+datos[0]+"','"+qRCode+"','"+cufe+"');",
                       connection).ExecuteNonQuery();
            connection.Close();           
        }
        public static List<EventoModel> GetEventos(DateTime datede, DateTime datehasta)
        {
            var connection = Conexion.ObtenerConexion();
            List<EventoModel> eventos = new();
            connection.Open();
            var _ = new OdbcCommand("SELECT even_evento,id_eventos,even_docum,even_receptor,even_identif,even_fecha,even_evento,even_xmlb64,even_codigo,even_response,even_qrcode,even_cufe FROM eventos where date(even_fecha) >= '" + datede+"' and  date(even_fecha) <= '"+datehasta+ "' or  even_fecha = '' " +
                "ORDER BY id_eventos desc;", connection).ExecuteReader();
            if (_.HasRows)
            {
                while (_.Read())
                {
                    string COLOR = string.Empty;
                    switch (_.GetString(0))
                    {
                        case "ACUSE_DOCUMENTO":
                            COLOR = "#779ECB";
                            break;
                        case "RECIBO_SERVICIO":
                            COLOR = "#bdecb6";
                            break;
                        case "ACEPTACION_EXPRESA":
                            COLOR = "yellow";
                            break;
                        case "RECLAMO":
                            COLOR = "Red";
                            break;
                        default:
                            break;
                    }
                    eventos.Add(new EventoModel
                    {
                        Id_eventos = _.GetInt32(1),
                        Even_docum = _.GetString(2),
                        Even_receptor = _.GetString(3),
                        Even_identif = _.GetString(4),
                        Even_fecha = _.GetString(5),
                        Even_evento = _.GetString(6),
                        Even_xmlb64 = _.GetString(7),
                        Even_codigo = _.GetString(8),
                        Even_response = _.GetString(9),
                        Even_qrcode = _.GetString(10),
                        Even_cufe = _.GetString(11),
                        Color = COLOR
                    }) ;
                }                
            }
            connection.Close();
            return eventos;        
        }
        public static List<EventoModel> GetEvento( string evento, DateTime datede, DateTime datehasta)
        {
            var connection = Conexion.ObtenerConexion();
            List<EventoModel> eventos = new();
            connection.Open();
            var _ = new OdbcCommand("SELECT even_evento,id_eventos,even_docum,even_receptor,even_identif,even_fecha,even_evento,even_xmlb64,even_codigo,even_response,even_qrcode FROM eventos where  date(even_fecha) >= '" + datede + "' and  date(even_fecha) <= '" + datehasta + "' and  even_evento='" + evento + "' ORDER BY id_eventos desc;", connection).ExecuteReader();
            if (_.HasRows)
            {
                while (_.Read())
                {
                    string COLOR = string.Empty;
                    switch (_.GetString(0))
                    {
                        case "ACUSE_DOCUMENTO":
                            COLOR = "#779ECB";
                            break;
                        case "RECIBO_SERVICIO":
                            COLOR = "#bdecb6";
                            break;
                        case "ACEPTACION_EXPRESA":
                            COLOR = "yellow";
                            break;
                        case "RECLAMO":
                            COLOR = "Red";
                            break;
                        default:
                            break;
                    }
                    eventos.Add(new EventoModel
                    {
                        Id_eventos = _.GetInt32(1),
                        Even_docum = _.GetString(2),
                        Even_receptor = _.GetString(3),
                        Even_identif = _.GetString(4),
                        Even_fecha = _.GetString(5),
                        Even_evento = _.GetString(6),
                        Even_xmlb64 = _.GetString(7),
                        Even_codigo = _.GetString(8),
                        Even_response = _.GetString(9),
                        Even_qrcode = _.GetString(10),
                        Color = COLOR
                    }) ;
                }                
            }
            connection.Close();
            return eventos;        
        }
        public static List<EventoModel> GetEventoCodigo( string codigo, DateTime datede, DateTime datehasta)
        {
            var connection = Conexion.ObtenerConexion();
            List<EventoModel> eventos = new();
            connection.Open();
            var _ = new OdbcCommand("SELECT even_evento,id_eventos,even_docum,even_receptor,even_identif,even_fecha,even_evento,even_xmlb64,even_codigo,even_response,even_qrcode FROM eventos where  date(even_fecha) >= '" + datede + "' and  date(even_fecha) <= '" + datehasta + "' and  even_codigo='" + codigo + "' ORDER BY id_eventos desc;", connection).ExecuteReader();
            if (_.HasRows)
            {
                while (_.Read())
                {
                    string COLOR = string.Empty;
                    switch (_.GetString(0))
                    {
                        case "ACUSE_DOCUMENTO":
                            COLOR = "#779ECB";
                            break;
                        case "RECIBO_SERVICIO":
                            COLOR = "#bdecb6";
                            break;
                        case "ACEPTACION_EXPRESA":
                            COLOR = "yellow";
                            break;
                        case "RECLAMO":
                            COLOR = "Red";
                            break;
                        default:
                            break;
                    }
                    eventos.Add(new EventoModel
                    {
                        Id_eventos = _.GetInt32(1),
                        Even_docum = _.GetString(2),
                        Even_receptor = _.GetString(3),
                        Even_identif = _.GetString(4),
                        Even_fecha = _.GetString(5),
                        Even_evento = _.GetString(6),
                        Even_xmlb64 = _.GetString(7),
                        Even_codigo = _.GetString(8),
                        Even_response = _.GetString(9),
                        Even_qrcode = _.GetString(10),
                        Color = COLOR
                    }) ;
                }                
            }
            connection.Close();
            return eventos;        
        }
        public static void PostUpdateEvento(string evento, string codigo, string fecha, List<string> datos, EventoModel factura, string qRCode, string cufe)
        {
            var connection = Conexion.ObtenerConexion();
            connection.Open();
            var _ = new OdbcCommand("UPDATE eventos" +
                " SET `even_evento` = '"+evento+"', `even_codigo` = '"+codigo+"', `even_response` = '"+datos[0]+ "',`even_qrcode` ='" + qRCode+"',`even_cufe` ='" + cufe+ "',`even_fecha` ='" +fecha+"'" +
                "WHERE (`id_eventos` = '" + factura.Id_eventos+"');", connection).ExecuteNonQuery();
            connection.Close();
        }
        public static List<EventoModel> PostBusquedaEvento( string filtro, string evento)
        {
            var connection = Conexion.ObtenerConexion();
            List<EventoModel> eventos = new();
            connection.Open();
            var _ = new OdbcCommand("SELECT even_evento,id_eventos,even_docum,even_receptor,even_identif,even_fecha,even_evento,even_xmlb64,even_codigo,even_response,even_qrcode " +
                " FROM eventos WHERE (even_docum like '%" + filtro + "%' or even_receptor like '%" + filtro + "%' or even_identif like '%" + filtro + "%' " +
                    "or even_evento like '%" + filtro + "%' or even_codigo like '%" + filtro + "%') and even_evento='"+evento+"' ;", connection).ExecuteReader();
            if (_.HasRows)
            {
                while (_.Read())
                {
                    string COLOR = string.Empty;
                    switch (_.GetString(0))
                    {
                        case "ACUSE_DOCUMENTO":
                            COLOR = "#779ECB";
                            break;
                        case "RECIBO_SERVICIO":
                            COLOR = "#bdecb6";
                            break;
                        case "ACEPTACION_EXPRESA":
                            COLOR = "yellow";
                            break;
                        case "RECLAMO":
                            COLOR = "Red";
                            break;
                        default:
                            break;
                    }
                    eventos.Add(new EventoModel
                    {
                        Id_eventos = _.GetInt32(1),
                        Even_docum = _.GetString(2),
                        Even_receptor = _.GetString(3),
                        Even_identif = _.GetString(4),
                        Even_fecha = _.GetString(5),
                        Even_evento = _.GetString(6),
                        Even_xmlb64 = _.GetString(7),
                        Even_codigo = _.GetString(8),
                        Even_response = _.GetString(9),
                        Even_qrcode = _.GetString(10),
                        Color = COLOR
                    });
                }
            }
            connection.Close();
            return eventos;
        }
        public static List<EventoModel> PostBusqueda(string filtro, DateTime datede, DateTime datehasta)
        {
            var connection = Conexion.ObtenerConexion();
            List<EventoModel> eventos = new();
            connection.Open();
            var _ = new OdbcCommand("SELECT even_evento,id_eventos,even_docum,even_receptor,even_identif,even_fecha,even_evento,even_xmlb64,even_codigo,even_response,even_qrcode " +
                "FROM eventos WHERE  date(even_fecha) >= '" + datede + "' and  date(even_fecha) <= '" + datehasta + "' and (even_docum like '%" + filtro + "%' or even_receptor like '%" + filtro + "%' or even_identif like '%" + filtro + "%' " +
                    "or even_evento like '%" + filtro + "%' or even_codigo like '%" + filtro + "%');", connection).ExecuteReader();
            if (_.HasRows)
            {
                while (_.Read())
                {
                    string COLOR = string.Empty;
                    switch (_.GetString(0))
                    {
                        case "ACUSE_DOCUMENTO":
                            COLOR = "#779ECB";
                            break;
                        case "RECIBO_SERVICIO":
                            COLOR = "#bdecb6";
                            break;
                        case "ACEPTACION_EXPRESA":
                            COLOR = "yellow";
                            break;
                        case "RECLAMO":
                            COLOR = "Red";
                            break;
                        default:
                            break;
                    }
                    eventos.Add(new EventoModel
                    {
                        Id_eventos = _.GetInt32(1),
                        Even_docum = _.GetString(2),
                        Even_receptor = _.GetString(3),
                        Even_identif = _.GetString(4),
                        Even_fecha = _.GetString(5),
                        Even_evento = _.GetString(6),
                        Even_xmlb64 = _.GetString(7),
                        Even_codigo = _.GetString(8),
                        Even_response = _.GetString(9),
                        Even_qrcode = _.GetString(10),
                        Color = COLOR
                    });
                }
            }
            connection.Close();
            return eventos;
        }
        public static string PostApiDomina(string parameters)
        {            
            var datosempresa = DatosEmpresaViewModel.GetDatosEmpresa();
            HttpWebRequest httpRequest =
                HttpWebRequest.Create(datosempresa[0].Dt_url + datosempresa[0].Dt_nit) as HttpWebRequest;
            httpRequest.Method = "POST";
            httpRequest.ProtocolVersion = HttpVersion.Version11;
            httpRequest.ContentLength = Encoding.ASCII.GetByteCount(parameters);
            httpRequest.ContentType = "application/json";
            httpRequest.Headers.Add("Authorization", datosempresa[0].Dt_token);
            httpRequest.Headers.Add("Content-Type", "application/json");
            using (Stream requestStream = httpRequest.GetRequestStream())
            {
                byte[] parametersBuffer = Encoding.ASCII.GetBytes(parameters);
                requestStream.Write(parametersBuffer, 0, parametersBuffer.Length);
            }
            using var streamReader = new StreamReader(httpRequest.GetResponse().GetResponseStream());
            return streamReader.ReadToEnd();
        }       
        public static void PostDeleteEvento(int idevento)
        {
            var connection = Conexion.ObtenerConexion();
            connection.Open();
            var _ = new OdbcCommand("DELETE FROM `eventos` WHERE (`id_eventos` = '"+idevento+"');", connection).ExecuteNonQuery();
            connection.Close();
        }
    }
}
