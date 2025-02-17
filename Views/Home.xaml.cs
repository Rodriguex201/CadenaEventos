using EventoMercantiles.Datos;
using EventoMercantiles.Models;
using EventoMercantiles.ViewsModel;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml;

namespace EventoMercantiles.Views
{
    /// <summary>
    /// Lógica de interacción para Home.xaml
    /// </summary>
    public partial class Home : Window
    {
        private string XMLBase64;
        private string PrefijoFactura;
        private string Codigo;
        private string Cufe;
        private string QRCode;
        private string Fecha;
        private string Emisor;
        private string identificacion;
        private string Dv = string.Empty;
        private string Idnit = string.Empty;
        private EventoModel Factura;
        private List<EventoModel> ListaFacturas = new();
        private readonly List<EventoModel> ListaFiltros = new();
        private readonly List<string> datosconexion;
        private readonly List<EmpresaModel> datosvalidos;
        private Storyboard myStoryboard;
        //CONTADORES
        private int CountExito;
        private int CountError;
        private int CountErrorDian;

        private int CountACUSE_DOCUMENTO;
        private int CountRECIBO_SERVICIO;
        private int CountACEPTACION_EXPRESA;
        private int CountRECLAMO;

        public Home()
        {
            InitializeComponent();
            // Llamar al método para cargar los años
            Informes.Visibility = Visibility.Hidden;
            CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.DateTimeFormat.ShortDatePattern = "dd-MM-yyyy";
            culture.DateTimeFormat.LongTimePattern = "";
            Thread.CurrentThread.CurrentCulture = culture;
            var mes = DateTime.Now.Month;
            var ano = DateTime.Now.Year;
            datede.SelectedDate = new DateTime(ano, mes, 1);
            datehasta.SelectedDate = DateTime.Now;
            try
            {
                datosconexion = CadenaConexion.ReadConexion();
                datosvalidos = DatosEquipo.Validar(datosconexion[2], datosconexion[1]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("validacion " + ex.Message);
            }
            try
            {
                ImageBrush myBrush = new();
                Image image = new();
                image.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/fondo.jpg"));
                myBrush.ImageSource = image.Source;
                contenedor.Background = myBrush;
                LogoEmpresa.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/logo.jpg"));
                if (datosvalidos != null && datosvalidos.Count > 0)
                {
                    DateTime expiration_date = Convert.ToDateTime(datosvalidos[0].Fsuspende);
                    DateTime currentDateTime = Convert.ToDateTime(DatosEquipo.GetFecha());
                    int daydiff = (int)((expiration_date - currentDateTime).TotalDays);
                    if (daydiff < 0)
                    {
                        MessageBox.Show("el servicio se encuentra inactivo");
                        if (datosconexion.Count > 0)
                        {
                            DialogoView dialogo = new(datosconexion);
                            dialogo.ShowDialog();
                            Close();
                        }
                        else
                        {
                            Close();
                        }
                    }
                    else
                    {
                        ListaFacturas = EventoViewModel.GetEventos(Convert.ToDateTime(datede.SelectedDate.ToString()), Convert.ToDateTime(datehasta.SelectedDate.ToString()));
                        LlenarListaFacturas(ListaFacturas);
                        Cbcodigorechazo.IsEnabled = false;
                        var datos = DatosEmpresaViewModel.GetDatosEmpresa();
                        if (datos.Count > 0)
                        {
                            Idnit = datos[0].Dt_nit.Split("-")[0];
                            Dv = datos[0].Dt_nit.Split("-")[0]; //Se cambio el 11/01/2023 antes de el 0 estaba un 1
                            lbnit.Content = "NIT " + datos[0].Dt_nit;
                            lbnombre.Content = datos[0].Dt_nombre;
                            lbnombre2.Content = datos[0].Dt_nombre2;
                        }
                    }
                }
                else
                {
                    DatosEmpresaView datos = new(false);
                    datos.Show();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("home " + ex.Message);
                DatosEmpresaView datos = new(false);
                datos.Show();
                Close();
            }
        }
        private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            if (openFileDialog.ShowDialog() == true)
            {
                Lista_eventos.SelectedItem = null;
                LbNombreArchivo.Content = openFileDialog.SafeFileName;
                byte[] arrayDeBytes = File.ReadAllBytes(openFileDialog.FileName);
                //XMLBase64 = Convert.ToBase64String(arrayDeBytes);
                Factura = null;
                BtnEnviar.IsEnabled = true;
                using MemoryStream stream = new(arrayDeBytes);
                XmlDocument doc = new();
                doc.Load(stream);
                Cbevento.SelectedIndex = 0;
                if (doc.DocumentElement.FirstChild.ParentNode.LocalName.Equals("AttachedDocument"))
                {
                    foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                    {
                        if (node.HasChildNodes)
                        {
                            for (int i = 0; i < node.ChildNodes.Count; i++)
                            {
                                if (node.ChildNodes[i].ParentNode.LocalName.Equals("SenderParty"))
                                {
                                    for (int j = 0; j < node.ChildNodes.Count; j++)
                                    {
                                        if (node.ChildNodes[j].LocalName.Equals("PartyTaxScheme"))
                                        {
                                            Emisor = node.ChildNodes[j].FirstChild.FirstChild.InnerText;
                                            identificacion = node.ChildNodes[j].FirstChild.NextSibling.InnerText;
                                            break;
                                        }
                                    }
                                    break;
                                }
                                else
                                {
                                    //se ajusta el digito de verificacion del nit 
                                    if (node.ChildNodes[i].ParentNode.LocalName.Equals("ReceiverParty"))
                                    {
                                        for (int j = 0; j < node.ChildNodes.Count; j++)
                                        {
                                            if (node.ChildNodes[j].LocalName.Equals("PartyTaxScheme"))
                                            {
                                                for (int b = 0; b < node.ChildNodes[j].ChildNodes.Count; b++)
                                                {
                                                    if (node.ChildNodes[j].ChildNodes[b].LocalName.Equals("CompanyID"))
                                                    {
                                                        if (!node.ChildNodes[j].ChildNodes[b].InnerText.Equals(Idnit))
                                                        {
                                                            BtnEnviar.IsEnabled = false;
                                                            XMLBase64 = null;
                                                            MessageBox.Show("el identificador de la factura no corresponde al identificador del cliente");
                                                            break;
                                                        }
                                                    }
                                                }
                                                for (int h = 0; h < node.ChildNodes[j].FirstChild.NextSibling.Attributes.Count; h++)
                                                {
                                                    if (node.ChildNodes[j].FirstChild.NextSibling.Attributes[h].LocalName.Equals("schemeID"))
                                                    {
                                                        node.ChildNodes[j].FirstChild.NextSibling.Attributes[h].InnerText = Dv;
                                                    }
                                                }
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        if (node.ChildNodes[i].ParentNode.LocalName.Equals("Attachment"))
                                        {
                                            for (int j = 0; j < node.ChildNodes.Count; j++)
                                            {
                                                if (node.ChildNodes[j].LocalName.Equals("ExternalReference"))
                                                {
                                                    for (int h = 0; h < node.ChildNodes[j].ChildNodes.Count; h++)
                                                    {
                                                        if (node.ChildNodes[j].ChildNodes[h].LocalName.Equals("Description"))
                                                        {
                                                            for (int l = 0; l < node.ChildNodes[j].ChildNodes[h].ChildNodes.Count; l++)
                                                            {
                                                                XmlDocument xmlDoc = new XmlDocument();
                                                                xmlDoc.LoadXml(node.ChildNodes[j].ChildNodes[h].InnerText);
                                                                foreach (XmlNode node2 in xmlDoc.DocumentElement.ChildNodes)
                                                                {
                                                                    if (node2.LocalName.Equals("AccountingCustomerParty"))
                                                                    {
                                                                        for (int k = 0; k < node2.ChildNodes.Count; k++)
                                                                        {
                                                                            if (node2.ChildNodes[k].LocalName.Equals("Party"))
                                                                            {
                                                                                for (int t = 0; t < node2.ChildNodes[k].ChildNodes.Count; t++)
                                                                                {
                                                                                    if (node2.ChildNodes[k].ChildNodes[t].LocalName.Equals("PartyIdentification"))
                                                                                    {
                                                                                        for (int z = 0; z < node2.ChildNodes[k].ChildNodes[t].ChildNodes.Count; z++)
                                                                                        {
                                                                                            for (int f = 0; f < node2.ChildNodes[k].ChildNodes[t].ChildNodes[z].Attributes.Count; f++)
                                                                                            {
                                                                                                if (node2.ChildNodes[k].ChildNodes[t].ChildNodes[z].Attributes[f].LocalName.Equals("schemeID"))
                                                                                                {
                                                                                                    node2.ChildNodes[k].ChildNodes[t].ChildNodes[z].Attributes[f].InnerText = Dv;
                                                                                                }

                                                                                            }
                                                                                        }
                                                                                    }
                                                                                    if (node2.ChildNodes[k].ChildNodes[t].LocalName.Equals("PartyTaxScheme"))
                                                                                    {
                                                                                        for (int z = 0; z < node2.ChildNodes[k].ChildNodes[t].ChildNodes.Count; z++)
                                                                                        {
                                                                                            if (node2.ChildNodes[k].ChildNodes[t].ChildNodes[z].LocalName.Equals("CompanyID"))
                                                                                            {
                                                                                                for (int f = 0; f < node2.ChildNodes[k].ChildNodes[t].ChildNodes[z].Attributes.Count; f++)
                                                                                                {
                                                                                                    if (node2.ChildNodes[k].ChildNodes[t].ChildNodes[z].Attributes[f].LocalName.Equals("schemeID"))
                                                                                                    {
                                                                                                        node2.ChildNodes[k].ChildNodes[t].ChildNodes[z].Attributes[f].InnerText = Dv;
                                                                                                    }

                                                                                                }
                                                                                            }

                                                                                        }
                                                                                    }

                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                node.ChildNodes[j].ChildNodes[h].InnerText = xmlDoc.InnerXml;
                                                            }
                                                        }
                                                    }
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                        else
                                        {
                                            if (node.ChildNodes[i].ParentNode.LocalName.Equals("ParentDocumentLineReference"))
                                            {
                                                for (int j = 0; j < node.ChildNodes.Count; j++)
                                                {
                                                    if (node.ChildNodes[j].LocalName.Equals("DocumentReference"))
                                                    {
                                                        for (int z = 0; z < node.ChildNodes.Count; z++)
                                                        {
                                                            for (int y = 0; y < node.ChildNodes[z].ChildNodes.Count; y++)
                                                            {
                                                                if (node.ChildNodes[z].ChildNodes[y].LocalName.Equals("UUID"))
                                                                {
                                                                    Cufe = node.ChildNodes[z].ChildNodes[y].InnerText;
                                                                    QRCode = "https://catalogo-vpfe.dian.gov.co/document/searchqr?documentkey=" + Cufe;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    }
                                                }
                                                break;
                                            }
                                            else
                                            {
                                                if (node.ChildNodes[i].ParentNode.LocalName.Equals("ParentDocumentID"))
                                                {
                                                    PrefijoFactura = node.ChildNodes[i].InnerText;
                                                    foreach (var item in ListaFacturas)
                                                    {
                                                        if (item.Even_docum.Equals(PrefijoFactura))
                                                        {
                                                            BtnEnviar.IsEnabled = false;
                                                            LbNombreArchivo.Content = "Cargar archivo xml";
                                                            XMLBase64 = null;
                                                            MessageBox.Show("La factura " + PrefijoFactura + " ya se encuentra en la base");
                                                            break;
                                                        }
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    LbNombreArchivo.Background = new SolidColorBrush(Colors.YellowGreen);
                    XMLBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(doc.InnerXml));
                }
                else
                {
                    MessageBox.Show("El XML no es un documento valido");
                }

            }
        }
        private void BtnEnvioPostEnventoMercantil_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(XMLBase64))
                {
                    if (Factura != null)
                    {
                        if (!(((ComboBoxItem)Cbevento.SelectedItem).Content).ToString().Equals(Factura.Even_evento) || Factura.Even_codigo.Equals("ERROR") || Factura.Even_codigo.Equals("ERRORDIAN"))
                        {
                            if (Factura.Even_evento.Equals("ACEPTACION_EXPRESA"))
                            {
                                MessageBox.Show("La factura ya tiene registrado el evento de ACEPTACION_EXPRESA no se pueden hacer mas eventos");
                            }
                            else
                            {
                                if (Factura.Even_evento.Equals("RECIBO_SERVICIO") && (((ComboBoxItem)Cbevento.SelectedItem).Content).ToString().Equals("ACUSE_DOCUMENTO"))
                                {
                                    MessageBox.Show("La factura ya tiene registrado el evento de RECIBO_SERVICIO no se puede hacer el evento ACUSE_DOCUMENTO ");
                                }
                                else
                                {
                                    if (Factura.Even_evento.Equals("RECLAMO"))
                                    {
                                        MessageBox.Show("La factura ya tiene registrado el evento de RECLAMO no se pueden hacer mas eventos");
                                    }
                                    else
                                    {
                                        if (Factura.Even_evento.Equals("ACUSE_DOCUMENTO") && (((ComboBoxItem)Cbevento.SelectedItem).Content).ToString().Equals("ACEPTACION_EXPRESA"))
                                        {
                                            MessageBox.Show("La factura se encuentra en ACUSE_DOCUMENTO no se puede realizar el evento");
                                        }
                                        else
                                        {
                                            if (Factura.Even_evento.Equals("ACUSE_DOCUMENTO") && (((ComboBoxItem)Cbevento.SelectedItem).Content).ToString().Equals("RECLAMO"))
                                            {
                                                MessageBox.Show("La factura se encuentra en ACUSE_DOCUMENTO no se puede realizar el evento");
                                            }
                                            else
                                            {
                                                ProcesoPeticion();
                                                XMLBase64 = null;
                                                LbNombreArchivo.Content = "Cargue el archivo .xml";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("La factura ya tiene registrado el evento de" + (((ComboBoxItem)Cbevento.SelectedItem).Content).ToString());
                        }
                    }
                    else
                    {
                        if ((((ComboBoxItem)Cbevento.SelectedItem).Content).ToString().Equals("ACUSE_DOCUMENTO"))
                        {

                            ProcesoPeticion();
                            XMLBase64 = null;
                            LbNombreArchivo.Content = "Cargue el archivo .xml";
                        }
                        else
                        {
                            MessageBox.Show("No se selecciono el evento ACUSE_DOCUMENTO");
                            Cbevento.SelectedIndex = 0;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No se han cargado facturas para ser enviadas!!!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex);
            }
        }
        private void ProcesoPeticion()
        {
            try
            {
                string comas = "\"";
                string parameters;
                if (((ComboBoxItem)Cbevento.SelectedItem).Content.ToString().Equals("RECLAMO"))
                {
                    parameters = "{\"portal\": \"INTEGRACION_API\"," +
                    "\"xmlattachdocumentb64\": " + comas + XMLBase64 + comas + "," +
                    "\"responsable\": \"ADQUIRIENTE\"," +
                    "\"estadoEvento\": " + comas + ((ComboBoxItem)Cbevento.SelectedItem).Content.ToString() + comas + ",\"codigorechazo\":" + comas + (((ComboBoxItem)Cbcodigorechazo.SelectedItem).Content).ToString() + comas + "}";
                }
                else
                {
                    parameters = "{\"portal\": \"INTEGRACION_API\"," +
                    "\"xmlattachdocumentb64\": " + comas + XMLBase64 + comas + "," +
                    "\"responsable\": \"ADQUIRIENTE\"," +
                    "\"estadoEvento\": " + comas + ((ComboBoxItem)Cbevento.SelectedItem).Content.ToString() + comas + "}";
                }
                var dato = JObject.Parse(EventoViewModel.PostApiDomina(parameters));
                //var dato = JObject.Parse("{\"prefijoyfactura\":\"sett1\",\"cufe\":\"sjdhkfsdkfhsdfhkjsfkshdfksdhfkjsfsdhfhfkshdfshdfsfkjjsd\", \"fecha\":\"2022-07-11 14:57:26\",\"estado\":{ \"codigo\":\"EXITOSO\", \"descripcion\":\"DOCUMENTO PROCESADO CORECTAMENTE\", \"QRCode\":\"httttttt\"} }");
                //var dato = JObject.Parse("{\"prefijoyfactura\":\"sett1\",\"estado\":{\"codigo\":\"ERROR\",\"descripcion\":\"Error en las validaciones\",\"errores\":[\"la factura sett1 ya tiene el vento\"]}}");
                List<string> datos = new();
                int cont = 0;
                Fecha = DateTime.Now.ToString();
                Codigo = string.Empty;
                foreach (var item in dato)
                { 
                    if (item.Key.Equals("prefijoyfactura"))
                    {
                        PrefijoFactura = string.Empty;
                        PrefijoFactura = (string)item.Value;
                    }
                    if (item.Key.Equals("cufe"))
                    {
                        Cufe = string.Empty;
                        Cufe = (string)item.Value;
                    }
                    if (item.Key.Equals("fecha"))
                    {
                        Fecha = (string)item.Value;
                    }
                    if (item.Key.Equals("estado"))
                    {
                        foreach (var item2 in item.Value)
                        {
                            if (item2.Path.Equals("estado.codigo"))
                            {
                                Codigo = (string)item2.First;
                            }
                            if (item2.Path.Equals("estado.QRCode"))
                            {
                                QRCode = string.Empty;
                                QRCode = (string)item2.First;
                            }
                            if (item2.Path.Equals("estado.errores"))
                            {
                                foreach (var item3 in item2.First)
                                {
                                    cont++;
                                    datos.Add(item3.ToString());
                                }
                            }
                            if (item2.Path.Equals("estado.notificaciones"))
                            {
                                cont++;
                                datos.Add(item2.First.ToString());
                            }
                        }
                    }
                }
                if (Factura != null)
                {
                    //facturas ya registradas
                    if (Codigo.Equals("ERROR") || Codigo.Equals("ERRORDIAN"))
                    {
                        EventoViewModel.PostUpdateEvento(Factura.Even_evento, Codigo, Factura.Even_fecha, datos, Factura, Factura.Even_qrcode, Factura.Even_cufe);
                    }
                    else
                    {
                        if (Codigo.Equals("EXITOSO"))
                        {
                            EventoViewModel.PostUpdateEvento(((ComboBoxItem)Cbevento.SelectedItem).Content.ToString(), Codigo, Factura.Even_fecha, datos, Factura, Factura.Even_qrcode, Factura.Even_cufe);
                        }
                    }
                    Factura = null;
                }
                else
                {
                    //facturas nuevas
                    if (Codigo.Equals("ERROR") || Codigo.Equals("ERRORDIAN"))
                    {
                        EventoViewModel.PostEvento(PrefijoFactura, Codigo, Fecha, xMLBase64: XMLBase64, "ACUSE_DOCUMENTO", datos, Emisor, identificacion, QRCode, Cufe);
                    }
                    else
                    {
                        if (Codigo.Equals("EXITOSO"))
                        {
                            EventoViewModel.PostEvento(PrefijoFactura, Codigo, Fecha, xMLBase64: XMLBase64, ((ComboBoxItem)Cbevento.SelectedItem).Content.ToString(), datos, Emisor, identificacion, QRCode, Cufe);
                        }
                    }
                }
                ListaFacturas = EventoViewModel.GetEventos(Convert.ToDateTime(datede.SelectedDate.ToString()), Convert.ToDateTime(datehasta.SelectedDate.ToString()));
                LlenarListaFacturas(ListaFacturas);
                MessageBox.Show(Codigo + " al enviar la factura");
            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex);
            }

        }
        private void Lista_eventos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Lista_eventos.SelectedItem != null)
            {
                Factura = Lista_eventos.SelectedItem as EventoModel;
                LbNombreArchivo.Background = new SolidColorBrush(Colors.YellowGreen);
                BtnEnviar.IsEnabled = true;
                switch (Factura.Even_evento)
                {
                    case "ACUSE_DOCUMENTO":
                        Cbevento.SelectedIndex = 0;
                        AddDatosFactura();
                        break;
                    case "RECIBO_SERVICIO":
                        Cbevento.SelectedIndex = 1;
                        AddDatosFactura();
                        break;
                    case "ACEPTACION_EXPRESA":
                        Cbevento.SelectedIndex = 2;
                        AddDatosFactura();
                        break;
                    case "RECLAMO":
                        Cbevento.SelectedIndex = 3;
                        AddDatosFactura();
                        break;
                    default:
                        break;
                }
            }
        }
        private void AddDatosFactura()
        {
            LbNombreArchivo.Content = Factura.Even_docum;
            XMLBase64 = Factura.Even_xmlb64;
        }
        private void Cbevento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBoxItem)Cbevento.SelectedItem).Content != null)
            {
                switch (((ComboBoxItem)Cbevento.SelectedItem).Content.ToString())
                {
                    case "ACUSE_DOCUMENTO":
                        Cbcodigorechazo.IsEnabled = false;
                        break;
                    case "RECIBO_SERVICIO":
                        Cbcodigorechazo.IsEnabled = false;
                        break;
                    case "ACEPTACION_EXPRESA":
                        Cbcodigorechazo.IsEnabled = false;
                        break;
                    case "RECLAMO":
                        Cbcodigorechazo.IsEnabled = true;
                        break;
                    default:
                        break;
                }
            }
        }
        private void BtnQRcode_Click(object sender, RoutedEventArgs e)
        {
            var enlace = (Lista_eventos.SelectedItem as EventoModel).Even_qrcode;
            Process myProcess = new();
            try
            {
                // true is the default, but it is important not to set it to false
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.FileName = enlace;
                myProcess.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //MessageBox.Show(Codigo + " al enviar la factura");
        }
        private void AbrirFormulario(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                DatosEmpresaView datosemp = new(true);
                datosemp.Show();

            }
            if (e.Key == Key.F1)
            {
                try
                {
                    if (Lista_eventos.SelectedItem is EventoModel factura)
                    {
                        GenerarJson.Jsontxt(factura);
                        MessageBox.Show("Json generado");
                        Process.Start("explorer.exe", Environment.CurrentDirectory + "\\Documentos");
                    }
                    else
                    {
                        MessageBox.Show("no se ha seleccionado ninguna factura");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
        }
        private void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            if (TxtBusqueda.Text.Length > 2)
            {
                ListaFacturas = EventoViewModel.PostBusqueda(TxtBusqueda.Text, Convert.ToDateTime(datede.SelectedDate.ToString()), Convert.ToDateTime(datehasta.SelectedDate.ToString()));
                LlenarListaFacturas(ListaFacturas);
            }
            else
            {
                ListaFacturas = EventoViewModel.GetEventos(Convert.ToDateTime(datede.SelectedDate.ToString()), Convert.ToDateTime(datehasta.SelectedDate.ToString()));
                LlenarListaFacturas(ListaFacturas);
            }
        }
        private void LlenarListaFacturas(List<EventoModel> listaFacturas)
        {
            CountExito = 0;
            CountError = 0;
            CountErrorDian = 0;
            CountACUSE_DOCUMENTO = 0;
            CountRECIBO_SERVICIO = 0;
            CountACEPTACION_EXPRESA = 0;
            CountRECLAMO = 0;
            if (listaFacturas.Count > 0)
            {
                foreach (var item in ListaFacturas)
                {
                    if (item.Even_codigo.Equals("EXITOSO"))
                    {
                        CountExito++;
                    }
                    if (item.Even_codigo.Equals("ERROR"))
                    {
                        CountError++;
                    }
                    if (item.Even_codigo.Equals("ERRORDIAN"))
                    {
                        CountErrorDian++;
                    }
                    if (item.Even_evento.Equals("ACUSE_DOCUMENTO"))
                    {
                        CountACUSE_DOCUMENTO++;
                    }
                    if (item.Even_evento.Equals("RECIBO_SERVICIO"))
                    {
                        CountRECIBO_SERVICIO++;
                    }
                    if (item.Even_evento.Equals("ACEPTACION_EXPRESA"))
                    {
                        CountACEPTACION_EXPRESA++;
                    }
                    if (item.Even_evento.Equals("RECLAMO"))
                    {
                        CountRECLAMO++;
                    }
                }
                filtrocodigo.ToolTip = "del: " + datede.SelectedDate.ToString() + " hasta: " + datehasta.SelectedDate.ToString() + "\nEXITOSO: " + CountExito + "\nERROR: " + CountError + "\nERRORDIAN: " + CountErrorDian;
                filtroevento.ToolTip = "del: " + datede.SelectedDate.ToString() + " hasta: " + datehasta.SelectedDate.ToString()
                    + "\nACUSE_DOCUMENTO: " + CountACUSE_DOCUMENTO + "\nRECIBO_SERVICIO: " + CountRECIBO_SERVICIO + "\nACEPTACION_EXPRESA: " + CountACEPTACION_EXPRESA + "\nRECLAMO: " + CountRECLAMO;
                Lista_eventos.ItemsSource = null;
                Lista_eventos.ItemsSource = listaFacturas;
            }
        }
        private void Filtroevento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBoxItem)filtroevento.SelectedItem).Content != null)
            {
                if (!((ComboBoxItem)filtroevento.SelectedItem).Content.ToString().Equals("Filtrar Eventos"))
                {
                    ListaFiltros.Clear();
                    foreach (var item in ListaFacturas)
                    {
                        if (item.Even_evento.Equals(((ComboBoxItem)filtroevento.SelectedItem).Content.ToString()))
                        {
                            ListaFiltros.Add(item);
                        }
                    }
                    LlenarListaFacturas(ListaFiltros);
                }
                else
                {
                    LlenarListaFacturas(ListaFacturas);
                }
            }
        }
        private void Filtrocodigo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBoxItem)filtrocodigo.SelectedItem).Content != null)
            {
                if (!((ComboBoxItem)filtrocodigo.SelectedItem).Content.ToString().Equals("Filtrar Codigo"))
                {
                    ListaFiltros.Clear();
                    foreach (var item in ListaFacturas)
                    {
                        if (item.Even_codigo.Equals(((ComboBoxItem)filtrocodigo.SelectedItem).Content.ToString()))
                        {
                            ListaFiltros.Add(item);
                        }
                    }
                    LlenarListaFacturas(ListaFiltros);
                }
                else
                {
                    LlenarListaFacturas(ListaFacturas);
                }
            }
        }
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ListaFacturas = EventoViewModel.GetEventos(Convert.ToDateTime(datede.SelectedDate.ToString()), Convert.ToDateTime(datehasta.SelectedDate.ToString()));
            LlenarListaFacturas(ListaFacturas);
        }
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        //Funcion para abrir el menu informativo
        private void TBShow(object sender, RoutedEventArgs e)
        {
            static string labelPoint(ChartPoint chartPoint) => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);
            SeriesCollection piechartData = new();
            List<EventoModel> Informes = new();
            int cont = 0;
            bool factura = true;
            foreach (var item in ListaFacturas)
            {
                foreach (var fact in Informes)
                {
                    if (fact.Even_receptor.Equals(item.Even_receptor))
                    {
                        factura = false;
                    }
                }
                if (factura)
                {
                    foreach (var item2 in ListaFacturas)
                    {
                        if (item2.Even_receptor.Equals(item.Even_receptor))
                        {
                            cont++;
                            //if (item.Even_codigo.Equals("EXITOSO"))
                            //{
                            //    CountExito++;
                            //}
                            //if (item.Even_codigo.Equals("ERROR"))
                            //{
                            //    CountError++;
                            //}
                            //if (item.Even_codigo.Equals("ERRORDIAN"))
                            //{
                            //    CountErrorDian++;
                            //}
                            //if (item.Even_evento.Equals("ACUSE_DOCUMENTO"))
                            //{
                            //    CountACUSE_DOCUMENTO++;
                            //}
                            //if (item.Even_evento.Equals("RECIBO_SERVICIO"))
                            //{
                            //    CountRECIBO_SERVICIO++;
                            //}
                            //if (item.Even_evento.Equals("ACEPTACION_EXPRESA"))
                            //{
                            //    CountACEPTACION_EXPRESA++;
                            //}
                            //if (item.Even_evento.Equals("RECLAMO"))
                            //{
                            //    CountRECLAMO++;
                            //}
                        }
                    }
                    item.Contador = cont;
                    Informes.Add(item);
                    piechartData.Add(new PieSeries
                    {
                        Title = item.Even_receptor,
                        Values = new ChartValues<double> { item.Contador },
                        DataLabels = true,
                        LabelPoint = labelPoint,
                    });
                }
                cont = 0;
                factura = true;
            }
            torta.Series = piechartData;
            //torta.LegendLocation = LegendLocation.Right;
            ListaInformes.ItemsSource = Informes;
            Abrirpanel();
        }
        //Funcion para ocultar el menu informativo
        private void TBHide(object sender, RoutedEventArgs e)
        {

            Cerrarpanel();
        }
        private void Abrirpanel()
        {
            Informes.Visibility = Visibility.Visible;
            Informes.Height = ActualHeight - 170;

            var myDoubleAnimation = new DoubleAnimation
            {
                From = 0,
                To = ActualWidth,
                Duration = new Duration(TimeSpan.FromSeconds(0.3))
            };
            //myDoubleAnimation.AutoReverse = true;
            //myDoubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            myStoryboard = new Storyboard();
            myStoryboard.Children.Add(myDoubleAnimation);
            Storyboard.SetTargetName(myDoubleAnimation, Informes.Name);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(Grid.WidthProperty));
            myStoryboard.Begin(this);
        }
        private void Cerrarpanel()
        {
            var myDoubleAnimation = new DoubleAnimation
            {
                From = ActualWidth,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.3))
            };
            //myDoubleAnimation.AutoReverse = true;
            //myDoubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            myStoryboard = new Storyboard();
            myStoryboard.Children.Add(myDoubleAnimation);
            Storyboard.SetTargetName(myDoubleAnimation, Informes.Name);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(Grid.WidthProperty));
            myStoryboard.Begin(this);
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var evento = Lista_eventos.SelectedItem as EventoModel;
                if (evento != null)
                {
                    MessageBoxResult result = MessageBox.Show("Desea eliminar la factura " + evento.Even_docum, "Información", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        EventoViewModel.PostDeleteEvento(evento.Id_eventos);
                        ListaFacturas = EventoViewModel.GetEventos(Convert.ToDateTime(datede.SelectedDate.ToString()), Convert.ToDateTime(datehasta.SelectedDate.ToString()));
                        LlenarListaFacturas(ListaFacturas);
                    }
                }
                else
                {
                    MessageBox.Show("No se ha seleccionado ninguna factura");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var datosConexion = CadenaConexion.ReadConexion(); // Método ya existente
            string ipActual = datosConexion[0]; // IP
            string codigoActual = datosConexion[1]; // Código

            if (ipActual == "192.168.1.150" && codigoActual == "a000")
            {
                BtnMostrarEmpresas.Visibility = Visibility.Visible;
            }
            else
            {
                BtnMostrarEmpresas.Visibility = Visibility.Hidden;
            }
        }


        private void BtnMostrarEmpresas_Click(object sender, RoutedEventArgs e)
        {
            // Reutilizar el método del helper para obtener la lista de empresas
            var empresas = EmpresasHelper.ObtenerEmpresasConFacturas();

            if (empresas.Count > 0)
            {
                // Abrir la ventana con el listado de empresas
                EmpresasView empresasView = new(empresas);
                empresasView.ShowDialog();
            }
            else
            {
                MessageBox.Show("No se encontraron empresas con valores de facturas.");
            }
        }




    }
}