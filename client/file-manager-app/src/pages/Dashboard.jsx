import React, { useEffect, useState } from "react";
import { getDashboard } from "@/lib/api";
import { Link } from "react-router-dom";
import { motion } from "framer-motion";
import {
  Files, FolderTree, Copy, HardDrive, Image as ImageIcon,
  FileVideo, FileText, Activity, ArrowRight, Zap
} from "lucide-react";
import {
  PieChart, Pie, Cell, ResponsiveContainer, BarChart, Bar,
  XAxis, YAxis, AreaChart, Area, Tooltip
} from "recharts";
import PageHeader from "@/components/PageHeader";
import StatCard from "@/components/StatCard";
import { dashboardStats, fileTypeDistribution, storageUsage, indexProgressData, recentFiles } from "@/lib/mockData";
import { getFileTypeMeta } from "@/lib/fileTypes";

export default function Dashboard() {

const [dashboard, setDashboard] = useState(null);
const [dashboardLoading, setDashboardLoading] = useState(true);
const [dashboardError, setDashboardError] = useState(null);

useEffect(() => {
  let cancelled = false;

  async function loadDashboard() {

    try {
      setDashboardLoading(true);

      const data = await getDashboard();

      if (!cancelled) {
        setDashboard(data);
        setDashboardError(null);
      }
    } catch (error) {
      if (!cancelled) {
        console.error("Failed to load dashboard", error);
        setDashboardError(error);
      }
    } finally {
      if (!cancelled) {
        setDashboardLoading(false);
      }
    }
  }

  loadDashboard();

  return () => {
    cancelled = true;
  };
}, []);

const formatBytes = (bytes) => {
  if (!bytes) return "0 B";

  const units = ["B", "KB", "MB", "GB", "TB"];
  const index = Math.floor(Math.log(bytes) / Math.log(1024));
  const value = bytes / Math.pow(1024, index);

  return `${value.toFixed(index === 0 ? 0 : 1)} ${units[index]}`;
};

  const totalFiles = fileTypeDistribution.reduce((a, b) => a + b.value, 0);

  return (
    <div className="p-6 lg:p-8 max-w-[1400px] mx-auto animate-fade-in">
      <PageHeader
        title="Dashboard"
        subtitle="Welcome back, Jordan — here's your file overview."
        actions={
          <div className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-emerald-500/10 border border-emerald-500/20">
            <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
            <span className="text-sm font-medium text-emerald-600 dark:text-emerald-400">All systems running</span>
          </div>
        }
      />

      {/* Stat cards */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        {/* <StatCard icon={Files} label="Indexed Files" value={dashboardStats.indexedFiles.toLocaleString()} change="+2.8%" trend="up" color="violet" delay={0} />
        <StatCard icon={FolderTree} label="Indexed Folders" value={dashboardStats.indexedFolders.toLocaleString()} change="+1.2%" trend="up" color="blue" delay={0.05} />
        <StatCard icon={Copy} label="Duplicate Files" value={dashboardStats.duplicateFiles.toLocaleString()} change="-340" trend="down" color="amber" delay={0.1} />
        <StatCard icon={HardDrive} label="Storage Saved" value={`${dashboardStats.storageSaved} GB`} change="+5.4%" trend="up" color="emerald" delay={0.15} /> */}
       <StatCard
  icon={Files}
  label="Indexed Files"
  value={
    dashboardLoading
      ? "..."
      : (dashboard?.indexedFileCount ?? 0).toLocaleString()
  }
  color="violet"
  delay={0}
/>

<StatCard
  icon={FolderTree}
  label="Duplicate Groups"
  value={
    dashboardLoading
      ? "..."
      : (dashboard?.duplicateGroupCount ?? 0).toLocaleString()
  }
  color="blue"
  delay={0.05}
/>

<StatCard
  icon={Copy}
  label="Duplicate Files"
  value={
    dashboardLoading
      ? "..."
      : (dashboard?.duplicateFileCount ?? 0).toLocaleString()
  }
  color="amber"
  delay={0.1}
/>

<StatCard
  icon={HardDrive}
  label="Potential Savings"
  value={
    dashboardLoading
      ? "..."
      : formatBytes(dashboard?.potentialStorageSavings ?? 0)
  }
  color="emerald"
  delay={0.15}
/>

        <StatCard icon={ImageIcon} label="Photos" value={dashboardStats.photos.toLocaleString()} change="+842" trend="up" color="rose" delay={0.2} />
        <StatCard icon={FileVideo} label="Videos" value={dashboardStats.videos.toLocaleString()} change="+12" trend="up" color="amber" delay={0.25} />
        <StatCard icon={FileText} label="Documents" value={dashboardStats.documents.toLocaleString()} change="+38" trend="up" color="blue" delay={0.3} />
        <StatCard icon={Activity} label="Index Status" value={dashboardStats.indexStatus} change={`${dashboardStats.indexProgress}%`} trend="up" color="cyan" delay={0.35} />
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-6">
        {/* File types donut */}
        <motion.div
          initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.3 }}
          className="bg-card border border-border rounded-xl p-5"
        >
          <div className="flex items-center justify-between mb-4">
            <div>
              <h3 className="text-sm font-semibold">File Types</h3>
              <p className="text-xs text-muted-foreground">Distribution by category</p>
            </div>
          </div>
          <div className="relative h-[200px]">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie data={fileTypeDistribution} dataKey="value" nameKey="name" cx="50%" cy="50%" innerRadius={55} outerRadius={85} paddingAngle={2} stroke="none">
                  {fileTypeDistribution.map((entry, i) => (
                    <Cell key={i} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip
                  contentStyle={{ background: "hsl(var(--popover))", border: "1px solid hsl(var(--border))", borderRadius: 8, fontSize: 12 }}
                  formatter={(v) => v.toLocaleString()}
                />
              </PieChart>
            </ResponsiveContainer>
            <div className="absolute inset-0 flex flex-col items-center justify-center pointer-events-none">
              <span className="text-2xl font-semibold tabular-nums">{totalFiles.toLocaleString()}</span>
              <span className="text-xs text-muted-foreground">Total files</span>
            </div>
          </div>
          <div className="grid grid-cols-2 gap-2 mt-4">
            {fileTypeDistribution.map(t => (
              <div key={t.name} className="flex items-center gap-1.5">
                <span className="w-2.5 h-2.5 rounded-sm" style={{ background: t.color }} />
                <span className="text-xs text-muted-foreground truncate">{t.name}</span>
                <span className="text-xs font-medium ml-auto tabular-nums">{Math.round((t.value / totalFiles) * 100)}%</span>
              </div>
            ))}
          </div>
        </motion.div>

        {/* Storage usage bar */}
        <motion.div
          initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.35 }}
          className="bg-card border border-border rounded-xl p-5"
        >
          <div className="flex items-center justify-between mb-4">
            <div>
              <h3 className="text-sm font-semibold">Storage Usage</h3>
              <p className="text-xs text-muted-foreground">By file category (GB)</p>
            </div>
          </div>
          <div className="h-[260px]">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={storageUsage} layout="vertical" margin={{ left: 0, right: 16 }}>
                <XAxis type="number" hide />
                <YAxis type="category" dataKey="label" width={60} tick={{ fontSize: 11, fill: "hsl(var(--muted-foreground))" }} axisLine={false} tickLine={false} />
                <Tooltip
                  cursor={{ fill: "hsl(var(--muted))", opacity: 0.3 }}
                  contentStyle={{ background: "hsl(var(--popover))", border: "1px solid hsl(var(--border))", borderRadius: 8, fontSize: 12 }}
                  formatter={(v) => `${v} GB`}
                />
                <Bar dataKey="used" radius={[0, 6, 6, 0]} fill="hsl(var(--chart-1))" barSize={14} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </motion.div>

        {/* Index progress area chart */}
        <motion.div
          initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.4 }}
          className="bg-card border border-border rounded-xl p-5"
        >
          <div className="flex items-center justify-between mb-4">
            <div>
              <h3 className="text-sm font-semibold">Index Progress</h3>
              <p className="text-xs text-muted-foreground">Files scanned per day</p>
            </div>
            <div className="flex items-center gap-1 text-xs font-medium text-violet-600 dark:text-violet-400">
              <Zap className="w-3.5 h-3.5" /> 2,840/min
            </div>
          </div>
          <div className="h-[260px]">
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={indexProgressData} margin={{ left: -20, right: 8, top: 8 }}>
                <defs>
                  <linearGradient id="grad" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor="hsl(var(--chart-1))" stopOpacity={0.3} />
                    <stop offset="100%" stopColor="hsl(var(--chart-1))" stopOpacity={0} />
                  </linearGradient>
                </defs>
                <XAxis dataKey="day" tick={{ fontSize: 11, fill: "hsl(var(--muted-foreground))" }} axisLine={false} tickLine={false} />
                <YAxis tick={{ fontSize: 11, fill: "hsl(var(--muted-foreground))" }} axisLine={false} tickLine={false} />
                <Tooltip
                  contentStyle={{ background: "hsl(var(--popover))", border: "1px solid hsl(var(--border))", borderRadius: 8, fontSize: 12 }}
                  formatter={(v) => [`${v.toLocaleString()} files`, "Scanned"]}
                />
                <Area type="monotone" dataKey="files" stroke="hsl(var(--chart-1))" strokeWidth={2.5} fill="url(#grad)" />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </motion.div>
      </div>

      {/* Recent activity */}
      <motion.div
        initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.45 }}
        className="bg-card border border-border rounded-xl overflow-hidden"
      >
        <div className="flex items-center justify-between px-5 py-4 border-b border-border">
          <div>
            <h3 className="text-sm font-semibold">Recent Activity</h3>
            <p className="text-xs text-muted-foreground">Latest indexed files</p>
          </div>
          <Link to="/recent" className="flex items-center gap-1 text-xs font-medium text-violet-600 dark:text-violet-400 hover:gap-1.5 transition-all">
            View all <ArrowRight className="w-3.5 h-3.5" />
          </Link>
        </div>
        <div className="divide-y divide-border">
          {recentFiles.slice(0, 6).map(file => {
            const meta = getFileTypeMeta(file.type);
            const Icon = meta.icon;
            return (
              <div key={file.id} className="flex items-center gap-3 px-5 py-3 hover:bg-muted/40 transition-colors">
                <div className={`w-9 h-9 rounded-lg ${meta.bg} flex items-center justify-center shrink-0`}>
                  <Icon className={`w-[18px] h-[18px] ${meta.color}`} />
                </div>
                <div className="min-w-0 flex-1">
                  <div className="text-sm font-medium truncate">{file.name}</div>
                  <div className="text-xs text-muted-foreground truncate">{file.path}</div>
                </div>
                <div className="text-xs text-muted-foreground tabular-nums hidden sm:block">{file.size}</div>
                <div className="text-xs text-muted-foreground hidden md:block w-24 text-right">{file.modified}</div>
              </div>
            );
          })}
        </div>
      </motion.div>
    </div>
  );
}