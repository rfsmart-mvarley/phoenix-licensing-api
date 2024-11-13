CREATE TABLE IF NOT EXISTS $schema_name$.licensing_issued
(
    row_id                  int  NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    created                 timestamp with time zone NOT NULL,
    created_by              text COLLATE pg_catalog."default" NOT NULL,
    last_modified           timestamp with time zone NOT NULL,
    last_modified_by        text COLLATE pg_catalog."default" NOT NULL,    
    feature_name            text COLLATE pg_catalog."default" NOT NULL,
    enabled_time            timestamp with time zone NOT NULL,
    disabled_time           timestamp with time zone NOT NULL,
    users                   numeric NOT NULL DEFAULT 0
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS $schema_name$.licensing_issued
    OWNER to $owner$;

CREATE TABLE IF NOT EXISTS $schema_name$.licensing_tracking
(
    row_id                  int  NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    feature_name            text COLLATE pg_catalog."default" NOT NULL,
    users                   text[] NOT NULL DEFAULT array[]::text[],
    created                 timestamp with time zone NOT NULL
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS $schema_name$.licensing_tracking
    OWNER to $owner$;