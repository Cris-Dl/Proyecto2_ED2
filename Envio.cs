namespace LogisticaEnvios
{
    public class Envio
    {
        public string CodigoRastreo;
        public string Remitente;
        public string Destinatario;
        public string CiudadOrigen;
        public string CiudadDestino;

        public Envio(string codigo, string remitente, string destinatario, string origen, string destino)
        {
            CodigoRastreo = codigo;
            Remitente = remitente;
            Destinatario = destinatario;
            CiudadOrigen = origen;
            CiudadDestino = destino;
        }

        public override string ToString()
        {
            return $"[{CodigoRastreo}] De: {Remitente} ({CiudadOrigen}) -> Para: {Destinatario} ({CiudadDestino})";
        }
    }
}