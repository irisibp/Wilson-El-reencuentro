# Dificultad adicional: secreto del mercado

Fuera de Play, abrir Nivel05_CiudadCostera y ejecutar **Tools > Wilson > Nivel 5 > Agregar coco oculto y balcones**. Guardar con Ctrl+S antes de probar. El menú edita la escena abierta, agrupa los cambios para deshacer con Ctrl+Z y evita duplicar el secreto.

Se agregan dos balcones físicos: X=12–14, con superficie en Y=4, y X=16–17, con superficie en Y=5. Hay que saltar desde el paseo al primer balcón y luego al segundo. Los colliders son celdas de Tilemap de la capa Ground; la madera visible reutiliza el sprite del muelle existente.

El coco dorado que estaba en (34,5; 3,5) pasa a (16,5; 5,5), detrás de una caja decorativa sin colisión. La caja desaparece al acercarse Wilson a la zona alta del balcón. Entonces se habilitan el render y el collider del coco; descubrirlo por sí solo no otorga puntos. Recogerlo da 5 puntos, como antes. Un sonido de descubrimiento reutiliza el efecto de desbloqueo.

Se mantienen 29 cocos y 35 puntos necesarios para el reencuentro con Chuck. El coco brillante continúa en el muelle original, de modo que ambas rutas elevadas tienen una recompensa necesaria. El objetivo del HUD incluye una pista sobre los balcones del mercado.

Si Wilson pierde una vida, el secreto descubierto y los puntos recogidos se conservan. Tras Game Over, Reintentar restaura la caja y vuelve a ocultar el coco. El secreto no consume vidas ni cambia el checkpoint, la música o el final.

## Verificación

- 57/57 apoyos seguros alcanzables, 11399 muestras de trayectorias y todos los cocos con apoyo accesible. Modelo balístico con tolerancia de radio de 0,04 unidades, como en la validación original.
- Prueba física: permanecer bajo el balcón no revela el coco; acercarse arriba lo muestra; recogerlo suma 5 una sola vez; una caída conserva el hallazgo; el reintento lo oculta otra vez; conseguir 35 puntos sigue desbloqueando el reencuentro final.
- Aplicar el menú dos veces no duplica objetos ni monedas.

El muestreo de trayectorias no sustituye una partida manual para valorar el ritmo y la dificultad. Las pruebas de contacto colocan al jugador en las zonas de interés; no recorren automáticamente todos los saltos del nivel.

Los assets visuales se reutilizan sin modificar los PNG originales. La posición de la caja compensa los márgenes transparentes de los sprites para apoyarla sobre la madera visible. Las fuentes y resultados de las pruebas se incluyen en esta carpeta.

Commit en primera persona: `feat: agregué un coco oculto y una ruta de balcones al nivel final`
