START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    ALTER TABLE "CONVOCATORIA" ADD codigo character varying(60) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    ALTER TABLE "CONVOCATORIA" ADD contacto character varying(500) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    ALTER TABLE "CONVOCATORIA" ADD creada_por_usuario_id integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    ALTER TABLE "CONVOCATORIA" ADD creada_utc timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    ALTER TABLE "CONVOCATORIA" ADD cronograma character varying(10000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    ALTER TABLE "CONVOCATORIA" ADD descripcion character varying(10000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    ALTER TABLE "CONVOCATORIA" ADD entidad_emisora character varying(200) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    ALTER TABLE "CONVOCATORIA" ADD fecha_cierre date;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    ALTER TABLE "CONVOCATORIA" ADD instrucciones character varying(10000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    ALTER TABLE "CONVOCATORIA" ADD lugar character varying(500) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    ALTER TABLE "CONVOCATORIA" ADD modalidad character varying(10000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    ALTER TABLE "CONVOCATORIA" ADD objetivo character varying(10000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    ALTER TABLE "CONVOCATORIA" ADD participantes character varying(10000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    ALTER TABLE "CONVOCATORIA" ADD publicada boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    ALTER TABLE "CONVOCATORIA" ADD requisitos character varying(10000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    ALTER TABLE "CONVOCATORIA" ADD resumen character varying(500) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    CREATE UNIQUE INDEX "IX_CONVOCATORIA_codigo" ON "CONVOCATORIA" (codigo) WHERE codigo <> '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    CREATE INDEX "IX_CONVOCATORIA_publicada_fecha_convocatoria_creada_utc" ON "CONVOCATORIA" (publicada, fecha_convocatoria, creada_utc);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122224930_AmpliarConvocatoriaFormal') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261122224930_AmpliarConvocatoriaFormal', '8.0.22');
    END IF;
END $EF$;
COMMIT;

