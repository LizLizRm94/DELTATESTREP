# Catálogo de cargos de personal

El catálogo compartido `Contracts/CatalogoCargos.cs` contiene los ocho cargos proporcionados por el usuario en su imagen del 23 de noviembre de 2026. Sustituye las diez áreas provisionales investigadas anteriormente.

Los desplegables de Puesto actual y Puesto a rotar (Empleado) y Puesto solicitado (Pasante) usan este catálogo. Inspector como cargo no asigna permisos: el rol de la cuenta se gestiona de forma independiente.

La sección opcional `Cargos` de la configuración de la API permite sustituir el catálogo. El endpoint también conserva como opciones los puestos existentes en registros históricos. No se modifican usuarios ni se requieren migraciones; las selecciones se guardan en las columnas existentes de cada puesto.

El cliente inicia los desplegables con el mismo catálogo compartido y consulta las opciones del servidor. Si falla la consulta conserva las opciones disponibles y permite reintentar. Los permisos y las validaciones de guardado siguen aplicándose en la API. Reiniciar API y cliente después de compilar evita seguir recibiendo el catálogo de la versión anterior.
