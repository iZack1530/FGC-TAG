using FgcTag.Nucleo.Combate;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace FgcTag.Entrada
{
    /// <summary>
    /// Traduce teclado y mandos a <see cref="BotonesEntrada"/>. Se consulta el estado del
    /// dispositivo directamente para no depender de eventos ni de assets de acciones.
    /// </summary>
    /// <remarks>
    /// Jugador 1: WASD para moverse; U, I, O = Débil, Medio, Fuerte; J = Especial; K y L = asistencias.
    /// Jugador 2: flechas; teclado numérico 4, 5, 6 = Débil, Medio, Fuerte; 1 = Especial; 2 y 3 = asistencias.
    /// Mandos: el primero es del jugador 1 y el segundo del jugador 2 (cuadrado/X = Débil,
    /// triángulo/Y = Medio, círculo/B = Fuerte, equis/A = Especial, L1 y R1 = asistencias).
    /// </remarks>
    public static class LectorEntradas
    {
        private const float UmbralPalanca = 0.5f;

        public static BotonesEntrada LeerJugador1() => LeerTecladoJugador1(Keyboard.current) | LeerMando(MandoEnIndice(0));

        public static BotonesEntrada LeerJugador2() => LeerTecladoJugador2(Keyboard.current) | LeerMando(MandoEnIndice(1));

        private static Gamepad MandoEnIndice(int indice) => indice < Gamepad.all.Count ? Gamepad.all[indice] : null;

        private static BotonesEntrada LeerTecladoJugador1(Keyboard teclado)
        {
            if (teclado == null)
            {
                return BotonesEntrada.Ninguno;
            }

            return Si(teclado.wKey, BotonesEntrada.Arriba)
                | Si(teclado.sKey, BotonesEntrada.Abajo)
                | Si(teclado.aKey, BotonesEntrada.Izquierda)
                | Si(teclado.dKey, BotonesEntrada.Derecha)
                | Si(teclado.uKey, BotonesEntrada.Debil)
                | Si(teclado.iKey, BotonesEntrada.Medio)
                | Si(teclado.oKey, BotonesEntrada.Fuerte)
                | Si(teclado.jKey, BotonesEntrada.Especial)
                | Si(teclado.kKey, BotonesEntrada.Asistencia1)
                | Si(teclado.lKey, BotonesEntrada.Asistencia2);
        }

        private static BotonesEntrada LeerTecladoJugador2(Keyboard teclado)
        {
            if (teclado == null)
            {
                return BotonesEntrada.Ninguno;
            }

            return Si(teclado.upArrowKey, BotonesEntrada.Arriba)
                | Si(teclado.downArrowKey, BotonesEntrada.Abajo)
                | Si(teclado.leftArrowKey, BotonesEntrada.Izquierda)
                | Si(teclado.rightArrowKey, BotonesEntrada.Derecha)
                | Si(teclado.numpad4Key, BotonesEntrada.Debil)
                | Si(teclado.numpad5Key, BotonesEntrada.Medio)
                | Si(teclado.numpad6Key, BotonesEntrada.Fuerte)
                | Si(teclado.numpad1Key, BotonesEntrada.Especial)
                | Si(teclado.numpad2Key, BotonesEntrada.Asistencia1)
                | Si(teclado.numpad3Key, BotonesEntrada.Asistencia2);
        }

        private static BotonesEntrada LeerMando(Gamepad mando)
        {
            if (mando == null)
            {
                return BotonesEntrada.Ninguno;
            }

            var palanca = mando.leftStick.ReadValue();

            return Si(mando.dpad.up.isPressed || palanca.y > UmbralPalanca, BotonesEntrada.Arriba)
                | Si(mando.dpad.down.isPressed || palanca.y < -UmbralPalanca, BotonesEntrada.Abajo)
                | Si(mando.dpad.left.isPressed || palanca.x < -UmbralPalanca, BotonesEntrada.Izquierda)
                | Si(mando.dpad.right.isPressed || palanca.x > UmbralPalanca, BotonesEntrada.Derecha)
                | Si(mando.buttonWest, BotonesEntrada.Debil)
                | Si(mando.buttonNorth, BotonesEntrada.Medio)
                | Si(mando.buttonEast, BotonesEntrada.Fuerte)
                | Si(mando.buttonSouth, BotonesEntrada.Especial)
                | Si(mando.leftShoulder, BotonesEntrada.Asistencia1)
                | Si(mando.rightShoulder, BotonesEntrada.Asistencia2);
        }

        private static BotonesEntrada Si(ButtonControl control, BotonesEntrada boton) => Si(control.isPressed, boton);

        private static BotonesEntrada Si(bool presionado, BotonesEntrada boton) => presionado ? boton : BotonesEntrada.Ninguno;
    }
}
