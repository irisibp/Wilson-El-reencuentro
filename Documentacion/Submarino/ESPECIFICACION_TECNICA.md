# Adaptación técnica de los prompts aprobados

Objetivo: sprites submarinos originales para Wilson, a 32 × 32 píxeles nativos; arena clara, roca azul, corales rosa/violeta, algas turquesas, conchas, burbujas y estrellas. La propuesta de paisaje es una referencia de ambiente y legibilidad, no una imagen para cortar ni un archivo editable del juego.

El prompt original de 8 × 4 pedía 32 piezas, nueve por material y decoración. Se amplía a 114 piezas en 8 × 15 para cumplir el informe técnico: incluir las 47 combinaciones válidas de bordes y diagonales para cada material, las esquinas interiores y las variantes de columna/plataforma necesarias para automatizar el terreno. Quedan 6 celdas vacías. No se introducen textos, damero, rótulos, iluminación de escena ni fondos dentro del atlas.

La geometría y color se construyen a escala nativa mediante código editable, tomando como referencia visual los bocetos generados con image_gen. No se usó image_gen para garantizar metadatos de sprites, medidas o Rule Tiles; esas propiedades se generaron y verificaron por separado. No se editan los bocetos originales.

Máscara de vecinos: N=1, E=2, S=4, W=8, NE=16, SE=32, SW=64, NW=128. Una diagonal solo cuenta si sus dos cardinales adyacentes están presentes. Así las 256 configuraciones posibles se reducen a 47 sprites por material. En Unity los ejes Y de los vecinos se invierten respecto de las filas del PNG (N es +Y).

Cada pieza posee ancho y alto 32, pivote central, PPU32 y límites de recorte enteros. Las comparaciones de bordes se hacen entre configuraciones compatibles, no entre todas las piezas de forma indiscriminada. La fuente Aseprite contiene cuatro capas de imagen, no tilesets de Aseprite ni reglas de Unity.

No se agrega agua opaca a las celdas transparentes. El fondo de agua de la demostración es un recurso separado; las burbujas son decoración sin colisión. La base y los corales no configuran movimiento submarino del personaje.
