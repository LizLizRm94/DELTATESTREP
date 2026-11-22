START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122231506_ConservarDetalleYPuntajes') THEN
    ALTER TABLE "PREGUNTA" ADD puntos_obtenidos integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122231506_ConservarDetalleYPuntajes') THEN
    ALTER TABLE "EVALUACION" ADD detalle_practico_json text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261122231506_ConservarDetalleYPuntajes') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261122231506_ConservarDetalleYPuntajes', '8.0.22');
    END IF;
END $EF$;
COMMIT;

