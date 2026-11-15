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

        public readonly ParametrosSalto Salto;
        public readonly MedidasCajaDanio MedidasDanio;

        /// <summary>Ataque del botón Débil en tierra.</summary>
        public readonly DatosMovimiento GolpeDebil;

        /// <summary>Ataque del botón Medio en tierra.</summary>
        public readonly DatosMovimiento GolpeMedio;

        /// <summary>Ataque del botón Fuerte en tierra.</summary>
        public readonly DatosMovimiento GolpeFuerte;

        public ParametrosLuchador(
            Fijo velocidadAdelante,
            Fijo velocidadAtras,
            Fijo medioAnchoEmpuje,
            Fijo medioAltoEmpuje,
            Fijo medioAnchoEmpujeAire,
            Fijo medioAltoEmpujeAire,
            ParametrosSalto salto,
            MedidasCajaDanio medidasDanio,
            DatosMovimiento golpeDebil,
            DatosMovimiento golpeMedio,
            DatosMovimiento golpeFuerte)
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

            VelocidadAdelante = velocidadAdelante;
            VelocidadAtras = velocidadAtras;
            MedioAnchoEmpuje = medioAnchoEmpuje;
            MedioAltoEmpuje = medioAltoEmpuje;
            MedioAnchoEmpujeAire = medioAnchoEmpujeAire;
            MedioAltoEmpujeAire = medioAltoEmpujeAire;
            Salto = salto;
            MedidasDanio = medidasDanio;
            GolpeDebil = golpeDebil;
            GolpeMedio = golpeMedio;
            GolpeFuerte = golpeFuerte;
        }

        /// <summary>Frame data del ataque indicado.</summary>
        public DatosMovimiento Movimiento(TipoAtaque ataque)
        {
            switch (ataque)
            {
                case TipoAtaque.Debil:
                    return GolpeDebil;
                case TipoAtaque.Medio:
                    return GolpeMedio;
                case TipoAtaque.Fuerte:
                    return GolpeFuerte;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ataque), "No hay frame data para un ataque vacío.");
            }
        }

        /// <summary>Medio ancho más grande entre tierra y aire; el escenario debe caber dos de cada lado.</summary>
        public Fijo MedioAnchoMaximo => Fijo.Max(MedioAnchoEmpuje, MedioAnchoEmpujeAire);

        /// <summary>Caja de empuje apoyada en los pies del luchador, según esté en el suelo o en el aire.</summary>
        public CajaColision CajaEmpuje(in EstadoLuchador luchador)
        {
            var enAire = luchador.Accion == AccionLuchador.EnAire;
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
            if (luchador.Accion != AccionLuchador.Atacando)
            {
                return null;
            }

            var movimiento = Movimiento(luchador.Ataque);

            if (!movimiento.EstaActivo(luchador.FramesEnAccion))
            {
                return null;
            }

            return movimiento.CajaGolpe.Orientada(luchador.MiraDerecha).Desplazada(luchador.Posicion);
        }
    }
}
