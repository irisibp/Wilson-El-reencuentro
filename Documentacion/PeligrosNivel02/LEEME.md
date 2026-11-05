# Peligros y reaparición — Caverna de coral

## Aplicar al nivel en Unity

1. Salir de Play Mode y esperar la compilación.
2. Abrir Assets/Scenes/Nivel02_CavernaDeCoral.unity.
3. Elegir Tools → Wilson → Nivel 2 → Agregar peligros y punto de reaparicion.
4. Guardar con Ctrl+S.

El menú prepara el mapa abierto usando Undo: agrega tres corales rojos peligrosos, un punto seguro cerca de la mitad, el componente PlayerRespawn a Wilson y un aviso en la interfaz. No duplica los objetos si se ejecuta otra vez. No cambia el archivo de la escena desde fuera de Unity; los cambios que tengas abiertos se conservan y se guardan desde el editor.

Para deshacer la preparación de la escena, usar Ctrl+Z antes de realizar otras acciones. Los scripts nuevos siguen disponibles para volver a aplicarla.

## Reglas

- Los corales rojos con signo de exclamación hacen reaparecer a Wilson. Los corales rosados y violetas decorativos mantienen su función visual.
- Antes de activar el punto seguro, Wilson vuelve a su posición inicial del nivel.
- Al cruzar el marcador Punto seguro, el indicador se vuelve verde y dice Activado. Los siguientes fallos vuelven a ese lugar.
- Cada fallo pausa el control durante 0,35 segundos. Tras reaparecer hay un segundo de protección con parpadeo.
- Se conserva el puntaje y los cocos recogidos permanecen recogidos. La escena no se recarga; la salida conserva su estado de desbloqueo.
- Actualmente hay 3 vidas y Game Over con OMEN. Los dos primeros fallos conservan cocos y checkpoint; Reintentar nivel reinicia el recorrido. Ver Documentacion/GameOver/LEEME.md.
- La cámara se reencuadra al reaparecer. Se limpian la velocidad y los saltos pendientes.
- Los peligros no pueden reactivar a Wilson después de completar el nivel.

## Ubicaciones y edición

Tres peligros en X=6, X=24,5 y X=35,5; el editor busca la superficie del suelo para ubicarlos. Se guardan bajo PeligrosNivel02. Sus Box Collider 2D son triggers y están ajustados al centro del dibujo para dar margen al jugador. Se conserva la ubicación y escala de los cocos del mapa del usuario.

CheckpointNivel02 está en X=17,5. El hijo PuntoSeguro define la posición exacta de reaparición y se calcula con el tamaño real del collider de Wilson. El marcador visual no tiene colisión sólida. Si se modifica el terreno, mover PuntoSeguro a una zona despejada y sin peligros, encima de una plataforma suficientemente ancha.

Los saltos de prueba usan el radio ajustado del collider de tu Wilson, no el radio anterior del prefab. Los tres peligros se pueden superar con velocidad 6, impulso de salto 12 y gravedad 3. La prueba automática muestrea el recorrido del salto; hace falta jugarlo para ajustar la dificultad y el ritmo a tu gusto.

Para el segundo peligro, la ruta comprobada sale de la plataforma elevada central y aterriza después del coral, cerca del coco. Así se evita saltar contra la parte inferior de esa misma plataforma desde el pasillo bajo.

## Archivos

- PlayerRespawn.cs: posición inicial, checkpoint, pausa, protección, conservación del estado del nivel y aviso.
- CoralHazard.cs: contactos con Wilson, incluidos colliders hijos, sin afectar otros objetos.
- RespawnCheckpoint.cs: activación del punto seguro e indicador.
- WilsonController.cs: se agrega ReiniciarMovimiento para limpiar entrada, velocidad y salto pendiente.
- CameraFollow.cs: se agrega Reencuadrar para el cambio instantáneo al reaparecer.
- LevelExit.cs: ignora callbacks de un Wilson con el control deshabilitado durante una reaparición.
- Editor/Level02HazardsSetup.cs: aplicación desde menú, con Undo y sin duplicados.

Se reutilizan sprites originales del tileset submarino (coral_rojo, roca_suelta y burbujas_a), con colores propios para distinguir su función. No se crean o modifican PNG, no se incorporan assets del pack de terceros y no se cambian las condiciones para completar los niveles.

## Verificación

VERIFICACION_MAPA.txt registra las tres trayectorias y el punto seguro despejado. VERIFICACION_PLAYMODE.txt registra contactos físicos reales con los peligros y el checkpoint, regreso al inicio y al punto seguro, conservación de cocos, protección, reencuadre y victoria. Los cocos de la prueba se recogen mediante el mismo mensaje de trigger del componente para verificar su conservación; no es una partida automatizada completa.

Las capturas muestran la configuración aplicada en el proyecto aislado de prueba. En el proyecto principal hay que aplicarla con el menú y guardar. Los tests y el código de las capturas quedan en Fuentes para reproducir la comprobación en un proyecto aislado.

Commit sugerido en primera persona, después de probar:
feat: agregué peligros y un punto de reaparición al nivel 2
