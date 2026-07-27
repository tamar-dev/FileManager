import React, { useState } from "react";
import { motion } from "framer-motion";
import { Clock, Download, Star, MoreVertical, Grid3x3, List } from "lucide-react";
import PageHeader from "@/components/PageHeader";
import { recentFiles } from "@/lib/mockData";
import { getFileTypeMeta } from "@/lib/fileTypes";
import { cn } from "@/lib/utils";

export default function Recent() {
  const [view, setView] = useState("list");
  const files = recentFiles;

  return (
    <div className="p-6 lg:p-8 max-w-[1400px] mx-auto animate-fade-in">
      <PageHeader
        title="Recent"
 subtitle="Files you've worked with most recently."
        actions={
          <div className="flex items-center gap-1 p-1 bg-muted rounded-lg">
            <button onClick={() => setView("list")} className={cn("w-7 h-7 rounded-md flex items-center justify-center transition-all", view === "list" ? "bg-background shadow-sm" : "text-muted-foreground")}>
              <List className="w-4 h-4" />
            </button>
            <button onClick={() => setView("grid")} className={cn("w-7 h-7 rounded-md flex items-center justify-center transition-all", view === "grid" ? "bg-background shadow-sm" : "text-muted-foreground")}>
              <Grid3x3 className="w-4 h-4" />
            </button>
          </div>
        }
      />

      {view === "list" ? (
        <div className="bg-card border border-border rounded-xl overflow-hidden">
          <div className="divide-y divide-border/50">
            {files.map((file, i) => {
              const meta = getFileTypeMeta(file.type);
              const Icon = meta.icon;
              return (
                <motion.div key={file.id} initial={{ opacity: 0, x: -8 }} animate={{ opacity: 1, x: 0 }} transition={{ delay: i * 0.03 }}
                  className="flex items-center gap-3 px-5 py-3 hover:bg-muted/40 transition-colors group">
                  <div className={cn("w-10 h-10 rounded-lg flex items-center justify-center shrink-0", meta.bg)}>
                    <Icon className={cn("w-5 h-5", meta.color)} />
                  </div>
                  <div className="min-w-0 flex-1">
                    <div className="text-sm font-medium truncate">{file.name}</div>
                    <div className="text-xs text-muted-foreground truncate">{file.path}</div>
                  </div>
                  <div className="text-xs text-muted-foreground tabular-nums hidden sm:block">{file.size}</div>
                  <div className="flex items-center gap-1 text-xs text-muted-foreground w-28">
                    <Clock className="w-3.5 h-3.5" /> {file.modified}
                  </div>
                  <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                    <button className="w-7 h-7 rounded-md hover:bg-muted flex items-center justify-center text-muted-foreground hover:text-foreground">
                      {file.starred ? <Star className="w-4 h-4 fill-amber-400 text-amber-400" /> : <Star className="w-4 h-4" />}
                    </button>
                    <button className="w-7 h-7 rounded-md hover:bg-muted flex items-center justify-center text-muted-foreground hover:text-foreground">
                      <Download className="w-4 h-4" />
                    </button>
                    <button className="w-7 h-7 rounded-md hover:bg-muted flex items-center justify-center text-muted-foreground hover:text-foreground">
                      <MoreVertical className="w-4 h-4" />
                    </button>
                  </div>
                </motion.div>
              );
            })}
          </div>
        </div>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
          {files.map((file, i) => {
            const meta = getFileTypeMeta(file.type);
            const Icon = meta.icon;
            return (
              <motion.div key={file.id} initial={{ opacity: 0, scale: 0.95 }} animate={{ opacity: 1, scale: 1 }} transition={{ delay: i * 0.03 }}
                className="group bg-card border border-border rounded-xl p-4 hover:shadow-lg hover:shadow-black/5 transition-all">
                <div className={cn("w-12 h-12 rounded-lg flex items-center justify-center mb-3", meta.bg)}>
                  <Icon className={cn("w-6 h-6", meta.color)} />
                </div>
                <div className="text-sm font-medium truncate">{file.name}</div>
                <div className="text-xs text-muted-foreground mt-0.5">{file.size} · {file.modified}</div>
              </motion.div>
            );
          })}
        </div>
      )}
    </div>
  );
}