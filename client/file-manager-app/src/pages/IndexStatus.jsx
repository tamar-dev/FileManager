import React, { useState, useEffect } from "react";
import { motion } from "framer-motion";
import {
  Activity, Clock, FileStack, FolderOpen, CheckCircle2, Loader2
} from "lucide-react";
import PageHeader from "@/components/PageHeader";
import { getIndexStatus, getIndexedLocations } from "@/lib/api";
import { cn } from "@/lib/utils";

const STATE_LABELS = {
  idle: "Idle",
  running: "Indexing",
  completed: "Completed",
  failed: "Failed",
};

const STATE_COLORS = {
  idle: "bg-muted text-muted-foreground",
  running: "bg-amber-500/10 border-amber-500/20 text-amber-600 dark:text-amber-400",
  completed: "bg-emerald-500/10 border-emerald-500/20 text-emerald-600 dark:text-emerald-400",
  failed: "bg-rose-500/10 border-rose-500/20 text-rose-600 dark:text-rose-400",
};

export default function IndexStatus() {
  const [status, setStatus] = useState(null);
  const [statusError, setStatusError] = useState(null);
  const [locations, setLocations] = useState([]);
  const [locationsLoading, setLocationsLoading] = useState(true);
  const [locationsError, setLocationsError] = useState(null);

  useEffect(() => {
    let cancelled = false;
    let timeoutId;

    async function poll() {
      try {
        const data = await getIndexStatus();
        if (cancelled) return;
        setStatus(data);
        setStatusError(null);
      } catch (err) {
        if (!cancelled) setStatusError(err.message);
      } finally {
        if (!cancelled) {
          const delay = status?.state === "running" ? 1500 : 2000;
          timeoutId = setTimeout(poll, delay);
        }
      }
    }

    poll();
    return () => { cancelled = true; clearTimeout(timeoutId); };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [status?.state]);

  useEffect(() => {
    let cancelled = false;

    async function loadLocations() {
      try {
        setLocationsLoading(true);
        setLocationsError(null);
        const data = await getIndexedLocations();
        if (!cancelled) setLocations(data);
      } catch (err) {
        if (!cancelled) setLocationsError(err.message);
      } finally {
        if (!cancelled) setLocationsLoading(false);
      }
    }

    loadLocations();
    return () => { cancelled = true; };
  }, []);

  const state = status?.state ?? "idle";
  const filesProcessed = status?.filesProcessed ?? 0;
  const totalFiles = status?.totalFiles;
  const progress = totalFiles ? Math.min((filesProcessed / totalFiles) * 100, 100) : null;

  return (
    <div className="p-6 lg:p-8 max-w-[1400px] mx-auto animate-fade-in">
      <PageHeader
        title="Index Status"
        subtitle="Live monitoring of your file indexing engine."
        actions={
          <div className={cn("flex items-center gap-2 px-3 py-1.5 rounded-lg border", STATE_COLORS[state])}>
            <span className={cn("w-2 h-2 rounded-full", state === "running" ? "bg-amber-500 animate-pulse" : state === "completed" ? "bg-emerald-500" : state === "failed" ? "bg-rose-500" : "bg-muted-foreground")} />
            <span className="text-sm font-medium">{STATE_LABELS[state]}</span>
          </div>
        }
      />

      {statusError && (
        <div className="mb-6 text-sm text-rose-500">{statusError}</div>
      )}

      {/* Progress hero */}
      <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }}
        className="bg-gradient-to-br from-violet-500/5 to-indigo-500/5 border border-violet-500/15 rounded-2xl p-6 mb-6">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-3 min-w-0">
            <div className="w-12 h-12 rounded-xl bg-violet-600 flex items-center justify-center shrink-0">
              <Activity className="w-6 h-6 text-white" />
            </div>
            <div className="min-w-0">
              <div className="text-sm font-semibold">
                {state === "running" ? "Scan in progress" : state === "completed" ? "Last scan completed" : state === "failed" ? "Last scan failed" : "No active scan"}
              </div>
              <div className="text-xs text-muted-foreground font-mono truncate max-w-[420px]" title={status?.path || undefined}>
                {status?.path || "—"}
              </div>
            </div>
          </div>
          {progress !== null ? (
            <div className="text-right shrink-0">
              <div className="text-3xl font-semibold tabular-nums">{progress.toFixed(1)}%</div>
              <div className="text-xs text-muted-foreground">complete</div>
            </div>
          ) : (
            <div className="text-right shrink-0">
              <div className="text-3xl font-semibold tabular-nums">{filesProcessed.toLocaleString()}</div>
              <div className="text-xs text-muted-foreground">files processed</div>
            </div>
          )}
        </div>
        {progress !== null && (
          <div className="h-2.5 rounded-full bg-muted overflow-hidden mb-2">
            <motion.div
              className="h-full bg-gradient-to-r from-violet-500 to-indigo-500 rounded-full"
              animate={{ width: `${progress}%` }}
              transition={{ duration: 0.5 }}
            />
          </div>
        )}
        {state === "running" && (
          <div className="mt-4 flex items-center gap-2 rounded-lg border border-border/60 bg-background/60 px-3 py-2 min-w-0">
            <Loader2 className="w-4 h-4 animate-spin text-violet-500 shrink-0" />
            <span className="text-xs font-medium text-muted-foreground shrink-0">Current file</span>
            <span
              className="text-xs font-mono truncate min-w-0"
              title={status?.currentFilePath || undefined}
            >
              {status?.currentFilePath || "Waiting for first file…"}
            </span>
          </div>
        )}
        {status?.error && (
          <div className="text-xs text-rose-500 mt-2">{status.error}</div>
        )}
      </motion.div>

      {/* Stats grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-6">
        <MetricCard icon={FileStack} label="Files processed" value={filesProcessed.toLocaleString()} color="violet" />
        <MetricCard icon={Clock} label="Started" value={status?.startedAt ? new Date(status.startedAt).toLocaleTimeString() : "—"} color="blue" />
        <MetricCard icon={CheckCircle2} label="Completed" value={status?.completedAt ? new Date(status.completedAt).toLocaleTimeString() : "—"} color="emerald" />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Indexed locations */}
        <div className="bg-card border border-border rounded-xl overflow-hidden">
          <div className="px-5 py-4 border-b border-border flex items-center justify-between">
            <div>
              <h3 className="text-sm font-semibold">Indexed Locations</h3>
              <p className="text-xs text-muted-foreground">{locations.length} folders monitored</p>
            </div>
          </div>
          {locationsLoading ? (
            <div className="flex items-center justify-center py-8 text-muted-foreground text-sm gap-2">
              <Loader2 className="w-4 h-4 animate-spin" /> Loading locations…
            </div>
          ) : locationsError ? (
            <div className="text-sm text-rose-500 px-5 py-4">{locationsError}</div>
          ) : locations.length === 0 ? (
            <div className="text-sm text-muted-foreground px-5 py-4">No indexed locations yet. Add one from Settings.</div>
          ) : (
            <div className="divide-y divide-border/50">
              {locations.map(loc => (
                <div key={loc.id} className="flex items-center gap-3 px-5 py-3 hover:bg-muted/30 transition-colors group">
                  <FolderOpen className="w-4 h-4 text-muted-foreground shrink-0" />
                  <div className="min-w-0 flex-1">
                    <div className="text-sm font-mono truncate">{loc.path}</div>
                    <div className="text-xs text-muted-foreground">
                      {loc.lastIndexedAt ? `Last indexed ${new Date(loc.lastIndexedAt).toLocaleString()}` : "Not indexed yet"}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

function MetricCard({ icon: Icon, label, value, color }) {
  const colorMap = {
    violet: "text-violet-600 dark:text-violet-400 bg-violet-500/10",
    blue: "text-blue-600 dark:text-blue-400 bg-blue-500/10",
    emerald: "text-emerald-600 dark:text-emerald-400 bg-emerald-500/10",
    amber: "text-amber-600 dark:text-amber-400 bg-amber-500/10",
  };
  return (
    <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }}
      className="bg-card border border-border rounded-xl p-5">
      <div className={cn("w-9 h-9 rounded-lg flex items-center justify-center mb-3", colorMap[color])}>
        <Icon className="w-[18px] h-[18px]" />
      </div>
      <div className="text-2xl font-semibold tabular-nums">{value}</div>
      <div className="text-sm text-muted-foreground mt-0.5">{label}</div>
    </motion.div>
  );
}
