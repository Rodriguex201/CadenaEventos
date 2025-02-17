using System.Collections.Generic;
using System.Windows;
using EventoMercantiles.Datos;

namespace EventoMercantiles.Views
{
    public partial class EquiposView : Window
    {
        private List<EquipoInfo> equiposOriginales;

        public EquiposView(List<EquipoInfo> equipos)
        {
            InitializeComponent();
            equiposOriginales = equipos;
            EquiposDataGrid.ItemsSource = equiposOriginales;
        }

        private void BtnEditarFechas_Click(object sender, RoutedEventArgs e)
        {
            // Obtener el equipo seleccionado
            if ((sender as FrameworkElement)?.Tag is EquipoInfo equipoSeleccionado)
            {
                EditarFechasView editarFechas = new EditarFechasView(equipoSeleccionado);
                editarFechas.ShowDialog();

                // Recargar los datos después de la actualización
                EquiposDataGrid.ItemsSource = null;
                EquiposDataGrid.ItemsSource = EquiposHelper.ObtenerEquipos();
            }
            else
            {
                MessageBox.Show("Seleccione un equipo para editar sus fechas.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchTextBox.Text.Trim();

            if (string.IsNullOrEmpty(searchTerm) || searchTerm == "Buscar por Empresa o MAC")
            {
                EquiposDataGrid.ItemsSource = equiposOriginales; // Restaurar lista original
                return;
            }

            // Filtrar equipos por Empresa o MAC
            List<EquipoInfo> equiposFiltrados = equiposOriginales.FindAll(equipo =>
                (!string.IsNullOrEmpty(equipo.Empresa) && equipo.Empresa.Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(equipo.NroMac) && equipo.NroMac.Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase))
            );

            // Mostrar resultados
            EquiposDataGrid.ItemsSource = equiposFiltrados;
        }



        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Buscar...")
            {
                SearchTextBox.Text = string.Empty;
                SearchTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Buscar...";
                SearchTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }



    }
}
