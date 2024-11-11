CREATE TABLE IF NOT EXISTS $schema_name$.licensing_issued
(
    row_id                  int  NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    created                 timestamp with time zone NOT NULL,
    created_by              text COLLATE pg_catalog."default" NOT NULL,
    last_modified           timestamp with time zone NOT NULL,
    last_modified_by        text COLLATE pg_catalog."default" NOT NULL,    
    license_name            text COLLATE pg_catalog."default" NOT NULL,
    enabled_time            timestamp with time zone NOT NULL,
    disabled_time           timestamp with time zone NOT NULL,
    users                   numeric NOT NULL DEFAULT 0
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS $schema_name$.licensing_tracking
    OWNER to $owner$;

CREATE TABLE IF NOT EXISTS $schema_name$.licensing_tracking
(
    row_id                  int  NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    created                 timestamp with time zone NOT NULL,
    created_by              text COLLATE pg_catalog."default" NOT NULL,
    license_name            text COLLATE pg_catalog."default" NOT NULL,
    event_name              text COLLATE pg_catalog."default" NOT NULL,
    role_name               text COLLATE pg_catalog."default" NOT NULL
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS $schema_name$.licensing_tracking
    OWNER to $owner$;