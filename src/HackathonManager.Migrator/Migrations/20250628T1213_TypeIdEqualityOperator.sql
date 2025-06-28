create operator ===(
    leftarg = uuid,
    rightarg = text,
    function = uuid_typeid_eq_operator,
    commutator = ===,
    negator = !==,
    hashes,
    merges
);
