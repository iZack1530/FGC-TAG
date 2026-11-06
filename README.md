# FGC-TAG

Juego de peleas 2.5D por equipos (3v3 con tag) hecho en Unity, con estética de cómic pintado a mano y animación en pasos sobre una lógica determinista a 60 Hz.

Proyecto de fans no oficial y sin fines de lucro. Sin relación con Nintendo, Game Freak, Creatures ni The Pokémon Company. No se usan assets extraídos de juegos oficiales.

El diseño completo, las decisiones y la hoja de ruta viven en el **Documento Maestro** del proyecto. Este README solo cubre cómo trabajar en el repositorio.

## Requisitos

- Unity **6000.6.8f1** (Unity 6.6). Se migrará a la 6.7 LTS cuando esté disponible.
- Git y **Git LFS** (`git lfs install` una vez por equipo).
- Opcional, para correr las pruebas del Núcleo fuera de Unity: SDK de .NET 8.

## Primera apertura del proyecto

El repositorio trae el código y la configuración de ensamblados; Unity genera el resto la primera vez.

1. Clona el repositorio: `git clone https://github.com/iZack1530/FGC-TAG.git`.
2. En Unity Hub: **Agregar → Agregar proyecto desde el disco** y elige la carpeta clonada. Unity creará `Packages/` y completará `ProjectSettings/`.
3. En **Window → Package Manager**, instala **Universal RP** y **Input System**, y confirma que **Test Framework** está instalado. Si Unity pregunta si quieres activar el nuevo sistema de entrada, acepta y deja que se reinicie.
4. En **Edit → Project Settings → Editor**, verifica *Asset Serialization: Force Text* y *Version Control: Visible Meta Files*.
5. Abre **Window → General → Test Runner → EditMode** y ejecuta las pruebas de `FgcTag.Pruebas.Nucleo`; deben pasar todas.
6. Sube a Git `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/` y todos los archivos `.meta` generados.

Si Unity Hub no acepta la carpeta, crea un proyecto nuevo con la plantilla **Universal 3D** en otra ubicación y copia sus carpetas `Packages/` y `ProjectSettings/` dentro del repositorio, conservando `ProjectSettings/ProjectVersion.txt`.

## Probar el prototipo de cajas grises

1. Crea una escena nueva (**File → New Scene → Basic (URP)**) y guárdala como `Assets/_Proyecto/Escenas/PruebaCajasGrises.unity`.
2. Crea un GameObject vacío llamado `Partida` y agrégale el componente **Arranque Partida Prueba**.
3. Pulsa Play. El componente crea el piso y los dos luchadores, y coloca la cámara.

| Acción | Jugador 1 (teclado) | Jugador 2 (teclado) | Mando |
| --- | --- | --- | --- |
| Moverse | W A S D | Flechas | Cruceta o palanca izquierda |
| Agacharse | S | Flecha abajo | Abajo |
| Débil / Medio / Fuerte | U / I / O | Num 4 / 5 / 6 | Cuadrado-X / Triángulo-Y / Círculo-B |
| Especial | J | Num 1 | Equis-A |
| Asistencias | K / L | Num 2 / 3 | L1 / R1 |

El primer mando conectado es del jugador 1 y el segundo del jugador 2. La esquina superior izquierda muestra el frame, la suma de verificación y el estado de cada luchador.

## Estructura

```
Assets/
  _Proyecto/Codigo/
    Nucleo/        Simulación determinista en C# puro (sin UnityEngine)
    Contenido/     Datos de personajes, movimientos y escenarios
    Entrada/       Mando, búfer y detección de movimientos
    Red/           Rollback y sincronización
    Presentacion/  Animación en pasos, render, VFX, cámara y audio
    Interfaz/      Menús, HUD y entrenamiento
    Herramientas/  Herramientas de editor
    Arranque/      Une las capas en escenas jugables (bucle de 60 Hz)
  Pruebas/Nucleo/  Pruebas del Núcleo (Unity Test Runner y CI)
CI/                Proyectos .NET que compilan y prueban el Núcleo sin Unity
```

Cada carpeta de `Codigo/` es un ensamblado (`.asmdef`). Las dependencias solo apuntan hacia el Núcleo y el Núcleo no puede referenciar `UnityEngine`: el compilador impide romper la arquitectura.

## Reglas del código

- Todo en español: identificadores, comentarios y textos de interfaz. Identificadores **sin tildes ni ñ** (`Anio`, `Accion`).
- PascalCase para tipos, métodos, propiedades y constantes; camelCase para variables y parámetros; `_camelCase` para campos privados.
- Nada de `float`, `UnityEngine.Random`, `Time.deltaTime` ni física de Unity dentro del Núcleo: se usa `Fijo` (punto fijo).
- Sin asignaciones de memoria por frame en la simulación.
- Sin código provisional ni comentarios de pendientes en `main`.

## Flujo de trabajo

- `main` siempre estable. Ramas cortas por tarea: `funcion/...`, `correccion/...`, `arte/...`.
- Mensajes de commit en español con prefijo: `funcion:`, `correccion:`, `arte:`, `datos:`, `docs:`, `ci:`.
- Cada push y pull request ejecuta las pruebas del Núcleo en GitHub Actions.

### Pruebas sin Unity

```
dotnet test CI/PruebasNucleo/PruebasNucleo.csproj
```

### Fusión de escenas y prefabs

Configura UnityYAMLMerge una vez por equipo (ajusta la ruta a tu instalación):

```
git config merge.unityyamlmerge.name "UnityYAMLMerge"
git config merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.6.8f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A"
```
