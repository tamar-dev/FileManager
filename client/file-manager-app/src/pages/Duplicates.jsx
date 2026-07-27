import React, { useEffect, useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { Copy, ChevronDown, ChevronRight, Eye, HardDrive } from "lucide-react";
import PageHeader from "@/components/PageHeader";
import { getDuplicates } from "@/lib/api";
import { getFileTypeMeta } from "@/lib/fileTypes";
import { cn } from "@/lib/utils";

export default function Duplicates() {
  const [groups, setGroups] = useState([]);
  const [expanded, setExpanded] = useState(() => new Set());
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    let cancelled = false;

    async function loadDuplicates() {
      try {
        setLoading(true);
        setError(null);

        const data = await getDuplicates();

        if (!cancelled) {
          setGroups(data);
          if (data.length > 0) {
            setExpanded(new Set([data[0].id]));
          }
        }
      } catch (err) {
        if (!cancelled) {
          setError(err.message || "Failed to load duplicates.");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    loadDuplicates();

    return () => { cancelled = true; };
  }, []);

  const toggleExpand = (id) => setExpanded(s => {
    const next = new Set(s);
    if (next.has(id)) next.delete(id); else next.add(id);
    return next;
  });

  const totalFiles = groups.reduce((a, g) => a + g.fileCount, 0);
  const potentialSavings = groups.reduce((a, g) => a + g.wastedSize, 0);

  return (
    <div className="p-6 lg:p-8 max-w-[1400px] mx-auto animate-fade-in">
      <PageHeader
        title="Duplicates"
        subtitle={`${groups.length} groups of duplicate files detected.`}
      />

      {/* Summary */}
      <div className="grid grid-cols-3 gap-4 mb-6">
        <div className="bg-card border border-border rounded-xl p-4">
          <div className="flex items-center gap-2 text-sm text-muted-foreground mb-1"><Copy className="w-4 h-4" /> Duplicate groups</div>
          <div className="text-2xl font-semibold tabular-nums">{groups.length}</div>
        </div>
        <div className="bg-card border border-border rounded-xl p-4">
          <div className="flex items-center gap-2 text-sm text-muted-foreground mb-1"><HardDrive className="w-4 h-4" /> Duplicate files</div>
          <div className="text-2xl font-semibold tabular-nums">{totalFiles}</div>
        </div>
        <div className="bg-card border border-border rounded-xl p-4">
          <div className="flex items-center gap-2 text-sm text-muted-foreground mb-1"><HardDrive className="w-4 h-4" /> Potential savings</div>
          <div className="text-2xl font-semibold tabular-nums text-emerald-600 dark:text-emerald-400">{formatBytes(potentialSavings)}</div>
        </div>
      </div>

      {error && (
        <div className="px-5 py-4 mb-4 text-sm text-rose-500 bg-card border border-border rounded-xl">{error}</div>
      )}

      {loading && !error && (
        <div className="px-5 py-8 text-sm text-muted-foreground text-center bg-card border border-border rounded-xl">Loading duplicates…</div>
      )}

      {!loading && !error && groups.length === 0 && (
        <div className="px-5 py-8 text-sm text-muted-foreground text-center bg-card border border-border rounded-xl">No duplicate files found.</div>
      )}

      {/* Groups */}
      {!loading && !error && groups.length > 0 && (
        <div className="space-y-3">
          {groups.map((group, gi) => {
            const meta = getFileTypeMeta(group.type);
            const Icon = meta.icon;
            const isExpanded = expanded.has(group.id);
            return (
              <motion.div
                key={group.id}
                initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: gi * 0.05 }}
                className="bg-card border border-border rounded-xl overflow-hidden"
              >
                {/* Group header */}
                <button
                  onClick={() => toggleExpand(group.id)}
                  className="w-full flex items-center gap-3 px-5 py-4 hover:bg-muted/30 transition-colors text-left"
                >
                  {isExpanded ? <ChevronDown className="w-4 h-4 text-muted-foreground" /> : <ChevronRight className="w-4 h-4 text-muted-foreground" />}
                  <div className={cn("w-10 h-10 rounded-lg flex items-center justify-center", meta.bg)}>
                    <Icon className={cn("w-5 h-5", meta.color)} />
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="text-sm font-medium">{group.fileCount} duplicate files</div>
                    <div className="text-xs text-muted-foreground font-mono">Hash: {group.hash}</div>
                  </div>
                  <div className="text-right">
                    <div className="text-sm font-medium tabular-nums">{formatBytes(group.totalSize)}</div>
                    <div className="text-xs text-muted-foreground">{group.fileCount} copies</div>
                  </div>
                </button>

                <AnimatePresence>
                  {isExpanded && (
                    <motion.div initial={{ height: 0, opacity: 0 }} animate={{ height: "auto", opacity: 1 }} exit={{ height: 0, opacity: 0 }} className="overflow-hidden">
                      <div className="border-t border-border divide-y divide-border/50">
                        {group.files.map(file => (
                          <div key={file.id} className="flex items-center gap-3 px-5 py-3 hover:bg-muted/30 transition-colors">
                            <div className={cn("w-10 h-10 rounded-lg flex items-center justify-center shrink-0", meta.bg)}>
                              <Icon className={cn("w-5 h-5", meta.color)} />
                            </div>
                            <div className="min-w-0 flex-1">
                              <div className="text-sm font-medium truncate">{file.name}</div>
                              <div className="text-xs text-muted-foreground truncate">{file.fullPath}</div>
                            </div>
                            <div className="text-xs text-muted-foreground hidden sm:block">{formatDate(file.lastModified)}</div>
                            <div className="text-sm font-medium tabular-nums hidden md:block">{formatBytes(file.size)}</div>
                            <button
                              title="View file"
                              className="w-8 h-8 rounded-lg hover:bg-muted flex items-center justify-center text-muted-foreground hover:text-foreground"
                            >
                              <Eye className="w-4 h-4" />
                            </button>
                          </div>
                        ))}
                      </div>
                    </motion.div>
                  )}
                </AnimatePresence>
              </motion.div>
            );
          })}
        </div>
      )}
    </div>
  );
}

function formatBytes(bytes) {
  if (!bytes || bytes <= 0) return "0 B";
  const units = ["B", "KB", "MB", "GB", "TB"];
  const i = Math.floor(Math.log(bytes) / Math.log(1024));
  const value = bytes / Math.pow(1024, i);
  return `${value.toFixed(value >= 10 || i === 0 ? 0 : 1)} ${units[i]}`;
}

function formatDate(value) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  return date.toLocaleDateString(undefined, { month: "short", day: "numeric", year: "numeric" });
}