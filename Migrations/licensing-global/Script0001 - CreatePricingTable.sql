CREATE TABLE IF NOT EXISTS features.pricing
(
    row_id                  int  NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    feature_name            text COLLATE pg_catalog."default" NOT NULL,
    price_per_user          numeric(5,2) NOT NULL DEFAULT 0,
    price_on_demand         numeric(5,2) NOT NULL DEFAULT 0,
    enforced_from           timestamp with time zone NOT NULL,
    enforced_until          timestamp with time zone NOT NULL,
    created                 timestamp with time zone NOT NULL,
    created_by              text COLLATE pg_catalog."default" NOT NULL,
    last_modified           timestamp with time zone NOT NULL,
    last_modified_by        text COLLATE pg_catalog."default" NOT NULL,
    is_active               boolean NOT NULL DEFAULT false
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS features.pricing
    OWNER to $owner$;