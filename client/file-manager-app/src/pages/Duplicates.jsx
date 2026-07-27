import React, { useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { Copy, Trash2, ChevronDown, ChevronRight, Check, Eye, HardDrive } from "lucide-react";
import PageHeader from "@/components/PageHeader";
import { duplicateGroups } from "@/lib/mockData";
import { getFileTypeMeta } from "@/lib/fileTypes";
import { cn } from "@/lib/utils";

export default function Duplicates() {
  const [groups, setGroups] = useState(duplicateGroups);
  const [expanded, setExpanded] = useState(() => new Set([duplicateGroups[0].id]));

  const toggleExpand = (id) => setExpanded(s => {
    const next = new Set(s);
    if (next.has(id)) next.delete(id); else next.add(id);
    return next;
  });

  const toggleSelect = (groupId, fileId) => setGroups(gs => gs.map(g => g.id !== groupId ? g : ({
    ...g, files: g.files.map(f => f.id === fileId ? { ...f, selected: !f.selected } : f)
  })));

  const totalDups = groups.reduce((a, g) => a + g.files.filter(f => f.selected).length, 0);
  const potentialSaved = "128 MB";

  return (
    <div className="p-6 lg:p-8 max-w-[1400px] mx-auto animate-fade-in">
      <PageHeader
        title="Duplicates"
        subtitle={`${groups.length} groups of duplicate files detected.`}
        actions={
          <button className="flex items-center gap-2 px-4 h-9 rounded-lg bg-rose-600 hover:bg-rose-700 text-white text-sm font-medium transition-colors">
            <Trash2 className="w-4 h-4" /> Delete selected ({totalDups})
          </button>
        }
      />

      {/* Summary */}
      <div className="grid grid-cols-3 gap-4 mb-6">
        <div className="bg-card border border-border rounded-xl p-4">
          <div className="flex items-center gap-2 text-sm text-muted-foreground mb-1"><Copy className="w-4 h-4" /> Duplicate groups</div>
          <div className="text-2xl font-semibold tabular-nums">{groups.length}</div>
        </div>
        <div className="bg-card border border-border rounded-xl p-4">
          <div className="flex items-center gap-2 text-sm text-muted-foreground mb-1"><HardDrive className="w-4 h-4" /> Files selected</div>
          <div className="text-2xl font-semibold tabular-nums">{totalDups}</div>
        </div>
        <div className="bg-card border border-border rounded-xl p-4">
          <div className="flex items-center gap-2 text-sm text-muted-foreground mb-1"><HardDrive className="w-4 h-4" /> Potential savings</div>
          <div className="text-2xl font-semibold tabular-nums text-emerald-600 dark:text-emerald-400">{potentialSaved}</div>
        </div>
      </div>

      {/* Groups */}
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
                  <div className="text-sm font-medium">{group.count} duplicate files</div>
                  <div className="text-xs text-muted-foreground font-mono">Hash: {group.hash}</div>
                </div>
                <div className="text-right">
                  <div className="text-sm font-medium tabular-nums">{group.size}</div>
                  <div className="text-xs text-muted-foreground">{group.count} copies</div>
                </div>
              </button>

              <AnimatePresence>
                {isExpanded && (
                  <motion.div initial={{ height: 0, opacity: 0 }} animate={{ height: "auto", opacity: 1 }} exit={{ height: 0, opacity: 0 }} className="overflow-hidden">
                    <div className="border-t border-border divide-y divide-border/50">
                      {group.files.map(file => (
                        <div key={file.id} className={cn("flex items-center gap-3 px-5 py-3 hover:bg-muted/30 transition-colors", file.selected && "bg-rose-500/[0.03]")}>
                          <button
                            onClick={() => toggleSelect(group.id, file.id)}
                            className={cn("w-5 h-5 rounded-md border flex items-center justify-center transition-all shrink-0",
                              file.selected ? "bg-rose-600 border-rose-600 text-white" : "border-border hover:border-rose-500")}
                          >
                            {file.selected && <Check className="w-3.5 h-3.5" strokeWidth={3} />}
                          </button>
                          <div className={cn("w-10 h-10 rounded-lg flex items-center justify-center shrink-0", meta.bg)}>
                            <Icon className={cn("w-5 h-5", meta.color)} />
                          </div>
                          <div className="min-w-0 flex-1">
                            <div className="text-sm font-medium truncate">{file.name}</div>
                            <div className="text-xs text-muted-foreground truncate">{file.path}</div>
                          </div>
                          <div className="text-xs text-muted-foreground hidden sm:block">{file.modified}</div>
                          <div className="text-sm font-medium tabular-nums hidden md:block">{group.size}</div>
                          <button className="w-8 h-8 rounded-lg hover:bg-muted flex items-center justify-center text-muted-foreground hover:text-foreground">
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
    </div>
  );
}