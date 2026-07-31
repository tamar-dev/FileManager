import React, { useState, useMemo, useCallback, useEffect } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { Sparkles, Info, X } from "lucide-react";

import TreeView from "@/components/virtualExplorer/TreeView";
import FileList from "@/components/virtualExplorer/FileList";
import ExplorerToolbar from "@/components/virtualExplorer/ExplorerToolbar";
import PropertiesPanel from "@/components/virtualExplorer/PropertiesPanel";
import FileContextMenu from "@/components/virtualExplorer/FileContextMenu";

import {
  buildTreeIndex,
  findNode,
  getBreadcrumb,
  getFolderNameMap,
} from "@/lib/virtualExplorerData";

import {
  getVirtualFolders,
  getVirtualFolderFiles,
  createVirtualFolder,
  updateVirtualFolder,
  deleteVirtualFolder,
  addFileToVirtualFolder,
  searchFiles,
  removeFileFromVirtualFolder,
  getFileVirtualFolders
} from "@/lib/api";

import { useToast } from "@/components/ui/use-toast";

function toExplorerTree(folders) {
  return {
    id: "root",
    name: "Virtual Library",
    children: folders ?? [],
  };
}

export default function VirtualExplorer() {
  const { toast } = useToast();

  const [tree, setTree] = useState(() => toExplorerTree([]));
  const [folderFiles, setFolderFiles] = useState([]);

  const [currentFolderId, setCurrentFolderId] = useState("root");
  const [history, setHistory] = useState(["root"]);
  const [historyIndex, setHistoryIndex] = useState(0);
  const [activeFileFolders, setActiveFileFolders] = useState([]);
  const [expanded, setExpanded] = useState(new Set(["root"]));
  const [selectedIds, setSelectedIds] = useState([]);

  const [viewMode, setViewMode] = useState("details");
  const [sortBy, setSortBy] = useState("name");
  const [sortDir, setSortDir] = useState("asc");
  const [search, setSearch] = useState("");

  const [renamingId, setRenamingId] = useState(null);
  const [fileMenu, setFileMenu] = useState(null);
  const [activeFile, setActiveFile] = useState(null);
  const [showMoveDialog, setShowMoveDialog] = useState(false);

  const [loadingTree, setLoadingTree] = useState(true);
  const [loadingFiles, setLoadingFiles] = useState(false);
  const [loadError, setLoadError] = useState(null);

  const treeIndex = useMemo(() => buildTreeIndex(tree), [tree]);

  const breadcrumb = useMemo(
    () => getBreadcrumb(treeIndex, currentFolderId),
    [treeIndex, currentFolderId]
  );

  const folderNameMap = useMemo(
    () => getFolderNameMap(tree),
    [tree]
  );

  const loadTree = useCallback(async () => {
    try {
      setLoadingTree(true);
      setLoadError(null);

      const folders = await getVirtualFolders();

      setTree(toExplorerTree(folders));
      setExpanded((prev) => new Set([...prev, "root"]));
    } catch (error) {
      setLoadError(error.message);

      toast({
        title: "Could not load virtual folders",
        description: error.message,
        variant: "destructive",
      });
    } finally {
      setLoadingTree(false);
    }
  }, [toast]);

  useEffect(() => {
    loadTree();
  }, [loadTree]);

  useEffect(() => {
    let cancelled = false;

    async function loadFiles() {
      try {
        setLoadingFiles(true);
        setLoadError(null);

        const files =
          currentFolderId === "root"
            ? await searchFiles({})
            : await getVirtualFolderFiles(currentFolderId);

        if (!cancelled) {
          setFolderFiles(files ?? []);
        }
      } catch (error) {
        if (!cancelled) {
          setLoadError(error.message);

          toast({
            title: "Could not load files",
            description: error.message,
            variant: "destructive",
          });
        }
      } finally {
        if (!cancelled) {
          setLoadingFiles(false);
        }
      }
    }

    loadFiles();

    return () => {
      cancelled = true;
    };
  }, [currentFolderId, toast]);
  useEffect(() => {
    if (!activeFile?.id) {
      setActiveFileFolders([]);
      return;
    }

    let cancelled = false;

    async function loadFileFolders() {
      try {
        const folders = await getFileVirtualFolders(activeFile.id);

        if (!cancelled) {
          setActiveFileFolders(folders ?? []);
        }
      } catch (error) {
        if (!cancelled) {
          setActiveFileFolders([]);

          toast({
            title: "Could not load file folders",
            description: error.message,
            variant: "destructive",
          });
        }
      }
    }

    loadFileFolders();

    return () => {
      cancelled = true;
    };
  }, [activeFile?.id, toast]);
  const explorerFiles = useMemo(
    () =>
      folderFiles.map((file) => ({
        id: file.id,
        name: file.name,
        type: file.type,
        extension: file.extension,
        size: file.size,
        sizeBytes: file.size,
        modified: file.modified,
        physicalPath: file.path,
        hash: file.hash,

        tags: [],
        folders:
          currentFolderId === "root"
            ? []
            : [currentFolderId],

        thumbnail: null,
      })),
    [folderFiles, currentFolderId]
  );

  const filesInFolder = useMemo(() => {
    let list = explorerFiles;

    if (search.trim()) {
      const q = search.trim().toLowerCase();

      list = list.filter((file) =>
        file.name.toLowerCase().includes(q)
      );
    }

    return [...list].sort((a, b) => {
      let cmp = 0;

      if (sortBy === "size") {
        cmp = (a.sizeBytes ?? 0) - (b.sizeBytes ?? 0);
      } else if (sortBy === "modified") {
        cmp =
          new Date(a.modified ?? 0) -
          new Date(b.modified ?? 0);
      } else {
        cmp = String(a[sortBy] ?? "").localeCompare(
          String(b[sortBy] ?? "")
        );
      }

      return sortDir === "asc" ? cmp : -cmp;
    });
  }, [explorerFiles, search, sortBy, sortDir]);

  const highlightFolderIds = useMemo(() => {
    if (selectedIds.length !== 1) {
      return [];
    }

    const file = explorerFiles.find(
      (item) => item.id === selectedIds[0]
    );

    return file?.folders ?? [];
  }, [selectedIds, explorerFiles]);

  // Navigation
  const navigateTo = useCallback(
    (id) => {
      setHistory((current) => [
        ...current.slice(0, historyIndex + 1),
        id,
      ]);

      setHistoryIndex((index) => index + 1);
      setCurrentFolderId(id);
      setSelectedIds([]);
      setActiveFile(null);
    },
    [historyIndex]
  );

  const goBack = () => {
    if (historyIndex <= 0) {
      return;
    }

    const nextIndex = historyIndex - 1;

    setHistoryIndex(nextIndex);
    setCurrentFolderId(history[nextIndex]);
    setSelectedIds([]);
    setActiveFile(null);
  };

  const goForward = () => {
    if (historyIndex >= history.length - 1) {
      return;
    }

    const nextIndex = historyIndex + 1;

    setHistoryIndex(nextIndex);
    setCurrentFolderId(history[nextIndex]);
    setSelectedIds([]);
    setActiveFile(null);
  };

  const goUp = () => {
    const path = treeIndex[currentFolderId]?.path;

    if (path && path.length > 1) {
      navigateTo(path[path.length - 2]);
    }
  };

  const toggleExpand = (id) => {
    setExpanded((prev) => {
      const next = new Set(prev);

      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }

      return next;
    });
  };

  // Selection
  const handleSelect = (event, id) => {
    if (event.metaKey || event.ctrlKey) {
      setSelectedIds((prev) =>
        prev.includes(id)
          ? prev.filter((x) => x !== id)
          : [...prev, id]
      );
    } else if (event.shiftKey) {
      const ids = filesInFolder.map((file) => file.id);

      const start = ids.indexOf(
        selectedIds[selectedIds.length - 1] ??
        selectedIds[0]
      );

      const end = ids.indexOf(id);

      if (start !== -1 && end !== -1) {
        const range = ids.slice(
          Math.min(start, end),
          Math.max(start, end) + 1
        );

        setSelectedIds((prev) =>
          Array.from(new Set([...prev, ...range]))
        );
      } else {
        setSelectedIds([id]);
      }
    } else {
      setSelectedIds([id]);
    }

    const file = explorerFiles.find(
      (item) => item.id === id
    );

    setActiveFile(file ?? null);
  };

  const addFolder = async (parentId) => {
    try {
      const actualParentId =
        parentId === "root" ? null : parentId;

      const created = await createVirtualFolder({
        name: "New Folder",
        parentId: actualParentId,
      });

      await loadTree();

      setExpanded((prev) =>
        new Set([
          ...prev,
          parentId === "root" ? "root" : parentId,
        ])
      );

      setRenamingId(created.id);

      toast({
        title: "Virtual folder created",
        description:
          actualParentId
            ? "The subfolder was saved inside the selected virtual folder."
            : "The folder was saved at the root of the virtual hierarchy.",
      });
    } catch (error) {
      toast({
        title: "Could not create folder",
        description: error.message,
        variant: "destructive",
      });
    }
  };

  const renameFolder = async (id, name) => {
    const trimmedName = name.trim();

    if (!trimmedName) {
      setRenamingId(null);
      return;
    }

    try {
      await updateVirtualFolder(id, {
        name: trimmedName,
      });

      await loadTree();

      setRenamingId(null);

      toast({
        title: "Virtual folder renamed",
      });
    } catch (error) {
      toast({
        title: "Could not rename folder",
        description: error.message,
        variant: "destructive",
      });
    }
  };

  const deleteFolder = async (id) => {
    if (id === "root") return;

    try {
      await deleteVirtualFolder(id);

      if (currentFolderId === id) {
        setCurrentFolderId("root");
        setHistory(["root"]);
        setHistoryIndex(0);
        setSelectedIds([]);
        setActiveFile(null);
      }

      await loadTree();

      toast({
        title: "Virtual folder deleted",
        description: "No physical files were affected.",
      });
    } catch (error) {
      toast({
        title: "Could not delete folder",
        description: error.message,
        variant: "destructive",
      });
    }
  };

  const handleContextAction = (action, node) => {
    if (action === "new") {
      addFolder(node.id);
    } else if (action === "rename") {
      setRenamingId(node.id);
    } else if (action === "delete") {
      deleteFolder(node.id);
    } else if (action === "favorite") {
      toast({
        title: "Favorites are not connected yet",
      });
    } else if (action === "copy") {
      navigator.clipboard?.writeText(node.name);

      toast({
        title: "Folder name copied",
      });
    }
  };

  const handleDropFiles = async (fileIds, folderId) => {
    if (!fileIds?.length || folderId === "root") {
      return;
    }

    try {
      await Promise.all(
        fileIds.map((fileId) =>
          addFileToVirtualFolder(folderId, fileId)
        )
      );

      if (currentFolderId === folderId) {
        const files = await getVirtualFolderFiles(folderId);
        setFolderFiles(files ?? []);
      }

      setSelectedIds([]);

      toast({
        title: "Added to virtual folder",
        description:
          `${fileIds.length} file${fileIds.length !== 1 ? "s" : ""} added. Physical files were not moved.`,
      });
    } catch (error) {
      toast({
        title: "Could not add files",
        description: error.message,
        variant: "destructive",
      });
    }
  };

  const handleDropFolder = async (draggedId, targetId) => {
    if (draggedId === targetId) return;

    try {
      await updateVirtualFolder(draggedId, {
        parentId: targetId === "root" ? null : targetId,
        parentIdSpecified: true,
      });

      await loadTree();

      setExpanded((prev) =>
        new Set([
          ...prev,
          targetId === "root" ? "root" : targetId,
        ])
      );

      toast({
        title: "Folder moved",
        description: "The virtual hierarchy was updated.",
      });
    } catch (error) {
      toast({
        title: "Could not move folder",
        description: error.message,
        variant: "destructive",
      });
    }
  };

  const handleDragStartFiles = (event, ids) => {
    event.dataTransfer.setData(
      "application/json",
      JSON.stringify({
        type: "files",
        ids,
      })
    );

    event.dataTransfer.effectAllowed = "move";
  };

  // File actions
  const handleFileContext = (event, file) => {
    event.preventDefault();

    if (!selectedIds.includes(file.id)) {
      setSelectedIds([file.id]);
    }

    setActiveFile(file);

    setFileMenu({
      x: event.clientX,
      y: event.clientY,
      file,
    });
  };

  const handleFileAction = async (action) => {
    const file = fileMenu?.file || activeFile;

    if (!file) {
      return;
    }

    try {
      if (action === "properties") {
        setActiveFile(file);
      } else if (action === "favorite") {
        toast({
          title: "Favorites are not connected yet",
        });
      } else if (action === "copy") {
        await navigator.clipboard?.writeText(file.physicalPath);

        toast({
          title: "Physical path copied",
        });
      } else if (action === "rename") {
        toast({
          title: "Virtual rename is not connected yet",
        });
      } else if (action === "remove") {
        if (currentFolderId === "root") {
          toast({
            title: "Cannot remove from Virtual Library",
            description: "Virtual Library shows all indexed files.",
          });

          return;
        }

        await removeFileFromVirtualFolder(
          currentFolderId,
          file.id
        );

        const files =
          await getVirtualFolderFiles(currentFolderId);

        setFolderFiles(files ?? []);
        setSelectedIds([]);
        setActiveFile(null);

        toast({
          title: "Removed from virtual folder",
          description: "The physical file was not changed.",
        });
      } else if (action === "move") {
        setShowMoveDialog(true);
      }
    } catch (error) {
      toast({
        title: "Operation failed",
        description: error.message,
        variant: "destructive",
      });
    } finally {
      setFileMenu(null);
    }
  };

  const onSort = (column) => {
    if (sortBy === column) {
      setSortDir((direction) =>
        direction === "asc" ? "desc" : "asc"
      );
    } else {
      setSortBy(column);
      setSortDir("asc");
    }
  };

  const onNewFolder = () => {
    addFolder(currentFolderId);
  };

  const onRename = () => {
    if (selectedIds.length === 1) {
      toast({
        title: "Virtual reference rename is not connected yet",
      });
    }
  };

  const onDelete = () => {
    if (selectedIds.length === 0) {
      return;
    }

    toast({
      title: "Not connected yet",
      description:
        "Removing file memberships will be connected in the next step.",
    });
  };

  const onMove = () => {
    if (selectedIds.length) {
      setShowMoveDialog(true);
    }
  };

  const handleRefresh = async () => {
    await loadTree();

    if (currentFolderId !== "root") {
      try {
        setLoadingFiles(true);

        const files =
          await getVirtualFolderFiles(currentFolderId);

        setFolderFiles(files ?? []);
      } catch (error) {
        toast({
          title: "Refresh failed",
          description: error.message,
          variant: "destructive",
        });
      } finally {
        setLoadingFiles(false);
      }
    }

    toast({
      title: "Refreshed",
    });
  };

  const folderCount = filesInFolder.length;

  return (
    <div className="flex flex-col h-full overflow-hidden bg-background">
      {/* Concept banner */}
      <div className="flex items-center gap-2 px-4 py-1.5 bg-gradient-to-r from-violet-500/10 via-blue-500/10 to-transparent border-b border-border">
        <Sparkles className="w-3.5 h-3.5 text-violet-500 shrink-0" />

        <p className="text-[11px] text-muted-foreground">
          <span className="font-semibold text-foreground">
            Virtual Explorer
          </span>{" "}
          — organize files into virtual folders without
          moving anything on disk. A file can live in many
          folders at once.
        </p>
      </div>

      {loadError && (
        <div className="px-4 py-2 text-xs border-b border-destructive/30 bg-destructive/10 text-destructive">
          {loadError}
        </div>
      )}

      <ExplorerToolbar
        canBack={historyIndex > 0}
        canForward={historyIndex < history.length - 1}
        onBack={goBack}
        onForward={goForward}
        onUp={goUp}
        onRefresh={handleRefresh}
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
            {loadingTree ? (
              <div className="px-3 py-4 text-xs text-muted-foreground">
                Loading virtual folders...
              </div>
            ) : (
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
            )}
          </div>
        </div>

        {/* Center: file list */}
        <div className="flex-1 flex flex-col min-h-0">
          {loadingFiles ? (
            <div className="flex-1 flex items-center justify-center text-sm text-muted-foreground">
              Loading files...
            </div>
          ) : (
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
              onOpen={(file) => setActiveFile(file)}
              folderNameMap={folderNameMap}
            />
          )}

          {/* Status bar */}
          <div className="flex items-center justify-between px-3 py-1.5 border-t border-border bg-card/50 text-[11px] text-muted-foreground">
            <span>
              {folderCount} item
              {folderCount !== 1 ? "s" : ""}
              {selectedIds.length > 0 &&
                ` · ${selectedIds.length} selected`}
            </span>

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

          <PropertiesPanel
            file={activeFile}
            folderCount={folderCount}
            treeIndex={treeIndex}
            virtualFolders={activeFileFolders}
            onNavigate={navigateTo}
          />
        </div>
      </div>

      <FileContextMenu
        menu={fileMenu}
        onClose={() => setFileMenu(null)}
        onAction={handleFileAction}
      />

      <AnimatePresence>
        {showMoveDialog && (
          <MoveDialog
            tree={tree}
            onClose={() => setShowMoveDialog(false)}
            onConfirm={() => {
              toast({
                title: "Not connected yet",
                description:
                  "Moving file memberships will be connected in the next step.",
              });

              setShowMoveDialog(false);
            }}
          />
        )}
      </AnimatePresence>
    </div>
  );
}

function MoveDialog({
  tree,
  onClose,
  onConfirm,
}) {
  const [target, setTarget] = useState(null);

  const flat = [];

  const walk = (node, depth) => {
    if (node.id !== "root") {
      flat.push({
        id: node.id,
        name: node.name,
        depth,
      });
    }

    (node.children || []).forEach((child) =>
      walk(child, depth + 1)
    );
  };

  walk(tree, 0);

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm"
      onClick={onClose}
    >
      <motion.div
        initial={{
          opacity: 0,
          scale: 0.96,
          y: 8,
        }}
        animate={{
          opacity: 1,
          scale: 1,
          y: 0,
        }}
        exit={{
          opacity: 0,
          scale: 0.96,
        }}
        className="w-full max-w-sm bg-card border border-border rounded-xl shadow-2xl p-4"
        onClick={(event) =>
          event.stopPropagation()
        }
      >
        <div className="flex items-center justify-between mb-3">
          <h3 className="font-semibold text-sm">
            Move to virtual folder
          </h3>

          <button
            onClick={onClose}
            className="p-1 rounded hover:bg-muted"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        <p className="text-xs text-muted-foreground mb-3">
          Adds a virtual reference — the original file is not
          moved on disk.
        </p>

        <div className="max-h-64 overflow-auto rounded-lg border border-border bg-background">
          {flat.map((folder) => (
            <button
              key={folder.id}
              onClick={() =>
                setTarget(folder.id)
              }
              className={`w-full text-left px-3 py-1.5 text-sm flex items-center gap-2 hover:bg-muted transition-colors ${target === folder.id
                ? "bg-primary/10 text-primary font-medium"
                : ""
                }`}
              style={{
                paddingLeft:
                  folder.depth * 14 + 12,
              }}
            >
              <span className="text-amber-500">
                📁
              </span>

              {folder.name}
            </button>
          ))}
        </div>

        <div className="flex justify-end gap-2 mt-4">
          <button
            onClick={onClose}
            className="px-3 py-1.5 text-sm rounded-md hover:bg-muted"
          >
            Cancel
          </button>

          <button
            disabled={!target}
            onClick={() =>
              onConfirm(target)
            }
            className="px-3 py-1.5 text-sm rounded-md bg-primary text-primary-foreground disabled:opacity-40"
          >
            Add reference
          </button>
        </div>
      </motion.div>
    </div>
  );
}