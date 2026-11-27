# Full-stack Codespaces preview

From GitHub: **Code → Codespaces → Create codespace on branch**. Choose the branch you want to develop on.

The devcontainer installs .NET 9 and Node 22, restores the solution, installs React dependencies with `npm ci`, and starts two background processes when the Codespace starts:

- React Vite: port **5173**, privately forwarded, opens in browser.
- ASP.NET Core API: port **5093**, privately forwarded.
- SQLite: created by the API using its existing configuration/migrations.

React requests to `/api/*` are proxied by Vite to the .NET server. No public API URL or cloud credentials are needed. Check `/api/health` on the React preview URL to verify the proxy.

The container waits for the API and React proxy health checks before reporting startup complete. If automatic startup fails, inspect the logs, then retry with `bash .devcontainer/start.sh` from the repository root. The script reuses listening ports and verifies health rather than reporting success merely because processes were launched.

### Diagnostics

```bash
tail -n 100 .devcontainer/logs/api.log
tail -n 100 .devcontainer/logs/react.log
curl -f http://127.0.0.1:5093/api/health
curl -f http://127.0.0.1:5173/api/health
```

The preview **does not** access files on your Windows machine. The API can only index paths inside the Codespace. To test indexing, create a dedicated directory under `/tmp/filemanager-fixtures` and pass that path explicitly using the API; never index the workspace or home directory by default. No automatic indexing or filesystem modification is performed by the startup scripts.

**Current limitation:** React still contains Base44 SDK/auth flows; this configuration does not claim to remove those dependencies or guarantee end-to-end UI integration. Track that migration in issue #21. API endpoints and proxy can be tested independently now.

Keep ports private. A public Codespaces URL is **not** suitable for this unauthenticated API because it exposes operations that accept filesystem paths. Codespaces usage consumes account quota and may incur charges beyond free limits.
