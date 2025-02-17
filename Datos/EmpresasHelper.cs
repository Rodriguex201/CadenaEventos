using System.Collections.Generic;
using System.Data.Odbc;
using EventoMercantiles.Models;

namespace EventoMercantiles.Datos
{
    public static class EmpresasHelper
    {
        /// <summary>
        /// Obtiene la lista de empresas con valores de facturas mayores a 0.
        /// </summary>
        public static List<EmpresaInfo> ObtenerEmpresasConFacturas()
        {
            List<EmpresaInfo> empresas = new();
            using (var connection = Conexion.ObtenerConexion())
            {
                connection.Open();
                string query = @"
        SELECT 
            c.nit, c.nombre, c.codigo, v.mes, v.año
        FROM 
            organizador.valores_externos v
        INNER JOIN 
            organizador.clientes_potenciales c
        ON 
            v.id_cliente = c.idclientes_potenciales
        WHERE 
            v.valor_facturas > 0";

                using (var command = new OdbcCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        empresas.Add(new EmpresaInfo
                        {
                            Nit = reader.GetString(0)?.Trim(),
                            Nombre = reader.GetString(1)?.Trim(),
                            Codigo = reader.GetString(2)?.Trim(),
                            Mes = reader.GetString(3)?.Trim(),
                            Año = reader.GetInt32(4)
                        });
                    }
                }
            }
            return empresas;
        }


    }
}
