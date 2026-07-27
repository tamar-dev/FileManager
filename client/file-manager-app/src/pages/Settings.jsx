import React, { useState } from "react";
import { motion } from "framer-motion";
import {
  FolderOpen, Gauge, Database, Palette, ShieldCheck, HardDrive,
  Plus, Trash2, Sun, Moon, Zap, Lock, RefreshCw
} from "lucide-react";
import PageHeader from "@/components/PageHeader";
import { indexedLocations } from "@/lib/mockData";
import { useTheme } from "@/lib/ThemeContext";
import { cn } from "@/lib/utils";

const sections = [
  { id: "locations", label: "Indexed Locations", icon: FolderOpen },
  { id: "performance", label: "Performance", icon: Gauge },
  { id: "cache", label: "Cache", icon: Database },
  { id: "appearance", label: "Appearance", icon: Palette },
  { id: "backup", label: "Backup", icon: HardDrive },
  { id: "security", label: "Security", icon: ShieldCheck },
];

export default function Settings() {
  const [active, setActive] = useState("locations");
  const { theme, toggleTheme } = useTheme();
  const [perfMode, setPerfMode] = useState("balanced");
  const [autoScan, setAutoScan] = useState(true);
  const [encrypt, setEncrypt] = useState(true);
  const [twofa, setTwofa] = useState(false);

  return (
    <div className="p-6 lg:p-8 max-w-[1400px] mx-auto animate-fade-in">
      <PageHeader title="Settings" subtitle="Configure FileManager AI to your needs." />

      <div className="flex flex-col lg:flex-row gap-6">
        {/* Section nav */}
        <div className="lg:w-56 shrink-0">
          <div className="flex lg:flex-col gap-1 overflow-x-auto lg:overflow-visible pb-2 lg:pb-0">
            {sections.map(s => {
              const Icon = s.icon;
              const isActive = active === s.id;
              return (
                <button
                  key={s.id}
                  onClick={() => setActive(s.id)}
                  className={cn("flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm font-medium transition-all whitespace-nowrap shrink-0",
                    isActive ? "bg-violet-500/10 text-violet-600 dark:text-violet-400" : "text-muted-foreground hover:text-foreground hover:bg-muted")}
                >
                  <Icon className="w-4 h-4" /> {s.label}
                </button>
              );
            })}
          </div>
        </div>

        {/* Content */}
        <div className="flex-1 min-w-0">
          <motion.div key={active} initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.25 }}>
            {active === "locations" && (
              <Card title="Indexed Locations" desc="Folders that FileManager AI monitors and indexes.">
                <div className="divide-y divide-border/50">
                  {indexedLocations.map(loc => (
                    <div key={loc.id} className="flex items-center gap-3 py-3">
                      <FolderOpen className="w-4.5 h-4.5 text-muted-foreground shrink-0" />
                      <div className="min-w-0 flex-1">
                        <div className="text-sm font-mono truncate">{loc.path}</div>
                        <div className="text-xs text-muted-foreground">{loc.files.toLocaleString()} files · {loc.size}</div>
                      </div>
                      <button className="w-8 h-8 rounded-lg hover:bg-muted flex items-center justify-center text-muted-foreground hover:text-rose-500">
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  ))}
                </div>
                <button className="mt-4 flex items-center gap-2 px-3 h-9 rounded-lg border border-dashed border-border hover:border-violet-500 hover:text-violet-500 text-sm font-medium transition-colors w-full justify-center">
                  <Plus className="w-4 h-4" /> Add location
                </button>
              </Card>
            )}

            {active === "performance" && (
              <Card title="Performance" desc="Control how aggressively the indexer uses system resources.">
                <div className="space-y-2">
                  {[
                    { id: "eco", label: "Eco", desc: "Minimal resource usage, slower scans" },
                    { id: "balanced", label: "Balanced", desc: "Recommended for most users" },
                    { id: "turbo", label: "Turbo", desc: "Maximum speed, higher CPU usage" },
                  ].map(opt => (
                    <button key={opt.id} onClick={() => setPerfMode(opt.id)}
                      className={cn("w-full flex items-center gap-3 p-3 rounded-lg border text-left transition-all",
                        perfMode === opt.id ? "border-violet-500 bg-violet-500/5" : "border-border hover:bg-muted/50")}>
                      <div className={cn("w-4 h-4 rounded-full border-2 flex items-center justify-center", perfMode === opt.id ? "border-violet-500" : "border-border")}>
                        {perfMode === opt.id && <div className="w-2 h-2 rounded-full bg-violet-500" />}
                      </div>
                      <div className="flex-1">
                        <div className="text-sm font-medium">{opt.label}</div>
                        <div className="text-xs text-muted-foreground">{opt.desc}</div>
                      </div>
                      {opt.id === "turbo" && <Zap className="w-4 h-4 text-amber-500" />}
                    </button>
                  ))}
                </div>
                <Toggle label="Auto-scan on startup" desc="Begin indexing when the app launches" checked={autoScan} onChange={setAutoScan} />
              </Card>
            )}

            {active === "cache" && (
              <Card title="Cache" desc="Manage local cache and thumbnails.">
                <div className="space-y-4">
                  <Row label="Thumbnail cache" value="1.4 GB">
                    <button className="flex items-center gap-1.5 px-3 h-8 rounded-lg border border-border hover:bg-muted text-xs font-medium transition-colors">
                      <RefreshCw className="w-3.5 h-3.5" /> Clear
                    </button>
                  </Row>
                  <Row label="Search index cache" value="640 MB">
                    <button className="flex items-center gap-1.5 px-3 h-8 rounded-lg border border-border hover:bg-muted text-xs font-medium transition-colors">
                      <RefreshCw className="w-3.5 h-3.5" /> Clear
                    </button>
                  </Row>
                  <Row label="Preview cache" value="2.1 GB">
                    <button className="flex items-center gap-1.5 px-3 h-8 rounded-lg border border-border hover:bg-muted text-xs font-medium transition-colors">
                      <RefreshCw className="w-3.5 h-3.5" /> Clear
                    </button>
                  </Row>
                </div>
              </Card>
            )}

            {active === "appearance" && (
              <Card title="Appearance" desc="Customize the look and feel of FileManager AI.">
                <div className="mb-4">
                  <div className="text-sm font-medium mb-2">Theme</div>
                  <div className="grid grid-cols-2 gap-3">
                    <button onClick={() => theme !== "light" && toggleTheme()}
                      className={cn("flex items-center gap-3 p-3 rounded-lg border transition-all", theme === "light" ? "border-violet-500 bg-violet-500/5" : "border-border hover:bg-muted/50")}>
                      <Sun className="w-5 h-5 text-amber-500" />
                      <span className="text-sm font-medium">Light</span>
                    </button>
                    <button onClick={() => theme !== "dark" && toggleTheme()}
                      className={cn("flex items-center gap-3 p-3 rounded-lg border transition-all", theme === "dark" ? "border-violet-500 bg-violet-500/5" : "border-border hover:bg-muted/50")}>
                      <Moon className="w-5 h-5 text-indigo-400" />
                      <span className="text-sm font-medium">Dark</span>
                    </button>
                  </div>
                </div>
                <Row label="Accent color" value="">
                  <div className="flex gap-2">
                    {["violet", "blue", "emerald", "rose"].map(c => (
                      <button key={c} className={cn("w-6 h-6 rounded-full ring-2 ring-offset-2 ring-offset-background transition-all", c === "violet" ? "ring-foreground" : "ring-transparent")} style={{ background: c === "violet" ? "#8b5cf6" : c === "blue" ? "#3b82f6" : c === "emerald" ? "#10b981" : "#f43f5e" }} />
                    ))}
                  </div>
                </Row>
              </Card>
            )}

            {active === "backup" && (
              <Card title="Backup" desc="Configure automatic backups of your index and metadata.">
                <Toggle label="Automatic backups" desc="Weekly backup of index database" checked={true} onChange={() => {}} />
                <Row label="Last backup" value="Jul 20, 2025 — 3:42 AM" />
                <Row label="Backup size" value="284 MB" />
                <Row label="Backup location" value="/Users/jdoe/Backups/FileManagerAI">
                  <button className="flex items-center gap-1.5 px-3 h-8 rounded-lg border border-border hover:bg-muted text-xs font-medium transition-colors">
                    Change
                  </button>
                </Row>
                <button className="mt-2 w-full flex items-center justify-center gap-2 h-10 rounded-lg bg-primary text-primary-foreground text-sm font-medium hover:bg-primary/90 transition-colors">
                  <HardDrive className="w-4 h-4" /> Backup now
                </button>
              </Card>
            )}

            {active === "security" && (
              <Card title="Security" desc="Protect your file index and data.">
                <Toggle icon={Lock} label="Encrypt index at rest" desc="AES-256 encryption for stored metadata" checked={encrypt} onChange={setEncrypt} />
                <Toggle icon={ShieldCheck} label="Two-factor authentication" desc="Require 2FA for account access" checked={twofa} onChange={setTwofa} />
                <Row label="Last login" value="Jul 26, 2025 — 12:30 PM UTC" />
                <Row label="Active sessions" value="2 devices" />
                <button className="mt-2 w-full flex items-center justify-center gap-2 h-10 rounded-lg border border-rose-500/20 text-rose-600 dark:text-rose-400 text-sm font-medium hover:bg-rose-500/5 transition-colors">
                  Sign out all devices
                </button>
              </Card>
            )}
          </motion.div>
        </div>
      </div>
    </div>
  );
}

function Card({ title, desc, children }) {
  return (
    <div className="bg-card border border-border rounded-xl p-5">
      <h3 className="text-base font-semibold">{title}</h3>
      <p className="text-sm text-muted-foreground mt-0.5 mb-4">{desc}</p>
      {children}
    </div>
  );
}

function Row({ label, value, children }) {
  return (
    <div className="flex items-center justify-between py-3 border-t border-border/50 first:border-0">
      <div>
        <div className="text-sm font-medium">{label}</div>
        {value && <div className="text-xs text-muted-foreground">{value}</div>}
      </div>
      {children}
    </div>
  );
}

function Toggle({ icon: Icon, label, desc, checked, onChange }) {
  return (
    <div className="flex items-center justify-between py-3 border-t border-border/50 first:border-0">
      <div className="flex items-center gap-2">
        {Icon && <Icon className="w-4 h-4 text-muted-foreground" />}
        <div>
          <div className="text-sm font-medium">{label}</div>
          <div className="text-xs text-muted-foreground">{desc}</div>
        </div>
      </div>
      <button
        onClick={() => onChange(!checked)}
        className={cn("relative w-11 h-6 rounded-full transition-colors shrink-0", checked ? "bg-violet-600" : "bg-muted-foreground/30")}
      >
        <motion.span layout className={cn("absolute top-0.5 w-5 h-5 rounded-full bg-white shadow-sm", checked ? "left-[22px]" : "left-0.5")} transition={{ type: "spring", stiffness: 500, damping: 30 }} />
      </button>
    </div>
  );
}