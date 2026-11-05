> Actualización: la selección activa ahora es Star Way para el nivel 1, Test Your Skills para el nivel 2 y OMEN para Game Over con tres vidas. Ver ../GameOver/LEEME.md. La selección Spinning Gears que se describe abajo corresponde a la primera integración.

# Música y efectos de Wilson

La pista elegida por el usuario, POL-spinning-gears-short.wav, suena en bucle en ambos niveles. El mismo reproductor persiste al continuar: la música no vuelve a empezar al cargar la Caverna de coral. Los efectos también persisten durante la transición y la reaparición.

## Uso

Salir de Play Mode, esperar la compilación y volver a Play. No hace falta agregar componentes ni modificar las escenas: el sistema se inicia al cargar una escena cuyo nombre comienza por Nivel.

Seleccionar Assets/Resources/WilsonAudioSettings.asset para ajustar los dos controles independientes:
- Volumen Musica: 0,18 inicial.
- Volumen Efectos: 0,70 inicial, con ganancias por efecto para evitar tapar la música.

Durante la victoria la música baja al 35 % de su volumen configurado. Al continuar recupera el volumen normal. Las dos fuentes son 2D; no cambia el volumen por la distancia a la cámara. Se conserva un solo AudioDelJuego y un AudioListener activo en la cámara de cada escena; el nivel 2 no tenía listener en su archivo original, por lo que el sistema lo agrega al ejecutar.

## Selección de archivos

| Acción | Archivo | Duración aproximada |
|---|---|---:|
| Música de ambos niveles | POL-spinning-gears-short.wav | 13,24 s |
| Recoger coco | Coin2.ogg | 0,139 s |
| Activar punto seguro | confirmation_003.ogg | 0,322 s |
| Fallar/reaparecer | error_007.ogg | 0,192 s |
| Completar nivel | confirmation_004.ogg | 0,490 s |
| Continuar al siguiente | toggle_004.ogg | 0,066 s |
| Desbloquear la salida | confirmation_002.ogg | 0,539 s |

Los efectos se disparan después de validar la acción: un coco ya recogido, un checkpoint ya activado o un contacto durante la protección de reaparición no generan efectos repetidos. El desbloqueo suena una sola vez al cruzar el mínimo de puntos.

Los originales de Descargas no se modificaron. El WAV se conserva a 22.050 Hz, estéreo, 8 bits de origen; la importación usa Streaming/Vorbis. Los efectos OGG se importan como PCM descomprimido, preservando su frecuencia. Los cambios son de importación y reproducción; no se recortaron, normalizaron ni remezclaron los archivos.

## Archivos locales y repositorio

Los clips están en Assets/Audio/Local/Music y Assets/Audio/Local/SFX. Por ahora los WAV y OGG de esa carpeta quedan excluidos de Git, porque la carpeta descargada no trae la licencia de cada archivo y el enlace educativo no se pudo leer desde la herramienta. Se conservan los .meta, configuración, scripts y documentación. La procedencia y las referencias de créditos están en CREDITOS_AUDIO.md.

Para restaurarlos en otra copia del proyecto, copiar la pista elegida desde MusicaYSonidos/Musica a Music y los seis archivos de la tabla desde MusicaYSonidos/Sonidos a SFX, conservando los archivos .meta del repositorio. Luego usar Tools → Wilson → Audio → Preparar musica y efectos. En el proyecto actual ya está configurado.

Si faltan clips en otra computadora, el juego puede ejecutarse y los efectos faltantes se omiten; restaurar los originales para recuperar el audio.

## Estado de la escena del nivel 2

En el archivo guardado inspeccionado durante esta tarea no figuraban todavía PeligrosNivel02 ni CheckpointNivel02. Si están abiertos en Unity, guardar con Ctrl+S fuera de Play Mode. Si no aparecen, usar Tools → Wilson → Nivel 2 → Agregar peligros y punto de reaparicion y guardar. Los sonidos de esos eventos se conectan a los componentes ya preparados.

## Verificación

VERIFICACION_IMPORTACION.txt registra duración, formato y decodificación de los seis efectos con datos de audio no silenciosos. VERIFICACION_PLAYMODE.txt registra reproducción de fuentes, continuidad de la pista entre niveles, volumen de victoria, listener único y efectos de acciones. La copia aislada de prueba prepara peligros y checkpoint cuando el archivo guardado del usuario todavía no los incluye.

La prueba valida la reproducción dentro del motor, sin sustituir una escucha personal para ajustar la mezcla. Los scripts de prueba y configuración se conservan en Fuentes; no se ejecutan ni se compilan como parte del juego.

Commit sugerido en primera persona después de probar: feat: incorporé música y efectos de sonido al juego
