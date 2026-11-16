# FGC-TAG

Juego de peleas 2.5D por equipos (3v3 con tag) hecho en Unity, con estética de cómic pintado a mano y animación en pasos sobre una lógica determinista a 60 Hz.

Proyecto de fans no oficial y sin fines de lucro. Sin relación con Nintendo, Game Freak, Creatures ni The Pokémon Company. No se usan assets extraídos de juegos oficiales.

El diseño completo, las decisiones y la hoja de ruta viven en el **Documento Maestro** del proyecto. Este README solo cubre cómo trabajar en el repositorio.

## Requisitos

El proyecto funciona igual en Linux y en Windows.

- Unity **6000.6.8f1** (Unity 6.6), instalado desde Unity Hub. Se migrará a la 6.7 LTS cuando esté disponible.
- Git y **Git LFS**. En Linux Mint o Ubuntu: `sudo apt install git git-lfs`. En Windows: el instalador de Git ya incluye LFS. Después, en ambos sistemas: `git lfs install` (una vez por equipo).
- SDK de **.NET 8**: lo necesitan VS Code para entender el C# del proyecto y las pruebas fuera de Unity. En Linux Mint o Ubuntu: `sudo apt install dotnet-sdk-8.0`.

## Primera apertura del proyecto

El repositorio trae el código y la configuración de ensamblados; Unity genera el resto la primera vez.

1. Clona el repositorio: `git clone https://github.com/iZack1530/FGC-TAG.git`.
2. En Unity Hub: **Agregar → Agregar proyecto desde el disco** y elige la carpeta clonada. Unity creará `Packages/` y completará `ProjectSettings/`.
3. En **Window → Package Manager → Unity Registry**, instala **Universal RP**, **Input System** y **Visual Studio Editor** (integración con VS Code), y confirma que **Test Framework** está instalado.
4. En **Edit → Project Settings → Player → Other Settings → Configuration**, cambia **Active Input Handling** a **Input System Package (New)** y acepta reiniciar el editor. El proyecto solo usa el sistema nuevo; con *Input Manager (Old)* o *Both*, Unity muestra un aviso de obsolescencia en la consola.
5. En **Edit → Project Settings → Editor**, verifica *Asset Serialization: Force Text* y *Version Control: Visible Meta Files*.
6. Abre **Window → General → Test Runner → EditMode** y ejecuta las pruebas de `FgcTag.Pruebas.Nucleo`; deben pasar todas.
7. Sube a Git `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/` y todos los archivos `.meta` generados.

Si Unity Hub no acepta la carpeta, crea un proyecto nuevo con la plantilla **Universal 3D** en otra ubicación y copia sus carpetas `Packages/` y `ProjectSettings/` dentro del repositorio, conservando `ProjectSettings/ProjectVersion.txt`.

## Editor de código: VS Code

El reparto de trabajo es: **VS Code** para leer y editar código y para Git; **Unity** para compilar, probar y jugar. La consola de Unity solo muestra mensajes y errores; no sube nada a GitHub.

### Configuración (una vez)

1. En VS Code, instala las extensiones **Unity** y **C# Dev Kit**, ambas de Microsoft.
2. En Unity: **Edit → Preferences → External Tools → External Script Editor** → elige **Visual Studio Code**. Si no aparece en la lista, usa *Browse* y selecciona el ejecutable (en Linux suele ser `/usr/bin/code` o `/usr/share/code/code`).
3. En esa misma ventana, pulsa **Regenerate project files**.
4. Abre el proyecto desde Unity con **Assets → Open C# Project**. Así VS Code carga los ensamblados del proyecto y ofrece autocompletado y errores en línea.

### Reglas para no romper el proyecto

- **Mover, renombrar o borrar archivos se hace siempre desde la ventana Project de Unity**, nunca desde VS Code ni desde el explorador. Cada archivo tiene un `.meta` con un identificador único; si se separan, Unity pierde las referencias.
- Después de bajar cambios (`git pull`), **abre Unity antes de hacer commit**, para que importe y genere los `.meta` nuevos.
- Antes de subir, revisa en Unity que la **Console** no tenga errores rojos y que las pruebas estén en verde.
- Git se puede usar desde el panel **Source Control** de VS Code (*Commit*, *Sync*, cambio de rama abajo a la izquierda) o desde la terminal; los dos hacen lo mismo.

## Probar el prototipo de cajas grises

1. Crea una escena nueva (**File → New Scene → Basic (URP)**) y guárdala como `Assets/_Proyecto/Escenas/PruebaCajasGrises.unity`.
2. Crea un GameObject vacío llamado `Partida` y agrégale el componente **Arranque Partida Prueba**.
3. Pulsa Play. El componente crea el piso y los dos luchadores, y coloca la cámara.

| Acción | Jugador 1 (teclado) | Jugador 2 (teclado) | Mando |
| --- | --- | --- | --- |
| Moverse | W A S D | Flechas | Cruceta o palanca izquierda |
| Saltar (neutral, adelante, atrás) | W, W + D, W + A | Flecha arriba (+ dirección) | Arriba (+ dirección) |
| Supersalto | S y enseguida W | Flecha abajo y enseguida arriba | Abajo y enseguida arriba |
| Agacharse | S | Flecha abajo | Abajo |
| Golpe débil / medio / fuerte | U / I / O | Num 4 / 5 / 6 | Cuadrado-X / Triángulo-Y / Círculo-B |
| Bloquear de pie, agachado o en el aire | Mantener la dirección que aleja del rival (con abajo para la guardia baja) | Igual | Igual |
| Golpes agachados (bajos) | S + U / I / O | Flecha abajo + Num 4 / 5 / 6 | Abajo + botón de golpe |
| Lanzador (botón Especial, de pie o agachado) | J | Num 1 | Equis-A |
| Ataques aéreos | U / I / O durante un salto | Num 4 / 5 / 6 | Igual que en tierra |
| Dash / dash aéreo (hacia el rival; con atrás, alejándose) | Dos de U / I / O a la vez | Dos de Num 4 / 5 / 6 | Dos botones de golpe |
| Agarre (cerca del rival) | A o D + O | ← o → + Num 6 | Dirección + Círculo-B |
| Asistencia del siguiente compañero / del otro (pulsar y soltar) | K / L | Num 2 / 3 | L1 / R1 |
| Cambiar a ese compañero (mantener) | K / L | Num 2 / 3 | L1 / R1 |
| Pausar / reanudar la simulación | F1 | | |
| Avanzar un frame (en pausa) | F2 | | |
| Empezar un combate nuevo | F3 | | |

El primer mando conectado es del jugador 1 y el segundo del jugador 2. La esquina superior izquierda muestra el frame, la suma de verificación, el estado de cada luchador y la frame data del último ataque del jugador 1.

Un golpe que conecta (golpeado o bloqueado) se cancela en otro más fuerte: Débil → Medio → Fuerte → Lanzador. El lanzador levanta al rival; si golpea, mantener arriba (o arriba y adelante) lo cancela con un salto para seguir el combo en el aire con los ataques aéreos, que también se encadenan de débil a fuerte. Un rival golpeado en el aire cae cada vez más rápido y, al tocar el suelo, queda derribado e invulnerable hasta levantarse. Los ataques pulsados hasta 5 frames antes de poder actuar se guardan en el búfer y salen solos.

La última línea del HUD mide la ventaja en frames del último golpe: cuántos frames antes (+) o después (−) que el rival recupera el control el atacante.

El combate es por equipos de 3: cada jugador tiene un luchador en pantalla y dos en reserva. Por ahora los tres miembros son el mismo luchador de cajas grises, con un tono distinto cada uno.

- **Asistencia:** pulsar y soltar K o L (Num 2 o 3) llama a ese compañero: aparece hasta 0.9 m por delante de ti (sin pasar del rival), hace un único ataque 18 frames después (por ahora el golpe medio; cada luchador tiene su propio ataque de asistencia en `MovimientosLuchador.Asistencia`), se queda 20 frames más y se marcha. Su golpe se puede bloquear y cuenta para el combo. Mientras está en pantalla el rival puede golpearlo: recibe un 50 % más de daño, pierde su ataque y hasta puede caer noqueado, pero todo ese daño es vida azul que recupera en la reserva. Un mismo ataque puede golpear al luchador y a su asistente. Hay que esperar 3 segundos para llamar a otro.
- **Cambio:** mantener K o L unos 10 frames cambia a ese compañero. Con control en tierra, el que sale se marcha saltando hacia atrás y el que entra llega con un salto desde ese mismo lado: mientras salta no tiene control ni puede bloquear, así que el cambio en neutral se puede castigar. Interrumpiendo un golpe en tierra que conectó sin ser bloqueado, quien entra aparece en el sitio con control al instante y puede seguir el combo. Cada equipo espera 2 segundos entre cambios.
- **Vida azul:** la mitad del daño de cada golpe no bloqueado queda como vida recuperable y cada golpe nuevo destruye un cuarto de la acumulada. En pantalla se regenera despacio tras 3 segundos sin recibir golpes; en reserva, tres veces más rápido y sin espera.
- **Relevo:** tras un K.O., el luchador derrotado se queda en el suelo 3.5 segundos; entonces desaparece y el siguiente compañero con vida entra saltando desde el lado de su equipo. Pierde el equipo que se queda sin luchadores; con el tiempo agotado, gana el que conserva más vida total. El combate es a una sola ronda.
- **Agarre:** una dirección (adelante o atrás) más el golpe fuerte, con el rival en tierra a 1.3 m o menos, es un agarre: supera la guardia, está activo en los frames 5 y 6 y tiene muy poco alcance (caja verde). Si atrapa, sujeta al rival, le hace 1000 de daño y lo lanza; con la dirección contraria al rival, intercambia las posiciones y lo lanza hacia el otro lado. Si falla, deja 30 frames al descubierto. No atrapa a un rival en el aire, despegando, aturdido o derribado, ni durante los 8 frames siguientes a un hitstun, un blockstun o un derribo. Un golpe gana a un agarre en el mismo frame y dos agarres simultáneos se rompen. Más lejos, o sin dirección, sale el golpe fuerte de siempre. El código (`DatosAgarre`, `ResolucionAgarres`) sirve también para futuros agarres de comando.
- **Escape de agarre:** el luchador sujeto tiene 9 frames para pulsar una dirección más el golpe fuerte y romper el agarre: los dos se separan sin daño y recuperan el control a la vez. Mantener el botón desde antes no vale. También se rompen dos agarres simultáneos y el agarre que atrapa a quien ya estaba intentando el suyo.
- **Dash:** dos botones de golpe a la vez (sin mantener abajo) hacen un dash hacia el rival o, manteniendo atrás, alejándose. Arranca rápido y se frena solo: recorre 1.6 m en 20 frames (los últimos ya parado) y durante el dash no se puede bloquear. Se cancela con un ataque desde su frame 3, que hereda el impulso, y agachándose o saltando desde el 7.
  - *Wavedash:* dash, abajo para cancelarlo, soltar abajo y dash otra vez. Va a unos 0.135 m/frame (el dash suelto, 0.08) y pasa por una postura desde la que se puede bloquear.
  - *Plink dash:* pulsar un segundo botón de golpe en los 2 primeros frames de un ataque lo convierte en dash. Dash, golpe, segundo botón un frame después: va a unos 0.17 m/frame, pero con un ritmo mucho más exigente y sin poder bloquear.
- **Dash aéreo:** lo mismo durante un salto, una vez por salto y a 0.7 m de altura o más: 14 frames en horizontal sin caer, cancelables con un ataque aéreo que conserva el impulso.
- **Barra de súper:** de 0 a 7 barras por equipo; aún no se gasta. Cada ataque da barra según su daño base: 3 % al ejecutarlo (lo único que da un ataque al aire), 15 % más si conecta o 6 % más si lo bloquean; quien lo recibe gana 12 % y quien lo bloquea, 5 %. La barra pasa al compañero que entra.

La guardia tiene altura. Los golpes agachados son bajos y solo se bloquean agachado; los ataques aéreos son altos y solo se bloquean de pie; los golpes de pie y el lanzador se bloquean de las dos formas. Con la guardia equivocada, el golpe entra. En el aire se bloquea cualquier golpe durante un salto libre, pero no mientras se ataca.

Un rival golpeado en el aire se recupera solo cuando se agota su aturdimiento aéreo (de 20 a 30 frames según el golpe, 2 menos por cada golpe del combo): da un pequeño salto hacia la dirección que mantenga. La recuperación dura 16 frames: en los 8 primeros es invencible y en los 8 restantes puede bloquear, pero no atacar. Si llega antes al suelo, queda derribado.

Saltar hasta 8 frames después de haber estado agachado es un supersalto, el doble de alto que el salto normal. El salto que cancela el lanzador es siempre normal.

Cada luchador tiene 10 000 de vida, con su barra en la parte baja de la pantalla junto al reloj de 99 segundos. El daño de cada golpe de un combo baja 10 puntos porcentuales respecto al anterior, hasta un mínimo del 30%. Un golpe bloqueado hace el 10% de su daño base (daño chip), que nunca noquea. La ronda termina por K.O. o por tiempo; con el tiempo agotado gana quien conserve más vida.

Durante los frames activos del ataque se dibuja en rojo su caja de golpe. Mientras dura el hitstop (4 frames los golpes débiles, 8 los medios y 13 los fuertes y el lanzador), atacante y rival quedan congelados: el atacante se pinta de blanco y el rival de negro.

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

Configura UnityYAMLMerge una vez por equipo, dentro de la carpeta del repositorio. Las rutas son las que usa Unity Hub por defecto; si instalaste el editor en otra carpeta, ajústalas.

Linux:

```
git config merge.unityyamlmerge.name "UnityYAMLMerge"
git config merge.unityyamlmerge.driver "'$HOME/Unity/Hub/Editor/6000.6.8f1/Editor/Data/Tools/UnityYAMLMerge' merge -p %O %B %A %A"
```

Windows:

```
git config merge.unityyamlmerge.name "UnityYAMLMerge"
git config merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.6.8f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A"
```
