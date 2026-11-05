# Salida del primer nivel

El contador existente activa SalidaNivel al reunir 15 cocos. LevelExit comprueba ese mismo estado antes de finalizar, reconoce WilsonController en el cuerpo o padre del collider, muestra un cartel de victoria, detiene el personaje y emite alCompletarNivel una sola vez.

## Conectar en Unity

1. Salir de Play Mode y esperar a que termine la compilación.
2. Seleccionar SalidaNivel. Agregar el componente Level Exit (archivo Assets/Scripts/Gameplay/LevelExit.cs).
3. Conservar Box Collider 2D con Is Trigger activo y cubrir la zona de llegada. El script también asegura el modo trigger.
4. En el CoconutManager de la escena, verificar Salida Nivel = SalidaNivel y Cocos Necesarios = 15.
5. Guardar la escena con Ctrl+S antes de entrar en Play Mode.
6. Reunir al menos 15 cocos y cruzar la salida: aparece el cartel de nivel completado y Wilson se detiene. Los objetos que no tengan WilsonController no completan el nivel.

El evento Al Completar Nivel puede conectarse más adelante a sonido o interfaz adicional. No requiere asignación para la victoria actual. No se cambia de escena: todavía no hay un segundo nivel configurado. Detener Play Mode para volver al estado inicial.

El script usa un cartel sencillo mediante OnGUI, sin requerir cambios en Canvas. El movimiento se detiene deshabilitando el controlador y la simulación del Rigidbody2D de Wilson; no se modifica Time.timeScale.

La escena no se edita desde archivos externos para conservar los cambios abiertos en Unity. Agregar el componente y guardar desde el editor es el paso pendiente de integración.
