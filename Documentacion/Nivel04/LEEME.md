# Nivel 4: La playa

Escena: `Assets/Scenes/Nivel04_LaPlaya.unity`. Abrirla y presionar Play, o completar las ruinas y pulsar Continuar. Está incluida entre Nivel03 y SampleScene en las escenas de la build. LevelExit resuelve la conexión sin reescribir la escena del nivel 3.

## Recorrido

Costa de 52 columnas, arena clara, palmeras, vegetación, conchas y horizonte tropical. Cinco huecos: columnas 8–9, 17–18, 26–27, 35–37 y 44–45. El de tres celdas queda después del checkpoint. Un muelle elevado en X=21–23 lleva al coco brillante. Las palmeras son decorativas y no bloquean el salto. No hay techo ni corales peligrosos submarinos.

Se necesitan 30 puntos: 22 cocos comunes de 1 punto, un brillante de 3 y un dorado de 5; 24 objetos en total. El cartel de salida en X=50,5 aparece al reunirlos. La salida de la playa ahora conduce a Nivel05_CiudadCostera. El cierre del juego ocurre junto a Chuck en el nivel 5; no se carga SampleScene.

El punto seguro está en X=24,5. Tres vidas; cada caída resta una y devuelve al inicio o al checkpoint. Los puntos se conservan en las reapariciones. La tercera caída activa Game Over con OMEN; Reintentar restaura todos los cocos y las tres vidas.

Star Way acompaña la playa. GameAudioSettings incorpora Musica Nivel 04 y el menú de configuración de audio conserva esa asignación. Se reutilizan audios y efectos ya integrados, sin nuevas descargas.

## Arte y edición

El terreno sigue siendo un Tilemap editable con Auto_arena y collider de grilla. El material `Assets/Art/Nivel04/ArenaPlaya.mat` usa `BeachSand.shader` para sustituir la piedra azul por arena clara con grano, conservando el borde arenoso y el alfa del atlas original. No se modifica el PNG fuente. El muelle emplea el sprite tropical existente y un Tilemap independiente para sus colisiones. Los límites laterales no se renderizan. La zona de caída cubre los cinco vacíos por debajo del mapa.

Palmeras, muelle, vegetación, cartel, conchas y fondo provienen de los assets existentes del proyecto. No se agregan nuevas condiciones de licencia ni se supone una licencia distinta para esos recursos; conservan su procedencia y las notas existentes. Las pistas POL siguen siendo locales según la configuración actual del repositorio.

El HUD tiene bandas oscuras para mantener legibles el contador, el título, las vidas y el objetivo sobre el cielo y la arena.

## Verificación

`VERIFICACION_MAPA.txt`: búsqueda de caminos con gravedad, velocidad, impulso de salto y radio del collider del jugador; 44 apoyos seguros, todos accesibles, y 5479 muestras de trayectorias. Incluye pequeños retardos en el movimiento horizontal para modelar saltos más verticales. Tolerancia de radio: 0,04 unidades. No reemplaza una pasada humana para afinar el ritmo.

`VERIFICACION_PLAYMODE.txt`: victoria de ruinas y transición a playa; contador nuevo; 24 cocos y 30 puntos; música; checkpoint; tres caídas físicas; conservación de puntos; Game Over y OMEN; reinicio; salida y victoria. La recolección de todos los cocos se verifica invocando sus mensajes de contacto; no es una partida automática de todo el recorrido.

Las imágenes muestran el escenario desde Unity con URP 17.6.0 y los ajustes gráficos del proyecto real. La arena también se comprobó con el renderizador integrado. Las vidas y los diálogos dibujados mediante OnGUI aparecen al jugar y no en la captura estática de Camera.Render. Las fuentes del generador y las pruebas quedan en Fuentes; el generador impide sobrescribir un nivel 4 existente.

Probar manualmente los cinco saltos, la ruta del muelle, el punto seguro y el cartel final antes de subir al repositorio.

Commit en primera persona: `feat: creé el nivel de la playa y lo conecté con las ruinas`

## Continuación

Nivel05_CiudadCostera añade el paseo urbano y el reencuentro final con Chuck. Sus pruebas en Documentacion/Nivel05 verifican la nueva transición. Las verificaciones originales de esta carpeta registran el estado al crear el nivel 4.

## Actualización de dificultad

La escena guardada incorpora trampas, huecos ampliados y dos plataformas móviles. Documentacion/DificultadCostera contiene el recorrido actualizado, las capturas y las pruebas. Los informes originales de esta carpeta registran la versión anterior del nivel.
