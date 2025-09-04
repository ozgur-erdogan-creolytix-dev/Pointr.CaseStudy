# DECISIONS

## Architecture
- Clean Architecture (DDD-lite).  
- Domain holds entities; Application hosts the use-case handler; Infrastructure provides EF Core (Npgsql) and caching.  
- Generic `IGenericRepository` + `IUnitOfWork`; query objects (`…By…Query`) instead of ad-hoc LINQ.  

## Contract
- Kept existing route/verb:  
  `DELETE /api/v1/sites/{siteId}/pages/{slug}`  
- Minimal controller; main logic in `ArchiveAndOptionallyPublishHandler`.  

## Persistence & Concurrency
- `RowVersion (bytea)` is the concurrency token.  
- Archive + optional publish run in one transaction.  
- On `DbUpdateConcurrencyException`, retry once; if it still conflicts → return **409**.  

## Idempotency
- `UNIQUE(PageId, DraftId)` guarantees publish is at most once; repeated calls are no-ops.  
- Publication Id can be random `Guid.NewGuid()`; idempotency relies on the unique key.  

## Validation & Semantics
- **404** if page missing.  
- **400** if `publishDraft` doesn’t belong to that page.  
- On success always **204**.  
- All timestamps UTC; `CancellationToken` propagated.  

## Cache (bonus)
- `IMemoryCache` for `GetPublishedPage(siteId, slug)` — **60s hit**, **10s negative**.  
- After archive/publish: always invalidate; warm only if a new publication was created.  



## Test Data

You can use the following SQL scripts for testing:

```sql
-- =========================
-- PAGES (minimal)
-- =========================
-- Fixed SiteId: 6f4a0001-... (ACME)
INSERT INTO "Pages" ("Id","SiteId","Slug","IsArchived","UpdatedUtc","RowVersion") VALUES
('6f4a1002-0000-0000-0000-000000000002','6f4a0001-0000-0000-0000-000000000001','about',   false, NOW() - INTERVAL '1 day', gen_random_bytes(8)),
('6f4a1003-0000-0000-0000-000000000003','6f4a0001-0000-0000-0000-000000000001','contact', false, NOW() - INTERVAL '2 days', gen_random_bytes(8)),
('6f4a1004-0000-0000-0000-000000000004','6f4a0001-0000-0000-0000-000000000001','blog',    false, NOW() - INTERVAL '3 days', gen_random_bytes(8))
ON CONFLICT DO NOTHING;

INSERT INTO "Pages" ("Id","SiteId","Slug","IsArchived","UpdatedUtc","RowVersion")
VALUES (
  '6f4a1bbb-0000-0000-0000-000000000001',
  '6f4a0001-0000-0000-0000-000000000001',
  'legal',
  false,
  NOW() - INTERVAL '1 day',
  gen_random_bytes(8)
)
ON CONFLICT DO NOTHING;

-- =========================
-- DRAFTS (minimal)
-- =========================
-- about: draft #2 exists → used for publish tests
INSERT INTO "PageDrafts" ("Id","PageId","DraftNumber","Content") VALUES
('7f4a1002-0000-0000-0000-000000000001','6f4a1002-0000-0000-0000-000000000002',1,'About v1'),
('7f4a1002-0000-0000-0000-000000000002','6f4a1002-0000-0000-0000-000000000002',2,'About v2')
ON CONFLICT DO NOTHING;

-- contact: only draft #1 → passing publishDraft=2 should return 400
INSERT INTO "PageDrafts" ("Id","PageId","DraftNumber","Content") VALUES
('7f4a1003-0000-0000-0000-000000000001','6f4a1003-0000-0000-0000-000000000003',1,'Contact v1')
ON CONFLICT DO NOTHING;

-- blog: draft #3 exists → together with a publication to validate already-published/no-op scenario
INSERT INTO "PageDrafts" ("Id","PageId","DraftNumber","Content") VALUES
('7f4a1004-0000-0000-0000-000000000003','6f4a1004-0000-0000-0000-000000000004',3,'Blog v3')
ON CONFLICT DO NOTHING;

-- =========================
-- PUBLICATIONS (minimal)
-- =========================
-- blog draft #3 is already published → useful to validate publish no-op (optional)
INSERT INTO "PagePublications" ("Id","PageId","DraftId","PublishedUtc") VALUES
('8f4a1004-0000-0000-0000-000000000003',
 '6f4a1004-0000-0000-0000-000000000004',
 '7f4a1004-0000-0000-0000-000000000003',
 NOW() - INTERVAL '2 days')
ON CONFLICT DO NOTHING;
