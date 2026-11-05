# Cueva de salida

Sprite decorativo Single, independiente de Tile Palette. Se genera como arte nuevo con image_gen integrado, usando el demo submarino como referencia de estilo. El PNG conserva su resolución y canal alfa originales; no se escala ni remuestrea el archivo para simular un tile nativo de 32 × 32.

La importación usa Point, sin compresión ni mipmaps, pivote central inferior y un ancho de 2,4 unidades de mundo. El PPU se calcula a partir del ancho real del PNG para respetar esa medida. Es una pieza decorativa, no un terreno sólido ni un atlas modular.

Aplicar desde Unity fuera de Play Mode: seleccionar SalidaNivel en Hierarchy y ejecutar Tools → Wilson → Salida → Aplicar cueva a la salida seleccionada. La opción agrega o actualiza el hijo VisualCueva y desactiva solo el Sprite Renderer del marcador rosa. Mantiene el GameObject, LevelExit, Box Collider 2D, su tamaño/offset y el transform originales. Se puede deshacer con Ctrl+Z. Guardar la escena con Ctrl+S.

El nuevo hijo queda bajo SalidaNivel: aparece cuando CoconutManager activa la salida al reunir 15 cocos y desaparece si se desactiva ese padre. No cambia las condiciones de victoria ni agrega colisiones. No cambia automáticamente de escena.

El menú conserva la base del antiguo marcador y compensa la escala del padre para no deformar el dibujo. Ajustar solo la posición de VisualCueva si la ilustración necesitara otra alineación con el suelo; dejar intacto el trigger ya probado.

No se modifica la escena guardada desde scripts externos para evitar sobreescribir cambios abiertos en Unity. La aplicación visual se realiza desde el menú con Undo y queda pendiente de guardar por el usuario.

El arte no incorpora archivos del pack de Crusenho. Es una nueva imagen asistida por IA para Wilson, con el mismo criterio de uso documentado para los recursos submarinos originales. El prompt completo se conserva junto a esta guía.
