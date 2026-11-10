# Inicio, pausa y salida del juego

La build comienza en `Assets/Scenes/MenuPrincipal.unity`, antes de los cinco niveles. La lista global conserva las demás entradas existentes y el estado deshabilitado de SampleScene. Si un Build Profile usa su propia lista de escenas, MenuSetup antepone el inicio y los cinco niveles también en esa lista, sin cambiar sus otras opciones.

## Pantalla de inicio

Jugar comienza una partida nueva en Nivel01_PerdidoEnElMar. Controles abre una tarjeta con las teclas de movimiento, salto y pausa; Volver o Esc cierran esa tarjeta. Salir del juego cierra completamente el ejecutable. En Play dentro de Unity detiene Play y conserva el editor abierto.

La pantalla reutiliza el horizonte, la isla tropical, Wilson y el prefab de Chuck. El diseño y los botones son UI nativa de Unity/TMP. Los materiales del inicio se separan por textura para evitar mezclas de atlas entre sprites en el renderizador. Los PNG originales no se modifican.

## Pausa

En los cinco niveles se puede pulsar Esc o el botón Pausa (Esc). Las opciones son Continuar, Reiniciar nivel, Volver al inicio y Salir del juego.

Pausar congela el tiempo, la física, las plataformas móviles y el audio global. Los botones siguen funcionando. Se limpia la entrada de salto pendiente para no saltar al cerrar un menú. Continuar restablece el tiempo y el audio; reiniciar o volver al inicio comienza de nuevo. No se guarda el progreso entre sesiones.

La pausa se bloquea durante la reaparición, Game Over, victoria y reencuentro final. Game Over y la victoria normal tienen accesos para volver al inicio y salir. El cierre con Chuck incluye los botones Volver al inicio y Salir del juego. Volver al inicio ahora vuelve a la pantalla de título.

GameMenus se instala automáticamente al iniciar Play o una build, incluso si se abre directamente un nivel. Conserva una sola instancia y un EventSystem entre escenas. No hace falta modificar cada mapa ni añadir componentes manualmente.

## Exportación

Menú: Tools > Wilson > Menus > Exportar Windows con inicio y pausa. Produce un player Windows Intel 64-bit, sin Development Build, con las seis escenas habilitadas. La carpeta de salida está bajo Documentos/ChatGPT/wilson/Entregas/WilsonDemo_InicioYPausa. Incluye COMO_JUGAR.txt.

Para compartir, comprimir toda esa carpeta. El ZIP anterior no incorpora estos cambios; usar la nueva entrega. La exportación anterior del usuario se conserva.

## Verificación

VERIFICACION_PLAYMODE.txt registra controles del inicio, entrada al nivel 1, congelación y restauración de física/reloj/audio, limpieza del salto, reinicio y regreso al inicio, pausa en los cinco niveles y plataformas congeladas, Game Over, regreso desde Chuck al título, ausencia de duplicados y salida desde pausa deteniendo Play sin cerrar el editor.

Inicio.png y Pausa.png son capturas desde Unity, con UI nativa. Los tests invocan botones y métodos públicos en Play Mode; la comprobación del ejecutable y su cierre se registra aparte.

Las fuentes de los generadores y tests están en Fuentes. Los tests de Editor y las herramientas temporales de aplicación no forman parte de los scripts del jugador exportado. Los scripts temporales de aplicación se retiran de Assets/Editor al terminar.

Commit en primera persona: `feat: agregué pantalla de inicio, pausa y salida del juego`
