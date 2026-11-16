using System;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Barra de súper del equipo, que lleva el luchador en pantalla. Se llena con cada ataque:
    /// un poco al ejecutarlo (aunque falle), más al conectarlo o recibirlo, y menos si se bloquea.
    /// Cada ganancia es un porcentaje del daño base del movimiento.
    /// </summary>
    public readonly struct ReglasBarra
    {
        /// <summary>Unidades que forman una barra completa.</summary>
        public readonly int UnidadesPorBarra;

        /// <summary>Barras que se pueden acumular como máximo.</summary>
        public readonly int BarrasMaximas;

        /// <summary>Lo gana quien ejecuta un ataque, conecte o no: es la ganancia de un ataque al aire.</summary>
        public readonly int PorcentajeAlEjecutar;

        /// <summary>Lo gana, además, el atacante cuyo golpe conecta sin ser bloqueado.</summary>
        public readonly int PorcentajeAlConectar;

        /// <summary>Lo gana, además, el atacante cuyo golpe es bloqueado.</summary>
        public readonly int PorcentajeAlSerBloqueado;

        /// <summary>Lo gana quien recibe un golpe sin bloquearlo.</summary>
        public readonly int PorcentajeAlRecibir;

        /// <summary>Lo gana quien bloquea un golpe.</summary>
        public readonly int PorcentajeAlBloquear;

        public ReglasBarra(
            int unidadesPorBarra,
            int barrasMaximas,
            int porcentajeAlEjecutar,
            int porcentajeAlConectar,
            int porcentajeAlSerBloqueado,
            int porcentajeAlRecibir,
            int porcentajeAlBloquear)
        {
            if (unidadesPorBarra < 1 || barrasMaximas < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(unidadesPorBarra), "Hace falta al menos 1 barra de al menos 1 unidad.");
            }

            if (porcentajeAlEjecutar < 0 || porcentajeAlConectar < 0 || porcentajeAlSerBloqueado < 0 || porcentajeAlRecibir < 0 || porcentajeAlBloquear < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(porcentajeAlEjecutar), "Las ganancias de barra no pueden ser negativas.");
            }

            UnidadesPorBarra = unidadesPorBarra;
            BarrasMaximas = barrasMaximas;
            PorcentajeAlEjecutar = porcentajeAlEjecutar;
            PorcentajeAlConectar = porcentajeAlConectar;
            PorcentajeAlSerBloqueado = porcentajeAlSerBloqueado;
            PorcentajeAlRecibir = porcentajeAlRecibir;
            PorcentajeAlBloquear = porcentajeAlBloquear;
        }

        public int Maximo => UnidadesPorBarra * BarrasMaximas;

        /// <summary>Barras completas disponibles con esas unidades.</summary>
        public int BarrasCompletas(int unidades) => unidades / UnidadesPorBarra;

        public int AlEjecutar(int barra, int danioBase) => Sumar(barra, danioBase, PorcentajeAlEjecutar);

        public int AlConectar(int barra, int danioBase) => Sumar(barra, danioBase, PorcentajeAlConectar);

        public int AlSerBloqueado(int barra, int danioBase) => Sumar(barra, danioBase, PorcentajeAlSerBloqueado);

        public int AlRecibir(int barra, int danioBase) => Sumar(barra, danioBase, PorcentajeAlRecibir);

        public int AlBloquear(int barra, int danioBase) => Sumar(barra, danioBase, PorcentajeAlBloquear);

        private int Sumar(int barra, int danioBase, int porcentaje) =>
            Math.Min(barra + danioBase * porcentaje / 100, Maximo);
    }
}
