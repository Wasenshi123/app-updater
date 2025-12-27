# App Updater

A Cross-platform updater targeted for Windows, Mac, Linux, and Linux-Arm platforms. Use together with the update server: https://github.com/Wasenshi123/app-update-server

## Development

- **Language**: C# (.NET 6.0)
- **UI Framework**: Avalonia UI

## Release Workflow

This repository uses a hybrid CI/CD pipeline involving GitHub Actions, CircleCI, and Argo Workflows to automate building, versioning, and deploying the updater to the internal NFS server.

### 1. Versioning & Changelog (GitHub Actions)

- **Workflow**: `.github/workflows/release.yml`
- **Trigger**: Push to `main` (usually via PR merge).
- **Process**:
  - Uses **Release Drafter** to categorize changes and determine the next version number based on PR labels (`major`, `minor`, `patch`).
  - Updates `version.json`.
  - Updates the `<Version>` tag in `Updater/Updater.csproj`.
  - Commits these version changes back to the repository.
  - Creates a Git Tag (e.g., `v1.2.3`).

### 2. Build & Package (CircleCI)

- **Workflow**: `.circleci/config.yml`
- **Trigger**: Creation of a new tag (`v*`).
- **Process**:
  - **Build**: Compiles the `Updater` project for `linux-arm` architecture.
    - _Note_: Builds as a dependent framework app (not self-contained) by default, consistent with `FolderProfile`.
  - **Package**: Compresses the publish output into `updater-{version}.tar.gz`.
  - **Artifact**: Stores the tarball as a CircleCI artifact.

### 3. Deployment (Argo Workflows @ Infrastructure)

- **Trigger**: CircleCI Webhook (`app-updater` event) -> Argo Events.
- **Process**:
  - Detects the successful completion of the `publish_artifact` workflow in CircleCI.
  - Triggers the `app-updater-trigger` Argo Workflow.
  - **Action**: Downloads the `updater-{version}.tar.gz` artifact from CircleCI.
  - **Target**: Deploys the file to the configured NFS path (mapped to `/exports/Updater`).

## How to Release

1.  **Create a Pull Request** to the `main` branch.
2.  **Label the PR** appropriately to control the version bump:
    - `major`: Major release (breaking changes).
    - `minor` / `enhancement` / `feature`: Minor release (new features).
    - `patch` / `fix` / `bug`: Patch release (bug fixes).
3.  **Merge the PR**.
4.  The automated pipeline will execute:
    - **GitHub**: Bumps version, updates changelog, creates tag.
    - **CircleCI**: Builds and creates artifact.
    - **Argo**: Deploys artifact to NFS.
