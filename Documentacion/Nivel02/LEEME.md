# Nivel 2 — Caverna de coral

Primera versión jugable del segundo nivel. Se entra desde la cueva de Perdido en el mar mediante el botón Continuar del cartel de victoria.

## Probar en Unity

1. Salir de Play Mode y esperar la compilación.
2. Abrir Assets/Scenes/Nivel02_CavernaDeCoral.unity para probar directamente el nuevo recorrido.
3. Usar los controles actuales de Wilson: movimiento horizontal y salto. Reunir 20 puntos de cocos y entrar en la cueva del extremo derecho.
4. Para probar la continuidad, abrir Nivel01_PerdidoEnElMar, conseguir sus 15 puntos, entrar en su cueva y pulsar Continuar. El nivel 2 inicia con el contador en 0/20 y un Wilson nuevo con movimiento activo.

## Objetivo y recorrido

- 12 cocos comunes de valor 1, uno brillante de valor 3 y uno dorado de valor 5: 14 objetos, 20 puntos en total.
- Galería de entrada con escalones de una celda; dos plataformas elevadas en la zona central; tramo final alto y cueva a la derecha.
- Ancho de 40 celdas, suelo continuo, techo y paredes de roca, corales y algas decorativos sin colisión. No hay zonas de caída al vacío en esta versión.
- El coco brillante está en la primera plataforma elevada. El dorado se encuentra en la meseta alta del tramo final.
- El salto conserva velocidad horizontal 6, impulso 12 y Gravity Scale 3. No se agregan natación, enemigos, daño ni nuevos controles.

Las plataformas y los cocos están colocados según ese salto. La navegación automática muestrea las trayectorias contra los colliders del escenario y comprobó 46 de 46 puntos de apoyo alcanzables. Sigue siendo necesaria una pasada humana para ajustar dificultad, ritmo y comodidad de los saltos.

## Cambios de código y escenas

- LevelExit conserva la condición por cocos y la finalización única. Agrega Siguiente Escena, que por defecto apunta a Nivel02_CavernaDeCoral, y el botón Continuar en la victoria. No cambia de escena antes de ganar. El nivel 2 deja Siguiente Escena vacío: su victoria no intenta cargar un tercer nivel.
- CoconutManager libera su referencia estática al destruirse. Cada escena crea su propio contador; no se arrastra el puntaje anterior.
- EditorBuildSettings incluye Nivel01_PerdidoEnElMar como primera escena y Nivel02_CavernaDeCoral como segunda. SampleScene se conserva después de ellas.
- El nivel 1 guardado no se reescribió. Los cambios nuevos se guardan en una escena independiente. No se modifican los controles existentes ni los assets de terceros.

## Arte y autoría

Se reutilizan los tiles submarinos originales, la cueva y los prefabs de Wilson y cocos que ya tiene el proyecto. El fondo reutiliza Fondo_demo con un tinte más oscuro. No se incorporan imágenes del pack de Crusenho ni arte externo nuevo. Las condiciones y prompts existentes continúan en Documentacion/Submarino y Documentacion/SalidaCueva.

## Validación

VERIFICACION_NIVEL02.txt registra configuración y comprobaciones de alcance. VERIFICACION_FLUJO.txt registra la prueba en Play Mode de victoria del nivel 1, carga del 2, reinicio del contador, recolección y victoria final. Preview_jugable.png y Mapa_completo.png son capturas de cámara del proyecto de prueba, no imágenes conceptuales.

El test del flujo invoca los mensajes de los triggers para comprobar las reglas y referencias; no es una partida automatizada que salta y recoge los cocos. La prueba de alcance usa trayectorias de salto muestreadas y el volumen circular de Wilson con una tolerancia de 0,04 unidades.

Las fuentes de generación y pruebas se conservan en Fuentes para reproducirlas en un proyecto aislado con los mismos assets. No es necesario ejecutar el generador para jugar o editar la escena y no permite sobrescribir un nivel 2 existente.

Commit sugerido en primera persona, después de probar el recorrido: feat: creé la Caverna de coral y conecté los dos niveles
