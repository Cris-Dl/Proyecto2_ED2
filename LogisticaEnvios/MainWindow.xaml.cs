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

        private bool EstaAislado(int nodoId)
        {
            if (redLogistica.ObtenerRutas(nodoId) != null) return false;

            for (int i = 0; i < redLogistica.Cantidad; i++)
            {
                if (i == nodoId) continue;
                Arista actual = redLogistica.ObtenerRutas(i);
                while (actual != null)
                {
                    if (actual.Destino == nodoId) return false;
                    actual = actual.Siguiente;
                }
            }
            return true;
        }

        private void BtnAgregarRuta_Click(object sender, RoutedEventArgs e)
        {
            string origen = TxtNuevaCiudadOrigen.Text.Trim();
            string destino = TxtNuevaCiudadDestino.Text.Trim();

            if (string.IsNullOrEmpty(origen) || string.IsNullOrEmpty(destino)) return;
            if (!double.TryParse(TxtNuevoCosto.Text, out double costo) || !double.TryParse(TxtNuevoTiempo.Text, out double tiempo)) return;

            bool unSoloSentido = ChkUnSoloSentido.IsChecked == true;

            if (redLogistica.BuscarIndice(origen) == -1) { redLogistica.AgregarCiudad(origen); trieCiudades.Insertar(origen); }
            if (redLogistica.BuscarIndice(destino) == -1) { redLogistica.AgregarCiudad(destino); trieCiudades.Insertar(destino); }

            int idOrigen = redLogistica.BuscarIndice(origen);
            int idDestino = redLogistica.BuscarIndice(destino);

            if (!validadorConexiones.EstanConectados(idOrigen, idDestino))
                validadorConexiones.Unir(idOrigen, idDestino);

            if (redLogistica.AgregarRuta(origen, destino, costo, tiempo, unSoloSentido))
            {
                TxtNuevaCiudadOrigen.Clear(); TxtNuevaCiudadDestino.Clear();
                TxtNuevoCosto.Clear(); TxtNuevoTiempo.Clear();
                ChkUnSoloSentido.IsChecked = false;
                ActualizarVistaGrafo();
            }
        }

        private void BtnCargarRutasCSV_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV y TXT|*.csv;*.txt";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string[] lineas = System.IO.File.ReadAllLines(openFileDialog.FileName);
                    int agregadas = 0;
                    for (int i = 0; i < lineas.Length; i++)
                    {
                        if (i == 0 || string.IsNullOrEmpty(lineas[i])) continue;
                        string[] p = lineas[i].Split(',');
                        if (p.Length < 4) continue;

                        string o = p[0].Trim(), d = p[1].Trim();
                        if (!double.TryParse(p[2], out double c) || !double.TryParse(p[3], out double t)) continue;
                        bool unSolo = (p.Length >= 5 && p[4].Trim().ToLower() == "true");

                        if (redLogistica.BuscarIndice(o) == -1) { redLogistica.AgregarCiudad(o); trieCiudades.Insertar(o); }
                        if (redLogistica.BuscarIndice(d) == -1) { redLogistica.AgregarCiudad(d); trieCiudades.Insertar(d); }

                        validadorConexiones.Unir(redLogistica.BuscarIndice(o), redLogistica.BuscarIndice(d));
                        if (redLogistica.AgregarRuta(o, d, c, t, unSolo)) agregadas++;
                    }
                    TxtEstadoCargaRutas.Text = $"Cargadas {agregadas} rutas";
                    TxtEstadoCargaRutas.Foreground = Brushes.Green;
                    ActualizarVistaGrafo();
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private void BtnEliminarRuta_Click(object sender, RoutedEventArgs e)
        {
            string origen = TxtEliminarOrigen.Text.Trim();
            string destino = TxtEliminarDestino.Text.Trim();

            if (redLogistica.EliminarRuta(origen, destino, false))
            {
                ReconstruirUnionFind();

                int idO = redLogistica.BuscarIndice(origen);
                int idD = redLogistica.BuscarIndice(destino);

                string msg = "Ruta desconectada correctamente.";
                if (idO != -1 && EstaAislado(idO)) msg += "\n- El nodo '" + origen + "' quedó sin conexiones y fue eliminado de la red activa.";
                if (idD != -1 && EstaAislado(idD)) msg += "\n- El nodo '" + destino + "' quedó sin conexiones y fue eliminado de la red activa.";

                ActualizarVistaGrafo();
                TxtEliminarOrigen.Clear(); TxtEliminarDestino.Clear();
                MessageBox.Show(msg, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else MessageBox.Show("No se encontró esa ruta.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void BtnEliminarSede_Click(object sender, RoutedEventArgs e)
        {
            string sede = TxtEliminarSede.Text.Trim();
            int id = redLogistica.BuscarIndice(sede);
            if (id == -1 || EstaAislado(id)) { MessageBox.Show("Sede no encontrada o ya eliminada."); return; }

            for (int i = 0; i < redLogistica.Cantidad; i++)
            {
                if (i != id)
                {
                    string nodo = redLogistica.ObtenerNombre(i);
                    redLogistica.EliminarRuta(nodo, sede, true);
                    redLogistica.EliminarRuta(sede, nodo, true);
                }
            }

            ReconstruirUnionFind();
            ActualizarVistaGrafo();
            TxtEliminarSede.Clear();
            MessageBox.Show($"La sede '{sede}' ha sido eliminada. Ya no forma parte de la red logística activa.", "Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ReconstruirUnionFind()
        {
            validadorConexiones = new UnionFind(100);
            for (int i = 0; i < redLogistica.Cantidad; i++)
            {
                if (EstaAislado(i)) continue;
                Arista actual = redLogistica.ObtenerRutas(i);
                while (actual != null)
                {
                    validadorConexiones.Unir(i, actual.Destino);
                    actual = actual.Siguiente;
                }
            }
        }

        private void CanvasGrafo_SizeChanged(object sender, SizeChangedEventArgs e) { ActualizarVistaGrafo(); }

        private void ActualizarVistaGrafo()
        {
            CanvasGrafo.Children.Clear();
            int n = redLogistica.Cantidad;
            if (n == 0 || CanvasGrafo.ActualWidth == 0) return;

            double ancho = CanvasGrafo.ActualWidth;
            double alto = CanvasGrafo.ActualHeight;
            double cx = ancho / 2;
            double cy = alto / 2;

            double radioX = Math.Max(50, cx - 140);
            double radioY = Math.Max(50, cy - 90);

            int activosCount = 0;
            for (int i = 0; i < n; i++) if (!EstaAislado(i)) activosCount++;
            if (activosCount == 0) return;

            Point[] posiciones = new Point[n];
            double[] angulos = new double[n];
            int idxActivo = 0;

            for (int i = 0; i < n; i++)
            {
                if (EstaAislado(i)) continue;

                double angulo = (2 * Math.PI / activosCount) * idxActivo;
                angulos[i] = angulo;
                posiciones[i] = new Point(cx + radioX * Math.Cos(angulo), cy + radioY * Math.Sin(angulo));
                idxActivo++;
            }

            for (int i = 0; i < n; i++)
            {
                if (EstaAislado(i)) continue;
                Arista actual = redLogistica.ObtenerRutas(i);
                while (actual != null)
                {
                    int d = actual.Destino;
                    if (EstaAislado(d)) { actual = actual.Siguiente; continue; }

                    double dx = posiciones[d].X - posiciones[i].X;
                    double dy = posiciones[d].Y - posiciones[i].Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);

                    if (dist > 0)
                    {
                        double ux = dx / dist, uy = dy / dist;
                        double px = -uy, py = ux;

                        Line linea = new Line
                        {
                            X1 = posiciones[i].X,
                            Y1 = posiciones[i].Y,
                            X2 = posiciones[d].X,
                            Y2 = posiciones[d].Y,
                            Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#718093")),
                            StrokeThickness = 2
                        };
                        CanvasGrafo.Children.Add(linea);

                        double puntaX = posiciones[d].X - 20 * ux;
                        double puntaY = posiciones[d].Y - 20 * uy;
                        double baseX = posiciones[d].X - 35 * ux;
                        double baseY = posiciones[d].Y - 35 * uy;
                        double anchoFlecha = 6;

                        Polygon flecha = new Polygon
                        {
                            Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E1B12C")),
                            Points = new PointCollection { new Point(puntaX, puntaY), new Point(baseX + anchoFlecha * px, baseY + anchoFlecha * py), new Point(baseX - anchoFlecha * px, baseY - anchoFlecha * py) }
                        };
                        CanvasGrafo.Children.Add(flecha);

                        bool bidir = false;
                        Arista inv = redLogistica.ObtenerRutas(d);
                        while (inv != null) { if (inv.Destino == i) { bidir = true; break; } inv = inv.Siguiente; }

                        if (!bidir || (bidir && i < d))
                        {
                            TextBlock txtPeso = new TextBlock
                            {
                                Text = $"Q{actual.Costo}|{actual.Tiempo}m",
                                Foreground = Brushes.Yellow,
                                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2F3640")),
                                Padding = new Thickness(2),
                                FontSize = 10
                            };

                            double centroLineaX = (posiciones[i].X + posiciones[d].X) / 2;
                            double centroLineaY = (posiciones[i].Y + posiciones[d].Y) / 2;

                            Canvas.SetLeft(txtPeso, centroLineaX - 25 + (15 * px));
                            Canvas.SetTop(txtPeso, centroLineaY - 10 + (15 * py));
                            CanvasGrafo.Children.Add(txtPeso);
                        }
                    }
                    actual = actual.Siguiente;
                }
            }

            for (int i = 0; i < n; i++)
            {
                if (EstaAislado(i)) continue;

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

                TextBlock txtN = new TextBlock
                {
                    Text = redLogistica.ObtenerNombre(i).ToUpper(),
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    FontSize = 12
                };

                txtN.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

                double outX = Math.Cos(angulos[i]);
                double outY = Math.Sin(angulos[i]);

                double finalX = posiciones[i].X - (txtN.DesiredSize.Width / 2) + (outX * 45);
                double finalY = posiciones[i].Y - (txtN.DesiredSize.Height / 2) + (outY * 35);

                Canvas.SetLeft(txtN, finalX);
                Canvas.SetTop(txtN, finalY);
                CanvasGrafo.Children.Add(txtN);
            }
        }

        private void BtnCargarDatos_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "CSV y TXT|*.csv;*.txt" };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    string[] lineas = System.IO.File.ReadAllLines(ofd.FileName);
                    int ac = 0, rR = 0, rC = 0, dup = 0;
                    string repA = "--- ACEPTADOS ---\n", repR = "--- RECHAZADOS ---\n";

                    for (int i = 1; i < lineas.Length; i++)
                    {
                        if (string.IsNullOrEmpty(lineas[i])) continue;
                        string[] c = lineas[i].Split(',');
                        if (c.Length < 5) continue;

                        string cod = c[0].Trim(), ori = c[3].Trim(), dest = c[4].Trim();
                        int idO = redLogistica.BuscarIndice(ori), idD = redLogistica.BuscarIndice(dest);

                        string prioridad = "Costo";
                        if (c.Length >= 6)
                        {
                            string priStr = c[5].Trim().ToLower();
                            if (priStr == "tiempo" || priStr == "rapido" || priStr == "rápido") prioridad = "Tiempo";
                        }

                        if (idO == -1 || idD == -1 || EstaAislado(idO) || EstaAislado(idD)) { rC++; repR += $"[X] {cod}: Sede no existe.\n"; continue; }
                        if (!validadorConexiones.EstanConectados(idO, idD)) { rR++; repR += $"[X] {cod}: Sede aislada o sin ruta.\n"; continue; }

                        if (tablaEnvios.Insertar(new Envio(cod, c[1].Trim(), c[2].Trim(), ori, dest, prioridad))) { ac++; repA += $"[✓] {cod} (P: {prioridad})\n"; }
                        else { dup++; repR += $"[X] {cod}: Duplicado.\n"; }
                    }
                    TxtResultadosCargaMasiva.Text = $"Guardados: {ac} | Sin Ruta: {rR} | Sin Sede: {rC} | Duplicados: {dup}\n\n" + repA + "\n" + repR;
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private void BtnAgregarEnvioManual_Click(object sender, RoutedEventArgs e)
        {
            string c = TxtManCodigo.Text.Trim(), o = TxtManOrigen.Text.Trim(), d = TxtManDestino.Text.Trim();
            if (string.IsNullOrEmpty(c) || string.IsNullOrEmpty(o) || string.IsNullOrEmpty(d)) return;

            int idO = redLogistica.BuscarIndice(o), idD = redLogistica.BuscarIndice(d);

            if (idO == -1 || idD == -1 || EstaAislado(idO) || EstaAislado(idD)) { MessageBox.Show("Error: La sede de origen o destino no existe.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            if (!validadorConexiones.EstanConectados(idO, idD)) { MessageBox.Show("Error: No hay conexión entre estas sedes.", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error); return; }

            string prioridadSeleccionada = ((ComboBoxItem)CmbPrioridad.SelectedItem).Content.ToString().Contains("Tiempo") ? "Tiempo" : "Costo";

            if (tablaEnvios.Insertar(new Envio(c, TxtManRemitente.Text.Trim(), TxtManDestinatario.Text.Trim(), o, d, prioridadSeleccionada)))
            {
                MessageBox.Show("Envío validado y registrado con éxito.");
                TxtManCodigo.Clear(); TxtManRemitente.Clear(); TxtManDestinatario.Clear(); TxtManOrigen.Clear(); TxtManDestino.Clear();
                CmbPrioridad.SelectedIndex = 0;
            }
            else MessageBox.Show("Código de rastreo duplicado.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnBuscarEnvio_Click(object sender, RoutedEventArgs e)
        {
            string codigo = TxtCodigoRastreo.Text.Trim();
            Envio res = tablaEnvios.Buscar(codigo);
            if (res != null)
            {
                Dijkstra dijkstra = new Dijkstra(redLogistica);
                bool porCosto = (res.Prioridad == "Costo");
                ResultadoRuta ruta = dijkstra.CaminoMasCorto(res.CiudadOrigen, res.CiudadDestino, porCosto);

                string caminoTexto = ruta.Encontrada ? string.Join(" -> ", ruta.Ciudades) : "Ruta bloqueada o sede eliminada";

                TxtResultadoEnvio.Text = $"✓ ENVÍO ENCONTRADO:\n" +
                                         $"   Código: {res.CodigoRastreo}\n" +
                                         $"   De: {res.Remitente} ({res.CiudadOrigen})\n" +
                                         $"   Para: {res.Destinatario} ({res.CiudadDestino})\n" +
                                         $"   Prioridad Elegida: {res.Prioridad}\n\n" +
                                         $"📍 HOJA DE RUTA ESTIMADA:\n" +
                                         $"   Trayecto: {caminoTexto}\n" +
                                         $"   Costo: Q{ruta.Costo} | Tiempo: {ruta.Tiempo} min";
            }
            else TxtResultadoEnvio.Text = "No se encontró ningún envío asociado a este código.";
        }

        private void BtnVerTodos_Click(object sender, RoutedEventArgs e)
        {
            Envio[] todos = tablaEnvios.ObtenerTodos();
            if (todos.Length == 0) { TxtResultadoEnvio.Text = "La base de datos de envíos está vacía."; return; }

            string informe = $"TOTAL DE ENVÍOS EN SISTEMA: {todos.Length}\n----------------------------------\n";
            Dijkstra dk = new Dijkstra(redLogistica);

            for (int i = 0; i < todos.Length; i++)
            {
                bool porCosto = (todos[i].Prioridad == "Costo");
                ResultadoRuta ruta = dk.CaminoMasCorto(todos[i].CiudadOrigen, todos[i].CiudadDestino, porCosto);
                string viaje = ruta.Encontrada ? $"Ruta: {string.Join(">", ruta.Ciudades)} (Q{ruta.Costo}|{ruta.Tiempo}m)" : "Sin ruta";
                informe += $"[{todos[i].CodigoRastreo}] ({todos[i].Prioridad}) {todos[i].CiudadOrigen} a {todos[i].CiudadDestino} | {viaje}\n";
            }
            TxtResultadoEnvio.Text = informe;
        }

        private void TxtOrigen_TextChanged(object sender, TextChangedEventArgs e) { ManejarAutocompletado(TxtOrigen, ListaSugerenciasOrigen); }
        private void TxtDestino_TextChanged(object sender, TextChangedEventArgs e) { ManejarAutocompletado(TxtDestino, ListaSugerenciasDestino); }
        private void TxtManOrigen_TextChanged(object sender, TextChangedEventArgs e) { ManejarAutocompletado(TxtManOrigen, ListaManOrigen); }
        private void TxtManDestino_TextChanged(object sender, TextChangedEventArgs e) { ManejarAutocompletado(TxtManDestino, ListaManDestino); }
        private void TxtEliminarOrigen_TextChanged(object sender, TextChangedEventArgs e) { ManejarAutocompletado(TxtEliminarOrigen, ListaEliminarOrigen); }
        private void TxtEliminarDestino_TextChanged(object sender, TextChangedEventArgs e) { ManejarAutocompletado(TxtEliminarDestino, ListaEliminarDestino); }
        private void TxtEliminarSede_TextChanged(object sender, TextChangedEventArgs e) { ManejarAutocompletado(TxtEliminarSede, ListaEliminarSede); }

        private void ManejarAutocompletado(TextBox input, ListBox lista)
        {
            string t = input.Text.Trim();
            if (t.Length > 0)
            {
                string[] sug = trieCiudades.Autocompletar(t);
                if (sug.Length > 0)
                {
                    int activasCount = 0;
                    string[] temp = new string[sug.Length];

                    for (int i = 0; i < sug.Length; i++)
                    {
                        int id = redLogistica.BuscarIndice(sug[i]);
                        if (id != -1 && !EstaAislado(id))
                        {
                            temp[activasCount++] = sug[i];
                        }
                    }

                    if (activasCount > 0)
                    {
                        string[] finales = new string[activasCount];
                        Array.Copy(temp, finales, activasCount);
                        lista.ItemsSource = finales;
                        lista.Visibility = Visibility.Visible;
                        return;
                    }
                }
            }
            lista.Visibility = Visibility.Collapsed;
        }

        private void ListaSugerenciasOrigen_SelectionChanged(object sender, SelectionChangedEventArgs e) { AsignarSeleccion(TxtOrigen, ListaSugerenciasOrigen); }
        private void ListaSugerenciasDestino_SelectionChanged(object sender, SelectionChangedEventArgs e) { AsignarSeleccion(TxtDestino, ListaSugerenciasDestino); }
        private void ListaManOrigen_SelectionChanged(object sender, SelectionChangedEventArgs e) { AsignarSeleccion(TxtManOrigen, ListaManOrigen); }
        private void ListaManDestino_SelectionChanged(object sender, SelectionChangedEventArgs e) { AsignarSeleccion(TxtManDestino, ListaManDestino); }
        private void ListaEliminarOrigen_SelectionChanged(object sender, SelectionChangedEventArgs e) { AsignarSeleccion(TxtEliminarOrigen, ListaEliminarOrigen); }
        private void ListaEliminarDestino_SelectionChanged(object sender, SelectionChangedEventArgs e) { AsignarSeleccion(TxtEliminarDestino, ListaEliminarDestino); }
        private void ListaEliminarSede_SelectionChanged(object sender, SelectionChangedEventArgs e) { AsignarSeleccion(TxtEliminarSede, ListaEliminarSede); }

        private void AsignarSeleccion(TextBox txt, ListBox lst)
        {
            if (lst.SelectedItem != null) { txt.Text = lst.SelectedItem.ToString(); lst.Visibility = Visibility.Collapsed; }
        }

        private void BtnCalcularRuta_Click(object sender, RoutedEventArgs e)
        {
            string o = TxtOrigen.Text.Trim(), d = TxtDestino.Text.Trim();
            int idO = redLogistica.BuscarIndice(o), idD = redLogistica.BuscarIndice(d);

            if (idO == -1 || idD == -1 || EstaAislado(idO) || EstaAislado(idD)) { TxtResultadoRuta.Text = "Una o ambas ciudades no están registradas."; return; }
            if (!validadorConexiones.EstanConectados(idO, idD)) { TxtResultadoRuta.Text = "Validación fallida: Sedes aisladas o sin conexión."; return; }

            Dijkstra dijkstra = new Dijkstra(redLogistica);
            bool porCosto = RbtnCosto.IsChecked == true;
            ResultadoRuta ruta = dijkstra.CaminoMasCorto(o, d, porCosto);

            if (ruta.Encontrada)
            {
                TxtResultadoRuta.Text = $"RUTA ENCONTRADA (Optimizada por {(porCosto ? "Costo" : "Tiempo")}):\n\n" +
                                        $"Trayecto: {string.Join(" -> ", ruta.Ciudades)}\n" +
                                        $"Costo Total: Q{ruta.Costo} | Tiempo Estimado: {ruta.Tiempo} minutos";
            }
            else TxtResultadoRuta.Text = "No existe camino posible.";
        }
    }
}