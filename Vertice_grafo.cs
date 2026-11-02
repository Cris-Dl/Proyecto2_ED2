namespace LogisticaEnvios
{
    public class Vertice
    {
        public int Indice;
        public string Nombre;
        public Arista Rutas;
 
        public Vertice(string nombre, int indice)
        {
            Nombre = nombre;
            Indice = indice;
        }
    }
}
