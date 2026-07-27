import React, { useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import {
  FolderTree, Plus, Tag, Zap, MoreVertical, Trash2, Edit3,
  Folder, Image as ImageIcon, File as FileIcon, Clock, Palette
} from "lucide-react";
import PageHeader from "@/components/PageHeader";
import { virtualFolders } from "@/lib/mockData";
import { cn } from "@/lib/utils";

const folderIcons = { folder: Folder, image: ImageIcon, file: FileIcon, clock: Clock, palette: Palette };

export default function VirtualFolders() {
  const [folders, setFolders] = useState(virtualFolders);
  const [showCreate, setShowCreate] = useState(false);
  const [dragOver, setDragOver] = useState(null);
  const [draggedTag, setDraggedTag] = useState(null);

  const availableTags = ["work", "design", "finance", "vacation", "urgent", "archive", "atlas", "media"];

  const handleDrop = (folderId) => {
    if (draggedTag) {
      setFolders(fs => fs.map(f => f.id === folderId && !f.tags.includes(draggedTag)
        ? { ...f, tags: [...f.tags, draggedTag] } : f));
    }
    setDragOver(null);
    setDraggedTag(null);
  };

  const removeTag = (folderId, tag) => {
    setFolders(fs => fs.map(f => f.id === folderId ? { ...f, tags: f.tags.filter(t => t !== tag) } : f));
  };

  const colorClasses = {
    blue: "from-blue-500/10 to-cyan-500/10 text-blue-500 border-blue-500/20",
    emerald: "from-emerald-500/10 to-teal-500/10 text-emerald-500 border-emerald-500/20",
    amber: "from-amber-500/10 to-orange-500/10 text-amber-500 border-amber-500/20",
    rose: "from-rose-500/10 to-pink-500/10 text-rose-500 border-rose-500/20",
    violet: "from-violet-500/10 to-indigo-500/10 text-violet-500 border-violet-500/20",
    cyan: "from-cyan-500/10 to-sky-500/10 text-cyan-500 border-cyan-500/20",
  };

  return (
    <div className="p-6 lg:p-8 max-w-[1400px] mx-auto animate-fade-in">
      <PageHeader
        title="Virtual Folders"
        subtitle="Organize files into smart collections without moving them."
        actions={
          <button onClick={() => setShowCreate(true)} className="flex items-center gap-2 px-4 h-9 rounded-lg bg-violet-600 hover:bg-violet-700 text-white text-sm font-medium transition-colors">
            <Plus className="w-4 h-4" /> New folder
          </button>
        }
      />

      {/* Drag & drop hint */}
      <div className="flex items-center gap-2 mb-5 p-3 rounded-lg bg-violet-500/5 border border-violet-500/15">
        <Zap className="w-4 h-4 text-violet-500 shrink-0" />
        <p className="text-xs text-muted-foreground">Drag tags from the tray below onto any folder to add smart rules. Files are never moved — only referenced.</p>
      </div>

      {/* Tag tray */}
      <div className="mb-6">
        <div className="flex items-center gap-1.5 text-xs font-medium text-muted-foreground mb-2">
          <Tag className="w-3.5 h-3.5" /> Tag tray — drag onto folders
        </div>
        <div className="flex flex-wrap gap-2">
          {availableTags.map(tag => (
            <div
              key={tag}
              draggable
              onDragStart={() => setDraggedTag(tag)}
              onDragEnd={() => setDraggedTag(null)}
              className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-card border border-border text-xs font-medium cursor-grab active:cursor-grabbing hover:border-violet-500/40 hover:bg-violet-500/5 transition-all"
            >
              <Tag className="w-3 h-3 text-muted-foreground" /> {tag}
            </div>
          ))}
        </div>
      </div>

      {/* Folders grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {folders.map((folder, i) => {
          const Icon = folderIcons[folder.icon] || Folder;
          return (
            <motion.div
              key={folder.id}
              initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: i * 0.04 }}
              onDragOver={(e) => { e.preventDefault(); setDragOver(folder.id); }}
              onDragLeave={() => setDragOver(null)}
              onDrop={() => handleDrop(folder.id)}
              className={cn("group relative bg-card border rounded-xl p-5 transition-all",
                dragOver === folder.id ? "border-violet-500 ring-2 ring-violet-500/20 scale-[1.02]" : "border-border hover:shadow-lg hover:shadow-black/5")}
            >
              <div className="flex items-start justify-between mb-4">
                <div className={cn("w-12 h-12 rounded-xl bg-gradient-to-br border flex items-center justify-center", colorClasses[folder.color])}>
                  <Icon className="w-6 h-6" strokeWidth={2} />
                </div>
                {folder.smart && (
                  <span className="flex items-center gap-1 px-2 py-0.5 rounded-md bg-violet-500/10 text-violet-600 dark:text-violet-400 text-[10px] font-semibold">
                    <Zap className="w-2.5 h-2.5" /> SMART
                  </span>
                )}
              </div>
              <h3 className="text-sm font-semibold mb-1">{folder.name}</h3>
              <div className="flex items-center gap-1.5 text-xs text-muted-foreground mb-3">
                <FolderTree className="w-3.5 h-3.5" /> {folder.count.toLocaleString()} files
              </div>
              <div className="text-xs text-muted-foreground mb-3 px-2.5 py-1.5 rounded-md bg-muted/50 font-mono truncate">
                {folder.rule}
              </div>
              <div className="flex flex-wrap gap-1.5">
                {folder.tags.map(tag => (
                  <span key={tag} className="group/tag flex items-center gap-1 px-2 py-0.5 rounded-md bg-muted text-xs">
                    {tag}
                    <button onClick={() => removeTag(folder.id, tag)} className="text-muted-foreground hover:text-rose-500 opacity-0 group-hover/tag:opacity-100 transition-opacity">
                      <Trash2 className="w-2.5 h-2.5" />
                    </button>
                  </span>
                ))}
              </div>
              <div className="absolute top-4 right-4 opacity-0 group-hover:opacity-100 transition-opacity">
                <button className="w-7 h-7 rounded-md hover:bg-muted flex items-center justify-center text-muted-foreground">
                  <MoreVertical className="w-4 h-4" />
                </button>
              </div>
            </motion.div>
          );
        })}
      </div>

      {/* Create modal */}
      <AnimatePresence>
        {showCreate && (
          <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
            onClick={() => setShowCreate(false)}
            className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm flex items-center justify-center p-4">
            <motion.div initial={{ scale: 0.95, opacity: 0 }} animate={{ scale: 1, opacity: 1 }} exit={{ scale: 0.95, opacity: 0 }}
              onClick={e => e.stopPropagation()}
              className="bg-card border border-border rounded-2xl w-full max-w-md p-6">
              <h3 className="text-lg font-semibold mb-1">New Virtual Folder</h3>
              <p className="text-sm text-muted-foreground mb-5">Create a smart collection that references files in place.</p>
              <div className="space-y-4">
                <div>
                  <label className="text-xs font-medium text-muted-foreground mb-1.5 block">Folder name</label>
                  <input autoFocus placeholder="e.g. Q4 Marketing Assets" className="w-full h-10 px-3 rounded-lg bg-background border border-border text-sm focus:outline-none focus:ring-2 focus:ring-violet-500/30" />
                </div>
                <div>
                  <label className="text-xs font-medium text-muted-foreground mb-1.5 block">Smart rule</label>
                  <input placeholder="e.g. Type: image + Tag: marketing" className="w-full h-10 px-3 rounded-lg bg-background border border-border text-sm focus:outline-none focus:ring-2 focus:ring-violet-500/30 font-mono" />
                </div>
              </div>
              <div className="flex items-center gap-2 mt-6">
                <button onClick={() => setShowCreate(false)} className="flex-1 h-10 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors">Cancel</button>
                <button onClick={() => setShowCreate(false)} className="flex-1 h-10 rounded-lg bg-violet-600 hover:bg-violet-700 text-white text-sm font-medium transition-colors">Create folder</button>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}