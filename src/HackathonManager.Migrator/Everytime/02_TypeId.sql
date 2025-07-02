-- Based on the implementation in https://github.com/jetify-com/typeid-sql

create or replace function typeid_encode(prefix text, id uuid)
returns text
as $$
begin
    return prefix || '_' || base32_encode(id);
end;
$$
language plpgsql
immutable
leakproof
parallel safe
returns null on null input;

comment on function typeid_encode(prefix text, id uuid) is 'Encodes a uuid and type prefix as a TypeId in the format prefix_2x4y6z8a0b1c2d3e4f5g6h7j8k.';

create or replace function uuid_from_typeid(typeid_str text)
returns uuid
as $$
declare
    suffix text;
begin
    if position('_' in typeid_str) = 0 then
        raise exception 'invalid typeid format';
    end if;

    suffix = split_part(typeid_str, '_', 2);
    return base32_decode(suffix);
end
$$
language plpgsql
immutable
leakproof
parallel safe
returns null on null input;

comment on function uuid_from_typeid(typeid_str text) is 'Decodes a typeid string, returning the uuid portion of the id.';

create or replace function uuid_typeid_eq_operator(lhs uuid, rhs text)
returns boolean
as $$
begin
    return (select lhs = uuid_from_typeid(rhs));
end;
$$
language plpgsql
immutable
leakproof
parallel safe
returns null on null input;
