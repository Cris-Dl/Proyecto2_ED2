namespace LogisticaEnvios
{
    public class Arista
    {
        public int Destino;
        public double Costo;
        public double Tiempo;
        public Arista Siguiente;

        public Arista(int destino, double costo, double tiempo)
        {
            Destino = destino;
            Costo = costo;
            Tiempo = tiempo;
            Siguiente = null;
        }
    }
}