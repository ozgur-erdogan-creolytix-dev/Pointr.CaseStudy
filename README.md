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
- Publication Id can be random Guid; idempotency relies on the unique key.  

## Validation & Semantics
- **404** if page missing.  
- **400** if `publishDraft` doesn’t belong to that page.  
- On success always **204**.  
- All timestamps UTC; `CancellationToken` propagated.  

## Cache (bonus)
- `IMemoryCache` for `GetPublishedPage(siteId, slug)` — **60s hit**, **10s negative**.  
- After archive/publish: always invalidate; warm only if a new publication was created.  


# Pointr Case Study – Run Guide

## Prerequisites
- Docker Desktop

## Option A — Docker Compose
```bash
# from the repository root
docker compose up --build
```

## Option B — Visual Studio

### Run with docker-compose (easiest in VS)
1. Right-click the **docker-compose** project → **Set as Startup Project**  
2. Select the **docker-compose** profile from the dropdown  
3. Run the project  


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

```

## Postman Test Samples

All requests are `DELETE`:

- **Idempotency**
- http://localhost:8080/api/v1/sites/6f4a0001-0000-0000-0000-000000000001/pages/about?publishDraft=2

- **400 (wrong draft)**  
- http://localhost:8080/api/v1/sites/6f4a0001-0000-0000-0000-000000000001/pages/contact?publishDraft=2


- **404 (missing page)**  
- http://localhost:8080/api/v1/sites/6f4a0001-0000-0000-0000-000000000001/pages/missing?publishDraft=2

- **No PublishDraft**
- http://localhost:8080/api/v1/sites/6f4a0001-0000-0000-0000-000000000001/pages/legal

- **Concurrency**  
http://localhost:8080/api/v1/sites/6f4a0001-0000-0000-0000-000000000001/pages/about?publishDraft=2
### Pre-request Script For Postman(for Concurrency Testing)
When two concurrent requests are sent, the outcome can vary depending on timing. Sometimes both return 204, because the second request finds the resource already in the desired state and the database does not register a conflict (idempotent behavior).

With three concurrent requests, the overlap window is larger. At least one request will inevitably attempt to update a stale RowVersion, triggering a DbUpdateConcurrencyException. After the single retry fails, it is converted into a 409 Conflict, which is the expected result.
```javascript
// === Concurrency spike: send N parallel DELETEs ===
const N = 3;
const url = "http://localhost:8080/api/v1/sites/6f4a0001-0000-0000-0000-000000000001/pages/about?publishDraft=2";

let done = 0;
const codes = [];

function finalize() {
if (done !== N) return;
codes.sort();
pm.test("Concurrency responses are either [204,204] or [204,409]", function () {
  const ok =
    (codes.length === 2 && ((codes[0] === 204 && codes[1] === 204) || (codes[0] === 204 && codes[1] === 409))) ||
    (codes.length > 2 && codes.includes(204) && (codes.includes(409) || codes.every(c => c === 204)));
  pm.expect(ok).to.be.true;
});
}

// Fire N parallel DELETEs
for (let i = 0; i < N; i++) {
pm.sendRequest({ url, method: "DELETE", header: { Accept: "application/json" } }, (err, res) => {
  if (err) console.log("err", err);
  else codes.push(res.code);
  done++;
  finalize();
});
}
```

