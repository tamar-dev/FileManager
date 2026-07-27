import React, { useState, useRef, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import {
  Search as SearchIcon, ScanLine, Bell, Sun, Moon, Menu,
  Check, Info, AlertTriangle, ChevronDown
} from "lucide-react";
import { useTheme } from "@/lib/ThemeContext";
import { notifications } from "@/lib/mockData";
import { cn } from "@/lib/utils";

export default function TopBar({ onMenuClick }) {
  const { theme, toggleTheme } = useTheme();
  const navigate = useNavigate();
  const [notifOpen, setNotifOpen] = useState(false);
  const [userOpen, setUserOpen] = useState(false);
  const [scanning, setScanning] = useState(false);
  const notifRef = useRef(null);
  const userRef = useRef(null);

  useEffect(() => {
    const handler = (e) => {
      if (notifRef.current && !notifRef.current.contains(e.target)) setNotifOpen(false);
      if (userRef.current && !userRef.current.contains(e.target)) setUserOpen(false);
    };
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, []);

  const handleScan = () => {
    setScanning(true);
    setTimeout(() => setScanning(false), 2500);
  };

  const notifIcon = { success: Check, info: Info, warning: AlertTriangle };
  const notifColor = {
    success: "bg-emerald-500 text-white",
    info: "bg-blue-500 text-white",
    warning: "bg-amber-500 text-white",
  };
  const unreadCount = notifications.filter(n => !n.read).length;

  return (
    <header className="sticky top-0 z-30 h-16 glass bg-background/80 border-b border-border flex items-center gap-3 px-4 lg:px-6">
      <button onClick={onMenuClick} className="lg:hidden text-muted-foreground hover:text-foreground">
        <Menu className="w-5 h-5" />
      </button>

      {/* Global search */}
      <button
        onClick={() => navigate("/search")}
        className="flex-1 max-w-md flex items-center gap-2.5 px-3.5 h-9 rounded-lg bg-muted/60 hover:bg-muted border border-transparent hover:border-border transition-all text-left group"
      >
        <SearchIcon className="w-4 h-4 text-muted-foreground group-hover:text-foreground" />
        <span className="text-sm text-muted-foreground">Search files, folders, tags…</span>
        <kbd className="ml-auto hidden md:inline-flex items-center gap-0.5 px-1.5 h-5 rounded text-[10px] font-medium bg-background border border-border text-muted-foreground">⌘K</kbd>
      </button>

      <div className="flex-1 lg:hidden" />

      <div className="flex items-center gap-1.5">
        {/* Scan */}
        <button
          onClick={handleScan}
          disabled={scanning}
          className={cn(
            "flex items-center gap-2 px-3 h-9 rounded-lg text-sm font-medium transition-all",
            scanning
              ? "bg-violet-500/10 text-violet-600 dark:text-violet-400"
              : "bg-violet-600 hover:bg-violet-700 text-white shadow-sm shadow-violet-500/20"
          )}
        >
          <ScanLine className={cn("w-4 h-4", scanning && "animate-pulse")} />
          <span className="hidden sm:inline">{scanning ? "Scanning…" : "Scan"}</span>
        </button>

        {/* Notifications */}
        <div className="relative" ref={notifRef}>
          <button
            onClick={() => setNotifOpen(o => !o)}
            className="relative w-9 h-9 flex items-center justify-center rounded-lg hover:bg-muted text-muted-foreground hover:text-foreground transition-colors"
          >
            <Bell className="w-[18px] h-[18px]" />
            {unreadCount > 0 && (
              <span className="absolute top-1.5 right-1.5 w-2 h-2 rounded-full bg-violet-500 ring-2 ring-background" />
            )}
          </button>
          <AnimatePresence>
            {notifOpen && (
              <motion.div
                initial={{ opacity: 0, y: 8, scale: 0.97 }}
                animate={{ opacity: 1, y: 0, scale: 1 }}
                exit={{ opacity: 0, y: 8, scale: 0.97 }}
                transition={{ duration: 0.15 }}
                className="absolute right-0 mt-2 w-80 rounded-xl bg-popover border border-border shadow-xl shadow-black/10 overflow-hidden"
              >
                <div className="px-4 py-3 border-b border-border flex items-center justify-between">
                  <span className="text-sm font-semibold">Notifications</span>
                  <span className="text-xs text-muted-foreground">{unreadCount} new</span>
                </div>
                <div className="max-h-80 overflow-y-auto">
                  {notifications.map(n => {
                    const Icon = notifIcon[n.type];
                    return (
                      <div key={n.id} className={cn("flex gap-3 px-4 py-3 border-b border-border/50 last:border-0 hover:bg-muted/40 transition-colors", !n.read && "bg-violet-500/[0.03]")}>
                        <div className={cn("w-7 h-7 rounded-lg flex items-center justify-center shrink-0", notifColor[n.type])}>
                          <Icon className="w-3.5 h-3.5" strokeWidth={2.5} />
                        </div>
                        <div className="min-w-0">
                          <div className="text-sm font-medium leading-tight">{n.title}</div>
                          <div className="text-xs text-muted-foreground mt-0.5 leading-snug">{n.body}</div>
                          <div className="text-[11px] text-muted-foreground/70 mt-1">{n.time}</div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </motion.div>
            )}
          </AnimatePresence>
        </div>

        {/* Theme toggle */}
        <button
          onClick={toggleTheme}
          className="w-9 h-9 flex items-center justify-center rounded-lg hover:bg-muted text-muted-foreground hover:text-foreground transition-colors"
        >
          <AnimatePresence mode="wait">
            {theme === "light" ? (
              <motion.span key="moon" initial={{ rotate: -90, opacity: 0 }} animate={{ rotate: 0, opacity: 1 }} exit={{ rotate: 90, opacity: 0 }} transition={{ duration: 0.2 }}>
                <Moon className="w-[18px] h-[18px]" />
              </motion.span>
            ) : (
              <motion.span key="sun" initial={{ rotate: 90, opacity: 0 }} animate={{ rotate: 0, opacity: 1 }} exit={{ rotate: -90, opacity: 0 }} transition={{ duration: 0.2 }}>
                <Sun className="w-[18px] h-[18px]" />
              </motion.span>
            )}
          </AnimatePresence>
        </button>

        {/* User menu */}
        <div className="relative" ref={userRef}>
          <button
            onClick={() => setUserOpen(o => !o)}
            className="flex items-center gap-2 pl-1.5 pr-2 h-9 rounded-lg hover:bg-muted transition-colors"
          >
            <div className="w-7 h-7 rounded-full bg-gradient-to-br from-violet-500 to-indigo-600 flex items-center justify-center text-white text-xs font-semibold">
              JD
            </div>
            <ChevronDown className="w-3.5 h-3.5 text-muted-foreground hidden sm:block" />
          </button>
          <AnimatePresence>
            {userOpen && (
              <motion.div
                initial={{ opacity: 0, y: 8, scale: 0.97 }}
                animate={{ opacity: 1, y: 0, scale: 1 }}
                exit={{ opacity: 0, y: 8, scale: 0.97 }}
                transition={{ duration: 0.15 }}
                className="absolute right-0 mt-2 w-60 rounded-xl bg-popover border border-border shadow-xl shadow-black/10 overflow-hidden"
              >
                <div className="px-4 py-3 border-b border-border">
                  <div className="text-sm font-semibold">Jordan Doe</div>
                  <div className="text-xs text-muted-foreground">jordan@filemanager.ai</div>
                </div>
                <div className="p-1.5">
                  {["Account Settings", "Billing & Plan", "Keyboard Shortcuts", "Help & Support"].map(item => (
                    <button key={item} className="w-full text-left px-3 py-2 rounded-lg text-sm hover:bg-muted transition-colors">
                      {item}
                    </button>
                  ))}
                </div>
                <div className="p-1.5 border-t border-border">
                  <button className="w-full text-left px-3 py-2 rounded-lg text-sm text-rose-600 hover:bg-rose-500/10 transition-colors">
                    Sign out
                  </button>
                </div>
              </motion.div>
            )}
          </AnimatePresence>
        </div>
      </div>
    </header>
  );
}