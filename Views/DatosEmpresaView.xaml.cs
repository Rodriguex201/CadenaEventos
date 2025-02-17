using EventoMercantiles.Datos;
using EventoMercantiles.Models;
using EventoMercantiles.ViewsModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace EventoMercantiles.Views
{
    /// <summary>
    /// Lógica de interacción para DatosEmpresaView.xaml
    /// </summary>
    public partial class DatosEmpresaView : Window
    {
        private readonly List<DatosEmrpesaModel> datos;
        private readonly List<string> datosconexion;

        public DatosEmpresaView(bool boolean)
        {
            InitializeComponent();
            lbserial.Content = "M14 29 Septiembre 2022";
            try
            {
                datosconexion = CadenaConexion.ReadConexion();
                if (datosconexion[0] == null || datosconexion[1] == null || datosconexion[1] == null)
                {
                    actualizardb.IsEnabled = false;
                }
                if (boolean)
                {
                    datos = DatosEmpresaViewModel.GetDatosEmpresa();
                    if (datos.Count > 0)
                    {
                        txtToken.Text = datos[0].Dt_token;
                        txturl.Text = datos[0].Dt_url;
                        txtnit.Text = datos[0].Dt_nit;
                        txtnombre.Text = datos[0].Dt_nombre;
                        txtnombre2.Text = datos[0].Dt_nombre2;
                    }
                }
                else
                {
                    txtToken.IsEnabled = false;
                    txtnit.IsEnabled = false;
                    txtnombre.IsEnabled = false;
                    txtnombre2.IsEnabled = false;
                }
                if (datosconexion.Count > 0)
                {
                    txtip.Text = datosconexion[0];
                    txtcode.Text = datosconexion[1];
                    txtmac.Text = datosconexion[2];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (datos != null && datos.Count == 0 && !string.IsNullOrEmpty(txtnombre2.Text) && !string.IsNullOrEmpty(txtnombre.Text)
                    && !string.IsNullOrEmpty(txtToken.Text) && !string.IsNullOrEmpty(txtnit.Text))
                {
                    DatosEmpresaViewModel.PostDatosEmpresa(txtToken.Text, "https://alfaprod.dominadigital.com.co/api/ReceptorEventoJson/", txtnit.Text, txtnombre.Text.ToUpper(), txtnombre2.Text.ToUpper());
                    MessageBox.Show("Guardado");
                    Close();
                    Close();
                }
                else
                {
                    if (datos != null && datos.Count > 0)
                    {
                        DatosEmpresaViewModel.PostUpdateDatosEmpresa(txtToken.Text, "https://alfaprod.dominadigital.com.co/api/ReceptorEventoJson/", txtnit.Text, txtnombre.Text.ToUpper(), txtnombre2.Text.ToUpper());
                        MessageBox.Show("Guardado");
                        Close();
                        Close();
                    }
                }
                if (!string.IsNullOrEmpty(txtip.Text) && !string.IsNullOrEmpty(txtcode.Text) && !string.IsNullOrEmpty(txtmac.Text))
                {
                    CadenaConexion.WriteConexion(txtip.Text, txtcode.Text, txtmac.Text);
                    Close();
                }
                else
                {
                    MessageBox.Show("faltan campos por llenar");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }





        private void Actualizardb_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DatosDB.CreateTablas();
                MessageBox.Show("creadas");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ViewActivacion(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
            {
                if (datosconexion.Count > 0)
                {
                    DialogoView dialogo = new(datosconexion);
                    dialogo.ShowDialog();
                }
            }
        }




    }
}
