namespace _02.CLIMOV.Modelo
{
    public class ConversionResult
    {
        private float _valorOriginal;
        private float _valorConvertido;

        public float ValorOriginal
        {
            get => (float)Math.Round(_valorOriginal, 2, MidpointRounding.AwayFromZero);
            set => _valorOriginal = (float)Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        public float ValorConvertido
        {
            get => (float)Math.Round(_valorConvertido, 2, MidpointRounding.AwayFromZero);
            set => _valorConvertido = (float)Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        public string UnidadOrigen { get; set; }
        public string UnidadDestino { get; set; }
        public string TipoConversion { get; set; } // Longitud, Temperatura, Peso
        public DateTime FechaConversion { get; set; }
    }

    public enum TipoConversion
    {
        Longitud,
        Temperatura,
        Peso
    }

    public class UnidadConversion
    {
        public string Nombre { get; set; }
        public string Simbolo { get; set; }
        public TipoConversion Tipo { get; set; }
    }
}
