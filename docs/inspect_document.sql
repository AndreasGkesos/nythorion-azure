-- ─────────────────────────────────────────────
-- Paste the document ID here and run all (F5)
-- ─────────────────────────────────────────────
SET app.doc_id = 'paste-document-id-here';

SELECT * FROM (

    -- Document
    SELECT
        1 AS sort,
        'Document' AS section,
        "Id"::text AS col1,
        "FileName" AS col2,
        CASE "Status"
            WHEN 0 THEN 'Uploading'
            WHEN 1 THEN 'Processing'
            WHEN 2 THEN 'Ready'
            WHEN 3 THEN 'Failed'
        END AS col3,
        "UploadedAt"::text AS col4,
        "FileSizeBytes"::text AS col5,
        NULL::text AS col6
    FROM documents
    WHERE "Id" = current_setting('app.doc_id')::uuid

    UNION ALL

    -- Chunks
    SELECT
        2,
        'Chunk',
        "ChunkIndex"::text,
        LEFT("Content", 200),
        CASE WHEN "Embedding" IS NULL THEN 'no embedding' ELSE 'has embedding' END,
        NULL, NULL, NULL
    FROM document_chunks
    WHERE "DocumentId" = current_setting('app.doc_id')::uuid

    UNION ALL

    -- Summary
    SELECT
        3,
        'Summary',
        "Id"::text,
        "Content",
        NULL, NULL, NULL, NULL
    FROM summaries
    WHERE "DocumentId" = current_setting('app.doc_id')::uuid

    UNION ALL

    -- Flashcards
    SELECT
        4,
        'Flashcard',
        "Id"::text,
        "Question",
        "Answer",
        NULL, NULL, NULL
    FROM flashcards
    WHERE "DocumentId" = current_setting('app.doc_id')::uuid

    UNION ALL

    -- Quiz questions
    SELECT
        5,
        'QuizQuestion',
        "Id"::text,
        "Question",
        array_to_string("Options", ' | '),
        "CorrectOptionIndex"::text,
        NULL, NULL
    FROM quiz_questions
    WHERE "DocumentId" = current_setting('app.doc_id')::uuid

    UNION ALL

    -- Notes
    SELECT
        6,
        'Note',
        "Id"::text,
        "Content",
        "CreatedAt"::text,
        "UpdatedAt"::text,
        NULL, NULL
    FROM notes
    WHERE "DocumentId" = current_setting('app.doc_id')::uuid

) AS results
ORDER BY sort, col1;
