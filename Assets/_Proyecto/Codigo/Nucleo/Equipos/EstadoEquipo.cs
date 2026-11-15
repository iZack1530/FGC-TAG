using System;
using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Simulacion;

namespace FgcTag.Nucleo.Equipos
{
    /// <summary>
    /// Lo que cambia de un equipo entre frames, aparte del luchador que está en pantalla: quién
    /// es ese luchador, la vida de los tres y la espera para volver a cambiar.
    /// </summary>
    /// <remarks>
    /// La vida del luchador activo vive en su <see cref="EstadoLuchador"/>; su casilla aquí solo se
    /// actualiza cuando sale de la pantalla. Para leer la vida de cualquier miembro se usa
    /// <see cref="VidaDe"/>.
    /// </remarks>
    public readonly struct EstadoEquipo
    {
        public const int Miembros = ConstantesSimulacion.PersonajesPorEquipo;

        /// <summary>Posición en el equipo, de 0 a 2, del luchador que está en pantalla.</summary>
        public readonly int IndiceActivo;

        public readonly int VidaPrimero;
        public readonly int VidaSegundo;
        public readonly int VidaTercero;

        /// <summary>Frames que faltan para poder cambiar de luchador otra vez.</summary>
        public readonly int FramesEsperaCambio;

        public EstadoEquipo(int indiceActivo, int vidaPrimero, int vidaSegundo, int vidaTercero, int framesEsperaCambio)
        {
            if (indiceActivo < 0 || indiceActivo >= Miembros)
            {
                throw new ArgumentOutOfRangeException(nameof(indiceActivo), "El luchador activo es uno de los tres del equipo.");
            }

            IndiceActivo = indiceActivo;
            VidaPrimero = vidaPrimero;
            VidaSegundo = vidaSegundo;
            VidaTercero = vidaTercero;
            FramesEsperaCambio = framesEsperaCambio;
        }

        /// <summary>Equipo al empezar la partida: el primero en pantalla y todos con la vida llena.</summary>
        public static EstadoEquipo Completo(in ParametrosEquipo parametros) =>
            new EstadoEquipo(0, parametros.Primero.VidaMaxima, parametros.Segundo.VidaMaxima, parametros.Tercero.VidaMaxima, 0);

        /// <summary>Vida guardada de un miembro; la del activo puede estar desfasada (ver <see cref="VidaDe"/>).</summary>
        public int VidaEnReserva(int indice)
        {
            switch (indice)
            {
                case 0:
                    return VidaPrimero;
                case 1:
                    return VidaSegundo;
                case 2:
                    return VidaTercero;
                default:
                    throw new ArgumentOutOfRangeException(nameof(indice), "Un equipo tiene tres luchadores, con índices de 0 a 2.");
            }
        }

        /// <summary>Vida real de un miembro: la del luchador en pantalla si es el activo, o la guardada si está en reserva.</summary>
        public int VidaDe(int indice, in EstadoLuchador activo) =>
            indice == IndiceActivo ? activo.Vida : VidaEnReserva(indice);

        public int VidaTotal(in EstadoLuchador activo) =>
            VidaDe(0, activo) + VidaDe(1, activo) + VidaDe(2, activo);

        /// <summary>Compañero que sigue al activo en el orden del equipo, saltando <paramref name="pasos"/> puestos (1 o 2).</summary>
        public int IndiceDelCompanero(int pasos) => (IndiceActivo + pasos) % Miembros;

        /// <summary>Primer compañero con vida siguiendo el orden del equipo, o -1 si no queda ninguno.</summary>
        public int SiguienteConVida()
        {
            for (var pasos = 1; pasos < Miembros; pasos++)
            {
                var indice = IndiceDelCompanero(pasos);

                if (VidaEnReserva(indice) > 0)
                {
                    return indice;
                }
            }

            return -1;
        }

        public EstadoEquipo ConVidaEnReserva(int indice, int vida) =>
            new EstadoEquipo(
                IndiceActivo,
                indice == 0 ? vida : VidaPrimero,
                indice == 1 ? vida : VidaSegundo,
                indice == 2 ? vida : VidaTercero,
                FramesEsperaCambio);

        public EstadoEquipo ConActivo(int indice) =>
            new EstadoEquipo(indice, VidaPrimero, VidaSegundo, VidaTercero, FramesEsperaCambio);

        public EstadoEquipo ConEsperaCambio(int frames) =>
            new EstadoEquipo(IndiceActivo, VidaPrimero, VidaSegundo, VidaTercero, frames);

        public SumaVerificacion AgregarA(SumaVerificacion suma) =>
            suma.Agregar(IndiceActivo)
                .Agregar(VidaPrimero)
                .Agregar(VidaSegundo)
                .Agregar(VidaTercero)
                .Agregar(FramesEsperaCambio);
    }
}
