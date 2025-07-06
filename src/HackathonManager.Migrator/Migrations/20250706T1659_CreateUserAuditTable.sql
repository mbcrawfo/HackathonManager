create schema enums;

create table enums.user_audit_events (
    id smallint primary key,
    name text not null unique
);

insert into enums.user_audit_events (id, name) values
(1, 'Registration'),
(2, 'EmailChanged'),
(3, 'DisplayNameChanged');

create table user_audit (
    id bigint primary key generated always as identity,
    user_id uuid not null references users (id),
    event smallint not null references enums.user_audit_events (id),
    timestamp timestamptz not null,
    metadata jsonb not null
);

comment on table user_audit is 'Tracks audit events related to user accounts.';

create index user_audit_user_id on user_audit (user_id);
create index user_audit_event on user_audit (event);
create index user_audit_timestamp on user_audit (timestamp);
