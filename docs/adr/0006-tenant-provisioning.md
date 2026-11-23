# ADR 0006: Tenant Provisioning and Platform Administration

## Status

Proposed

## Date

2026-11-26

## Context

SmartInventory is evolving from a tenant-aware Inventory API into a multi-tenant SaaS application. Tenant creation, onboarding, invitations, and tenant-membership administration are platform-level responsibilities. These operations do not require an inventory tenant context and should not introduce platform-administrator exceptions into the Inventory API's existing authentication and tenant-resolution pipeline.

The Inventory API currently authenticates users using its existing custom JWT flow and relies on tenant context and `TenantMembership` to authorize tenant-scoped access. That pipeline should remain tenant-oriented.

We need a way to administer tenants, invite their initial administrators and other users, manage memberships and roles, and inspect or retry provisioning operations. A platform administrator must be able to perform these tasks without being a member of an inventory tenant.

## Decision

### 1. Introduce a separate Provisioning API and SPA

Create a dedicated Provisioning API and a small SPA for platform-level tenant administration. The API and SPA will be separate application hosts/boundaries from the Inventory API and its UI (if/when an inventory UI is built).

The Provisioning API will be responsible for platform and tenant-lifecycle operations, including:

- Creating tenants and managing tenant lifecycle/status.
- Creating and managing `TenantMembership` records and tenant-scoped roles.
- Issuing, expiring, and resending invitations, including the initial tenant administrator invitation.
- Orchestrating tenant provisioning, recording its status, and supporting retries where appropriate.
- Providing the administration UI with tenant, membership, invitation, and provisioning-operation information.

The Inventory API remains responsible for inventory business capabilities and tenant-scoped access. It will not gain a platform-administrator role or tenantless-login exception to support provisioning.

### 2. Use Microsoft Entra ID for Provisioning authentication

Microsoft Entra ID will be the identity provider for the Provisioning API. Platform administrators will authenticate through Entra ID; the Provisioning API will authorize platform administration using a dedicated role/policy (for example, `PlatformAdmin`) assigned to appropriately restricted identities or groups.

This does not replace or modify the Inventory API's current custom JWT authentication flow as part of this ADR. The two APIs will validate tokens for their own intended audiences and enforce their own authorization policies. Sharing an identity provider does not mean sharing an authorization boundary.

The Provisioning API will not implement a separate local username/password authentication system.

### 3. Keep the existing monorepo and reuse shared assemblies

The Provisioning API will be added to the existing GitHub monorepo. Reuse the existing `SmartInventory.Domain` and `SmartInventory.Infrastructure` assemblies where doing so is appropriate to reduce duplicate code and prevent divergent tenant/membership definitions.

Reuse of shared code does not imply that the APIs must share controllers, endpoint contracts, authentication configuration, authorization policies, or application boundaries. Provisioning-specific logic and contracts should live in its own projects/modules.

### 4. Persist platform operations directly; no API-to-API calls initially

The Provisioning API will persist tenant and membership lifecycle changes directly to the database through its own application/persistence configuration. It may share the existing database infrastructure initially, but logical ownership of writes will be explicit:

- **Provisioning API:** `Tenant`, `TenantMembership`, invitations, and provisioning-operation state.
- **Inventory API:** inventory-domain entities and inventory business rules.

The Inventory API may read membership/tenant data as required for tenant resolution and authorization, but tenant and membership lifecycle changes are owned by Provisioning.

No synchronous API-to-API calls are required for the initial implementation. On-Behalf-Of (OBO) and other service-to-service communication patterns are deferred until a concrete cross-API use case justifies them. Database separation is also deferred.

### 5. Provisioning workflow

Tenant creation will be treated as a provisioning workflow rather than only a `Tenant` insert. The workflow should be designed to be retryable and idempotent, and should record progress/failure so a partially completed operation is not silently treated as successful.

The initial workflow is expected to cover tenant creation, default tenant data as needed, initial administrator invitation and membership setup, and a clear provisioning completion state. Detailed step orchestration, invitation acceptance, delivery mechanics, and retry strategy will be specified during implementation or in a follow-up decision if they require additional architectural choices.

## Alternatives

### Add platform-administrator endpoints to the Inventory API

Rejected. It would mix platform-level and tenant-level security concerns and require exceptions in the Inventory API's tenant-oriented authentication/authorization pipeline.

### Add a platform identity type or special-case role to the Inventory API

Rejected for this scope. A tenantless platform identity or cross-tenant administrator role would weaken the simplicity of the current tenant-context assumptions and couple inventory authentication to platform administration.

### Build a second local username/password authentication system for Provisioning

Rejected. Entra ID will provide authentication, avoiding a second password-management and account-security implementation.

### Have Provisioning call the Inventory API for all provisioning actions

Deferred. There is no current need for API-to-API calls. Direct database persistence is simpler for the initial workflow, provided data ownership and invariants are explicit. OBO or another service-to-service approach can be evaluated when a real cross-API use case emerges.

### Split the platform/tenant registry into a separate database now

Deferred. Separate APIs and ownership boundaries can be established without introducing database separation yet. Database-per-tenant or other database isolation strategies remain future architectural options.

## Consequences

### Benefits

- Keeps the Inventory API's authentication and tenant-resolution pipeline tenant-oriented, without platform-admin exceptions.
- Separates platform administration and tenant lifecycle responsibilities from inventory business capabilities.
- Uses Entra ID for platform authentication rather than implementing a second local password system.
- Provides a focused administration UI for tenant onboarding, invitations, memberships, and provisioning status.
- Establishes explicit ownership of tenant and membership writes.
- Preserves the monorepo and allows reuse of shared domain/infrastructure code.
- Leaves room for independent deployment, API-to-API communication, and database separation later without requiring them now.

### Trade-offs

- Adds another API host, SPA, configuration surface, and deployment unit to maintain.
- Requires separate authorization configuration and token audience validation for the Provisioning API.
- Shared assemblies must be managed carefully so reuse does not create accidental coupling between API boundaries.
- Provisioning writes directly to shared persistence, so the ownership rules and consistency invariants must be maintained.

### Risks and mitigations

- **Platform access is overly broad:** restrict the Entra app role/group assignment and enforce platform-admin authorization on all provisioning endpoints.
- **Token accepted by the wrong API:** configure and validate API-specific audiences/scopes; do not accept Inventory tokens as Provisioning tokens or vice versa.
- **Partial provisioning or duplicate requests:** persist provisioning-operation state and use idempotent operations; expose failures and retry deliberately rather than activating incomplete tenants.
- **Shared persistence coupling:** keep write ownership explicit and avoid allowing the Inventory API to mutate platform lifecycle records.
- **Invitation identity mismatch or abuse:** use secure, expiring invitation tokens and validate invitation acceptance against the intended tenant/email and membership state; implementation details will be defined when the invitation flow is designed.

## Related ADRs

- ADR 0001: Startup decoupling / provisioning-runner foundations (reuse where applicable).
- ADR 0002: Tenancy model.
- ADR 0004: Tenant resolution.
- ADR 0005: Tenant isolation.
