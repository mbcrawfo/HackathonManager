-- Creates an equality operator for comparing a uuid and a typeid string.
create operator ===( -- noqa: LT01
    leftarg = uuid,
    rightarg = text,
    function = uuid_typeid_eq_operator,
    commutator = ===, -- noqa: LT01
    negator = !==, -- noqa: LT01
    hashes,
    merges
);
