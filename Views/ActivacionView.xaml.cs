using EventoMercantiles.Datos;
using EventoMercantiles.ViewsModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EventoMercantiles.Views
{
    /// <summary>
    /// Lógica de interacción para ActivacionView.xaml
    /// </summary>
    public partial class ActivacionView : Window
    {
        private string modulos;
        private List<string> datos;
        private bool claveEmpresa; // Nueva bandera para identificar si la clave es de empresa

        public ActivacionView(List<string> datosconexion, bool esClaveEmpresa = false)
        {
            InitializeComponent();
            datos = datosconexion;
            claveEmpresa = esClaveEmpresa; // Asignar el valor de la bandera

            CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            culture.DateTimeFormat.LongTimePattern = "";
            Thread.CurrentThread.CurrentCulture = culture;

            var mes = DateTime.Now.Month + 1;
            var ano = DateTime.Now.Year;
            var dia = DateTime.Now.Day;

            // Si el mes es mayor a 12, avanzar al siguiente año
            if (mes > 12)
            {
                mes = 1;
                ano += 1;
            }

            // Ajustar el día si es mayor al número de días del mes calculado
            int maxDiasEnMes = DateTime.DaysInMonth(ano, mes);
            if (dia > maxDiasEnMes)
            {
                dia = maxDiasEnMes; // Ajustar al último día del mes válido
            }

            // Asignar la fecha corregida
            dateactivacion.SelectedDate = new DateTime(ano, mes, dia);

            Txtempresa.Text = datosconexion[1];
            Txtmac.Text = datosconexion[2];

            foreach (var item in DatosEmpresaViewModel.GetDatosModulos(datosconexion[1], datosconexion[2]))
            {
                // Transformar automáticamente "M" a "m" antes de "14"
                string modulosTransformados = item.Modulos.Replace("M14", "m14");

                Txtmodulos.Text = modulosTransformados;
                modulos = modulosTransformados; // Guardar el valor transformado en la variable `modulos`
            }

            // Deshabilitar controles si es clave de empresa
            if (claveEmpresa)
            {
                Txtmodulos.IsEnabled = false;
                dateactivacion.IsEnabled = false;
            }
        }


        private void BtnActivar_Click(object sender, RoutedEventArgs e)
        {
            if (claveEmpresa)
            {
                // En caso de clave específica, no modificar módulos ni fecha
                MessageBox.Show("Activación completada para la empresa con la clave específica.");
                Close();
            }
            else
            {
                // Flujo normal de activación
                if (!Txtmodulos.Text.Equals(modulos))
                {
                    DatosEmpresaViewModel.PostUpdateDatosModulos(Txtmodulos.Text.ToUpper(), dateactivacion.SelectedDate.ToString(), datos[2], datos[1]);
                    Close();
                }
                else
                {
                    MessageBox.Show("No se han modificado los módulos");
                }
            }
        }

        private void Txtmodulos_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
