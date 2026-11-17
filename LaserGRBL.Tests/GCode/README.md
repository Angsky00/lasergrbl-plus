# Pruebas de G-code (lasergrbl-plus)

- `Inputs/`: archivos de entrada (SVG; el raster se genera en código para que sea determinista).
- `Golden/`: G-code de referencia generado con el código de LaserGRBL **sin modificar**
  (rama `master` del fork = `arkypita/LaserGRBL` 1f9337b) y la configuración por defecto.
  Se compara byte por byte (`.gitattributes` los marca `-text`).
- Cada corrida escribe la salida actual en `bin/<Config>/net48/gcode-out/`; en CI se sube como
  artefacto `gcode-generado-<sha>`.

Para regenerar una referencia (solo si el cambio de G-code es intencional y aprobado):
copiar el archivo de `gcode-out/` a `Golden/` y explicarlo en el PR.
