import React from "react";
import {
  ArrowLeft,
  ArrowRight,
  RotateCw,
  ArrowUp,
  Plus,
  Pencil,
  Trash2,
  FolderInput,
  Search,
  LayoutGrid,
  List,
  ChevronRight,
  Monitor,
} from "lucide-react";
import { cn } from "@/lib/utils";

export default function ExplorerToolbar({
  canBack,
  canForward,
  onBack,
  onForward,
  onUp,
  onRefresh,
  breadcrumb,
  onSelectFolder,
  search,
  setSearch,
  viewMode,
  setViewMode,
  onNewFolder,
  onRename,
  onDelete,
  onMove,
  hasSelection,
}) {
  return (
    <div className="flex flex-col gap-2 px-3 py-2 border-b border-border bg-card/50">
      {/* Row 1: navigation + breadcrumb */}
      <div className="flex items-center gap-1.5">
        <ToolButton icon={ArrowLeft} disabled={!canBack} onClick={onBack} label="Back" />
        <ToolButton icon={ArrowRight} disabled={!canForward} onClick={onForward} label="Forward" />
        <ToolButton icon={ArrowUp} onClick={onUp} label="Up" />
        <ToolButton icon={RotateCw} onClick={onRefresh} label="Refresh" />

        <div className="flex-1 min-w-0 flex items-center gap-1 mx-1 px-2.5 h-8 rounded-md bg-muted/50 border border-border/60 overflow-hidden">
          <Monitor className="w-3.5 h-3.5 shrink-0 text-muted-foreground" />
          <div className="flex items-center gap-0.5 overflow-x-auto no-scrollbar">
            {breadcrumb.map((node, i) => (
              <React.Fragment key={node.id}>
                {i > 0 && <ChevronRight className="w-3 h-3 shrink-0 text-muted-foreground/50" />}
                <button
                  className={cn(
                    "px-1.5 py-0.5 rounded text-xs whitespace-nowrap hover:bg-muted transition-colors",
                    i === breadcrumb.length - 1 ? "font-semibold text-foreground" : "text-muted-foreground"
                  )}
                  onClick={() => onSelectFolder(node.id)}
                >
                  {node.name}
                </button>
              </React.Fragment>
            ))}
          </div>
        </div>

        <div className="relative">
          <Search className="w-3.5 h-3.5 absolute left-2.5 top-1/2 -translate-y-1/2 text-muted-foreground" />
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search this folder"
            className="w-44 lg:w-56 pl-8 pr-3 h-8 text-sm rounded-md bg-background border border-border focus:border-ring focus:outline-none transition-colors"
          />
        </div>
      </div>

      {/* Row 2: command bar */}
      <div className="flex items-center gap-1">
        <ToolButton icon={Plus} onClick={onNewFolder} label="New folder" />
        <ToolButton icon={Pencil} onClick={onRename} disabled={!hasSelection} label="Rename" />
        <ToolButton icon={Trash2} onClick={onDelete} disabled={!hasSelection} label="Delete" />
        <ToolButton icon={FolderInput} onClick={onMove} disabled={!hasSelection} label="Move" />
        <div className="w-px h-5 bg-border mx-1" />
        <div className="flex items-center gap-0.5 ml-auto bg-muted/50 rounded-md p-0.5">
          <ViewToggle active={viewMode === "details"} onClick={() => setViewMode("details")} icon={List} label="Details" />
          <ViewToggle active={viewMode === "grid"} onClick={() => setViewMode("grid")} icon={LayoutGrid} label="Grid" />
        </div>
      </div>
    </div>
  );
}

function ToolButton({ icon: Icon, onClick, disabled, label }) {
  return (
    <button
      onClick={onClick}
      disabled={disabled}
      title={label}
      className={cn(
        "p-1.5 rounded-md transition-colors",
        disabled
          ? "text-muted-foreground/40 cursor-not-allowed"
          : "text-muted-foreground hover:bg-muted hover:text-foreground"
      )}
    >
      <Icon className="w-4 h-4" />
    </button>
  );
}

function ViewToggle({ active, onClick, icon: Icon, label }) {
  return (
    <button
      onClick={onClick}
      title={label}
      className={cn(
        "p-1.5 rounded transition-colors",
        active ? "bg-background text-foreground shadow-sm" : "text-muted-foreground hover:text-foreground"
      )}
    >
      <Icon className="w-3.5 h-3.5" />
    </button>
  );
}