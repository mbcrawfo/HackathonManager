create table test (id uuid primary key, name text not null, description text);

insert into test (id, name) values
(uuidv7(), 'foo'),
(uuidv7(), 'bar'),
(uuidv7(), 'baz'),
(uuidv7(), 'qux');
