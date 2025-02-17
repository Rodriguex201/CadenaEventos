using EventoMercantiles.Models;
using System;

namespace EventoMercantiles.Datos
{
    public class GenerarJson
    {
        public static void Jsontxt(EventoModel factura)
        {
            string comas = "\"";
            string text = "{\"portal\":\"INTEGRACION_API\",\"xmlattachdocumentb64\":"+comas+factura.Even_xmlb64+comas+",\"responsable\": \"ADQUIRIENTE\",\"estadoEvento\":\"ACUSE_DOCUMENTO\"}";
            System.IO.File.WriteAllText(Environment.CurrentDirectory + "/Documentos/" + factura.Even_docum+".txt", text);
        }
    }
}
