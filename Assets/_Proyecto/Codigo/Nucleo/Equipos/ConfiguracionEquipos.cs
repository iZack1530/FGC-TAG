using System;
using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Equipos
{
    /// <summary>Reglas fijas de un combate por equipos: los seis luchadores, las reglas del duelo y las del tag.</summary>
    public readonly struct ConfiguracionEquipos
    {
        public readonly ParametrosEquipo Equipo1;
        public readonly ParametrosEquipo Equipo2;

        /// <summary>
        /// Reglas del duelo en pantalla (escenario, daño, juggle, reloj). Sus luchadores son solo
        /// un valor inicial: cada frame se sustituyen por los activos con <see cref="DueloEntre"/>.
        /// </summary>
        public readonly ConfiguracionCombate Reglas;

        /// <summary>Frames que un equipo espera tras un cambio antes de poder cambiar otra vez.</summary>
        public readonly int FramesEsperaCambio;

        /// <summary>Frames entre un K.O. y la entrada del siguiente luchador del equipo.</summary>
        public readonly int FramesPausaNocaut;

        /// <summary>Distancia al rival, hacia el lado de su equipo, a la que aparece el luchador que releva a un noqueado.</summary>
        public readonly Fijo DistanciaEntrada;

        /// <summary>
        /// Frames que hay que mantener un botón de asistencia para cambiar de luchador. Soltarlo
        /// antes llama al compañero como asistente.
        /// </summary>
        public readonly int FramesMantenerParaCambiar;

        /// <summary>Frames que tarda el asistente en llegar antes de empezar su ataque.</summary>
        public readonly int FramesLlegadaAsistencia;

        /// <summary>Frames que un equipo espera tras llamar a una asistencia antes de poder llamar a otra.</summary>
        public readonly int FramesEsperaAsistencia;

        /// <summary>Frames que dura el salto con el que un luchador entra en pantalla.</summary>
        public readonly int FramesEntrada;

        /// <summary>Dónde empieza el salto de entrada respecto a su destino: X hacia atrás e Y de altura.</summary>
        public readonly VectorFijo DesplazamientoEntrada;

        /// <summary>Frames durante los que se ve salir al luchador que deja la pantalla.</summary>
        public readonly int FramesSalida;

        /// <summary>Velocidad por frame del salto de salida: X hacia atrás e Y hacia arriba.</summary>
        public readonly VectorFijo VelocidadSalida;

        /// <summary>Frames que el asistente permanece en pantalla, vulnerable, después de terminar su ataque.</summary>
        public readonly int FramesRetiradaAsistencia;

        /// <summary>Distancia por delante del luchador activo a la que aparece el asistente.</summary>
        public readonly Fijo DistanciaAsistencia;

        /// <summary>Porcentaje del daño normal que recibe un asistente golpeado; por ejemplo 150 es un 50 % más.</summary>
        public readonly int PorcentajeDanioAAsistentes;

        public ConfiguracionEquipos(
            ParametrosEquipo equipo1,
            ParametrosEquipo equipo2,
            ConfiguracionCombate reglas,
            int framesEsperaCambio,
            int framesPausaNocaut,
            Fijo distanciaEntrada,
            int framesMantenerParaCambiar,
            int framesLlegadaAsistencia,
            int framesEsperaAsistencia,
            int framesEntrada,
            VectorFijo desplazamientoEntrada,
            int framesSalida,
            VectorFijo velocidadSalida,
            int framesRetiradaAsistencia,
            Fijo distanciaAsistencia,
            int porcentajeDanioAAsistentes)
        {
            if (framesRetiradaAsistencia < 0 || distanciaAsistencia < Fijo.Cero || porcentajeDanioAAsistentes < 100)
            {
                throw new ArgumentOutOfRangeException(nameof(framesRetiradaAsistencia), "La retirada y la distancia del asistente no pueden ser negativas, y el asistente recibe al menos el 100 % del daño.");
            }

            if (framesEntrada < 1 || framesSalida < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(framesEntrada), "Los saltos de entrada y de salida duran al menos 1 frame.");
            }

            if (desplazamientoEntrada.X < Fijo.Cero || desplazamientoEntrada.Y <= Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(desplazamientoEntrada), "Quien entra empieza detrás de su destino y por encima del suelo.");
            }

            if (framesMantenerParaCambiar < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(framesMantenerParaCambiar), "Hay que mantener el botón al menos 2 frames para distinguirlo de una pulsación.");
            }

            if (framesLlegadaAsistencia < 0 || framesEsperaAsistencia < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(framesLlegadaAsistencia), "Los tiempos de la asistencia no pueden ser negativos.");
            }

            if (framesEsperaCambio < 0 || framesPausaNocaut < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(framesPausaNocaut), "La espera de cambio no puede ser negativa y la pausa de K.O. dura al menos 1 frame.");
            }

            if (distanciaEntrada <= Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(distanciaEntrada), "La distancia de entrada debe ser positiva.");
            }

            Equipo1 = equipo1;
            Equipo2 = equipo2;
            Reglas = reglas;
            FramesEsperaCambio = framesEsperaCambio;
            FramesPausaNocaut = framesPausaNocaut;
            DistanciaEntrada = distanciaEntrada;
            FramesMantenerParaCambiar = framesMantenerParaCambiar;
            FramesLlegadaAsistencia = framesLlegadaAsistencia;
            FramesEsperaAsistencia = framesEsperaAsistencia;
            FramesEntrada = framesEntrada;
            DesplazamientoEntrada = desplazamientoEntrada;
            FramesSalida = framesSalida;
            VelocidadSalida = velocidadSalida;
            FramesRetiradaAsistencia = framesRetiradaAsistencia;
            DistanciaAsistencia = distanciaAsistencia;
            PorcentajeDanioAAsistentes = porcentajeDanioAAsistentes;
        }

        /// <summary>Configuración del duelo entre los luchadores que cada equipo tiene en pantalla.</summary>
        public ConfiguracionCombate DueloEntre(int indiceActivo1, int indiceActivo2) =>
            Reglas.ConLuchadores(Equipo1.Obtener(indiceActivo1), Equipo2.Obtener(indiceActivo2));
    }
}
