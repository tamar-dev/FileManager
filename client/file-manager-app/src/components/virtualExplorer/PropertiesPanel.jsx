import React from "react";
import { motion } from "framer-motion";
import { getFileTypeMeta } from "@/lib/fileTypes";
import { cn } from "@/lib/utils";
import { Hash, Tag, MapPin, FolderTree, Calendar, HardDrive, FileText, Layers, ChevronRight } from "lucide-react";
import { getBreadcrumb } from "@/lib/virtualExplorerData";

export default function PropertiesPanel({ file, folderCount, treeIndex, onNavigate }) {
  if (!file) {
    return (
      <div className="h-full flex flex-col items-center justify-center text-muted-foreground/60 gap-2 p-6 text-center">
        <div className="w-12 h-12 rounded-full bg-muted flex items-center justify-center">
          <FileText className="w-5 h-5" />
        </div>
        <p className="text-xs">Select a file to view its properties</p>
        {folderCount > 0 && (
          <p className="text-[11px] text-muted-foreground/50">This folder contains {folderCount} virtual items</p>
        )}
      </div>
    );
  }

  const meta = getFileTypeMeta(file.type);
  const Icon = meta.icon;

  return (
    <motion.div
      key={file.id}
      initial={{ opacity: 0, x: 8 }}
      animate={{ opacity: 1, x: 0 }}
      transition={{ duration: 0.2 }}
      className="h-full overflow-auto p-4 flex flex-col gap-4"
    >
      {/* Thumbnail / icon */}
      <div className="rounded-xl overflow-hidden border border-border bg-muted aspect-video flex items-center justify-center">
        {file.thumbnail ? (
          <img src={file.thumbnail} alt={file.name} className="w-full h-full object-cover" />
        ) : (
          <div className={cn("w-16 h-16 rounded-2xl flex items-center justify-center", meta.bg)}>
            <Icon className={cn("w-8 h-8", meta.color)} />
          </div>
        )}
      </div>

      <div>
        <h3 className="font-semibold text-sm truncate" title={file.name}>{file.name}</h3>
        <p className="text-xs text-muted-foreground">{meta.label}</p>
        {file.folders.length > 1 && (
          <span className="mt-1.5 inline-flex items-center gap-1 text-[10px] px-1.5 py-0.5 rounded-full bg-violet-500/10 text-violet-600 dark:text-violet-400 border border-violet-500/20">
            <Layers className="w-2.5 h-2.5" />
            Appears in {file.folders.length} virtual folders
          </span>
        )}
      </div>

      <div className="flex flex-col gap-0">
        <PropRow icon={FileText} label="File name" value={file.name} />
        <PropRow icon={HardDrive} label="Size" value={file.size} />
        <PropRow icon={Calendar} label="Created" value={file.created} />
        <PropRow icon={Calendar} label="Modified" value={file.modified} />
        <PropRow icon={MapPin} label="Physical location" value={file.physicalPath} mono />
        <PropRow icon={Hash} label="Hash" value={file.hash} mono />
      </div>

      {/* Virtual Locations — clickable hierarchy */}
      <div>
        <div className="flex items-center gap-1.5 text-xs font-medium text-muted-foreground mb-2">
          <FolderTree className="w-3.5 h-3.5" />
          Virtual Locations
          <span className="ml-auto text-[10px] tabular-nums px-1.5 py-0.5 rounded-full bg-violet-500/10 text-violet-600 dark:text-violet-400 border border-violet-500/20">
            {file.folders.length}
          </span>
        </div>
        <div className="flex flex-col gap-1.5">
          {file.folders.map((fid) => {
            const path = (getBreadcrumb(treeIndex, fid) || []).filter((n) => n.id !== "root");
            if (!path.length) return null;
            return (
              <button
                key={fid}
                onClick={() => onNavigate?.(fid)}
                className="group text-left rounded-lg border border-border hover:border-violet-500/40 hover:bg-violet-500/5 transition-colors p-2"
              >
                {path.map((n, i) => (
                  <div
                    key={n.id}
                    style={{ paddingLeft: i * 12 }}
                    className="flex items-center gap-1 text-xs py-0.5"
                  >
                    <span className="text-amber-500 shrink-0">{i === path.length - 1 ? "📂" : "📁"}</span>
                    <span className={cn(i === path.length - 1 ? "font-medium text-foreground" : "text-muted-foreground")}>
                      {n.name}
                    </span>
                    {i < path.length - 1 && <ChevronRight className="w-3 h-3 text-muted-foreground/40 ml-auto" />}
                  </div>
                ))}
              </button>
            );
          })}
        </div>
        <p className="text-[10px] text-muted-foreground/60 mt-2 leading-relaxed">
          Click any location to navigate there. The original file stays in its physical location.
        </p>
      </div>

      {/* Tags */}
      <div>
        <div className="flex items-center gap-1.5 text-xs font-medium text-muted-foreground mb-1.5">
          <Tag className="w-3.5 h-3.5" />
          Tags
        </div>
        <div className="flex flex-wrap gap-1.5">
          {file.tags.map((tag) => (
            <span key={tag} className="text-[11px] px-2 py-0.5 rounded-full bg-primary/10 text-primary border border-primary/20">
              {tag}
            </span>
          ))}
        </div>
      </div>

      <div className="mt-auto pt-3 border-t border-border">
        <p className="text-[11px] text-muted-foreground/60 leading-relaxed">
          This file is referenced virtually. The original on disk is never moved or modified.
        </p>
      </div>
    </motion.div>
  );
}

function PropRow({ icon: Icon, label, value, mono }) {
  return (
    <div className="flex items-start gap-2 py-1.5 border-b border-border/40">
      <Icon className="w-3.5 h-3.5 mt-0.5 text-muted-foreground/60 shrink-0" />
      <div className="min-w-0 flex-1">
        <div className="text-[11px] text-muted-foreground/70">{label}</div>
        <div className={cn("text-xs text-foreground truncate", mono && "font-mono text-[11px]")}>{value}</div>
      </div>
    </div>
  );
}