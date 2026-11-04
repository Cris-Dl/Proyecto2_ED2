namespace LogisticaEnvios
{
    public class Trie
    {
        private NodoTrie raiz;

        public Trie()
        {
            raiz = new NodoTrie();
        }

        private int ObtenerIndice(char c)
        {
            int indice = char.ToLower(c) - 'a';
            if (indice < 0 || indice >= NodoTrie.TotalSimbolos) return 26;
            return indice;
        }

        public void Insertar(string palabra)
        {
            if (string.IsNullOrEmpty(palabra)) return;

            NodoTrie actual = raiz;
            for (int i = 0; i < palabra.Length; i++)
            {
                int indice = ObtenerIndice(palabra[i]);
                if (actual.Hijos[indice] == null)
                    actual.Hijos[indice] = new NodoTrie();

                actual = actual.Hijos[indice];
            }
            actual.EsFinDePalabra = true;
            actual.Ciudad = palabra;
        }

        public string[] Autocompletar(string prefijo)
        {
            if (string.IsNullOrEmpty(prefijo)) return new string[0];

            NodoTrie actual = raiz;
            for (int i = 0; i < prefijo.Length; i++)
            {
                int indice = ObtenerIndice(prefijo[i]);
                if (actual.Hijos[indice] == null) return new string[0];
                actual = actual.Hijos[indice];
            }

            string[] resultados = new string[10];
            int contador = 0;
            RecolectarPalabras(actual, ref resultados, ref contador);

            string[] final = new string[contador];
            for (int i = 0; i < contador; i++) final[i] = resultados[i];
            return final;
        }

        private void RecolectarPalabras(NodoTrie nodo, ref string[] resultados, ref int contador)
        {
            if (nodo == null) return;

            if (nodo.EsFinDePalabra)
            {
                if (contador == resultados.Length) CrecerArreglo(ref resultados);
                resultados[contador++] = nodo.Ciudad;
            }

            for (int i = 0; i < NodoTrie.TotalSimbolos; i++)
            {
                if (nodo.Hijos[i] != null)
                    RecolectarPalabras(nodo.Hijos[i], ref resultados, ref contador);
            }
        }

        private void CrecerArreglo(ref string[] arreglo)
        {
            string[] nuevo = new string[arreglo.Length * 2];
            for (int i = 0; i < arreglo.Length; i++) nuevo[i] = arreglo[i];
            arreglo = nuevo;
        }
    }
}