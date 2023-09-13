create table main.categories
(
    id            integer not null
        constraint categories_pk
            primary key autoincrement,
    name          TEXT    not null,
    description   text,
    deckCount     integer not null default 0,
    imageFilename text    not null default 'default.png'
);

create table main.decks
(
    id             integer not null
        constraint decks_pk primary key autoincrement,
    name           text    not null,
    description    text,
    flashcardCount integer not null default 0,
    categoryId     integer not null
        constraint decks_categories_id_fk references categories (id) on delete cascade
);

create table main.flashcards
(
    id       integer not null
        constraint flashcards_pk
            primary key autoincrement,
    question text    not null,
    answer   text    not null,
    deckId   integer not null
        constraint flashcards_decks_id_fk references decks (id) on delete cascade
);

create table main.challenges
(
    id integer not null
        constraint challenge_pk
            primary key autoincrement,
    deckId integer not null
        constraint challenge_decks_id_fk references decks(id) on delete cascade,
    state integer not null default 0,
    currentStepIndex integer not null default 0,
    createdAt text not null default CURRENT_TIMESTAMP,
    updatedAt text not null default CURRENT_TIMESTAMP
);

create table main.steps
(
    id integer not null
        constraint step_pk
            primary key autoincrement,
    stepIndex integer not null,
    needsReview integer not null default 0,
    flashcardId integer not null
        constraint step_flashcards_id_fk references flashcards(id) on delete cascade,
    challengeId integer not null
        constraint step_challenges_id_fk references challenges(id) on delete cascade
);

create table main.profiles
(
    id             integer not null
        constraint profiles_pk
            primary key autoincrement,
    userId         text    not null,
    name           text    not null,
    avatar         text    not null default 'default.png',
    preferredTheme text    not null default 'light'
);

create table main.avatars
(
    id   integer not null
        constraint avatars_pk
            primary key autoincrement,
    name text    not null
);
