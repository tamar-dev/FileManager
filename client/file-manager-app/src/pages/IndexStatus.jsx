import React, { useState, useEffect } from "react";
import { motion } from "framer-motion";
import {
  Activity, Zap, Clock, FileStack, FolderOpen, ArrowRight,
  Plus, Edit3, Trash2, CheckCircle2, FilePlus, FileEdit, FileMinus, FileOutput
} from "lucide-react";
import PageHeader from "@/components/PageHeader";
import { indexStatusData, indexedLocations } from "@/lib/mockData";
import { cn } from "@/lib/utils";

export default function IndexStatus() {
  const [progress, setProgress] = useState(indexStatusData.progress);
  const [filesScanned, setFilesScanned] = useState(indexStatusData.filesScanned);

  useEffect(() => {
    const interval = setInterval(() => {
      setProgress(p => Math.min(p + 0.05, 99.9));
      setFilesScanned(f => f + Math.floor(Math.random() * 50 + 20));
    }, 1500);
    return () => clearInterval(interval);
  }, []);

  const changeIcons = { Added: FilePlus, Modified: FileEdit, Deleted: FileMinus, Moved: FileOutput };
  const changeColors = {
    Added: "text-emerald-500 bg-emerald-500/10",
    Modified: "text-blue-500 bg-blue-500/10",
    Deleted: "text-rose-500 bg-rose-500/10",
    Moved: "text-amber-500 bg-amber-500/10",
  };
  const locStatusColor = {
    Indexed: "bg-emerald-500",
    Indexing: "bg-amber-500 animate-pulse",
    Queued: "bg-muted-foreground",
  };

  return (
    <div className="p-6 lg:p-8 max-w-[1400px] mx-auto animate-fade-in">
      <PageHeader
        title="Index Status"
        subtitle="Live monitoring of your file indexing engine."
        actions={
          <div className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-amber-500/10 border border-amber-500/20">
            <span className="w-2 h-2 rounded-full bg-amber-500 animate-pulse" />
            <span className="text-sm font-medium text-amber-600 dark:text-amber-400">Indexing</span>
          </div>
        }
      />

      {/* Progress hero */}
      <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }}
        className="bg-gradient-to-br from-violet-500/5 to-indigo-500/5 border border-violet-500/15 rounded-2xl p-6 mb-6">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-3">
            <div className="w-12 h-12 rounded-xl bg-violet-600 flex items-center justify-center">
              <Activity className="w-6 h-6 text-white" />
            </div>
            <div>
              <div className="text-sm font-semibold">Current scan in progress</div>
              <div className="text-xs text-muted-foreground font-mono truncate max-w-[300px]">{indexStatusData.currentScan}</div>
            </div>
          </div>
          <div className="text-right">
            <div className="text-3xl font-semibold tabular-nums">{progress.toFixed(1)}%</div>
            <div className="text-xs text-muted-foreground">complete</div>
          </div>
        </div>
        <div className="h-2.5 rounded-full bg-muted overflow-hidden mb-2">
          <motion.div
            className="h-full bg-gradient-to-r from-violet-500 to-indigo-500 rounded-full"
            animate={{ width: `${progress}%` }}
            transition={{ duration: 0.5 }}
          />
        </div>
        <div className="flex items-center justify-between text-xs text-muted-foreground">
          <span>{filesScanned.toLocaleString()} / {indexStatusData.totalFiles.toLocaleString()} files</span>
          <span>{indexStatusData.queue.toLocaleString()} remaining</span>
        </div>
      </motion.div>

      {/* Stats grid */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <MetricCard icon={FileStack} label="Files scanned" value={filesScanned.toLocaleString()} color="violet" />
        <MetricCard icon={FolderOpen} label="In queue" value={indexStatusData.queue.toLocaleString()} color="amber" />
        <MetricCard icon={Zap} label="Scan speed" value={`${indexStatusData.speed.toLocaleString()}/min`} color="emerald" />
        <MetricCard icon={Clock} label="ETA" value={indexStatusData.eta} color="blue" />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Indexed locations */}
        <div className="bg-card border border-border rounded-xl overflow-hidden">
          <div className="px-5 py-4 border-b border-border flex items-center justify-between">
            <div>
              <h3 className="text-sm font-semibold">Indexed Locations</h3>
              <p className="text-xs text-muted-foreground">{indexedLocations.length} folders monitored</p>
            </div>
            <button className="flex items-center gap-1 px-2.5 h-7 rounded-lg bg-muted hover:bg-muted/70 text-xs font-medium transition-colors">
              <Plus className="w-3.5 h-3.5" /> Add
            </button>
          </div>
          <div className="divide-y divide-border/50">
            {indexedLocations.map(loc => (
              <div key={loc.id} className="flex items-center gap-3 px-5 py-3 hover:bg-muted/30 transition-colors group">
                <span className={cn("w-2 h-2 rounded-full shrink-0", locStatusColor[loc.status])} />
                <FolderOpen className="w-4 h-4 text-muted-foreground shrink-0" />
                <div className="min-w-0 flex-1">
                  <div className="text-sm font-mono truncate">{loc.path}</div>
                  <div className="text-xs text-muted-foreground">{loc.files.toLocaleString()} files · {loc.size}</div>
                </div>
                <span className={cn("text-xs font-medium px-2 py-0.5 rounded-md", loc.status === "Indexed" ? "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400" : loc.status === "Indexing" ? "bg-amber-500/10 text-amber-600 dark:text-amber-400" : "bg-muted text-muted-foreground")}>
                  {loc.status}
                </span>
                <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                  <button className="w-7 h-7 rounded-md hover:bg-muted flex items-center justify-center text-muted-foreground hover:text-foreground">
                    <Edit3 className="w-3.5 h-3.5" />
                  </button>
                  <button className="w-7 h-7 rounded-md hover:bg-muted flex items-center justify-center text-muted-foreground hover:text-rose-500">
                    <Trash2 className="w-3.5 h-3.5" />
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Recent changes */}
        <div className="bg-card border border-border rounded-xl overflow-hidden">
          <div className="px-5 py-4 border-b border-border">
            <h3 className="text-sm font-semibold">Recent Changes</h3>
            <p className="text-xs text-muted-foreground">Live file system events</p>
          </div>
          <div className="divide-y divide-border/50 max-h-[400px] overflow-y-auto">
            {indexStatusData.recentChanges.map(change => {
              const Icon = changeIcons[change.action];
              return (
                <div key={change.id} className="flex items-center gap-3 px-5 py-3 hover:bg-muted/30 transition-colors">
                  <div className={cn("w-7 h-7 rounded-lg flex items-center justify-center shrink-0", changeColors[change.action])}>
                    <Icon className="w-3.5 h-3.5" />
                  </div>
                  <div className="min-w-0 flex-1">
                    <div className="text-sm truncate">
                      <span className="font-medium">{change.action}</span>{" "}
                      <span className="text-muted-foreground">{change.file}</span>
                    </div>
                    <div className="text-xs text-muted-foreground font-mono truncate">{change.path}</div>
                  </div>
                  <div className="text-xs text-muted-foreground shrink-0">{change.time}</div>
                </div>
              );
            })}
          </div>
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