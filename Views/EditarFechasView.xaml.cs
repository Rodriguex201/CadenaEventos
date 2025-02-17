using System;
using System.Windows;
using EventoMercantiles.Datos;

namespace EventoMercantiles.Views
{
    public partial class EditarFechasView : Window
    {
        private EquipoInfo equipo;

        public EditarFechasView(EquipoInfo equipoSeleccionado)
        {
            InitializeComponent();
            equipo = equipoSeleccionado;

            // Prellenar valores actuales
            DateActivacion.SelectedDate = DateTime.TryParse(equipo.FechaActivacion, out DateTime fechaActivacion) ? fechaActivacion : (DateTime?)null;
            DateSuspension.SelectedDate = DateTime.TryParse(equipo.FechaSuspension, out DateTime fechaSuspension) ? fechaSuspension : (DateTime?)null;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string nuevaFechaActivacion = DateActivacion.SelectedDate.HasValue ? DateActivacion.SelectedDate.Value.ToString("yyyy-MM-dd") : "Fecha no registrada";
            string nuevaFechaSuspension = DateSuspension.SelectedDate.HasValue ? DateSuspension.SelectedDate.Value.ToString("yyyy-MM-dd") : null;

            // Actualizar en la base de datos
            EquiposHelper.ActualizarFechasEquipo(equipo.Empresa, equipo.NroMac, nuevaFechaActivacion, nuevaFechaSuspension);

            MessageBox.Show("Fechas actualizadas correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }
    }
}
