namespace FgcTag.Nucleo.Combate
{
    /// <summary>Entradas de ambos jugadores para un frame: lo único que el rollback necesita enviar por la red.</summary>
    public readonly struct EntradasFrame
    {
        public static readonly EntradasFrame Vacias = new EntradasFrame(BotonesEntrada.Ninguno, BotonesEntrada.Ninguno);

        public readonly BotonesEntrada Jugador1;
        public readonly BotonesEntrada Jugador2;

        public EntradasFrame(BotonesEntrada jugador1, BotonesEntrada jugador2)
        {
            Jugador1 = jugador1;
            Jugador2 = jugador2;
        }
    }
}
