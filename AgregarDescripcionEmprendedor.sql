-- PITCHR: el emprendedor ahora tiene "Editar Perfil" igual que el inversor (nombre, descripción y foto).
-- Su descripción se guarda en esta columna nueva. Correr UNA sola vez.
-- Si sale el error 1060 "Duplicate column name 'description'", es que ya existe y no hay que hacer nada.
ALTER TABLE integrative.entrepreneur ADD COLUMN description VARCHAR(500) NULL;
