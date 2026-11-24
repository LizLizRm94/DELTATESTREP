# Catálogo provisional de áreas farmacéuticas

Registro y edición usan temporalmente áreas funcionales como opciones de los puestos. No representan un organigrama validado de Laboratorios Delta ni cargos o títulos profesionales definitivos.

La API ofrece diez áreas predeterminadas en `Contracts/AreasFarmaceuticas.cs`. Para sustituirlas, definir un arreglo `Cargos` en la configuración de la API (por ejemplo `"Cargos": ["Área real 1", "Área real 2"]`). Se conservan como opciones los puestos ya guardados, para permitir editar registros históricos sin perderlos. No se modifican usuarios ni se requieren migraciones para cargar el catálogo. Las selecciones nuevas se guardan en las columnas existentes de cada puesto.

## Referencias

- OMS, buenas prácticas de fabricación: producción, aseguramiento/control de calidad, personal, instalaciones, almacenamiento y validación: https://www.who.int/publications/m/item/trs986-annex2
- FDA, enfoque de sistemas de calidad: producción, materiales, laboratorio, instalaciones/equipos y envasado/etiquetado: https://www.fda.gov/files/drugs/published/Quality-Systems-Approach-to-Pharmaceutical-Current-Good-Manufacturing-Practice-Regulations.pdf
- OMS, registro y autorización de comercialización: https://www.who.int/tools/health-products-regulation-and-prequalification-learning-catalogue/registration-and-marketing-authorization
- EMA, sistemas de farmacovigilancia: https://www.ema.europa.eu/en/documents/scientific-guideline/guideline-good-pharmacovigilance-practices-module-i-pharmacovigilance-systems-and-their-quality-systems_en.pdf

Las funciones se agruparon para obtener opciones sencillas en español. Su existencia no implica que todas deban ser departamentos separados en cada empresa.

El cliente inicia los desplegables de puestos con este mismo catálogo compartido y consulta las opciones del servidor. Si la consulta falla, conserva las opciones disponibles y permite reintentar; las validaciones y los permisos de guardado siguen aplicándose en la API.
