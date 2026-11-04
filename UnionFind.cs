namespace LogisticaEnvios
{
    public class UnionFind
    {
        private int[] padre;
        private int[] rango;

        public UnionFind(int cantidadElementos)
        {
            padre = new int[cantidadElementos];
            rango = new int[cantidadElementos];
            for (int i = 0; i < cantidadElementos; i++)
            {
                padre[i] = i;
                rango[i] = 0;
            }
        }

        public int Buscar(int i)
        {
            if (padre[i] == i)
                return i;

            padre[i] = Buscar(padre[i]);
            return padre[i];
        }

        public void Unir(int i, int j)
        {
            int raizI = Buscar(i);
            int raizJ = Buscar(j);

            if (raizI != raizJ)
            {
                if (rango[raizI] < rango[raizJ])
                {
                    padre[raizI] = raizJ;
                }
                else if (rango[raizI] > rango[raizJ])
                {
                    padre[raizJ] = raizI;
                }
                else
                {
                    padre[raizJ] = raizI;
                    rango[raizI]++;
                }
            }
        }

        public bool EstanConectados(int i, int j)
        {
            return Buscar(i) == Buscar(j);
        }
    }
}