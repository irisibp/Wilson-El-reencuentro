# Nivel 5: Ciudad costera — final del juego

Escena: `Assets/Scenes/Nivel05_CiudadCostera.unity`. Se abre directamente con Play o completando la playa y pulsando Continuar. Está después de Nivel04 en las escenas habilitadas de la build. Los mapas de los niveles anteriores no se reescriben.

## Objetivo y recorrido

60 columnas de costa urbanizada: siete edificios con comercios, ocho vecinos decorativos, seis faroles, palmeras, un paseo de baldosas y una plaza final. Los edificios y vecinos no tienen colliders: las superficies jugables son los Tilemaps del suelo y del muelle.

Cinco huecos: X=8–9, 18–19, 28–29, 38–40 y 48–49. El hueco de tres celdas queda después del punto seguro, en X=26,5. El muelle elevado abarca X=23–25 y conduce al coco brillante. Tras el último hueco hay un tramo amplio y seguro hasta Chuck.

Se requieren 35 puntos: 27 cocos comunes de 1, un brillante de 3 y un dorado de 5. Son 29 objetos. Se conservan tres vidas, reaparición inicial/checkpoint y Game Over con OMEN. Los puntos persisten al reaparecer; Reintentar restaura cocos, vidas y checkpoint.

Chuck espera visible desde el inicio en la plaza, X=58. No depende de que la salida esté habilitada para mostrarse. El trigger del reencuentro, cerca de X=57,1, se activa al alcanzar 35 puntos.

## Reencuentro y cierre

LevelExit incorpora `finalDelJuego`, desactivado por defecto para los niveles anteriores y activado únicamente en el nivel 5. Una salida final no ofrece ni carga otra escena, aunque se le asigne accidentalmente un destino explícito.

Su evento persistente llama a `ReunionEnding.MostrarFinal()`. Se congela la física de Wilson, se ocultan el HUD y las vidas, y Wilson se acerca a Chuck durante 1,2 segundos. Luego aparece «¡Wilson y Chuck, juntos otra vez!», el mensaje de cierre y el botón Volver al inicio. Este botón carga Nivel01_PerdidoEnElMar con su estado nuevo. No se cierra la aplicación ni se carga SampleScene.

La victoria no puede activarse antes de los 35 puntos ni dispararse dos veces. Las caídas no resucitan a Wilson después de ganar. No se altera Time.timeScale.

## Arte y audio

La arena, el muelle, el horizonte y las palmeras reutilizan los recursos ya integrados en el proyecto. Edificios, vecinos, faroles, baldosas y portal son objetos editables con Mesh assets y color por vértice, sin colliders; CityGeometry.cs documenta su construcción.

Chuck utiliza un PNG nuevo generado con imagegen; el original se conserva sin retoques. Se incluye como Sprite Single con transparencia, Point y sin compresión, y como prefab independiente. PROMPTS.md registra prompt, dimensiones, escala y pivote. No es un tile ni una animación de varios cuadros.

Star Way acompaña el nivel, reutilizando la pista local ya elegida para la playa. Se añade Musica Nivel 05 a GameAudioSettings y se conserva esa asignación desde GameAudioSetup. No se descargan nuevos audios. Los recursos reutilizados mantienen sus notas de procedencia existentes.

## Verificación

VERIFICACION_MAPA.txt: 52 apoyos seguros alcanzables, 6476 muestras de trayectorias contra colisiones, 29 cocos con apoyo accesible; velocidad, salto, gravedad y collider del jugador actual. El modelo considera retardos cortos antes de desplazarse horizontalmente y usa tolerancia de radio de 0,04. No sustituye una partida humana para valorar el ritmo.

VERIFICACION_PLAYMODE.txt: victoria de playa y carga de ciudad, estado nuevo, Chuck visible, contacto prematuro, edificios y vecinos, música, checkpoint, tres caídas físicas, Game Over, reintento, recolección de 35 puntos, evento persistente del final, aproximación a Chuck, HUD oculto y vuelta al primer nivel. La recolección completa se verifica invocando los mensajes de contacto, no recorriendo automáticamente todos los saltos.

Las capturas del escenario se renderizan en Unity con URP y los ajustes gráficos del proyecto. OnGUI dibuja las vidas y el mensaje final durante Play; no aparecen en las capturas estáticas de Camera.Render.

Antes del commit, jugar los cinco saltos, recoger el coco del muelle, activar el checkpoint y terminar junto a Chuck.

Commit en primera persona: `feat: creé la ciudad costera y el reencuentro final con Chuck`

## Dificultad adicional: coco oculto

El menu Tools > Wilson > Nivel 5 > Agregar coco oculto y balcones incorpora una ruta elevada al mercado y mueve el coco dorado a una caja oculta. Consultar SecretoDelMercado/LEEME.md para aplicarlo y guardar la escena. Se mantienen 29 cocos y 35 puntos. Los informes originales de esta carpeta registran el nivel antes de este agregado; SecretoDelMercado contiene las nuevas pruebas y capturas.

## Actualización de dificultad

La escena guardada incorpora trampas, huecos ampliados y dos plataformas móviles. Documentacion/DificultadCostera contiene el recorrido actualizado, las capturas y las pruebas. Los informes originales de esta carpeta registran la versión anterior del nivel.
