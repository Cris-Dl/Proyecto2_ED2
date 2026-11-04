using System;
using System.IO;

namespace LogisticaEnvios
{
    public class TablaHash
    {
        private class Nodo
        {
            public string Llave;
            public Envio Valor;
            public Nodo Siguiente;

            public Nodo(string llave, Envio valor)
            {
                Llave = llave;
                Valor = valor;
                Siguiente = null;
            }
        }

        private const int CapacidadInicial = 11;
        private const double FactorCargaMaximo = 0.75;

        private Nodo[] cubetas;
        private int cantidad;

        public TablaHash() : this(CapacidadInicial) { }

        public TablaHash(int capacidadInicial)
        {
            if (capacidadInicial < 3) capacidadInicial = CapacidadInicial;
            cubetas = new Nodo[SiguientePrimo(capacidadInicial)];
            cantidad = 0;
        }

        public int Cantidad => cantidad;

        public int Capacidad => cubetas.Length;

        public double FactorCarga => (double)cantidad / cubetas.Length;

        private static string Normalizar(string codigo)
        {
            return (codigo ?? "").Trim().ToUpperInvariant();
        }

        private static int CalcularIndice(string llave, int capacidad)
        {
            uint hash = 5381;
            for (int i = 0; i < llave.Length; i++)
            {
                hash = unchecked((hash << 5) + hash + llave[i]);
            }
            return (int)(hash % (uint)capacidad);
        }

        public bool Insertar(Envio envio)
        {
            if (envio == null) return false;
            string llave = Normalizar(envio.CodigoRastreo);
            if (llave.Length == 0) return false;

            if (Existe(llave)) return false;

            if ((double)(cantidad + 1) / cubetas.Length > FactorCargaMaximo)
                Redimensionar();

            int indice = CalcularIndice(llave, cubetas.Length);
            Nodo nuevo = new Nodo(llave, envio);
            nuevo.Siguiente = cubetas[indice];
            cubetas[indice] = nuevo;
            cantidad++;
            return true;
        }

        public Envio Buscar(string codigoRastreo)
        {
            string llave = Normalizar(codigoRastreo);
            if (llave.Length == 0) return null;

            Nodo actual = cubetas[CalcularIndice(llave, cubetas.Length)];
            while (actual != null)
            {
                if (actual.Llave == llave) return actual.Valor;
                actual = actual.Siguiente;
            }
            return null;
        }

        public bool Existe(string codigoRastreo)
        {
            return Buscar(codigoRastreo) != null;
        }

        public bool Eliminar(string codigoRastreo)
        {
            string llave = Normalizar(codigoRastreo);
            if (llave.Length == 0) return false;

            int indice = CalcularIndice(llave, cubetas.Length);
            Nodo actual = cubetas[indice];
            Nodo anterior = null;

            while (actual != null)
            {
                if (actual.Llave == llave)
                {
                    if (anterior == null) cubetas[indice] = actual.Siguiente;
                    else anterior.Siguiente = actual.Siguiente;
                    cantidad--;
                    return true;
                }
                anterior = actual;
                actual = actual.Siguiente;
            }
            return false;
        }

        public bool Actualizar(string codigoRastreo, string remitente, string destinatario,
                               string ciudadOrigen, string ciudadDestino)
        {
            Envio e = Buscar(codigoRastreo);
            if (e == null) return false;
            e.Remitente = remitente;
            e.Destinatario = destinatario;
            e.CiudadOrigen = ciudadOrigen;
            e.CiudadDestino = ciudadDestino;
            return true;
        }

        private void Redimensionar()
        {
            Nodo[] anteriores = cubetas;
            Nodo[] nuevas = new Nodo[SiguientePrimo(anteriores.Length * 2)];

            for (int i = 0; i < anteriores.Length; i++)
            {
                Nodo actual = anteriores[i];
                while (actual != null)
                {
                    Nodo siguiente = actual.Siguiente;
                    int nuevoIndice = CalcularIndice(actual.Llave, nuevas.Length);
                    actual.Siguiente = nuevas[nuevoIndice];
                    nuevas[nuevoIndice] = actual;
                    actual = siguiente;
                }
            }
            cubetas = nuevas;
        }

        private static bool EsPrimo(int n)
        {
            if (n < 2) return false;
            if (n % 2 == 0) return n == 2;
            for (int i = 3; (long)i * i <= n; i += 2)
                if (n % i == 0) return false;
            return true;
        }

        private static int SiguientePrimo(int n)
        {
            while (!EsPrimo(n)) n++;
            return n;
        }

        public Envio[] ObtenerTodos()
        {
            Envio[] resultado = new Envio[cantidad];
            int pos = 0;
            for (int i = 0; i < cubetas.Length; i++)
            {
                Nodo actual = cubetas[i];
                while (actual != null)
                {
                    resultado[pos++] = actual.Valor;
                    actual = actual.Siguiente;
                }
            }
            return resultado;
        }

        public int CargarDesdeArchivo(string ruta, out int duplicados, out int invalidos)
        {
            duplicados = 0;
            invalidos = 0;
            int cargados = 0;

            if (!File.Exists(ruta)) throw new FileNotFoundException("No se encontró el archivo: " + ruta);

            string[] lineas = File.ReadAllLines(ruta);
            for (int i = 0; i < lineas.Length; i++)
            {
                string linea = lineas[i].Trim();
                if (linea.Length == 0) continue;
                if (i == 0 && linea.ToLowerInvariant().StartsWith("codigo")) continue;

                string[] campos = linea.Split(',');
                if (campos.Length < 5) { invalidos++; continue; }

                Envio e = new Envio(campos[0].Trim(), campos[1].Trim(), campos[2].Trim(),
                                    campos[3].Trim(), campos[4].Trim());

                if (e.CodigoRastreo.Length == 0) { invalidos++; continue; }

                if (Insertar(e)) cargados++;
                else duplicados++;
            }
            return cargados;
        }
    }
}