using EventoMercantiles.Models;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Globalization;
using System.Management;
using System.Threading;

namespace EventoMercantiles.Datos
{

    public class DatosEquipo
    {
        public static string GetSerialNumber()
        {

            ManagementObjectSearcher searcher = null;
            searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            string serial = string.Empty;
            foreach (ManagementObject wmi_HD in searcher.Get())
            {
                HardDriveModel hd = new HardDriveModel();
                try
                {
                    var t = hd.Caption = wmi_HD["Caption"].ToString();
                    serial = (hd.SerialNo = wmi_HD.GetPropertyValue("SerialNumber").ToString());
                }
                catch (Exception)
                {

                    throw;
                }
            }
            return serial;
        }

        public static List<EmpresaModel> Validar(string mac, string codigo)
        {
            CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            culture.DateTimeFormat.LongTimePattern = "";
            Thread.CurrentThread.CurrentCulture = culture;
            //Environment.MachineName
            var connection = Conexion.ObtenerConexion();
            connection.Open();
            //var connection = Conexion.ObtenerConexion();
            List<EmpresaModel> datos = new List<EmpresaModel>();
            //connection.Open();
            var reader = new OdbcCommand("SELECT empresa,nro_mac,activar,modulos,fsuspende FROM empresas.llequipo where empresa='" + codigo+"' and nro_mac='"+mac+"' and modulos like '%M14%';", connection).ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    datos.Add(new EmpresaModel
                    {
                        Empresa = reader.GetString(0),
                        Nro_mac = reader.GetString(1),
                        Activar = reader.GetString(2),
                        Modulos = reader.GetString(3),
                        Fsuspende= reader.GetDateTime(4)
                    });
                }
            }
            reader.Close();
            connection.Close();
            return datos;
        }

        public static string GetFecha()
        {
            using (var connection = Conexion.ObtenerConexion())
            {
                string horaactual = null;
                connection.Open();
                var fecha = new OdbcCommand("SELECT NOW() as fecha;", connection);
                var read = fecha.ExecuteReader();
                while (read.Read())
                {
                    horaactual = read[0].ToString();
                }
                connection.Close();
                return horaactual;
            }
        }
    }
}
