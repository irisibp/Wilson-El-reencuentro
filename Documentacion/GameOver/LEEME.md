# Música por nivel y Game Over

## Selección solicitada

| Momento | Archivo |
|---|---|
| Nivel 1 — Perdido en el mar | POL-star-way-short.wav |
| Nivel 2 — Caverna de coral | POL-test-your-skills-short.wav |
| Game Over | OMEN_chip.WAV, versión chiptune de madameberry |

La música de los niveles se reproduce en bucle. Al pasar del nivel 1 al 2, el reproductor persistente cambia de canción. OMEN se reproduce al perder la última vida; se deja sin bucle porque la ficha no declara una unión de loop. Al reintentar vuelve a comenzar Test Your Skills.

## Tres vidas en el nivel 2

PlayerRespawn inicia con 3 vidas y muestra el contador en pantalla. Los dos primeros fallos consumen una vida y conservan los cocos recogidos y el checkpoint. Continúan la pausa breve y la protección temporal ya implementadas. El tercer fallo deja 0 vidas, detiene el control y la simulación física, presenta Game Over y cambia la música a OMEN.

Reintentar nivel recarga la escena actual: restaura 3 vidas, contador de cocos en 0, todos los coleccionables y checkpoint desactivado. El botón no se activa antes de Game Over ni puede solicitar varias cargas simultáneas. No hay reaparición automática con 0 vidas y un nivel completado no consume vidas.

El nivel 1 no incorpora nuevos peligros ni PlayerRespawn automáticamente; la configuración de tres vidas actúa donde está ese componente, actualmente el nivel 2.

## Probar

1. Salir de Play Mode y esperar la compilación.
2. Abrir Nivel01_PerdidoEnElMar: al iniciar debe sonar Star Way. Completarlo y pulsar Continuar para escuchar Test Your Skills en el nivel 2.
3. En el nivel 2, tocar los corales rojos tres veces, dejando terminar la protección después de cada reaparición. El contador baja de 3 a 2, 1 y 0.
4. Con 0 vidas aparece Game Over y suena OMEN. Reintentar devuelve el estado inicial del nivel 2.

La escena principal se conserva sin modificaciones externas. Si no aparecen los peligros o el punto seguro, usar Tools → Wilson → Nivel 2 → Agregar peligros y punto de reaparicion fuera de Play Mode y guardar con Ctrl+S. Si los cambios ya están abiertos en Unity, guardarlos antes de cerrar la escena.

## Configuración y procedencia

Assets/Resources/WilsonAudioSettings contiene las tres pistas y los volúmenes. El campo Musica se conserva como respaldo para otras escenas; los campos Musica Nivel 01, Musica Nivel 02 y Musica Game Over definen la selección específica. Se conservan los efectos y los ajustes de volumen anteriores.

Las pistas POL se copian byte a byte desde Descargas y siguen en Assets/Audio/Local/Music, con los binarios locales excluidos de Git mientras se confirman sus licencias individuales. La antigua pista Spinning Gears se conserva en el disco, pero deja de estar asignada como música activa.

OMEN se descarga de la ficha original de madameberry en OpenGameArt, que la publica bajo CC0: https://opengameart.org/content/patreon-song-omen. Se guarda en Assets/Audio/ThirdParty/OMEN junto con su nota de autoría y fuente. El archivo puede versionarse con esa documentación; no se atribuye al creador del juego.

## Verificación

VERIFICACION_IMPORTACION.txt registra las tres pistas y sus duraciones. VERIFICACION_PLAYMODE.txt registra la transición de canciones, pérdida de 3 vidas, conservación del progreso en los primeros fallos, derrota y reinicio completo, reproducción de OMEN y listener único. El test usa el método central que invocan los peligros para consumir vidas; los contactos físicos de los peligros se verificaron en la fase anterior.

Las pruebas se hicieron en una copia aislada, preparando ahí peligros y checkpoint cuando no figuraban en el archivo guardado del usuario. No sustituyen una escucha personal para ajustar el volumen ni una pasada manual del recorrido.

Commit sugerido en primera persona: feat: asigné música por nivel y agregué Game Over con tres vidas
