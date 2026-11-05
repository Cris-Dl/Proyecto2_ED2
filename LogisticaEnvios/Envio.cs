namespace LogisticaEnvios
{
    public class Envio
    {
        public string CodigoRastreo { get; set; }
        public string Remitente { get; set; }
        public string Destinatario { get; set; }
        public string CiudadOrigen { get; set; }
        public string CiudadDestino { get; set; }

        public string Prioridad { get; set; }

        public Envio(string codigoRastreo, string remitente, string destinatario, string ciudadOrigen, string ciudadDestino, string prioridad = "Costo")
        {
            CodigoRastreo = codigoRastreo;
            Remitente = remitente;
            Destinatario = destinatario;
            CiudadOrigen = ciudadOrigen;
            CiudadDestino = ciudadDestino;
            Prioridad = prioridad;
        }
    }
}