using System.Collections.Generic;

namespace LogisticaEnvios
{
    public class Grafo
    {
        private Vertice[] vertices = new Vertice[10];

        public int Cantidad { get; private set; }

        public bool AgregarCiudad(string nombre)
        {
            if (string.IsNullOrEmpty(nombre)) return false;
            if (BuscarIndice(nombre) != -1) return false;

            if (Cantidad == vertices.Length) Crecer();

            vertices[Cantidad] = new Vertice(nombre, Cantidad);
            Cantidad++;
            return true;
        }

        public int BuscarIndice(string nombre)
        {
            for (int i = 0; i < Cantidad; i++)
            {
                if (vertices[i].Nombre == nombre) return i;
            }
            return -1;
        }

        public string ObtenerNombre(int indice)
        {
            if (indice < 0 || indice >= Cantidad) return null;
            return vertices[indice].Nombre;
        }

        public Arista ObtenerRutas(int indice)
        {
            if (indice < 0 || indice >= Cantidad) return null;
            return vertices[indice].Rutas;
        }

        public bool AgregarRuta(string origen, string destino, double costo, double tiempo, bool unSoloSentido)
        {
            int o = BuscarIndice(origen);
            int d = BuscarIndice(destino);

            if (o < 0 || d < 0 || o == d || costo < 0 || tiempo < 0) return false;
            if (BuscarArista(o, d) != null) return false;
            if (!unSoloSentido && BuscarArista(d, o) != null) return false;

            Insertar(o, d, costo, tiempo);
            if (!unSoloSentido) Insertar(d, o, costo, tiempo);

            return true;
        }

        public bool EliminarRuta(string origen, string destino, bool unSoloSentido)
        {
            int o = BuscarIndice(origen);
            int d = BuscarIndice(destino);

            if (o < 0 || d < 0) return false;

            bool eliminada = Quitar(o, d);
            if (!unSoloSentido) eliminada = Quitar(d, o) || eliminada;

            return eliminada;
        }

        private bool Quitar(int o, int d)
        {
            Arista anterior = null;
            Arista actual = vertices[o].Rutas;

            while (actual != null)
            {
                if (actual.Destino == d)
                {
                    if (anterior == null) vertices[o].Rutas = actual.Siguiente;
                    else anterior.Siguiente = actual.Siguiente;
                    return true;
                }
                anterior = actual;
                actual = actual.Siguiente;
            }
            return false;
        }

        private Arista BuscarArista(int origen, int destino)
        {
            Arista actual = vertices[origen].Rutas;
            while (actual != null)
            {
                if (actual.Destino == destino) return actual;
                actual = actual.Siguiente;
            }
            return null;
        }

        private void Insertar(int origen, int destino, double costo, double tiempo)
        {
            Arista nueva = new Arista(destino, costo, tiempo);
            nueva.Siguiente = vertices[origen].Rutas;
            vertices[origen].Rutas = nueva;
        }

        private void Crecer()
        {
            Vertice[] nuevo = new Vertice[vertices.Length * 2];
            for (int i = 0; i < Cantidad; i++) nuevo[i] = vertices[i];
            vertices = nuevo;
        }

        public string[] RecorridoBFS(string inicio)
        {
            int origen = BuscarIndice(inicio);
            if (origen < 0) return new string[0];

            string[] resultado = new string[Cantidad];
            int contador = 0;
            bool[] visitado = new bool[Cantidad];
            Queue<int> cola = new Queue<int>();

            visitado[origen] = true;
            cola.Enqueue(origen);

            while (cola.Count > 0)
            {
                int u = cola.Dequeue();
                resultado[contador++] = vertices[u].Nombre;

                Arista actual = vertices[u].Rutas;
                while (actual != null)
                {
                    if (!visitado[actual.Destino])
                    {
                        visitado[actual.Destino] = true;
                        cola.Enqueue(actual.Destino);
                    }
                    actual = actual.Siguiente;
                }
            }

            string[] final = new string[contador];
            for (int i = 0; i < contador; i++) final[i] = resultado[i];
            return final;
        }

        public string[] RecorridoDFS(string inicio)
        {
            int origen = BuscarIndice(inicio);
            if (origen < 0) return new string[0];

            string[] resultado = new string[Cantidad];
            int contador = 0;
            bool[] visitado = new bool[Cantidad];

            DFSRecursivo(origen, visitado, resultado, ref contador);

            string[] final = new string[contador];
            for (int i = 0; i < contador; i++) final[i] = resultado[i];
            return final;
        }

        private void DFSRecursivo(int u, bool[] visitado, string[] resultado, ref int contador)
        {
            visitado[u] = true;
            resultado[contador++] = vertices[u].Nombre;

            Arista actual = vertices[u].Rutas;
            while (actual != null)
            {
                if (!visitado[actual.Destino])
                {
                    DFSRecursivo(actual.Destino, visitado, resultado, ref contador);
                }
                actual = actual.Siguiente;
            }
        }
    }
}