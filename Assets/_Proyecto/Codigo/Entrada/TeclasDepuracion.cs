using UnityEngine.InputSystem;

namespace FgcTag.Entrada
{
    /// <summary>
    /// Teclas de las herramientas de depuración del prototipo. No llegan a la simulación:
    /// solo controlan cómo avanza el bucle de 60 Hz.
    /// </summary>
    /// <remarks>
    /// F1 pausa o reanuda la simulación; F2, en pausa, avanza exactamente un frame; F3 empieza
    /// una ronda nueva.
    /// </remarks>
    public static class TeclasDepuracion
    {
        public static bool AlternarPausaPulsada() => Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame;

        public static bool ReiniciarRondaPulsado() => Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame;

        public static bool AvanzarFramePulsado() => Keyboard.current != null && Keyboard.current.f2Key.wasPressedThisFrame;
    }
}
