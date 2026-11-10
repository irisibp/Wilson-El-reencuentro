# Dificultad de los niveles 4 y 5

Los dos niveles incluyen trampas que restan una vida, cinco huecos con dos saltos ampliados y dos plataformas móviles cada uno. Se conservan 30 puntos en la playa y 35 en la ciudad, el checkpoint y las tres vidas. La ciudad incluye el coco oculto y el final con Chuck.

## Nivel 4: playa

- Cuatro erizos peligrosos: X=5,5; 13,5; 31,5; 47,5. Sus pinchos rojos y el signo de advertencia distinguen las trampas de las estrellas de mar decorativas.
- Primer hueco ampliado a X=8–10: tres celdas y una subida de una unidad al aterrizar. Es necesario despegar cerca del borde.
- Hueco principal X=34–39: seis celdas. No se puede saltar directamente con el salto normal; el muelle horizontal recorre X=35–38, en Y=2,5. Velocidad 1,4 y pausas de 0,6 s.
- Elevador de la playa en X=22,5: centro entre Y=3,1 y 4,15, velocidad 0,9. Sustituye al muelle fijo junto al coco brillante.
- Se reubican tres cocos comunes fuera de las columnas retiradas; se mantienen 24 objetos y 30 puntos.

## Nivel 5: ciudad

- Cinco grupos de pinchos de obras: X=5,5; 24; 31,5; 44,5; 53,5. Los de X=24 se evitan usando el muelle superior del coco brillante.
- Primer hueco ampliado a X=8–10: tres celdas, con aterrizaje una unidad más alto.
- Hueco principal X=37–42: seis celdas. El muelle horizontal recorre X=38–41, en Y=2,5. Velocidad 1,8, pausas de 0,55 s.
- Elevador del mercado en X=13,5: centro entre Y=3,1 y 4,15, velocidad 1,1. Sustituye al primer balcón fijo; desde él se salta al balcón del coco dorado oculto. El balcón alto y la caja se conservan.
- Se reubican tres cocos comunes; se mantienen 29 objetos y 35 puntos. La plaza final queda despejada.
- Se ajustan las baldosas del paseo y las posiciones de dos edificios y sus letreros para apoyarlos en las nuevas orillas. Los edificios siguen siendo decorativos, sin colliders.

## Plataformas y jugador

MovingPlatform2D usa un Rigidbody2D cinemático, MovePosition y pausas en ambos extremos. Los colliders pertenecen a Ground y no tienen fricción. WilsonController detecta la superficie móvil bajo sus pies mediante un cast corto y añade la velocidad de la plataforma al movimiento. Solo se engancha desde arriba. Al saltar se libera el apoyo; se conserva la componente vertical ascendente de la plataforma para un despegue natural.

Wilson no se emparenta a las plataformas. ReiniciarMovimiento limpia la referencia al apoyo, por lo que una reaparición o victoria no arrastra al jugador. Los elevadores dejan al menos 0,95 unidades bajo su collider en la posición inferior, más que el diámetro del collider actual de Wilson; no aplastan al jugador contra el suelo.

LifeHazard usa el mismo método central de pérdida de vidas que los corales. La inmunidad evita pérdidas repetidas durante el contacto continuo. Las caídas siguen usando FallZone y no permiten caer indefinidamente durante la protección.

## Aplicación en Unity

Menú: Tools > Wilson > Niveles 4 y 5 > Aumentar dificultad de ambos niveles. Aplica y guarda las dos escenas; conserva las escenas que estuvieran abiertas y vuelve a la escena activa original. Las consultas se limitan a la escena que se está editando para no mezclar objetos cuando hay varias escenas abiertas. La preparación es idempotente.

El menú Preparar solo la escena abierta permite aplicar los cambios con un grupo de Undo antes de guardar manualmente. El menú del coco oculto también consulta solo la escena activa.

Se guarda una copia previa de cada escena en `Temp/WilsonAntesDeDificultad/`. Temp no se sube a Git y Unity puede limpiarlo; el historial de Git mantiene la versión anterior. Los PNG originales y los mapas de los niveles 1–3 no se modifican.

## Pruebas y límites

VERIFICACION_MAPA.txt registra superficies estáticas y estados de las plataformas en los extremos y el punto medio, con conexiones de viaje sobre la misma plataforma. Se muestrean las trayectorias de salto contra el terreno y las trampas. Se asume que el jugador espera una fase favorable para abordar; no es una simulación de todos los tiempos posibles. Se usa tolerancia de radio de 0,035 unidades.

VERIFICACION_PLAYMODE.txt: viaje físico durante 6,3 s sobre cada una de las cuatro plataformas, incluyendo pausas y cambios de sentido; control real de Wilson con entrada determinista; salto y liberación de apoyo; trampas e inmunidad; reaparición sin arrastre; caídas; Game Over con OMEN; reintentos; 30 puntos y transición a la ciudad; coco oculto; 35 puntos y final con Chuck.

Las pruebas de rutas usan el modelo balístico y las pruebas de recolección completa invocan los mensajes de contacto. No sustituyen una partida humana completa para afinar el ritmo. VERIFICACION_REGRESION.txt comprueba movimiento, salto y reinicio del controlador en las escenas 1–3, fuera de Play; no es una prueba completa de esos niveles.

Las capturas son del escenario en Unity/URP. Las plataformas aparecen en su posición inicial y se mueven al pulsar Play.

Commit en primera persona: `feat: aumenté la dificultad de la playa y la ciudad con trampas y plataformas móviles`
