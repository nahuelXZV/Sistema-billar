# Reglas para el código generado

- Se debe seguir estrictamente el formato de archivos y la nomenclatura existentes en todo el proyecto.
- No se deben instalar librerías externas sin la autorización expresa del usuario.
- No se debe modificar la configuración de ningún proyecto sin la autorización expresa del usuario.
- El formato del código debe ser consistente con las demás clases: los parámetros de funciones o clases deben permanecer en una sola línea, sin saltos de línea entre cada parámetro.
- No se debe ejecutar el proyecto sin la autorización del usuario.
- La compilación solo se debe intentar una vez mediante el procedimiento normal. Si no es posible completarla, no se debe volver a intentar.
- Antes de crear una clase CSS, se debe verificar si ya existe un estilo reutilizable en `theme.cs`. No se deben crear clases repetidas para cada vista; solo se podrá crear una clase de estilos nueva en casos específicos y con la aprobación previa del usuario.
- En la configuración de las entidades, solo se debe incluir lo mínimo necesario: nombre y esquema, clave primaria y relaciones. Cualquier configuración adicional requiere una solicitud explícita del usuario.
- Las clases de validación deben ubicarse en `Domain/Validators`, no dentro de cada funcionalidad ni en el frontend. Estas clases deben reutilizarse tanto en la API como en los formularios.
