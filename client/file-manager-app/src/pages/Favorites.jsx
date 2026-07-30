import React, { useState } from "react";
import { motion } from "framer-motion";
import { Star, Download, MoreVertical, Grid3x3, List, Heart } from "lucide-react";
import PageHeader from "@/components/PageHeader";
import { recentFiles } from "@/lib/mockData";
import { getFileTypeMeta } from "@/lib/fileTypes";
import { cn } from "@/lib/utils";

export default function Favorites() {
  const [view, setView] = useState("grid");
  const files = recentFiles.filter(f => f.starred);

  return (
    <div className="p-6 lg:p-8 max-w-[1400px] mx-auto animate-fade-in">
      <PageHeader
        title="Favorites"
        subtitle="Your starred files, all in one place."
        actions={
          <div className="flex items-center gap-1 p-1 bg-muted rounded-lg">
            <button onClick={() => setView("grid")} className={cn("w-7 h-7 rounded-md flex items-center justify-center transition-all", view === "grid" ? "bg-background shadow-sm" : "text-muted-foreground")}>
              <Grid3x3 className="w-4 h-4" />
            </button>
            <button onClick={() => setView("list")} className={cn("w-7 h-7 rounded-md flex items-center justify-center transition-all", view === "list" ? "bg-background shadow-sm" : "text-muted-foreground")}>
              <List className="w-4 h-4" />
            </button>
          </div>
        }
      />

      {view === "grid" ? (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
          {files.map((file, i) => {
            const meta = getFileTypeMeta(file.type);
            const Icon = meta.icon;
            return (
              <motion.div key={file.id} initial={{ opacity: 0, scale: 0.95 }} animate={{ opacity: 1, scale: 1 }} transition={{ delay: i * 0.04 }}
                className="group relative bg-card border border-border rounded-xl p-4 hover:shadow-lg hover:shadow-black/5 transition-all cursor-pointer">
                <div className="absolute top-3 right-3">
                  <Heart className="w-4 h-4 fill-rose-400 text-rose-400" />
                </div>
                <div className={cn("w-12 h-12 rounded-lg flex items-center justify-center mb-3", meta.bg)}>
                  <Icon className={cn("w-6 h-6", meta.color)} />
                </div>
                <div className="text-sm font-medium truncate">{file.name}</div>
                <div className="text-xs text-muted-foreground mt-0.5 truncate">{file.path}</div>
                <div className="flex items-center justify-between mt-2">
                  <span className="text-xs text-muted-foreground">{file.size}</span>
                  <span className="text-xs text-muted-foreground">{file.modified}</span>
                </div>
              </motion.div>
            );
          })}
        </div>
      ) : (
        <div className="bg-card border border-border rounded-xl overflow-hidden">
          <div className="divide-y divide-border/50">
            {files.map((file, i) => {
              const meta = getFileTypeMeta(file.type);
              const Icon = meta.icon;
              return (
                <motion.div key={file.id} initial={{ opacity: 0, x: -8 }} animate={{ opacity: 1, x: 0 }} transition={{ delay: i * 0.03 }}
                  className="flex items-center gap-3 px-5 py-3 hover:bg-muted/40 transition-colors group">
                  <Star className="w-4 h-4 fill-amber-400 text-amber-400 shrink-0" />
                  <div className={cn("w-9 h-9 rounded-lg flex items-center justify-center shrink-0", meta.bg)}>
                    <Icon className={cn("w-[18px] h-[18px]", meta.color)} />
                  </div>
                  <div className="min-w-0 flex-1">
                    <div className="text-sm font-medium truncate">{file.name}</div>
                    <div className="text-xs text-muted-foreground truncate">{file.path}</div>
                  </div>
                  <div className="text-xs text-muted-foreground tabular-nums hidden sm:block">{file.size}</div>
                  <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
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
      )}
    </div>
  );
}