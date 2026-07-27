import React from "react";
import { motion } from "framer-motion";
import { getFileTypeMeta } from "@/lib/fileTypes";
import { cn } from "@/lib/utils";
import { Check, Layers } from "lucide-react";

export default function FileList({
  files,
  viewMode,
  selectedIds,
  onSelect,
  onContextMenu,
  onDragStartFiles,
  sortBy,
  sortDir,
  onSort,
  onOpen,
  folderNameMap = {},
}) {
  if (files.length === 0) {
    return (
      <div className="flex-1 flex flex-col items-center justify-center text-muted-foreground gap-3">
        <div className="w-16 h-16 rounded-full bg-muted flex items-center justify-center">
          <span className="text-2xl">📁</span>
        </div>
        <p className="text-sm">This virtual folder is empty</p>
        <p className="text-xs text-muted-foreground/70">Drag files here or add them from another folder</p>
      </div>
    );
  }

  if (viewMode === "grid") {
    return (
      <div className="flex-1 overflow-auto p-3">
        <div className="grid grid-cols-[repeat(auto-fill,minmax(120px,1fr))] gap-2">
          {files.map((file, i) => (
            <FileGridItem
              key={file.id}
              file={file}
              index={i}
              selected={selectedIds.includes(file.id)}
              onSelect={onSelect}
              onContextMenu={onContextMenu}
              onDragStartFiles={onDragStartFiles}
              onOpen={onOpen}
              folderNameMap={folderNameMap}
            />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="flex-1 overflow-auto">
      <table className="w-full text-sm">
        <thead className="sticky top-0 bg-card z-10">
          <tr className="border-b border-border text-muted-foreground text-xs">
            <Th label="Name" sortBy="name" current={sortBy} dir={sortDir} onSort={onSort} className="text-left" />
            <Th label="Size" sortBy="size" current={sortBy} dir={sortDir} onSort={onSort} className="text-right" />
            <Th label="Type" sortBy="type" current={sortBy} dir={sortDir} onSort={onSort} className="text-left" />
            <Th label="Date modified" sortBy="modified" current={sortBy} dir={sortDir} onSort={onSort} className="text-left" />
            <Th label="Physical location" sortBy="physicalPath" current={sortBy} dir={sortDir} onSort={onSort} className="text-left" />
            <Th label="Appears in" current={sortBy} dir={sortDir} onSort={onSort} className="text-left" />
          </tr>
        </thead>
        <tbody>
          {files.map((file, i) => (
            <FileRow
              key={file.id}
              file={file}
              index={i}
              selected={selectedIds.includes(file.id)}
              onSelect={onSelect}
              onContextMenu={onContextMenu}
              onDragStartFiles={onDragStartFiles}
              onOpen={onOpen}
              folderNameMap={folderNameMap}
            />
          ))}
        </tbody>
      </table>
    </div>
  );
}

function Th({ label, sortBy: col, current, dir, onSort, className }) {
  const active = current === col;
  return (
    <th
      onClick={col ? () => onSort(col) : undefined}
      className={cn("px-3 py-2 font-medium select-none transition-colors", col ? "cursor-pointer hover:text-foreground" : "", className)}
    >
      <span className="inline-flex items-center gap-1">
        {label}
        {active && <span className="text-[10px]">{dir === "asc" ? "▲" : "▼"}</span>}
      </span>
    </th>
  );
}

function AppearsInCell({ file, folderNameMap }) {
  const count = file.folders.length;
  return (
    <div className="relative group/refs inline-flex">
      <span className="inline-flex items-center gap-1 text-muted-foreground cursor-help tabular-nums">
        <Layers className="w-3.5 h-3.5 text-violet-500 shrink-0" />
        {count} folder{count !== 1 ? "s" : ""}
      </span>
      <div className="invisible opacity-0 group-hover/refs:visible group-hover/refs:opacity-100 transition-opacity absolute z-20 bottom-full left-0 mb-1 min-w-[180px] max-w-[240px] bg-popover border border-border rounded-lg shadow-xl p-2 text-xs">
        <div className="font-medium text-[10px] uppercase tracking-wide text-muted-foreground/70 mb-1.5">Appears in</div>
        <div className="flex flex-col gap-1 max-h-44 overflow-auto">
          {file.folders.map((fid) => (
            <div key={fid} className="flex items-center gap-1.5 text-foreground truncate">
              <span className="text-amber-500 shrink-0">📁</span>
              <span className="truncate">{folderNameMap[fid] || fid}</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

function FileRow({ file, index, selected, onSelect, onContextMenu, onDragStartFiles, onOpen, folderNameMap }) {
  const meta = getFileTypeMeta(file.type);
  const Icon = meta.icon;
  const multiFolder = file.folders.length > 1;
  return (
    <motion.tr
      initial={{ opacity: 0, y: 4 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.15, delay: Math.min(index * 0.015, 0.2) }}
      draggable
      onDragStart={(e) => onDragStartFiles(e, [file.id])}
      onClick={(e) => onSelect(e, file.id)}
      onContextMenu={(e) => onContextMenu(e, file)}
      onDoubleClick={() => onOpen(file)}
      className={cn(
        "border-b border-border/50 cursor-default transition-colors",
        selected ? "bg-primary/10" : "hover:bg-muted/50"
      )}
    >
      <td className="px-3 py-2">
        <div className="flex items-center gap-2.5">
          {file.thumbnail ? (
            <div className="w-6 h-6 rounded overflow-hidden shrink-0 border border-border">
              <img src={file.thumbnail} alt="" className="w-full h-full object-cover" />
            </div>
          ) : (
            <div className={cn("w-6 h-6 rounded flex items-center justify-center shrink-0", meta.bg)}>
              <Icon className={cn("w-3.5 h-3.5", meta.color)} />
            </div>
          )}
          <span className="font-medium truncate">{file.name}</span>
          {file.tags.includes("favorite") && <span className="text-amber-500 text-xs">★</span>}
          {selected && multiFolder && (
            <span className="inline-flex items-center gap-1 text-[10px] px-1.5 py-0.5 rounded-full bg-violet-500/10 text-violet-600 dark:text-violet-400 border border-violet-500/20 whitespace-nowrap">
              <Layers className="w-2.5 h-2.5" />
              Appears in {file.folders.length} virtual folders
            </span>
          )}
        </div>
      </td>
      <td className="px-3 py-2 text-right text-muted-foreground tabular-nums">{file.size}</td>
      <td className="px-3 py-2 text-muted-foreground">{meta.label}</td>
      <td className="px-3 py-2 text-muted-foreground whitespace-nowrap">{file.modified}</td>
      <td className="px-3 py-2 text-muted-foreground/70 truncate max-w-[200px]" title={file.physicalPath}>
        {file.physicalPath}
      </td>
      <td className="px-3 py-2">
        <AppearsInCell file={file} folderNameMap={folderNameMap} />
      </td>
    </motion.tr>
  );
}

function FileGridItem({ file, index, selected, onSelect, onContextMenu, onDragStartFiles, onOpen, folderNameMap }) {
  const meta = getFileTypeMeta(file.type);
  const Icon = meta.icon;
  const multiFolder = file.folders.length > 1;
  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.96 }}
      animate={{ opacity: 1, scale: 1 }}
      transition={{ duration: 0.15, delay: Math.min(index * 0.015, 0.2) }}
      draggable
      onDragStart={(e) => onDragStartFiles(e, [file.id])}
      onClick={(e) => onSelect(e, file.id)}
      onContextMenu={(e) => onContextMenu(e, file)}
      onDoubleClick={() => onOpen(file)}
      className={cn(
        "group relative flex flex-col items-center gap-1.5 p-2.5 rounded-lg cursor-default border transition-all",
        selected ? "border-primary bg-primary/10 ring-1 ring-primary/30" : "border-transparent hover:border-border hover:bg-muted/40"
      )}
    >
      {selected && (
        <div className="absolute top-1.5 left-1.5 w-4 h-4 rounded-full bg-primary flex items-center justify-center">
          <Check className="w-2.5 h-2.5 text-primary-foreground" />
        </div>
      )}
      {multiFolder && (
        <div className="absolute top-1.5 right-1.5 inline-flex items-center gap-0.5 text-[9px] px-1 py-0.5 rounded-full bg-violet-500/10 text-violet-600 dark:text-violet-400 border border-violet-500/20">
          <Layers className="w-2.5 h-2.5" />
          {file.folders.length}
        </div>
      )}
      <div className="w-20 h-20 rounded-lg overflow-hidden bg-muted flex items-center justify-center border border-border">
        {file.thumbnail ? (
          <img src={file.thumbnail} alt="" className="w-full h-full object-cover" />
        ) : (
          <Icon className={cn("w-8 h-8", meta.color)} />
        )}
      </div>
      <span className="text-xs text-center line-clamp-2 leading-tight w-full">{file.name}</span>
      {selected && multiFolder && (
        <span className="text-[9px] text-violet-600 dark:text-violet-400 text-center leading-tight">
          Appears in {file.folders.length} folders
        </span>
      )}
    </motion.div>
  );
}