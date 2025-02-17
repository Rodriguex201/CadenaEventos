using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EventoMercantiles.Datos;
using EventoMercantiles.Models;

namespace EventoMercantiles.Views
{
    public partial class EmpresasView : Window
    {
        private List<EmpresaInfo> empresasOriginales; // Lista para guardar los datos originales

        public EmpresasView(List<EmpresaInfo> empresas)
        {
            InitializeComponent();

            // Guardar los datos originales
            empresasOriginales = empresas;

            // Mostrar las empresas iniciales en el DataGrid
            EmpresasDataGrid.ItemsSource = empresasOriginales;

            // Cargar años dinámicamente en el ComboBox
            CargarAnios();
        }

        private void CargarAnios()
        {
            YearComboBox.Items.Clear();
            int anioActual = DateTime.Now.Year;

            // Llenar ComboBox con años desde 2024 hasta el año actual + 5
            for (int i = 2024; i <= anioActual + 5; i++)
            {
                YearComboBox.Items.Add(i);
            }

            // Seleccionar el año actual como predeterminado
            YearComboBox.SelectedItem = anioActual;
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (YearComboBox.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione un año.");
                return;
            }

            int anioSeleccionado = (int)YearComboBox.SelectedItem;

            // Filtrar empresas por año usando los datos originales
            List<EmpresaInfo> empresasFiltradas = empresasOriginales
                .Where(emp => emp.Año == anioSeleccionado)
                .ToList();

            EmpresasDataGrid.ItemsSource = empresasFiltradas;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Obtener el término de búsqueda
            string searchTerm = SearchTextBox.Text.Trim();

            if (string.IsNullOrEmpty(searchTerm))
            {
                MessageBox.Show("Por favor, ingrese un nombre, NIT o código para buscar.");
                EmpresasDataGrid.ItemsSource = empresasOriginales; // Restaurar los datos originales
                return;
            }

            // Depuración: Mostrar término de búsqueda en consola
            Console.WriteLine($"Buscando: {searchTerm}");

            // Depuración: Imprimir los valores cargados
            foreach (var empresa in empresasOriginales)
            {
                Console.WriteLine($"Empresa: Nombre={empresa.Nombre}, NIT={empresa.Nit}, Código={empresa.Codigo}");
            }

            // Realizar la búsqueda usando los datos originales
            List<EmpresaInfo> empresasFiltradas = empresasOriginales
                .Where(emp =>
                    (!string.IsNullOrEmpty(emp.Nombre) && emp.Nombre.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(emp.Nit) && emp.Nit.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(emp.Codigo) && emp.Codigo.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))) // Nueva condición para 'codigo'
                .ToList();

            // Verificar si se encontraron resultados
            if (empresasFiltradas.Count == 0)
            {
                MessageBox.Show("No se encontraron empresas que coincidan con la búsqueda.");
                EmpresasDataGrid.ItemsSource = empresasOriginales; // Restaurar los datos originales
            }
            else
            {
                EmpresasDataGrid.ItemsSource = empresasFiltradas; // Mostrar los resultados de la búsqueda
            }
        }



        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Buscar por Nombre, NIT o Código")
            {
                SearchTextBox.Text = string.Empty;
                SearchTextBox.Foreground = new SolidColorBrush(Colors.Black);
            }
        }


        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Buscar por Nombre, NIT o Código";
                SearchTextBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }



        private void GenerarClaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (EmpresasDataGrid.SelectedItem is EmpresaInfo empresaSeleccionada)
            {
                // Datos necesarios para generar la clave
                string nit = empresaSeleccionada.Nit;
                string nombre = empresaSeleccionada.Nombre;
                int mesActual = DateTime.Now.Month;
                int anioActual = DateTime.Now.Year;

                // Generar la clave
                string clave = ActivacionHelper.GenerarClave(nit, nombre, mesActual, anioActual);

                // Guardar la clave en la base de datos principal y secundaria
                DatosDB.GuardarClave(nit, clave, nombre); // <-- Se agrega el tercer parámetro 'nombre' como la base de datos

                // Mostrar la clave en un TextBox para copiar
                Window claveWindow = new Window
                {
                    Title = "Clave de Activación",
                    Height = 150,
                    Width = 400,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                StackPanel panel = new StackPanel
                {
                    Margin = new Thickness(10)
                };

                TextBlock label = new TextBlock
                {
                    Text = $"Clave generada para {nombre}:",
                    Margin = new Thickness(0, 0, 0, 10),
                    FontWeight = FontWeights.Bold
                };

                TextBox textBox = new TextBox
                {
                    Text = clave,
                    IsReadOnly = true,
                    Margin = new Thickness(0, 0, 0, 10),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                Button copyButton = new Button
                {
                    Content = "Copiar Clave",
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Width = 100
                };
                copyButton.Click += (s, args) =>
                {
                    Clipboard.SetText(clave);
                    MessageBox.Show("Clave copiada al portapapeles.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                };

                panel.Children.Add(label);
                panel.Children.Add(textBox);
                panel.Children.Add(copyButton);

                claveWindow.Content = panel;
                claveWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Por favor, seleccione una empresa del listado.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnConsultarEquipos_Click(object sender, RoutedEventArgs e)
        {
            // Obtener la lista de equipos desde la base de datos
            var equipos = EquiposHelper.ObtenerEquipos();

            if (equipos.Count > 0)
            {
                // Abrir la vista EquiposView con los equipos obtenidos
                EquiposView equiposView = new EquiposView(equipos);
                equiposView.ShowDialog();
            }
            else
            {
                MessageBox.Show("No se encontraron registros de equipos.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }




    }
}
