# CI/CD

GitHub Actions builds, tests and publishes the API image. Workflows live in [.github/workflows](../.github/workflows).

| File | Role |
|------|------|
| [ci-smart-inventory-api.yml](../.github/workflows/ci-smart-inventory-api.yml) | Entry point. Defines triggers, concurrency and the inputs for the API. |
| [build-dotnet.yml](../.github/workflows/build-dotnet.yml) | Reusable workflow (`workflow_call`) that restores, builds, tests and optionally publishes a Docker image. |

## Triggers

- `push` to `main`, `pull_request` targeting `main`, and manual `workflow_dispatch`.
- Only runs when something relevant changes: `src/**`, `tests/**`, `seeds/**`, `SmartInventoryManagement.slnx` or the workflow files. Documentation and `scripts/**` changes do not trigger CI.
- Concurrency is grouped per ref. A new push cancels an in-progress **pull request** run, but a publish run on `main` is never cancelled.

## Pipeline

Runs on `ubuntu-latest` with .NET `10.0.x` and the NuGet cache enabled.

1. `dotnet restore` on `SmartInventoryManagement.slnx`
2. `dotnet build -c Release --no-restore`
3. `dotnet test -c Release --no-build`. This runs the unit tests and the Testcontainers integration tests, which use the Docker daemon that GitHub-hosted Ubuntu runners provide, so no service containers are configured in the workflow.
4. **Publish (only on a push to `main`):** log in to GitHub Container Registry and build and push the image from [src/SmartInventory.API/Dockerfile](../src/SmartInventory.API/Dockerfile).

Pull requests run steps 1-3 only. They never push images.

## Published image

```text
ghcr.io/<owner>/smartinventory-api:latest
ghcr.io/<owner>/smartinventory-api:sha-<commit>
```

The image carries `org.opencontainers.image.source` and `org.opencontainers.image.revision` labels. Authentication uses the built-in `GITHUB_TOKEN` (`packages: write`), so no extra secrets are needed.

## Reproducing CI locally

```powershell
dotnet restore SmartInventoryManagement.slnx
dotnet build SmartInventoryManagement.slnx -c Release --no-restore
dotnet test SmartInventoryManagement.slnx -c Release --no-build   # Docker must be running
.\scripts\builds\build-images.ps1                                  # local image: smartinventory-api:1.0
```

[build-images.ps1](../scripts/builds/build-images.ps1) builds the same Dockerfile and context as CI, but tags the image `smartinventory-api:<Version>` locally instead of the registry tags above.

## Troubleshooting

- **Integration tests fail to start locally:** make sure Docker is running. The tests start SQL Server, Redis and LocalStack containers, and the first run pulls the images.
- **A docs-only PR shows no CI run:** this is expected because of the path filters.
- **A change to `scripts/**` should run CI:** add the path to both the `push` and `pull_request` filters in `ci-smart-inventory-api.yml`.
