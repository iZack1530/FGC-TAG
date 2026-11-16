using System;
using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Simulacion;

namespace FgcTag.Nucleo.Equipos
{
    /// <summary>Vida de un miembro del equipo mientras no está en pantalla.</summary>
    public readonly struct VidaEnReserva
    {
        public readonly int Vida;

        /// <summary>Vida azul: en reserva se regenera sola hasta agotarse.</summary>
        public readonly int Recuperable;

        public VidaEnReserva(int vida, int recuperable)
        {
            Vida = vida;
            Recuperable = recuperable;
        }

        public SumaVerificacion AgregarA(SumaVerificacion suma) => suma.Agregar(Vida).Agregar(Recuperable);
    }

    /// <summary>
    /// Lo que cambia de un equipo entre frames, aparte del luchador que está en pantalla: quién
    /// es ese luchador, la vida de los tres, las esperas de cambio y asistencia, cuánto lleva
    /// mantenido cada botón de equipo y el asistente que esté en pantalla.
    /// </summary>
    /// <remarks>
    /// La vida del luchador activo vive en su <see cref="EstadoLuchador"/>; su casilla aquí solo se
    /// actualiza cuando sale de la pantalla. Para leer la vida de cualquier miembro se usa
    /// <see cref="VidaDe"/>.
    /// </remarks>
    public readonly struct EstadoEquipo
    {
        public const int Miembros = ConstantesSimulacion.PersonajesPorEquipo;

        /// <summary>Valor de un contador de botón mantenido cuando esa pulsación ya se usó para un cambio.</summary>
        public const int PulsacionConsumida = -1;

        /// <summary>Posición en el equipo, de 0 a 2, del luchador que está en pantalla.</summary>
        public readonly int IndiceActivo;

        public readonly VidaEnReserva Primero;
        public readonly VidaEnReserva Segundo;
        public readonly VidaEnReserva Tercero;

        /// <summary>Frames que faltan para poder cambiar de luchador otra vez.</summary>
        public readonly int FramesEsperaCambio;

        /// <summary>Frames que lleva mantenido el botón Asistencia 1; 0 si está suelto.</summary>
        public readonly int FramesMantenido1;

        /// <summary>Frames que lleva mantenido el botón Asistencia 2; 0 si está suelto.</summary>
        public readonly int FramesMantenido2;

        /// <summary>Frames que faltan para poder llamar a otra asistencia.</summary>
        public readonly int FramesEsperaAsistencia;

        public readonly EstadoAsistente Asistente;

        /// <summary>Compañero que está abandonando la pantalla tras un cambio, si lo hay.</summary>
        public readonly EstadoSaliente Saliente;

        public EstadoEquipo(
            int indiceActivo,
            VidaEnReserva primero,
            VidaEnReserva segundo,
            VidaEnReserva tercero,
            int framesEsperaCambio,
            int framesMantenido1,
            int framesMantenido2,
            int framesEsperaAsistencia,
            EstadoAsistente asistente,
            EstadoSaliente saliente)
        {
            if (indiceActivo < 0 || indiceActivo >= Miembros)
            {
                throw new ArgumentOutOfRangeException(nameof(indiceActivo), "El luchador activo es uno de los tres del equipo.");
            }

            IndiceActivo = indiceActivo;
            Primero = primero;
            Segundo = segundo;
            Tercero = tercero;
            FramesEsperaCambio = framesEsperaCambio;
            FramesMantenido1 = framesMantenido1;
            FramesMantenido2 = framesMantenido2;
            FramesEsperaAsistencia = framesEsperaAsistencia;
            Asistente = asistente;
            Saliente = saliente;
        }

        /// <summary>Equipo al empezar una ronda: el primero en pantalla y todos con la vida llena.</summary>
        public static EstadoEquipo Completo(in ParametrosEquipo parametros) =>
            new EstadoEquipo(
                0,
                new VidaEnReserva(parametros.Primero.VidaMaxima, 0),
                new VidaEnReserva(parametros.Segundo.VidaMaxima, 0),
                new VidaEnReserva(parametros.Tercero.VidaMaxima, 0),
                0,
                0,
                0,
                0,
                EstadoAsistente.Ninguno,
                EstadoSaliente.Ninguno);

        /// <summary>Vida guardada de un miembro; la del activo puede estar desfasada (ver <see cref="VidaDe"/>).</summary>
        public VidaEnReserva Reserva(int indice)
        {
            switch (indice)
            {
                case 0:
                    return Primero;
                case 1:
                    return Segundo;
                case 2:
                    return Tercero;
                default:
                    throw new ArgumentOutOfRangeException(nameof(indice), "Un equipo tiene tres luchadores, con índices de 0 a 2.");
            }
        }

        public int VidaEnReserva(int indice) => Reserva(indice).Vida;

        /// <summary>Vida real de un miembro: la del luchador en pantalla si es el activo, o la guardada si está en reserva.</summary>
        public int VidaDe(int indice, in EstadoLuchador activo) =>
            indice == IndiceActivo ? activo.Vida : Reserva(indice).Vida;

        /// <summary>Vida recuperable de un miembro, esté en pantalla o en reserva.</summary>
        public int RecuperableDe(int indice, in EstadoLuchador activo) =>
            indice == IndiceActivo ? activo.VidaRecuperable : Reserva(indice).Recuperable;

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

                if (Reserva(indice).Vida > 0)
                {
                    return indice;
                }
            }

            return -1;
        }

        public EstadoEquipo ConReserva(int indice, VidaEnReserva reserva) =>
            new EstadoEquipo(
                IndiceActivo,
                indice == 0 ? reserva : Primero,
                indice == 1 ? reserva : Segundo,
                indice == 2 ? reserva : Tercero,
                FramesEsperaCambio,
                FramesMantenido1,
                FramesMantenido2,
                FramesEsperaAsistencia,
                Asistente,
                Saliente);

        /// <summary>Cambia la vida guardada de un miembro conservando su vida recuperable; con 0 de vida no queda nada que recuperar.</summary>
        public EstadoEquipo ConVidaEnReserva(int indice, int vida) =>
            ConReserva(indice, new VidaEnReserva(vida, vida > 0 ? Reserva(indice).Recuperable : 0));

        public EstadoEquipo ConActivo(int indice) =>
            new EstadoEquipo(indice, Primero, Segundo, Tercero, FramesEsperaCambio, FramesMantenido1, FramesMantenido2, FramesEsperaAsistencia, Asistente, Saliente);

        public EstadoEquipo ConEsperaCambio(int frames) =>
            new EstadoEquipo(IndiceActivo, Primero, Segundo, Tercero, frames, FramesMantenido1, FramesMantenido2, FramesEsperaAsistencia, Asistente, Saliente);

        public EstadoEquipo ConBotonesMantenidos(int frames1, int frames2) =>
            new EstadoEquipo(IndiceActivo, Primero, Segundo, Tercero, FramesEsperaCambio, frames1, frames2, FramesEsperaAsistencia, Asistente, Saliente);

        public EstadoEquipo ConEsperaAsistencia(int frames) =>
            new EstadoEquipo(IndiceActivo, Primero, Segundo, Tercero, FramesEsperaCambio, FramesMantenido1, FramesMantenido2, frames, Asistente, Saliente);

        public EstadoEquipo ConAsistente(EstadoAsistente asistente) =>
            new EstadoEquipo(IndiceActivo, Primero, Segundo, Tercero, FramesEsperaCambio, FramesMantenido1, FramesMantenido2, FramesEsperaAsistencia, asistente, Saliente);

        public EstadoEquipo ConSaliente(EstadoSaliente saliente) =>
            new EstadoEquipo(IndiceActivo, Primero, Segundo, Tercero, FramesEsperaCambio, FramesMantenido1, FramesMantenido2, FramesEsperaAsistencia, Asistente, saliente);

        public SumaVerificacion AgregarA(SumaVerificacion suma) =>
            Saliente.AgregarA(Asistente.AgregarA(
                Tercero.AgregarA(Segundo.AgregarA(Primero.AgregarA(suma.Agregar(IndiceActivo))))
                    .Agregar(FramesEsperaCambio)
                    .Agregar(FramesMantenido1)
                    .Agregar(FramesMantenido2)
                    .Agregar(FramesEsperaAsistencia)));
    }
}
