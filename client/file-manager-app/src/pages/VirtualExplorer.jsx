import React, { useState, useMemo, useCallback } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { Sparkles, Info, X } from "lucide-react";
import TreeView from "@/components/virtualExplorer/TreeView";
import FileList from "@/components/virtualExplorer/FileList";
import ExplorerToolbar from "@/components/virtualExplorer/ExplorerToolbar";
import PropertiesPanel from "@/components/virtualExplorer/PropertiesPanel";
import FileContextMenu from "@/components/virtualExplorer/FileContextMenu";
import {
  virtualTree,
  virtualFiles,
  buildTreeIndex,
  findNode,
  getBreadcrumb,
  getFolderNameMap,
} from "@/lib/virtualExplorerData";
import { useToast } from "@/components/ui/use-toast";

let _idCounter = 100;
const newId = () => `vf-new-${_idCounter++}`;

export default function VirtualExplorer() {
  const { toast } = useToast();
  const [tree, setTree] = useState(virtualTree);
  const [currentFolderId, setCurrentFolderId] = useState("root");
  const [history, setHistory] = useState(["root"]);
  const [historyIndex, setHistoryIndex] = useState(0);
  const [expanded, setExpanded] = useState(new Set(["root", "vf-atlas", "vf-vacation"]));
  const [selectedIds, setSelectedIds] = useState([]);
  const [viewMode, setViewMode] = useState("details");
  const [sortBy, setSortBy] = useState("name");
  const [sortDir, setSortDir] = useState("asc");
  const [search, setSearch] = useState("");
  const [renamingId, setRenamingId] = useState(null);
  const [fileMenu, setFileMenu] = useState(null);
  const [activeFile, setActiveFile] = useState(null);
  const [showMoveDialog, setShowMoveDialog] = useState(false);

  const treeIndex = useMemo(() => buildTreeIndex(tree), [tree]);
  const breadcrumb = useMemo(() => getBreadcrumb(treeIndex, currentFolderId), [treeIndex, currentFolderId]);
  const folderNameMap = useMemo(() => getFolderNameMap(tree), [tree]);
  const highlightFolderIds = useMemo(() => {
    if (selectedIds.length !== 1) return [];
    const f = virtualFiles.find((x) => x.id === selectedIds[0]);
    return f ? f.folders : [];
  }, [selectedIds]);

  const filesInFolder = useMemo(() => {
    let list = virtualFiles.filter((f) => f.folders.includes(currentFolderId));
    if (search.trim()) {
      const q = search.toLowerCase();
      list = list.filter((f) => f.name.toLowerCase().includes(q) || f.tags.some((t) => t.includes(q)));
    }
    const sorted = [...list].sort((a, b) => {
      let cmp = 0;
      if (sortBy === "size") cmp = a.sizeBytes - b.sizeBytes;
      else if (sortBy === "modified") cmp = new Date(a.modified) - new Date(b.modified);
      else cmp = String(a[sortBy]).localeCompare(String(b[sortBy]));
      return sortDir === "asc" ? cmp : -cmp;
    });
    return sorted;
  }, [currentFolderId, search, sortBy, sortDir]);

  // Navigation
  const navigateTo = useCallback((id) => {
    setHistory((h) => [...h.slice(0, historyIndex + 1), id]);
    setHistoryIndex((i) => i + 1);
    setCurrentFolderId(id);
    setSelectedIds([]);
    setActiveFile(null);
  }, [historyIndex]);

  const goBack = () => {
    if (historyIndex > 0) {
      const ni = historyIndex - 1;
      setHistoryIndex(ni);
      setCurrentFolderId(history[ni]);
      setSelectedIds([]);
    }
  };
  const goForward = () => {
    if (historyIndex < history.length - 1) {
      const ni = historyIndex + 1;
      setHistoryIndex(ni);
      setCurrentFolderId(history[ni]);
      setSelectedIds([]);
    }
  };
  const goUp = () => {
    const path = treeIndex[currentFolderId]?.path;
    if (path && path.length > 1) navigateTo(path[path.length - 2]);
  };

  const toggleExpand = (id) => {
    setExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  // Selection
  const handleSelect = (e, id) => {
    if (e.metaKey || e.ctrlKey) {
      setSelectedIds((prev) => (prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]));
    } else if (e.shiftKey) {
      const ids = filesInFolder.map((f) => f.id);
      const start = ids.indexOf(selectedIds[selectedIds.length - 1] ?? selectedIds[0]);
      const end = ids.indexOf(id);
      if (start !== -1 && end !== -1) {
        const range = ids.slice(Math.min(start, end), Math.max(start, end) + 1);
        setSelectedIds((prev) => Array.from(new Set([...prev, ...range])));
      } else {
        setSelectedIds([id]);
      }
    } else {
      setSelectedIds([id]);
    }
    const file = virtualFiles.find((f) => f.id === id);
    setActiveFile(file || null);
  };

  // Tree mutations
  const addFolder = (parentId) => {
    const node = { id: newId(), name: "New Folder", children: [] };
    const insert = (n) => {
      if (n.id === parentId) {
        n.children = [...(n.children || []), node];
        return true;
      }
      return (n.children || []).some(insert);
    };
    setTree((prev) => {
      const next = JSON.parse(JSON.stringify(prev));
      insert(next);
      return next;
    });
    setExpanded((prev) => new Set([...prev, parentId]));
    setRenamingId(node.id);
    toast({ title: "Virtual folder created", description: "It exists only in the virtual hierarchy." });
  };

  const renameFolder = (id, name) => {
    if (!name.trim()) {
      setRenamingId(null);
      return;
    }
    setTree((prev) => {
      const next = JSON.parse(JSON.stringify(prev));
      const node = findNode(next, id);
      if (node) node.name = name.trim();
      return next;
    });
    setRenamingId(null);
  };

  const deleteFolder = (id) => {
    if (id === "root") return;
    const remove = (n) => {
      n.children = (n.children || []).filter((c) => c.id !== id);
      (n.children || []).forEach(remove);
    };
    setTree((prev) => {
      const next = JSON.parse(JSON.stringify(prev));
      remove(next);
      return next;
    });
    if (currentFolderId === id) navigateTo("root");
    toast({ title: "Virtual folder removed", description: "No physical files were affected." });
  };

  const handleContextAction = (action, node) => {
    if (action === "new") addFolder(node.id);
    else if (action === "rename") setRenamingId(node.id);
    else if (action === "delete") deleteFolder(node.id);
    else if (action === "favorite") toast({ title: "Pinned to favorites" });
    else if (action === "copy") {
      navigator.clipboard?.writeText(node.name);
      toast({ title: "Folder name copied" });
    }
  };

  // Drag & drop
  const handleDropFiles = (fileIds, folderId) => {
    // In a real app this would update the virtual mapping. Here we just confirm.
    toast({
      title: "✓ Added to virtual folder",
      description: `Original file remains in its physical location. Now also appears in "${findNode(tree, folderId)?.name}".`,
    });
    setSelectedIds([]);
  };
  const handleDropFolder = (draggedId, targetId) => {
    if (draggedId === targetId) return;
    setTree((prev) => {
      const next = JSON.parse(JSON.stringify(prev));
      let moved = null;
      const detach = (n) => {
        const idx = (n.children || []).findIndex((c) => c.id === draggedId);
        if (idx >= 0) {
          moved = n.children.splice(idx, 1)[0];
          return;
        }
        (n.children || []).forEach(detach);
      };
      detach(next);
      if (moved) {
        const target = findNode(next, targetId);
        if (target) {
          target.children = [...(target.children || []), moved];
        } else {
          // reattach to root if target gone
          next.children.push(moved);
        }
      }
      return next;
    });
    toast({ title: "Folder moved", description: "Reorganized the virtual hierarchy." });
  };

  const handleDragStartFiles = (e, ids) => {
    e.dataTransfer.setData("application/json", JSON.stringify({ type: "files", ids }));
    e.dataTransfer.effectAllowed = "move";
  };

  // File actions
  const handleFileContext = (e, file) => {
    e.preventDefault();
    if (!selectedIds.includes(file.id)) setSelectedIds([file.id]);
    setActiveFile(file);
    setFileMenu({ x: e.clientX, y: e.clientY, file });
  };

  const handleFileAction = (action) => {
    const file = fileMenu?.file || activeFile;
    if (!file) return;
    if (action === "properties") setActiveFile(file);
    else if (action === "favorite") toast({ title: "Favorite toggled" });
    else if (action === "copy") {
      navigator.clipboard?.writeText(file.physicalPath);
      toast({ title: "Physical path copied" });
    } else if (action === "rename") toast({ title: "Rename (virtual label only)" });
    else if (action === "remove") {
      toast({ title: "Removed from this virtual folder", description: "The physical file is untouched." });
      setSelectedIds([]);
      setActiveFile(null);
    } else if (action === "move") {
      setShowMoveDialog(true);
    }
  };

  const onSort = (col) => {
    if (sortBy === col) setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    else {
      setSortBy(col);
      setSortDir("asc");
    }
  };

  const onNewFolder = () => addFolder(currentFolderId);
  const onRename = () => { if (selectedIds.length === 1) toast({ title: "Rename virtual reference" }); };
  const onDelete = () => {
    if (selectedIds.length === 0) return;
    toast({ title: "Removed from virtual folder", description: `${selectedIds.length} reference(s) removed. Physical files untouched.` });
    setSelectedIds([]);
    setActiveFile(null);
  };
  const onMove = () => { if (selectedIds.length) setShowMoveDialog(true); };

  const folderCount = filesInFolder.length;

  return (
    <div className="flex flex-col h-full overflow-hidden bg-background">
      {/* Concept banner */}
      <div className="flex items-center gap-2 px-4 py-1.5 bg-gradient-to-r from-violet-500/10 via-blue-500/10 to-transparent border-b border-border">
        <Sparkles className="w-3.5 h-3.5 text-violet-500 shrink-0" />
        <p className="text-[11px] text-muted-foreground">
          <span className="font-semibold text-foreground">Virtual Explorer</span> — organize files into virtual folders without moving anything on disk. A file can live in many folders at once.
        </p>
      </div>

      <ExplorerToolbar
        canBack={historyIndex > 0}
        canForward={historyIndex < history.length - 1}
        onBack={goBack}
        onForward={goForward}
        onUp={goUp}
        onRefresh={() => toast({ title: "Refreshed" })}
        breadcrumb={breadcrumb}
        onSelectFolder={navigateTo}
        search={search}
        setSearch={setSearch}
        viewMode={viewMode}
        setViewMode={setViewMode}
        onNewFolder={onNewFolder}
        onRename={onRename}
        onDelete={onDelete}
        onMove={onMove}
        hasSelection={selectedIds.length > 0}
      />

      <div className="flex-1 flex min-h-0">
        {/* Left: tree */}
        <div className="w-60 shrink-0 border-r border-border bg-card/30 flex flex-col min-h-0">
          <div className="px-3 py-2 text-[11px] font-semibold uppercase tracking-wide text-muted-foreground/70 border-b border-border">
            Virtual Folders
          </div>
          <div className="flex-1 overflow-auto p-1.5">
            <TreeView
              tree={tree}
              treeIndex={treeIndex}
              currentFolderId={currentFolderId}
              expanded={expanded}
              toggleExpand={toggleExpand}
              onSelectFolder={navigateTo}
              onContextAction={handleContextAction}
              selectedFileIds={selectedIds}
              highlightFolderIds={highlightFolderIds}
              onDropFiles={handleDropFiles}
              onDropFolder={handleDropFolder}
              renamingId={renamingId}
              setRenamingId={setRenamingId}
              onRename={renameFolder}
            />
          </div>
        </div>

        {/* Center: file list */}
        <div className="flex-1 flex flex-col min-h-0">
          <FileList
            files={filesInFolder}
            viewMode={viewMode}
            selectedIds={selectedIds}
            onSelect={handleSelect}
            onContextMenu={handleFileContext}
            onDragStartFiles={handleDragStartFiles}
            sortBy={sortBy}
            sortDir={sortDir}
            onSort={onSort}
            onOpen={(f) => setActiveFile(f)}
            folderNameMap={folderNameMap}
          />
          {/* Status bar */}
          <div className="flex items-center justify-between px-3 py-1.5 border-t border-border bg-card/50 text-[11px] text-muted-foreground">
            <span>{folderCount} item{folderCount !== 1 ? "s" : ""}{selectedIds.length > 0 && ` · ${selectedIds.length} selected`}</span>
            <span className="flex items-center gap-1">
              <Info className="w-3 h-3" />
              Virtual view — originals stay in place
            </span>
          </div>
        </div>

        {/* Right: properties */}
        <div className="w-72 shrink-0 border-l border-border bg-card/30 flex flex-col min-h-0">
          <div className="px-3 py-2 text-[11px] font-semibold uppercase tracking-wide text-muted-foreground/70 border-b border-border">
            Properties
          </div>
          <PropertiesPanel file={activeFile} folderCount={folderCount} treeIndex={treeIndex} onNavigate={navigateTo} />
        </div>
      </div>

      <FileContextMenu menu={fileMenu} onClose={() => setFileMenu(null)} onAction={handleFileAction} />

      <AnimatePresence>
        {showMoveDialog && (
          <MoveDialog
            tree={tree}
            onClose={() => setShowMoveDialog(false)}
            onConfirm={(targetId) => {
              handleDropFiles(selectedIds, targetId);
              setShowMoveDialog(false);
            }}
          />
        )}
      </AnimatePresence>
    </div>
  );
}

function MoveDialog({ tree, onClose, onConfirm }) {
  const [target, setTarget] = useState(null);
  const flat = [];
  const walk = (n, depth) => {
    if (n.id !== "root") flat.push({ id: n.id, name: n.name, depth });
    (n.children || []).forEach((c) => walk(c, depth + 1));
  };
  walk(tree, 0);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm" onClick={onClose}>
      <motion.div
        initial={{ opacity: 0, scale: 0.96, y: 8 }}
        animate={{ opacity: 1, scale: 1, y: 0 }}
        exit={{ opacity: 0, scale: 0.96 }}
        className="w-full max-w-sm bg-card border border-border rounded-xl shadow-2xl p-4"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between mb-3">
          <h3 className="font-semibold text-sm">Move to virtual folder</h3>
          <button onClick={onClose} className="p-1 rounded hover:bg-muted"><X className="w-4 h-4" /></button>
        </div>
        <p className="text-xs text-muted-foreground mb-3">Adds a virtual reference — the original file is not moved on disk.</p>
        <div className="max-h-64 overflow-auto rounded-lg border border-border bg-background">
          {flat.map((f) => (
            <button
              key={f.id}
              onClick={() => setTarget(f.id)}
              className={`w-full text-left px-3 py-1.5 text-sm flex items-center gap-2 hover:bg-muted transition-colors ${target === f.id ? "bg-primary/10 text-primary font-medium" : ""}`}
              style={{ paddingLeft: f.depth * 14 + 12 }}
            >
              <span className="text-amber-500">📁</span>
              {f.name}
            </button>
          ))}
        </div>
        <div className="flex justify-end gap-2 mt-4">
          <button onClick={onClose} className="px-3 py-1.5 text-sm rounded-md hover:bg-muted">Cancel</button>
          <button
            disabled={!target}
            onClick={() => onConfirm(target)}
            className="px-3 py-1.5 text-sm rounded-md bg-primary text-primary-foreground disabled:opacity-40"
          >
            Add reference
          </button>
        </div>
      </motion.div>
    </div>
  );
}