import React, { useState } from "react";
import { NavLink, useLocation } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import {
  LayoutDashboard, Search, FolderTree, Copy, Image as ImageIcon,
  Clock, Star, Activity, Settings, ChevronRight, Sparkles, X, FolderOpen
} from "lucide-react";
import { cn } from "@/lib/utils";

const navItems = [
  { to: "/", label: "Dashboard", icon: LayoutDashboard },
  { to: "/virtual-explorer", label: "Virtual Explorer", icon: FolderOpen, badge: "New" },
  { to: "/search", label: "Search", icon: Search },
  { to: "/virtual-folders", label: "Virtual Folders", icon: FolderTree },
  { to: "/duplicates", label: "Duplicates", icon: Copy },
  { to: "/photos", label: "Photos", icon: ImageIcon },
  { to: "/recent", label: "Recent", icon: Clock },
  { to: "/favorites", label: "Favorites", icon: Star },
  { to: "/index-status", label: "Index Status", icon: Activity },
  { to: "/settings", label: "Settings", icon: Settings },
];

export default function Sidebar({ open, onClose }) {
  const location = useLocation();

  return (
    <>
      {/* Mobile overlay */}
      <AnimatePresence>
        {open && (
          <motion.div
            initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
            onClick={onClose}
            className="fixed inset-0 bg-black/40 z-40 lg:hidden"
          />
        )}
      </AnimatePresence>

      <aside className={cn(
        "fixed lg:sticky top-0 left-0 h-screen w-64 z-50 shrink-0",
        "bg-sidebar border-r border-sidebar-border flex flex-col",
        "transition-transform duration-300 lg:translate-x-0",
        open ? "translate-x-0" : "-translate-x-full"
      )}>
        {/* Logo */}
        <div className="h-16 flex items-center justify-between px-5 border-b border-sidebar-border">
          <div className="flex items-center gap-2.5">
            <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-violet-500 to-indigo-600 flex items-center justify-center shadow-lg shadow-violet-500/20">
              <Sparkles className="w-[18px] h-[18px] text-white" strokeWidth={2.5} />
            </div>
            <div className="leading-none">
              <div className="text-[15px] font-semibold tracking-tight">FileManager</div>
              <div className="text-[10px] font-medium text-muted-foreground tracking-wide">AI EDITION</div>
            </div>
          </div>
          <button onClick={onClose} className="lg:hidden text-muted-foreground hover:text-foreground">
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Nav */}
        <nav className="flex-1 overflow-y-auto px-3 py-4 space-y-0.5">
          <div className="px-3 pb-2 text-[11px] font-semibold text-muted-foreground/70 uppercase tracking-wider">Workspace</div>
          {navItems.map((item) => {
            const active = location.pathname === item.to;
            const Icon = item.icon;
            return (
              <NavLink
                key={item.to}
                to={item.to}
                onClick={onClose}
                className={cn(
                  "group relative flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-all",
                  active
                    ? "text-sidebar-accent-foreground bg-sidebar-accent"
                    : "text-muted-foreground hover:text-foreground hover:bg-sidebar-accent/50"
                )}
              >
                {active && (
                  <motion.div
                    layoutId="sidebar-active"
                    className="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-5 rounded-r-full bg-violet-500"
                  />
                )}
                <Icon className="w-[18px] h-[18px] shrink-0" strokeWidth={active ? 2.4 : 2} />
                <span className="flex-1">{item.label}</span>
                {item.badge && (
                  <span className="text-[9px] font-bold px-1.5 py-0.5 rounded-full bg-violet-500 text-white">
                    {item.badge}
                  </span>
                )}
                {active && <ChevronRight className="w-4 h-4 text-muted-foreground" />}
              </NavLink>
            );
          })}
        </nav>

        {/* Upgrade card */}
        <div className="p-3">
          <div className="rounded-xl bg-gradient-to-br from-violet-500/10 to-indigo-500/10 border border-violet-500/20 p-4">
            <div className="flex items-center gap-2 mb-1.5">
              <div className="w-6 h-6 rounded-md bg-gradient-to-br from-violet-500 to-indigo-600 flex items-center justify-center">
                <Sparkles className="w-3.5 h-3.5 text-white" />
              </div>
              <span className="text-sm font-semibold">Pro Plan</span>
            </div>
            <p className="text-xs text-muted-foreground mb-3">Unlimited indexing & AI organization across all drives.</p>
            <button className="w-full text-xs font-semibold bg-violet-600 hover:bg-violet-700 text-white py-2 rounded-lg transition-colors">
              Upgrade
            </button>
          </div>
        </div>
      </aside>
    </>
  );
}