#!/usr/bin/env bash
set -euo pipefail
mkdir -p .devcontainer/logs
# Bind to loopback; Codespaces forwards ports privately.
if ! (echo > /dev/tcp/127.0.0.1/5093) >/dev/null 2>&1; then
  nohup env ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://127.0.0.1:5093 dotnet run --no-launch-profile --project FileManager.Api/FileManager.Api.csproj --urls http://127.0.0.1:5093 > .devcontainer/logs/api.log 2>&1 < /dev/null &
fi
if ! (echo > /dev/tcp/127.0.0.1/5173) >/dev/null 2>&1; then
  nohup npm --prefix client/file-manager-app run dev -- --host 127.0.0.1 --port 5173 --strictPort > .devcontainer/logs/react.log 2>&1 < /dev/null &
fi
echo "Starting FileManager API :5093 and React :5173 (private forwarded ports)."
echo "API log: .devcontainer/logs/api.log | React log: .devcontainer/logs/react.log"
