create table users (
    id uuid primary key,
    created_at timestamptz not null default (now() at time zone 'utc'),
    email text not null unique check (length(email) between 3 and 254),
    display_name text not null unique check (length(display_name) between 2 and 100),
    password_hash text not null check (length(password_hash) < 1000)
);

comment on table users is 'Core user account information.';
