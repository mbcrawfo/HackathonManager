create or replace function set_updated_at()
returns trigger
language plpgsql
as
$$
begin
    new.updated_at = now() at time zone 'utc';
    return new;
end;
$$;

comment on function set_updated_at() is 'Used in triggers to set the table''s updated_at timestamp.';
