# Sprite de Chuck

Generado con la habilidad imagegen y la herramienta de generación de imágenes de OpenAI. El PNG original se conserva sin recortar ni retocar; Unity usa Point, compresión desactivada y un ajuste de escala y posición para apoyar los pies en el suelo.

## Prompt

Use case: stylized-concept. Asset type: a single production game sprite for Unity 2D platformer Wilson-El-Reencuentro. Subject: Chuck, the adult male castaway reunited with his volleyball friend Wilson at a tropical coastal town. Full body standing idle, facing left in three-quarter side view, friendly relieved expression, brown medium-length slightly tousled hair and short beard, sun-tanned skin, worn cream short sleeve shirt, blue knee length shorts, bare feet. Pixel art matching classic 16-bit tropical platformer sprites: bold readable silhouette, limited warm palette, crisp square pixels and dark outline, small character proportions like a 48 by 80 pixel logical sprite enlarged cleanly with nearest-neighbor squares. Composition: exactly one character, centered, entirety of feet and hair visible, compact transparent margins, no props, no environment, no ground shadow, no text, no frame, no additional poses. Real transparent background. Suitable to scale to about two world units tall in a side-scrolling 32px tile game. Avoid realistic illustration, antialiasing and blurry edges.

## Datos técnicos del resultado

- PNG RGBA: 1086 × 1448 píxeles; un personaje, sin grilla de animación.
- No es un tileset de 32 × 32: se importa como Sprite Single.
- Point, sin mipmaps, Compression None, PPU 570.4166667.
- Pivote inferior al centro; compensación del margen alfa en el objeto visual del prefab.
- Altura visible aproximada: 2,4 unidades.
- Prefab: `Assets/Art/Nivel05/Chuck.prefab`.

Los edificios, baldosas, faroles y vecinos usan geometría nativa con colores por vértice, guardada como assets Mesh y objetos separados de la escena. No son imágenes generadas ni tienen un prompt de imagen.
