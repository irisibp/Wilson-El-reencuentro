# Wilson: El Reencuentro

Documento de diseño del juego (GDD) y guía del proyecto.
Versión documentada: 1.0 · Actualización: 10 de noviembre de 2026.

Wilson: El Reencuentro es una aventura de plataformas 2D para un jugador. Wilson, una pelota de vóley, debe atravesar cinco escenarios, reunir cocos y superar obstáculos hasta reencontrarse con Chuck en una ciudad costera.

El documento completo también está disponible en [README.txt](README.txt), en texto plano.

## Índice

1. Ficha y especificaciones técnicas
2. Sinopsis y objetivo
3. Cómo se juega y controles
4. Elementos y reglas
5. Los cinco niveles
6. Victoria, derrota y transiciones
7. Flujo de la partida
8. Jugador y propiedades
9. Interfaz y menús
10. Guía de diseño y características clave
11. Abrir, ejecutar y exportar el proyecto
12. Organización y verificación
13. Autoría, recursos, créditos y enlaces

## 1. Ficha y especificaciones técnicas

- Nombre: Wilson: El Reencuentro.
- Género: plataformas, aventura y exploración.
- Modalidad: un jugador, partida local.
- Vista: lateral 2D con cámara que acompaña el recorrido.
- Plataforma de la entrega: PC con Windows de 64 bits.
- Idioma de la interfaz: español.
- Entrada: teclado para jugar; ratón para los botones del menú.
- Motor: Unity 6000.6.3f1.
- Lenguaje: C#.
- Renderizado: Universal Render Pipeline 2D, versión 17.6.0.
- Escenarios: Tilemap, Tile Palette y Rule Tiles de 2D Tilemap Extras.
- Física: Rigidbody2D y Collider2D.
- Interfaz: Unity UI y TextMeshPro.
- Contenido: pantalla de inicio, cinco niveles, pausa, Game Over y cierre narrativo.

La compilación distribuida se ejecuta sin instalar Unity. No se han establecido requisitos mínimos de procesador, memoria o tarjeta gráfica mediante mediciones; la plataforma de entrega es Windows de 64 bits.

## 2. Sinopsis y objetivo

Wilson se encuentra perdido en el mar, separado de Chuck. Su viaje comienza entre arena, corales y plataformas marinas; continúa por una caverna y unas ruinas sumergidas hasta alcanzar la playa. Finalmente llega a una costa poblada donde Chuck lo espera.

El objetivo general es completar los cinco niveles y lograr el reencuentro. En cada escenario hay que reunir los puntos de coco requeridos y alcanzar la salida. El avance combina saltos, búsqueda de rutas y, en los niveles posteriores, control del riesgo y de los tiempos de las plataformas móviles.

## 3. Cómo se juega y controles

1. Seleccionar Jugar en la pantalla de inicio.
2. Mover a Wilson por las superficies y saltar entre plataformas.
3. Recoger los cocos; los especiales aportan más puntos.
4. Evitar trampas y caídas en los niveles con vidas.
5. Activar el checkpoint cuando esté disponible.
6. Alcanzar el objetivo de puntos y entrar en la salida habilitada.
7. Continuar al siguiente escenario; en el quinto, llegar hasta Chuck.

- A o flecha izquierda: moverse a la izquierda.
- D o flecha derecha: moverse a la derecha.
- Barra espaciadora: saltar cuando Wilson está apoyado.
- Esc: abrir o cerrar la pausa durante el juego.
- Botón Pausa (Esc): abrir la pausa con el ratón.
- Clic izquierdo: seleccionar las opciones de los menús.

El juego usa desplazamiento y salto de plataformas incluso en la ambientación submarina: no incorpora un sistema de natación libre ni un medidor de oxígeno.

## 4. Elementos y reglas

### Wilson y Chuck

Wilson es el personaje controlable. Chuck es el objetivo narrativo del recorrido y espera en la plaza final del nivel 5. Los vecinos de la ciudad son decorativos.

### Cocos y puntuación

- Coco común: 1 punto.
- Coco brillante: 3 puntos.
- Coco dorado: 5 puntos.

El contador representa puntos, no cantidad de objetos recogidos. Cada nivel empieza con su propio contador en cero. La salida solo se habilita al alcanzar el objetivo de ese escenario.

En los niveles con reaparición, los puntos obtenidos y los cocos recogidos se conservan al perder una vida. Reiniciar o reintentar el nivel restaura los cocos y comienza el escenario de nuevo.

### Terreno y plataformas

El suelo, los bordes y las plataformas delimitan el recorrido. Las plataformas móviles de los niveles 4 y 5 transportan a Wilson cuando está sobre ellas. Hay movimientos horizontales para cruzar grandes huecos y elevadores para acceder a rutas altas.

### Peligros, vidas y checkpoint

El nivel 1 es introductorio y no tiene configurado el componente de tres vidas de los niveles posteriores. Desde el nivel 2, cada escenario comienza con tres vidas.

El contacto con un peligro o una caída fuera del mapa resta una vida. Mientras quedan vidas, Wilson reaparece en el inicio o en el último checkpoint activado. Existe una protección breve para evitar que un mismo contacto quite varias vidas consecutivas. Al agotar las tres vidas aparece Game Over.

Los checkpoints cambian su indicador al activarse. Su estado se conserva entre reapariciones del mismo intento y se reinicia al volver a empezar el nivel.

### Coco oculto

En la ciudad hay un coco dorado escondido en un balcón del mercado. Es necesario llegar a la zona de descubrimiento para revelar el objeto y después recogerlo. Descubrirlo por sí solo no suma puntos. La exploración de esa ruta forma parte del objetivo final.

## 5. Los cinco niveles

### Nivel 1: Perdido en el mar

- Escena: Nivel01_PerdidoEnElMar.
- Objetivo: 15 puntos de coco.
- Ambiente: recorrido marino con arena, roca, corales y decoración submarina.
- Desafío: aprender a desplazarse, saltar y reconocer los coleccionables.
- Salida: acceso al siguiente escenario al completar la puntuación.

Es la introducción a los controles y a la regla de desbloquear la salida mediante cocos.

### Nivel 2: Caverna de coral

- Escena: Nivel02_CavernaDeCoral.
- Objetivo: 20 puntos.
- Coleccionables: 12 comunes, un brillante y un dorado; 14 objetos.
- Ambiente: caverna marina con plataformas a distintas alturas.
- Desafío: tres corales peligrosos y elección de recorridos elevados.
- Ayuda: un checkpoint y tres vidas.

Introduce el daño, la reaparición y el Game Over dentro de una ambientación más cerrada.

### Nivel 3: Ruinas sumergidas

- Escena: Nivel03_RuinasSumergidas.
- Objetivo: 25 puntos.
- Coleccionables: 17 comunes, un brillante y un dorado; 19 objetos.
- Ambiente: ruinas bajo el mar, arcos y estructuras deterioradas.
- Desafío: huecos entre apoyos, cuatro peligros de coral y plataformas elevadas.
- Ayuda: un checkpoint y tres vidas.

Aumenta la importancia de calcular los saltos y observar la continuidad del suelo.

### Nivel 4: La playa

- Escena: Nivel04_LaPlaya.
- Objetivo: 30 puntos.
- Coleccionables: 22 comunes, un brillante y un dorado; 24 objetos.
- Ambiente: playa con arena, palmeras, horizonte marino y muelles.
- Desafío: cuatro erizos peligrosos y cinco huecos.
- Plataformas móviles: un muelle horizontal y un elevador.
- Ayuda: un checkpoint y tres vidas.

Incluye un salto de tres celdas con aterrizaje más alto y un hueco de seis celdas que exige usar el muelle móvil. La ruta elevada permite alcanzar el coco brillante.

### Nivel 5: Ciudad costera

- Escena: Nivel05_CiudadCostera.
- Objetivo: 35 puntos.
- Coleccionables: 27 comunes, un brillante y un dorado; 29 objetos.
- Ambiente: paseo de playa urbanizado con comercios, vecinos, faroles y plaza.
- Desafío: cinco grupos de pinchos de obras y cinco huecos.
- Plataformas móviles: un muelle horizontal y el elevador del mercado.
- Exploración: coco dorado oculto en el balcón del mercado.
- Ayuda: un checkpoint y tres vidas.
- Final: Chuck espera en la plaza.

El recorrido combina saltos de precisión, un cruce de seis celdas con plataforma móvil y la búsqueda del coco oculto. Tras reunir los 35 puntos y entrar en la zona final, Wilson se acerca a Chuck y se presenta el cierre del juego.

## 6. Victoria, derrota y transiciones

### Victoria de nivel

Se obtiene al reunir la puntuación requerida y tocar la salida habilitada. En los niveles 1 a 4 se muestra la finalización y se puede continuar a la siguiente escena. Al cambiar de nivel se restablecen los estados propios del nuevo escenario.

### Derrota y reintento

Desde el nivel 2, perder las tres vidas produce Game Over con la música OMEN. Reintentar reinicia el nivel actual: restaura vidas, coleccionables, puntuación y checkpoint. No obliga a repetir los niveles anteriores.

### Fin del juego

El nivel 5 es el último. Después del reencuentro aparece el mensaje «¡Wilson y Chuck, juntos otra vez!» y el cierre de la aventura. Se puede volver al inicio o salir del juego. No hay un sexto nivel.

### Progreso

No hay guardado entre sesiones. Jugar desde el menú inicia una partida nueva en el nivel 1. Reiniciar un nivel descarta el progreso de ese intento; volver al inicio también abandona la partida actual.

## 7. Flujo de la partida

Inicio -> Jugar -> Nivel 1 -> Nivel 2 -> Nivel 3 -> Nivel 4 -> Nivel 5 -> Reencuentro -> Fin

Durante un nivel:
- Recolectar -> alcanzar objetivo -> tocar salida -> victoria -> continuar.
- Peligro o caída -> perder una vida -> reaparecer si quedan vidas.
- Cero vidas -> Game Over -> reintentar el nivel, volver al inicio o salir.
- Esc -> pausa -> continuar, reiniciar, volver al inicio o salir.

## 8. Jugador y propiedades

El jugador debe observar las superficies, calcular distancias y coordinar el movimiento con el salto. En las plataformas móviles también debe elegir el momento de abordar y de abandonar el apoyo.

WilsonController gestiona movimiento horizontal, detección de suelo, salto y apoyo en plataformas móviles. Sus valores base son velocidad 6 y velocidad vertical de salto 12, ajustables desde Unity. El movimiento usa física 2D; los valores son unidades del motor, no medidas reales.

PlayerRespawn administra las vidas y el retorno al punto seguro en los niveles correspondientes. El salto requiere apoyo: no hay doble salto, ataque ni sistema de combate en esta versión.

## 9. Interfaz y menús

### Pantalla de inicio

- Jugar: comienza una partida en el nivel 1.
- Controles: muestra las teclas del juego.
- Salir del juego: cierra la aplicación.

### Información durante la partida

La interfaz informa la puntuación y el objetivo del nivel, las vidas cuando corresponden, los mensajes de estado y el acceso a pausa. El checkpoint y la salida ofrecen señales de activación.

### Menú de pausa

- Continuar: vuelve al juego.
- Reiniciar nivel: comienza de nuevo el escenario actual.
- Volver al inicio: regresa a la pantalla de título.
- Salir del juego: cierra completamente el ejecutable.

La pausa congela física, plataformas y audio. Los botones siguen funcionando. No se abre durante la reaparición, Game Over, victoria o secuencia final, para evitar superponer estados.

En una compilación Windows, Salir cierra el juego. Al probar dentro de Unity, detiene Play y mantiene abierto el editor.

## 10. Guía de diseño y características clave

La propuesta visual combina pixel art marino y tropical con escenarios construidos mediante tiles. La progresión pasa de azules submarinos a arena, vegetación y arquitectura costera. Los peligros deben distinguirse de la decoración por sus formas, colores y señales de advertencia.

Los recursos submarinos usan sprites de 32 por 32 píxeles y 32 Pixels Per Unit. Las paletas manual y automática permiten construir mapas coherentes. Los nuevos assets deben respetar la escala, transparencia, bordes y continuidad de los tiles; un personaje como Chuck se importa como sprite independiente.

Los prompts y los criterios de producción se conservan en Documentacion/Submarino y Documentacion/Nivel05. Funcionan como referencia técnica para futuros recursos y permiten registrar cómo se preparó el arte.

La diversión se basa en dominar saltos cada vez más exigentes, descubrir rutas de mayor valor y completar el viaje hacia Chuck. Las características principales son:

- Cinco ambientes conectados por una historia sencilla.
- Objetivos de puntuación progresivos: 15, 20, 25, 30 y 35.
- Cocos con distinto valor y un secreto en el nivel final.
- Checkpoints y tres vidas desde el segundo nivel.
- Peligros, huecos y plataformas móviles en la costa.
- Música retro, efectos de interacción y cierre narrativo.
- Inicio, pausa, reintento y salida del juego.


## 11. Organización y verificación

- wilson/Assets/Scenes/: inicio y escenas jugables.
- wilson/Assets/Scripts/Player/: movimiento del protagonista.
- wilson/Assets/Scripts/Gameplay/: salidas, peligros, vidas, checkpoints, plataformas y final.
- wilson/Assets/Scripts/Collectibles/: cocos y contador.
- wilson/Assets/Scripts/UI/: inicio, pausa y botones.
- wilson/Assets/Scripts/Audio/: configuración y reproducción de sonidos.
- wilson/Assets/WilsonSubmarino32/: arte, tiles y paletas submarinas.
- wilson/Assets/Editor/: herramientas de preparación y exportación.
- wilson/Packages/ y wilson/ProjectSettings/: dependencias y configuración.
- Documentacion/: decisiones, prompts, procedencia de recursos y verificaciones.

Las carpetas de documentación registran pruebas de importación, rutas y saltos, reaparición, puntuación, transiciones, plataformas, coco oculto, menús y compilación. Algunas verificaciones de recolección llaman a los contactos desde pruebas; no equivalen a una partida humana completa. Los análisis de trayectorias tampoco sustituyen las pruebas de jugabilidad.

Para el comportamiento actual de la costa y los menús, consultar Documentacion/DificultadCostera/LEEME.md y Documentacion/Menus/LEEME.md: amplían las notas originales de creación de los niveles.

## 12. Autoría, recursos, créditos y enlaces

### Responsable del proyecto

Grupo: Binary Code
Participantes: Iris Pereyra 
               Gemma Paez 
— dirección del proyecto, decisiones de diseño y desarrollo del juego. 
La producción contó con asistencia de IA para programación, documentación y parte de los recursos visuales. Los detalles de esos recursos y sus prompts se conservan en la documentación correspondiente.

### Arte

El tileset submarino preparado para Wilson tiene su registro en Documentacion/Submarino/LICENCIA_Y_AUTORIA.txt. El pack de Crusenho se utilizó como referencia técnica; su copia local está excluida del repositorio por sus condiciones de redistribución y no está referenciada por las escenas y paletas guardadas. El arte nuevo se documenta por separado y no se atribuye al autor del pack de referencia.

### Música y efectos

- POL-star-way-short.wav: música seleccionada para los niveles 1, 4 y 5.
- POL-test-your-skills-short.wav: música seleccionada para los niveles 2 y 3.
- OMEN (chiptune), de madameberry: música de Game Over, publicada con CC0 en OpenGameArt.
- Efectos: recogida de cocos, checkpoint, fallo, victoria, continuar y desbloqueo.

Las pistas POL y los efectos proceden del pack educativo Música y sonidos compartido por Luis Canary en Patreon. Ese enlace registra la procedencia de la descarga; la autoría y licencia de cada archivo no se han confirmado. No se atribuye la composición del pack completo a Luis Canary ni se declara CC0 para esos archivos. Sus binarios locales permanecen excluidos de Git.

Créditos detallados: Documentacion/Audio/CREDITOS_AUDIO.md y Documentacion/GameOver/OMEN_CREDITOS.md. Los recursos de terceros conservan sus condiciones originales; este documento no les asigna una licencia nueva.

### Enlaces de interés

- Repositorio del juego: https://github.com/irisibp/Wilson-El-reencuentro
- Referencia de estructura para este documento: https://github.com/Wilson-Cabrera/Unity-Dinosaur-Park-2D
- Procedencia del pack de audio: https://www.patreon.com/LuisCanary/posts/musica-y-sonidos-134940729
- OMEN y licencia publicada: https://opengameart.org/content/patreon-song-omen
- Motor: https://unity.com/

La referencia Dinosaur Park se usa como ejemplo de organización de un GDD. La historia, las reglas y los niveles descritos aquí corresponden a Wilson: El Reencuentro.
