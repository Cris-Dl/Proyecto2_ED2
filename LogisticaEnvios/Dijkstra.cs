namespace LogisticaEnvios
{
    public class ResultadoRuta
    {
        public bool Encontrada;
        public string[] Ciudades;
        public double Costo;
        public double Tiempo;

        public ResultadoRuta()
        {
            Encontrada = false;
            Ciudades = new string[0];
        }

        public ResultadoRuta(string[] ciudades, double costo, double tiempo)
        {
            Encontrada = true;
            Ciudades = ciudades;
            Costo = costo;
            Tiempo = tiempo;
        }
    }

    public class Dijkstra
    {
        private Grafo grafo;

        public Dijkstra(Grafo grafo)
        {
            this.grafo = grafo;
        }

        public ResultadoRuta CaminoMasCorto(string origen, string destino, bool porCosto)
        {
            int o = grafo.BuscarIndice(origen);
            int d = grafo.BuscarIndice(destino);

            if (o < 0 || d < 0) return new ResultadoRuta();

            int n = grafo.Cantidad;
            double[] distancia = new double[n];
            double[] costo = new double[n];
            double[] tiempo = new double[n];
            int[] anterior = new int[n];
            bool[] visitado = new bool[n];

            for (int i = 0; i < n; i++)
            {
                distancia[i] = double.MaxValue;
                anterior[i] = -1;
            }
            distancia[o] = 0;

            for (int paso = 0; paso < n; paso++)
            {
                int u = -1;
                for (int i = 0; i < n; i++)
                {
                    if (!visitado[i] && distancia[i] != double.MaxValue && (u == -1 || distancia[i] < distancia[u]))
                        u = i;
                }

                if (u == -1 || u == d) break;

                visitado[u] = true;

                Arista ruta = grafo.ObtenerRutas(u);
                while (ruta != null)
                {
                    double peso = porCosto ? ruta.Costo : ruta.Tiempo;
                    double nueva = distancia[u] + peso;

                    if (!visitado[ruta.Destino] && nueva < distancia[ruta.Destino])
                    {
                        distancia[ruta.Destino] = nueva;
                        anterior[ruta.Destino] = u;
                        costo[ruta.Destino] = costo[u] + ruta.Costo;
                        tiempo[ruta.Destino] = tiempo[u] + ruta.Tiempo;
                    }
                    ruta = ruta.Siguiente;
                }
            }

            if (distancia[d] == double.MaxValue) return new ResultadoRuta();

            int largo = 0;
            for (int v = d; v != -1; v = anterior[v]) largo++;

            string[] ciudades = new string[largo];
            int posicion = largo - 1;
            for (int v = d; v != -1; v = anterior[v])
            {
                ciudades[posicion] = grafo.ObtenerNombre(v);
                posicion--;
            }

            return new ResultadoRuta(ciudades, costo[d], tiempo[d]);
        }
    }
}