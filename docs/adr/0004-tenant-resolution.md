# ADR 0004: Tenant Resolution

## Status: 
Accepted

## Date: 
2026-11-16

## Context
ADR 0002 defined the pool tenancy model and ADR 0003 (expand, backfill, contract) moved the database and EF infrastructure to support tenants. Every tenant-scoped row now carries a `TenantId`, but the API still has no notion of "the current tenant".

This ADR covers the API and Application layers: how a request is mapped to a tenant, how the user's membership in that tenant is authenticated and authorized, and how the result is exposed to the rest of the code through `ITenantContext`.

Two phases need different answers:
- **Before authentication** (login, refresh, health): the client has no token, so the tenant can only be *discovered* from the request.
- **After authentication**: the JWT is the source of truth for the tenant. Anything the client sends on the request is only a hint and must never override the token (the cross-tenant leak that ADR 0002's Risks section is most worried about).


## Decision
### 1. Discovery: subdomain first, header fallback (`TenantResolutionMiddleware`)
- The slug is resolved with a chain, implemented once in `TenantUtils.GetSlug` so both middleware use the same rule:
  1. `Host` header of the form `<slug>.<BaseDomain>` (prod: `acme.smartinventory.io`).
  2. Otherwise the `X-Tenant-Slug` header (dev, API clients, `.http` files, integration tests).
  3. Otherwise no slug: the request continues without tenant (health, swagger, and so on).
- The chain is not "subdomain XOR header". Both are supported so the same code runs on `localhost` and in production. `BaseDomain` and the header name come from `TenantResolution` settings.
- The slug is trimmed and lower-cased, then validated against `^[a-z0-9](?:[a-z0-9-]{0,48}[a-z0-9])?$` (max 50, same as `Tenant.Slug`) before any lookup. An invalid slug returns `400 tenant_slug_invalid`.
- A valid slug is looked up (`GetTenantBySlugQuery`). Unknown slug returns `404 tenant_not_found`. A tenant whose status is not `Active` returns `403 tenant_inactive`.
- On success the middleware calls `TenantContext.Set(tenantId)`: `TenantId` is populated but `IsAvailable` stays `false`. This "resolved, not yet authorized" state is only meant for the anonymous session endpoints.

### 2. Token generation is tenant-bound
- `JwtTokenService.GenerateAccessToken` requires an active `TenantMembership` for the resolved tenant and adds two claims: `tenantid` (Guid) and `tenantrole` (`TenantRole`). Without a resolved tenant or an active membership the token is not issued (`403 tenant_access_denied`).
- The tenant comes from the resolved slug (step 1), not from the request body. The same rule applies to the refresh-token flow because it also calls `GenerateAccessToken`.

### 3. Authenticated resolution: JWT hint + cached DB validation (`TenantContextMiddleware`)
- Two middleware, one per phase. Pipeline order in `Program.cs`: exception handler, rate limiter, CORS, authentication, authorization, `TenantResolutionMiddleware`, `TenantContextMiddleware`.
- Authorization requires authentication by default, including endpoints without `[Authorize]`. `TenantContextMiddleware` validates every matched endpoint except those explicitly marked `[AllowAnonymous]` or `.AllowAnonymous()`.
- It takes `tenantid` and `tenantrole` from the token, then loads the user's membership joined to the tenant (`GetTenantMembershipsQuery`, cached in Garnet for 30 s, keyed by tenant and user). The request is rejected when:
  - a claim is missing or invalid (`401 tenant_context_required`);
  - there is no membership, the membership is inactive, or its role differs from the token claim (`403 tenant_access_denied`);
  - the tenant is not `Active` (`403 tenant_inactive`);
  - a slug was sent (subdomain or header) and it differs from the token's tenant (`403 tenant_mismatch`).
- This covers a user removed from a tenant mid-session, a suspended tenant, and a soft-deleted membership, within the cache TTL rather than at token expiry.
- On success `TenantContext.Initialize(tenantId, role)` sets `IsAvailable = true`. Tenant-scoped writes fail closed when `IsAvailable` is `false`, and there is no sentinel tenant (ADR 0002 §2.3).
- Client-facing errors expose only a stable `errorCode` and a safe message (`TenantErrorCodes.GetPublicMessage`). Internal detail goes to the logs only. 

### 4. Scope and deferrals
- Integration tests use the legacy tenant (`SystemTenants.LegacyTenantId`) for now. A default tenant from the seed data can replace it later.
- A login without a tenant is not supported for now. A global or system login (and the `IsTenantScoped = false` operator mode) will be decided later.
- A tenant-switching endpoint (listed in ADR 0002's roadmap for this milestone) is not part of this ADR. Today a user changes tenant by logging in again against another slug.
- Application-level caches keyed by tenant (products, warehouses, stocks) are not changed here. That is ADR 0005.
- The `HasQueryFilter` implementation and tenant read isolation are ADR 0005.

## Alternatives
- **Subdomain only**: the production-realistic choice, but it breaks `localhost`, `.http` files and API clients. Rejected in favour of subdomain plus header fallback. `acme.lvh.me` (wildcard DNS to 127.0.0.1) allows the real subdomain experience locally.
- **Header only**: simple, but loses branding, per-subdomain cookie isolation and the natural tenant-switch UX in production.
- **Tenant in the URL path** (`/t/{slug}/...`): explicit, but changes every route and API version. Not worth it for this API.
- **JWT only, no DB validation**: cheapest, but a removed member or suspended tenant keeps access until the token expires (ADR 0002 "JWT-claim staleness").
- **DB-only, no tenant claim in the JWT**: always fresh, but needs a lookup on every request with no cheap hint to validate against. A short-lived cache gives almost the same freshness.
- **Letting the header override the token on authenticated calls**: rejected. It is the cross-tenant leak this ADR exists to prevent.

## Consequences
### Benefits
- The same code works on `localhost`, behind a wildcard domain, and for API clients.
- The tenant of an authenticated request always comes from the token. A client-supplied slug can only be rejected, never obeyed.
- Removed members and suspended tenants lose access within the cache TTL (30 s) instead of at token expiry.
- Tenant errors have stable machine-readable codes and do not leak internal detail. 
### Trade-offs
- Every authenticated request pays one cache lookup, and one DB lookup on a miss.
- Revocation is eventually consistent: up to 30 s for membership and role changes, and the slug-to-tenant lookup uses the cache default TTL (300 s).
- A role change forces the user to refresh or log in again, because a token whose `tenantrole` differs from the DB is rejected.
- Login cannot work without a tenant until a system login is designed. 
### Risks
- Tenant isolation depends on `TenantContextMiddleware` validating every protected endpoint. Until ADR 0005 adds query filters, incorrect anonymous endpoint metadata can still permit unscoped reads.
- Tenant resolution can be probed anonymously on endpoints explicitly marked anonymous (see note 6).
- Behind a reverse proxy or gateway the `Host` header and client IP may not be what the middleware expects (see note 8).

## Related ADRS
ADR 0002: Multi-Tenant Isolation Model
ADR 0003: Tenant expand, backfill, contract
ADR 0005 (planned): Tenant read isolation (`HasQueryFilter`) and tenant-aware application caches

# Implementation notes

Concerns and improvements found while comparing this ADR with the current code. They have not been triaged: each may or may not be fixed in this ADR. Items are grouped by theme and roughly ordered by impact.

## Isolation and security
Defered to next ADR
1. **Refresh token is not tenant-bound.** `RefreshToken` has no `TenantId`, and `/refresh-token` is anonymous, so the tenant of the new access token comes from whatever slug the caller sends. A refresh cookie issued for tenant A can mint a token for tenant B, if the user has a membership there, without re-entering the password. This is an implicit tenant switch. Either store the tenant on the refresh token and require it to match, or make switching an explicit, audited endpoint (see note 13).

## Resolution behaviour
Defered to next ADR:
2. **Proxy and gateway support.** The code reads `Request.Host` directly and the rate limiter partitions by client IP. Behind a load balancer, `ForwardedHeaders` (`X-Forwarded-Host` / `X-Forwarded-For`) must be configured and the known proxies restricted, otherwise tenant resolution and rate limiting break or can be spoofed.
3. **Subdomain parsing edge cases.** `a.b.<BaseDomain>` becomes slug `a.b` and fails with `400`. Reserved subdomains (`www`, `api`, `admin`) become slugs and return `404`. The base-domain equality check also means the apex domain and bare IPs always fall back to the header. Decide the rules, add a reserved-slug list to slug creation, and test them.

Defered too th SPA development ADR
4. **Subdomains and the refresh cookie.** The cookie is `SameSite=Strict`, `Secure`, with no `Domain`, so it is already per-host. Confirm that is what the SPA expects when the API and SPA live on different subdomains.

## Design gaps
Defered to next ADR:
6. **No tenant-switch flow.** ADR 0002's roadmap lists it for this milestone and this ADR defers it. Users with several memberships need a way to list them and switch. It should reuse note 2's rule (an explicit endpoint, not an implicit refresh).

## Observability and tests
Dismissed for now
7. **Metrics.** Only `401` is counted. Add counters per tenant error code (`mismatch`, `inactive`, `access_denied`) to spot probing or misconfigured clients. Avoid tenant slug as a label (cardinality).
