# Nivel 3 — Ruinas sumergidas

Nueva escena: Assets/Scenes/Nivel03_RuinasSumergidas.unity.

## Jugar

Salir de Play Mode y esperar la compilación. Completar la Caverna de coral y pulsar Continuar para entrar en las ruinas, o abrir directamente la nueva escena y presionar Play.

La escena ya incluye terreno, decoración, cuatro peligros y el punto seguro. No requiere aplicar el menú de preparación del nivel 2.

## Objetivo

- 25 puntos: 17 cocos comunes de valor 1, uno brillante de valor 3 y uno dorado de valor 5. Son 19 objetos.
- Recorrido de 48 columnas, con arcos de piedra al fondo, sectores de techo roto, meseta alta y una plataforma elevada para el coco brillante.
- Cuatro corales rojos peligrosos, uno antes de cada tramo de salto principal. Se reutilizan las señales de peligro y el comportamiento probado.
- Punto seguro alrededor de X=25,5. Antes de activarlo se vuelve al inicio; después se vuelve al punto seguro.
- Tres vidas. Los primeros fallos conservan los cocos. Con 0 vidas suena OMEN y aparece Game Over. Reintentar restaura 3 vidas, los 19 cocos, contador 0/25 y checkpoint desactivado.

Se conservan el tamaño del collider, detector, velocidad, salto, gravedad y escala del Wilson guardado en el nivel 2. También se reutilizan los modelos de cocos, peligro, checkpoint y cueva para mantener su aspecto. Los arcos del fondo son decorativos y no tienen colisión; el terreno y las paredes sí.

## Conexión de niveles

LevelExit mantiene la siguiente escena explícita cuando está configurada. Si ese campo está vacío, consulta la siguiente entrada habilitada en la lista de escenas de la build, únicamente cuando su nombre comienza por Nivel. Así la salida del nivel 2, que tenía el campo vacío, ahora conecta con el nivel 3 sin reescribir su escena.

El orden actual es Nivel01_PerdidoEnElMar, Nivel02_CavernaDeCoral, Nivel03_RuinasSumergidas y Nivel04_LaPlaya, seguido de las demás escenas conservadas. Al completar las ruinas, Continuar carga la playa. SampleScene no se toma como un nivel jugable. Las verificaciones originales de esta carpeta registran el estado al crear el nivel 3; Documentacion/Nivel04 contiene las pruebas de la nueva conexión.

Las escenas anteriores se conservan sin cambios en sus mapas ni objetos. Los cambios de código y orden de build se aplican al proyecto; la escena nueva es independiente.

## Música

Test Your Skills acompaña las ruinas, igual que la caverna. GameAudioSettings agrega Musica Nivel 03, inicialmente vinculada a la pista del nivel 2. Al continuar entre esas dos escenas el reproductor mantiene la pista en curso, y al reintentar después de Game Over vuelve a la música del nivel.

No se descargan ni agregan nuevos recursos externos. Se conservan las notas de autoría de los tiles, cueva y audios. Las pistas POL siguen siendo archivos locales según la configuración existente del repositorio.

## Pruebas

VERIFICACION_MAPA.txt registra una búsqueda de caminos con el collider real de Wilson: posiciones seguras y trayectorias que evitan terreno y peligros. Todos los cocos tienen un apoyo accesible. Se usa una tolerancia de 0,04 unidades y muestreo temporal de 0,02 segundos. No sustituye una pasada humana para ajustar ritmo y dificultad.

VERIFICACION_PLAYMODE.txt registra victoria del nivel 2 y carga del 3, contador 0/25, 19 cocos, tres vidas, checkpoint, Game Over, reintento y victoria de las ruinas. La prueba invoca los mensajes de recolección y el método central que usan los peligros; no es una partida que recorra automáticamente todos los saltos.

Preview_jugable.png y Mapa_completo.png son capturas del escenario en Unity. Las fuentes del generador y las pruebas quedan en Fuentes para reproducir el resultado en un proyecto aislado. El generador se niega a sobrescribir una escena de nivel 3 existente.

Commit sugerido en primera persona, después de jugarlo:
feat: creé las Ruinas sumergidas y conecté el tercer nivel
