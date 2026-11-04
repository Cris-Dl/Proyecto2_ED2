namespace LogisticaEnvios
{
    public class NodoTrie
    {
        public const int TotalSimbolos = 29;

        public NodoTrie[] Hijos = new NodoTrie[TotalSimbolos];
        public bool EsFinDePalabra;
        public string Ciudad;
    }
}