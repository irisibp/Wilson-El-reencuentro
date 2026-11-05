# Wilson-El-reencuentro

Videojuego de plataformas en Unity 2D.

## Abrir el proyecto

En Unity Hub, agregar la carpeta **wilson/** de este repositorio: contiene Assets, Packages y ProjectSettings. Versión del editor: **6000.6.3f1**.

Escena principal: `wilson/Assets/Scenes/Nivel01_PerdidoEnElMar.unity`.

## Ambientación submarina

El primer nivel incluye terreno de arena y roca, plataformas y decoración marina. Los recursos originales están en `wilson/Assets/WilsonSubmarino32/`, con 114 sprites de 32 × 32, Tile Assets y paletas manual y automática. Usan 32 Pixels Per Unit.

Los Rule Tiles requieren 2D Tilemap Extras, ya declarado en Packages/manifest.json. La escena de ejemplo está en `wilson/Assets/WilsonSubmarino32/Demo/Submarino_Demo.unity`.

La licencia y los prompts del arte submarino se conservan en `Documentacion/Submarino/`. Una copia local del tileset de referencia de Crusenho queda excluida del repositorio por sus condiciones de redistribución; no tiene referencias en las escenas y paletas guardadas.

Las carpetas Library, Temp, Logs y UserSettings son datos locales regenerables y no se versionan. Conservar los archivos .meta de Unity junto con los assets.
