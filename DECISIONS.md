# DECISIONS

## Architecture
- Clean Architecture (Domain Driven Design).  
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
