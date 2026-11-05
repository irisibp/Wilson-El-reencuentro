# Vacíos y caídas del nivel 3

En Unity, fuera de Play Mode, abrir `Nivel03_RuinasSumergidas` y ejecutar **Tools > Wilson > Nivel 3 > Agregar vacios y caidas**. Guardar con Ctrl+S. El menú edita la escena abierta, permite deshacer y evita duplicados.

Se abren tres huecos de dos celdas: columnas 11–12, 20–21 y 42–43. La plataforma elevada sobre el primero se conserva. Un coco se mueve desde (21.5, 1.5) a (22.5, 1.5), conservando 19 cocos y 25 puntos. Se quitan los adornos que quedarían sin apoyo.

Una zona de caída situada bajo el mapa resta una vida y devuelve a Wilson al inicio o al checkpoint. También funciona durante la protección temporal contra corales; evita caer indefinidamente. La tercera caída activa el Game Over existente y OMEN. Reintentar restaura el nivel guardado y las tres vidas.

Prueba manual: saltar los tres huecos, caer antes y después del punto seguro, agotar tres vidas y reintentar. Guardar la escena para que los huecos persistan en el reintento.

Commit sugerido: `feat: agregué vacíos y caídas con pérdida de vidas al nivel 3`
