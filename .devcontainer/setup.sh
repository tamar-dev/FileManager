#!/usr/bin/env bash
set -euo pipefail
dotnet restore FileManager.Engine.sln
npm ci --prefix client/file-manager-app
mkdir -p .devcontainer/logs
printf '\nDependencies installed. Preview services start automatically when the Codespace starts.\n'
