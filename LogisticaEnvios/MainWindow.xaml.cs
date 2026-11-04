using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LogisticaEnvios
{
    public partial class MainWindow : Window
    {
        private TablaHash tablaEnvios;
        private Grafo redLogistica;
        private Trie trieCiudades;
        private UnionFind validadorConexiones;

        public MainWindow()
        {
            InitializeComponent();
            InicializarEstructuras();
        }

        private void InicializarEstructuras()
        {
            tablaEnvios = new TablaHash();
            redLogistica = new Grafo();
            trieCiudades = new Trie();
            validadorConexiones = new UnionFind(100);
        }

        private void BtnMenu_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            PanelRutas.Visibility = Visibility.Collapsed;
            PanelEnvios.Visibility = Visibility.Collapsed;
            PanelRastreo.Visibility = Visibility.Collapsed;
            PanelEnrutamiento.Visibility = Visibility.Collapsed;

            BtnMenuRutas.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#353B48"));
            BtnMenuEnvios.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#353B48"));
            BtnMenuRastreo.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#353B48"));
            BtnMenuEnrutamiento.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#353B48"));

            btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0097E6"));

            if (btn.Name == "BtnMenuRutas") PanelRutas.Visibility = Visibility.Visible;
            else if (btn.Name == "BtnMenuEnvios") PanelEnvios.Visibility = Visibility.Visible;
            else if (btn.Name == "BtnMenuRastreo") PanelRastreo.Visibility = Visibility.Visible;
            else if (btn.Name == "BtnMenuEnrutamiento") PanelEnrutamiento.Visibility = Visibility.Visible;
        }

        private void BtnAgregarRuta_Click(object sender, RoutedEventArgs e)
        {
            string origen = TxtNuevaCiudadOrigen.Text.Trim();
            string destino = TxtNuevaCiudadDestino.Text.Trim();

            if (string.IsNullOrEmpty(origen) || string.IsNullOrEmpty(destino))
            {
                MessageBox.Show("Debe ingresar origen y destino válidos.");
                return;
            }

            if (!double.TryParse(TxtNuevoCosto.Text, out double costo) || !double.TryParse(TxtNuevoTiempo.Text, out double tiempo))
            {
                MessageBox.Show("El costo y tiempo deben ser numéricos.");
                return;
            }

            bool unSoloSentido = ChkUnSoloSentido.IsChecked == true;

            if (redLogistica.BuscarIndice(origen) == -1)
            {
                redLogistica.AgregarCiudad(origen);
                trieCiudades.Insertar(origen);
            }
            if (redLogistica.BuscarIndice(destino) == -1)
            {
                redLogistica.AgregarCiudad(destino);
                trieCiudades.Insertar(destino);
            }

            int idOrigen = redLogistica.BuscarIndice(origen);
            int idDestino = redLogistica.BuscarIndice(destino);

            if (validadorConexiones.EstanConectados(idOrigen, idDestino))
            {
                MessageBox.Show($"Validación Union-Find: '{origen}' y '{destino}' ya pertenecen a la misma región conectada.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                validadorConexiones.Unir(idOrigen, idDestino);
            }

            if (redLogistica.AgregarRuta(origen, destino, costo, tiempo, unSoloSentido))
            {
                MessageBox.Show("Ruta agregada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                TxtNuevaCiudadOrigen.Clear();
                TxtNuevaCiudadDestino.Clear();
                TxtNuevoCosto.Clear();
                TxtNuevoTiempo.Clear();
                ChkUnSoloSentido.IsChecked = false;

                ActualizarVistaGrafo();
            }
            else
            {
                MessageBox.Show("No se pudo agregar la ruta. Verifique si ya existe.");
            }
        }

        private void CanvasGrafo_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ActualizarVistaGrafo();
        }

        private void ActualizarVistaGrafo()
        {
            CanvasGrafo.Children.Clear();
            int n = redLogistica.Cantidad;

            if (n == 0) return;

            double ancho = CanvasGrafo.ActualWidth;
            double alto = CanvasGrafo.ActualHeight;

            if (ancho == 0 || alto == 0) return;

            double centroX = ancho / 2;
            double centroY = alto / 2;
            double radio = Math.Min(centroX, centroY) - 60;

            Point[] posiciones = new Point[n];
            for (int i = 0; i < n; i++)
            {
                double angulo = (2 * Math.PI / n) * i;
                double x = centroX + radio * Math.Cos(angulo);
                double y = centroY + radio * Math.Sin(angulo);
                posiciones[i] = new Point(x, y);
            }

            for (int i = 0; i < n; i++)
            {
                Arista actual = redLogistica.ObtenerRutas(i);
                while (actual != null)
                {
                    int d = actual.Destino;

                    double x1 = posiciones[i].X;
                    double y1 = posiciones[i].Y;
                    double x2 = posiciones[d].X;
                    double y2 = posiciones[d].Y;

                    double dx = x2 - x1;
                    double dy = y2 - y1;
                    double distancia = Math.Sqrt(dx * dx + dy * dy);

                    if (distancia > 0)
                    {
                        double ux = dx / distancia;
                        double uy = dy / distancia;

                        double px = -uy;
                        double py = ux;

                        Line linea = new Line
                        {
                            X1 = x1,
                            Y1 = y1,
                            X2 = x2,
                            Y2 = y2,
                            Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#718093")),
                            StrokeThickness = 2
                        };
                        CanvasGrafo.Children.Add(linea);

                        double puntaX = x2 - 20 * ux;
                        double puntaY = y2 - 20 * uy;

                        double baseX = x2 - 35 * ux;
                        double baseY = y2 - 35 * uy;
                        double anchoFlecha = 6;

                        Polygon flecha = new Polygon
                        {
                            Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E1B12C")),
                            Points = new PointCollection
                            {
                                new Point(puntaX, puntaY),
                                new Point(baseX + anchoFlecha * px, baseY + anchoFlecha * py),
                                new Point(baseX - anchoFlecha * px, baseY - anchoFlecha * py)
                            }
                        };
                        CanvasGrafo.Children.Add(flecha);

                        bool esBidireccional = false;
                        Arista rutaInversa = redLogistica.ObtenerRutas(d);

                        while (rutaInversa != null)
                        {
                            if (rutaInversa.Destino == i)
                            {
                                esBidireccional = true;
                                break;
                            }
                            rutaInversa = rutaInversa.Siguiente;
                        }

                        if (!esBidireccional || (esBidireccional && i < d))
                        {
                            TextBlock txtPeso = new TextBlock
                            {
                                Text = $"Q{actual.Costo} | {actual.Tiempo}m",
                                Foreground = Brushes.Yellow,
                                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2F3640")),
                                Padding = new Thickness(2),
                                FontSize = 10
                            };

                            double centroLineaX = (x1 + x2) / 2;
                            double centroLineaY = (y1 + y2) / 2 - 15;

                            Canvas.SetLeft(txtPeso, centroLineaX - 30);
                            Canvas.SetTop(txtPeso, centroLineaY);
                            CanvasGrafo.Children.Add(txtPeso);
                        }
                    }

                    actual = actual.Siguiente;
                }
            }

            for (int i = 0; i < n; i++)
            {
                Ellipse nodo = new Ellipse
                {
                    Width = 40,
                    Height = 40,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0097E6")),
                    Stroke = Brushes.White,
                    StrokeThickness = 2
                };

                Canvas.SetLeft(nodo, posiciones[i].X - 20);
                Canvas.SetTop(nodo, posiciones[i].Y - 20);
                CanvasGrafo.Children.Add(nodo);

                TextBlock txtNombre = new TextBlock
                {
                    Text = redLogistica.ObtenerNombre(i).ToUpper(),
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    FontSize = 13
                };

                Canvas.SetLeft(txtNombre, posiciones[i].X + 25);
                Canvas.SetTop(txtNombre, posiciones[i].Y - 10);
                CanvasGrafo.Children.Add(txtNombre);
            }
        }

        private void BtnCargarDatos_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Archivos CSV y TXT|*.csv;*.txt";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string[] lineas = System.IO.File.ReadAllLines(openFileDialog.FileName);

                    int aceptados = 0, rechazadosRuta = 0, rechazadosCiudad = 0, duplicados = 0, invalidos = 0;
                    string reporteAceptados = "--- ENVÍOS ACEPTADOS (Ruta Confirmada) ---\n";
                    string reporteRechazados = "--- ENVÍOS RECHAZADOS ---\n";

                    for (int i = 0; i < lineas.Length; i++)
                    {
                        string linea = lineas[i].Trim();
                        if (linea.Length == 0) continue;
                        if (i == 0 && linea.ToLowerInvariant().StartsWith("codigo")) continue;

                        string[] campos = linea.Split(',');
                        if (campos.Length < 5) { invalidos++; continue; }

                        string codigo = campos[0].Trim();
                        string remitente = campos[1].Trim();
                        string destinatario = campos[2].Trim();
                        string origen = campos[3].Trim();
                        string destino = campos[4].Trim();

                        if (codigo.Length == 0) { invalidos++; continue; }

                        int idO = redLogistica.BuscarIndice(origen);
                        int idD = redLogistica.BuscarIndice(destino);

                        if (idO == -1 || idD == -1)
                        {
                            rechazadosCiudad++;
                            reporteRechazados += $"[X] {codigo}: Ciudades no existen en la red actual ({origen} o {destino}).\n";
                            continue;
                        }

                        if (!validadorConexiones.EstanConectados(idO, idD))
                        {
                            rechazadosRuta++;
                            reporteRechazados += $"[X] {codigo}: Rechazado. No hay ninguna ruta conectando {origen} con {destino}.\n";
                            continue;
                        }

                        Envio env = new Envio(codigo, remitente, destinatario, origen, destino);
                        if (tablaEnvios.Insertar(env))
                        {
                            aceptados++;
                            reporteAceptados += $"[✓] {codigo}: Aceptado ({origen} -> {destino})\n";
                        }
                        else
                        {
                            duplicados++;
                            reporteRechazados += $"[X] {codigo}: Código de rastreo duplicado en el sistema.\n";
                        }
                    }

                    string resumen = $"RESULTADOS DE LA CARGA:\n" +
                                     $"Aceptados y guardados: {aceptados}\n" +
                                     $"Rechazados (Sin ruta posible): {rechazadosRuta}\n" +
                                     $"Rechazados (Ciudad no existe): {rechazadosCiudad}\n" +
                                     $"Rechazados (Código duplicado): {duplicados}\n" +
                                     $"Líneas inválidas en archivo: {invalidos}\n\n";

                    TxtResultadosCargaMasiva.Text = resumen + reporteAceptados + "\n" + reporteRechazados;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al leer el archivo: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnAgregarEnvioManual_Click(object sender, RoutedEventArgs e)
        {
            string codigo = TxtManCodigo.Text.Trim();
            string remitente = TxtManRemitente.Text.Trim();
            string destinatario = TxtManDestinatario.Text.Trim();
            string origen = TxtManOrigen.Text.Trim();
            string destino = TxtManDestino.Text.Trim();

            if (string.IsNullOrEmpty(codigo) || string.IsNullOrEmpty(origen) || string.IsNullOrEmpty(destino))
            {
                MessageBox.Show("El código, origen y destino son obligatorios.");
                return;
            }

            Envio nuevo = new Envio(codigo, remitente, destinatario, origen, destino);

            if (tablaEnvios.Insertar(nuevo))
            {
                MessageBox.Show("Envío registrado con éxito en la Tabla Hash.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                TxtManCodigo.Clear(); TxtManRemitente.Clear(); TxtManDestinatario.Clear();
                TxtManOrigen.Clear(); TxtManDestino.Clear();
            }
            else
            {
                MessageBox.Show("El código de rastreo ya existe. Inserción denegada por colisión.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnBuscarEnvio_Click(object sender, RoutedEventArgs e)
        {
            string codigo = TxtCodigoRastreo.Text.Trim();
            if (string.IsNullOrEmpty(codigo)) return;

            Envio resultado = tablaEnvios.Buscar(codigo);
            if (resultado != null)
                TxtResultadoEnvio.Text = $"ESTADO: Encontrado en Tabla Hash\n\nRemitente: {resultado.Remitente}\nDestinatario: {resultado.Destinatario}\nCiudad Origen: {resultado.CiudadOrigen}\nCiudad Destino: {resultado.CiudadDestino}";
            else
                TxtResultadoEnvio.Text = "No se encontró ningún envío asociado a este código.";
        }

        private void TxtOrigen_TextChanged(object sender, TextChangedEventArgs e)
        {
            ManejarAutocompletado(TxtOrigen, ListaSugerenciasOrigen);
        }

        private void TxtDestino_TextChanged(object sender, TextChangedEventArgs e)
        {
            ManejarAutocompletado(TxtDestino, ListaSugerenciasDestino);
        }

        private void ManejarAutocompletado(TextBox input, ListBox lista)
        {
            string texto = input.Text.Trim();
            if (texto.Length > 0)
            {
                string[] sugerencias = trieCiudades.Autocompletar(texto);
                if (sugerencias.Length > 0)
                {
                    lista.ItemsSource = sugerencias;
                    lista.Visibility = Visibility.Visible;
                    return;
                }
            }
            lista.Visibility = Visibility.Collapsed;
        }

        private void ListaSugerenciasOrigen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaSugerenciasOrigen.SelectedItem != null)
            {
                TxtOrigen.Text = ListaSugerenciasOrigen.SelectedItem.ToString();
                ListaSugerenciasOrigen.Visibility = Visibility.Collapsed;
            }
        }

        private void ListaSugerenciasDestino_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaSugerenciasDestino.SelectedItem != null)
            {
                TxtDestino.Text = ListaSugerenciasDestino.SelectedItem.ToString();
                ListaSugerenciasDestino.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnCalcularRuta_Click(object sender, RoutedEventArgs e)
        {
            string origen = TxtOrigen.Text.Trim();
            string destino = TxtDestino.Text.Trim();
            int idOrigen = redLogistica.BuscarIndice(origen);
            int idDestino = redLogistica.BuscarIndice(destino);

            if (idOrigen == -1 || idDestino == -1)
            {
                TxtResultadoRuta.Text = "Una o ambas ciudades no están registradas en la red logística.";
                return;
            }

            if (!validadorConexiones.EstanConectados(idOrigen, idDestino))
            {
                TxtResultadoRuta.Text = "Validación fallida: Las ciudades no están conectadas en la misma región (Union-Find).";
                return;
            }

            Dijkstra dijkstra = new Dijkstra(redLogistica);
            bool porCosto = RbtnCosto.IsChecked == true;
            ResultadoRuta ruta = dijkstra.CaminoMasCorto(origen, destino, porCosto);

            if (ruta.Encontrada)
            {
                string criterio = porCosto ? "Costo Económico" : "Tiempo Rápido";
                TxtResultadoRuta.Text = $"RUTA ENCONTRADA (Optimizada por {criterio}):\n\n" +
                                        $"Trayecto: {string.Join(" -> ", ruta.Ciudades)}\n" +
                                        $"Costo Total: Q{ruta.Costo}\n" +
                                        $"Tiempo Estimado: {ruta.Tiempo} minutos";
            }
            else
            {
                TxtResultadoRuta.Text = "No existe un camino posible entre estas ciudades.";
            }
        }
    }
}