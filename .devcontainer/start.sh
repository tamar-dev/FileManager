#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")/.."
mkdir -p .devcontainer/logs

port_in_use() {
  (echo > "/dev/tcp/127.0.0.1/$1") >/dev/null 2>&1
}

wait_for_health() {
  local url="$1" label="$2" logfile="$3"
  local deadline=$((SECONDS + 120))
  until curl --fail --silent --max-time 2 "$url" | grep -Eq '"status"[[:space:]]*:[[:space:]]*"ok"'; do
    if (( SECONDS >= deadline )); then
      echo "$label did not become healthy: $url" >&2
      if [[ -f "$logfile" ]]; then tail -n 60 "$logfile" >&2; fi
      return 1
    fi
    sleep 2
  done
  echo "$label is healthy."
}

# Bind to loopback; Codespaces forwards ports privately.
if ! port_in_use 5093; then
  nohup env ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://127.0.0.1:5093 dotnet run --no-launch-profile --project FileManager.Api/FileManager.Api.csproj --urls http://127.0.0.1:5093 > .devcontainer/logs/api.log 2>&1 < /dev/null &
fi
if ! port_in_use 5173; then
  nohup npm --prefix client/file-manager-app run dev -- --host 127.0.0.1 --port 5173 --strictPort > .devcontainer/logs/react.log 2>&1 < /dev/null &
fi

wait_for_health http://127.0.0.1:5093/api/health "FileManager API" .devcontainer/logs/api.log
wait_for_health http://127.0.0.1:5173/api/health "React API proxy" .devcontainer/logs/react.log
echo "Preview ready on :5173 (private forwarded port)."
