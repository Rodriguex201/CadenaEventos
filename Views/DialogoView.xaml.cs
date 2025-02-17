using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EventoMercantiles.Datos;

namespace EventoMercantiles.Views
{
    /// <summary>
    /// Lógica de interacción para DialogoView.xaml
    /// </summary>
    public partial class DialogoView : Window
    {
        private List<string> datos;
        public DialogoView(List<string> datosconexion)
        {
            InitializeComponent();
            datos = datosconexion;
        }

        private void Btnclave_Click(object sender, RoutedEventArgs e)
        {
            string claveIngresada = txtclave.Password.Trim();

            // Verificar si es la clave general
            if (claveIngresada.Equals("*M0D4L0S*"))
            {
                Close();
                ActivacionView activacion = new(datos);
                activacion.ShowDialog();
            }
            else
            {
                // Verificar si es una clave específica de empresa
                var empresa = DatosDB.ObtenerEmpresaPorClave(claveIngresada);

                if (empresa != null)
                {
                    Close();
                    MessageBox.Show($"Clave válida para la empresa: {empresa.Nombre}", "Activación");

                    // Abrir ActivacionView con controles deshabilitados
                    ActivacionView activacion = new(datos, true);
                    activacion.ShowDialog();
                }
                else
                {
                    MessageBox.Show("La clave no coincide con ninguna empresa.", "Error");
                }
            }
        }


    }
}
