import React from "react";
import { motion } from "framer-motion";
import { Pencil, Trash2, FolderInput, Star, Copy, Info, X } from "lucide-react";
import { cn } from "@/lib/utils";

export default function FileContextMenu({ menu, onClose, onAction }) {
  if (!menu) return null;
  return (
    <>
      <div className="fixed inset-0 z-40" onClick={onClose} onContextMenu={(e) => { e.preventDefault(); onClose(); }} />
      <motion.div
        initial={{ opacity: 0, scale: 0.96 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.12 }}
        className="fixed z-50 min-w-[190px] bg-popover border border-border rounded-lg shadow-xl py-1 text-sm"
        style={{
          left: Math.min(menu.x, window.innerWidth - 210),
          top: Math.min(menu.y, window.innerHeight - 260),
        }}
      >
        <Item icon={Info} label="Properties" onClick={() => { onAction("properties"); onClose(); }} />
        <Item icon={FolderInput} label="Move to folder" onClick={() => { onAction("move"); onClose(); }} />
        <Item icon={Star} label="Toggle favorite" onClick={() => { onAction("favorite"); onClose(); }} />
        <Item icon={Copy} label="Copy virtual reference" onClick={() => { onAction("copy"); onClose(); }} />
        <div className="my-1 h-px bg-border" />
        <Item icon={Pencil} label="Rename" onClick={() => { onAction("rename"); onClose(); }} />
        <Item icon={Trash2} label="Remove from folder" danger onClick={() => { onAction("remove"); onClose(); }} />
      </motion.div>
    </>
  );
}

function Item({ icon: Icon, label, onClick, danger }) {
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