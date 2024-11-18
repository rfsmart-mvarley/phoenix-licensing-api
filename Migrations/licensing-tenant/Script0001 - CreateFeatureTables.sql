CREATE TABLE IF NOT EXISTS $schema_name$.feature_issued
(
    row_id                  int  NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    created                 timestamp with time zone NOT NULL,
    created_by              text COLLATE pg_catalog."default" NOT NULL,
    last_modified           timestamp with time zone NOT NULL,
    last_modified_by        text COLLATE pg_catalog."default" NOT NULL,    
    feature_name            text COLLATE pg_catalog."default" NOT NULL,
    enabled_time            timestamp with time zone NOT NULL,
    disabled_time           timestamp with time zone,
    licensed_users          numeric NOT NULL DEFAULT 0
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS $schema_name$.feature_issued
    OWNER to $owner$;

CREATE INDEX feature_issued_feature_name_idx ON $schema_name$.feature_issued 
 USING btree (feature_name, created DESC);

CREATE TABLE IF NOT EXISTS $schema_name$.feature_tracking
(
    row_id                  int  NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    feature_name            text COLLATE pg_catalog."default" NOT NULL,
    users                   text[] NOT NULL DEFAULT array[]::text[],
    created                 timestamp with time zone NOT NULL
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS $schema_name$.feature_tracking
    OWNER to $owner$;

CREATE INDEX feature_tracking_feature_name_idx ON $schema_name$.feature_tracking 
 USING btree (feature_name, created DESC);

CREATE INDEX feature_tracking_created_idx ON $schema_name$.feature_tracking 
 USING BRIN (created)
 WITH (pages_per_range = 32);