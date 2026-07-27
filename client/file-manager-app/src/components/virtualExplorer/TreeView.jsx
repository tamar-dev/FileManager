import React, { useState, useRef, useEffect } from "react";
import { motion, AnimatePresence } from "framer-motion";
import {
  ChevronRight,
  Folder as FolderIcon,
  FolderOpen,
  Plus,
  Pencil,
  Trash2,
  Star,
  Copy,
} from "lucide-react";
import { cn } from "@/lib/utils";

export default function TreeView({
  tree,
  treeIndex,
  currentFolderId,
  expanded,
  toggleExpand,
  onSelectFolder,
  onContextAction,
  selectedFileIds,
  onDropFiles,
  onDropFolder,
  renamingId,
  setRenamingId,
  onRename,
  highlightFolderIds,
}) {
  return (
    <div className="select-none">
      <TreeNode
        node={tree}
        depth={0}
        treeIndex={treeIndex}
        currentFolderId={currentFolderId}
        expanded={expanded}
        toggleExpand={toggleExpand}
        onSelectFolder={onSelectFolder}
        onContextAction={onContextAction}
        selectedFileIds={selectedFileIds}
        onDropFiles={onDropFiles}
        onDropFolder={onDropFolder}
        renamingId={renamingId}
        setRenamingId={setRenamingId}
        onRename={onRename}
        highlightFolderIds={highlightFolderIds}
        isRoot
      />
    </div>
  );
}

function TreeNode({
  node,
  depth,
  treeIndex,
  currentFolderId,
  expanded,
  toggleExpand,
  onSelectFolder,
  onContextAction,
  selectedFileIds,
  onDropFiles,
  onDropFolder,
  renamingId,
  setRenamingId,
  onRename,
  highlightFolderIds,
  isRoot,
}) {
  const hasChildren = node.children && node.children.length > 0;
  const isOpen = expanded.has(node.id);
  const isActive = currentFolderId === node.id;
  const [menu, setMenu] = useState(null);
  const [dragOver, setDragOver] = useState(false);
  const menuRef = useRef(null);

  useEffect(() => {
    if (!menu) return;
    const handler = (e) => {
      if (menuRef.current && !menuRef.current.contains(e.target)) setMenu(null);
    };
    window.addEventListener("mousedown", handler);
    return () => window.removeEventListener("mousedown", handler);
  }, [menu]);

  const handleContextMenu = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setMenu({ x: e.clientX, y: e.clientY });
  };

  const runAction = (action) => {
    setMenu(null);
    onContextAction(action, node);
  };

  const handleDrop = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setDragOver(false);
    const payload = e.dataTransfer.getData("application/json");
    if (!payload) return;
    const data = JSON.parse(payload);
    if (data.type === "files") {
      onDropFiles(data.ids, node.id);
    } else if (data.type === "folder") {
      onDropFolder(data.id, node.id);
    }
  };

  return (
    <div>
      <div
        className={cn(
          "group flex items-center gap-1 pr-2 pl-1 py-1 rounded-md cursor-pointer text-sm transition-colors",
          isActive ? "bg-primary/10 text-primary" : "hover:bg-muted/60",
          dragOver && "ring-2 ring-primary/40 bg-primary/5"
        )}
        style={{ paddingLeft: depth * 12 + 4 }}
        draggable={!isRoot}
        onDragStart={(e) => {
          if (isRoot) return;
          e.dataTransfer.setData("application/json", JSON.stringify({ type: "folder", id: node.id }));
        }}
        onDragOver={(e) => {
          e.preventDefault();
          setDragOver(true);
        }}
        onDragLeave={() => setDragOver(false)}
        onDrop={handleDrop}
        onClick={() => onSelectFolder(node.id)}
        onContextMenu={handleContextMenu}
      >
        <button
          className={cn(
            "shrink-0 p-0.5 rounded hover:bg-muted",
            hasChildren ? "opacity-100" : "opacity-0 pointer-events-none"
          )}
          onClick={(e) => {
            e.stopPropagation();
            toggleExpand(node.id);
          }}
        >
          <ChevronRight
            className={cn("w-3.5 h-3.5 transition-transform", isOpen && "rotate-90")}
          />
        </button>
        {isOpen && hasChildren ? (
          <FolderOpen className="w-4 h-4 shrink-0 text-amber-500" />
        ) : (
          <FolderIcon className="w-4 h-4 shrink-0 text-amber-500" />
        )}
        {renamingId === node.id ? (
          <input
            autoFocus
            defaultValue={node.name}
            className="flex-1 min-w-0 bg-background border border-ring rounded px-1 py-0.5 text-xs outline-none"
            onClick={(e) => e.stopPropagation()}
            onBlur={(e) => onRename(node.id, e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter") onRename(node.id, e.target.value);
              if (e.key === "Escape") setRenamingId(null);
            }}
          />
        ) : (
          <span className="flex-1 truncate font-medium">{node.name}</span>
        )}
        {highlightFolderIds?.includes(node.id) && !isActive && (
          <span
            className="w-1.5 h-1.5 rounded-full bg-violet-500 shrink-0"
            title="Contains the selected file"
          />
        )}
        {isRoot && (
          <span className="text-[10px] text-muted-foreground/70 font-normal px-1 rounded bg-muted">
            virtual
          </span>
        )}
      </div>

      <AnimatePresence initial={false}>
        {isOpen && hasChildren && (
          <motion.div
            initial={{ height: 0, opacity: 0 }}
            animate={{ height: "auto", opacity: 1 }}
            exit={{ height: 0, opacity: 0 }}
            transition={{ duration: 0.18, ease: "easeOut" }}
            className="overflow-hidden"
          >
            {node.children.map((child) => (
              <TreeNode
                key={child.id}
                node={child}
                depth={depth + 1}
                treeIndex={treeIndex}
                currentFolderId={currentFolderId}
                expanded={expanded}
                toggleExpand={toggleExpand}
                onSelectFolder={onSelectFolder}
                onContextAction={onContextAction}
                selectedFileIds={selectedFileIds}
                onDropFiles={onDropFiles}
                onDropFolder={onDropFolder}
                renamingId={renamingId}
                setRenamingId={setRenamingId}
                onRename={onRename}
                highlightFolderIds={highlightFolderIds}
              />
            ))}
          </motion.div>
        )}
      </AnimatePresence>

      {menu && (
        <div
          ref={menuRef}
          className="fixed z-50 min-w-[180px] bg-popover border border-border rounded-lg shadow-xl py-1 text-sm animate-fade-in"
          style={{ left: Math.min(menu.x, window.innerWidth - 200), top: Math.min(menu.y, window.innerHeight - 220) }}
        >
          <MenuItem icon={Plus} label="New subfolder" onClick={() => runAction("new")} />
          <MenuItem icon={Pencil} label="Rename" onClick={() => runAction("rename")} />
          <MenuItem icon={Star} label="Add to favorites" onClick={() => runAction("favorite")} />
          <MenuItem icon={Copy} label="Copy path" onClick={() => runAction("copy")} />
          <div className="my-1 h-px bg-border" />
          <MenuItem icon={Trash2} label="Delete" danger onClick={() => runAction("delete")} />
        </div>
      )}
    </div>
  );
}

function MenuItem({ icon: Icon, label, onClick, danger }) {
  return (
    <button
      onClick={onClick}
      className={cn(
        "w-full flex items-center gap-2.5 px-3 py-1.5 text-left hover:bg-muted transition-colors",
        danger && "text-destructive hover:bg-destructive/10"
      )}
    >
      <Icon className="w-3.5 h-3.5" />
      {label}
    </button>
  );
}