using System;
using FgcTag.Nucleo.Colisiones;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>Datos fijos de un luchador que la simulación consulta pero nunca modifica.</summary>
    public readonly struct ParametrosLuchador
    {
        /// <summary>Unidades por frame al caminar hacia el rival.</summary>
        public readonly Fijo VelocidadAdelante;

        /// <summary>Unidades por frame al caminar alejándose del rival.</summary>
        public readonly Fijo VelocidadAtras;

        public readonly Fijo MedioAnchoEmpuje;
        public readonly Fijo MedioAltoEmpuje;

        /// <summary>
        /// Caja de empuje en el aire: más estrecha y baja que la de tierra, para que un salto
        /// alto pueda pasar por encima del rival (cross-up).
        /// </summary>
        public readonly Fijo MedioAnchoEmpujeAire;

        public readonly Fijo MedioAltoEmpujeAire;

        /// <summary>Vida con la que el luchador empieza la ronda.</summary>
        public readonly int VidaMaxima;

        public readonly ParametrosSalto Salto;
        public readonly ParametrosDash Dash;
        public readonly MedidasCajaDanio MedidasDanio;

        public readonly MovimientosLuchador Movimientos;

        public ParametrosLuchador(
            int vidaMaxima,
            Fijo velocidadAdelante,
            Fijo velocidadAtras,
            Fijo medioAnchoEmpuje,
            Fijo medioAltoEmpuje,
            Fijo medioAnchoEmpujeAire,
            Fijo medioAltoEmpujeAire,
            ParametrosSalto salto,
            ParametrosDash dash,
            MedidasCajaDanio medidasDanio,
            MovimientosLuchador movimientos)
        {
            if (velocidadAdelante < Fijo.Cero || velocidadAtras < Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(velocidadAdelante), "Las velocidades de caminata no pueden ser negativas.");
            }

            if (medioAnchoEmpuje <= Fijo.Cero || medioAltoEmpuje <= Fijo.Cero ||
                medioAnchoEmpujeAire <= Fijo.Cero || medioAltoEmpujeAire <= Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(medioAnchoEmpuje), "Las cajas de empuje deben tener tamaño positivo.");
            }

            if (vidaMaxima < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(vidaMaxima), "La vida máxima debe ser positiva.");
            }

            VidaMaxima = vidaMaxima;
            VelocidadAdelante = velocidadAdelante;
            VelocidadAtras = velocidadAtras;
            MedioAnchoEmpuje = medioAnchoEmpuje;
            MedioAltoEmpuje = medioAltoEmpuje;
            MedioAnchoEmpujeAire = medioAnchoEmpujeAire;
            MedioAltoEmpujeAire = medioAltoEmpujeAire;
            Salto = salto;
            Dash = dash;
            MedidasDanio = medidasDanio;
            Movimientos = movimientos;
        }

        /// <summary>Frame data del ataque que el luchador está ejecutando, según su postura.</summary>
        public DatosMovimiento MovimientoEnCurso(in EstadoLuchador luchador) =>
            Movimientos.Obtener(luchador.Ataque, EstadosAccion.PosturaDeAtaque(luchador.Accion));

        /// <summary>Medio ancho más grande entre tierra y aire; el escenario debe caber dos de cada lado.</summary>
        /// <summary>
        /// Agarre que está intentando o ejecutando el luchador. Hoy solo existe el agarre normal;
        /// los agarres de comando se elegirán aquí.
        /// </summary>
        public DatosAgarre AgarreEnCurso(in EstadoLuchador luchador) => Movimientos.Agarre;

        /// <summary>Caja de agarre en el escenario durante los frames activos de un intento de agarre.</summary>
        public CajaColision? CajaAgarreActiva(in EstadoLuchador luchador)
        {
            if (luchador.Accion != AccionLuchador.IntentandoAgarre || luchador.EnHitstop)
            {
                return null;
            }

            var agarre = AgarreEnCurso(luchador);

            if (!agarre.EstaActivo(luchador.FramesEnAccion))
            {
                return null;
            }

            return agarre.CajaAgarre.Orientada(luchador.MiraDerecha).Desplazada(luchador.Posicion);
        }

        public Fijo MedioAnchoMaximo => Fijo.Max(MedioAnchoEmpuje, MedioAnchoEmpujeAire);

        /// <summary>Caja de empuje apoyada en los pies del luchador, según esté en el suelo o en el aire.</summary>
        public CajaColision CajaEmpuje(in EstadoLuchador luchador)
        {
            var enAire = EstadosAccion.EstaEnElAire(luchador.Accion);
            var medioAncho = enAire ? MedioAnchoEmpujeAire : MedioAnchoEmpuje;
            var medioAlto = enAire ? MedioAltoEmpujeAire : MedioAltoEmpuje;
            var centro = new VectorFijo(luchador.Posicion.X, luchador.Posicion.Y + medioAlto);

            return new CajaColision(centro, medioAncho, medioAlto);
        }

        /// <summary>Caja de daño (hurtbox) apoyada en los pies del luchador, según su postura.</summary>
        public CajaColision CajaDanio(in EstadoLuchador luchador)
        {
            var medioAlto = MedidasDanio.MedioAlto(luchador.Accion);
            var centro = new VectorFijo(luchador.Posicion.X, luchador.Posicion.Y + medioAlto);

            return new CajaColision(centro, MedidasDanio.MedioAncho, medioAlto);
        }

        /// <summary>
        /// Caja de golpe (hitbox) en el escenario si el luchador está en un frame activo de su
        /// ataque; <c>null</c> en cualquier otro caso. No indica si ya conectó.
        /// </summary>
        public CajaColision? CajaGolpeActiva(in EstadoLuchador luchador)
        {
            if (!EstadosAccion.EstaAtacando(luchador.Accion))
            {
                return null;
            }

            var movimiento = MovimientoEnCurso(luchador);

            if (!movimiento.EstaActivo(luchador.FramesEnAccion))
            {
                return null;
            }

            return movimiento.CajaGolpe.Orientada(luchador.MiraDerecha).Desplazada(luchador.Posicion);
        }
    }
}
