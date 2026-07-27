import React, { useState, useMemo, useEffect } from "react";
import { motion, AnimatePresence } from "framer-motion";
import {
  Search as SearchIcon, Filter, Calendar, HardDrive, Tag,
  ChevronDown, Star, Download, X
} from "lucide-react";
import PageHeader from "@/components/PageHeader";
import { fileTags } from "@/lib/mockData";
import { searchFiles } from "@/lib/api";
import { getFileTypeMeta } from "@/lib/fileTypes";
import { cn } from "@/lib/utils";

const typeFilters = ["all", "pdf", "document", "image", "video", "audio", "archive", "spreadsheet"];
const dateFilters = [
  { label: "Any time", value: "all" },
  { label: "Today", value: "today" },
  { label: "This week", value: "week" },
  { label: "This month", value: "month" },
];
const sizeFilters = [
  { label: "Any size", value: "all" },
  { label: "< 1 MB", value: "xs" },
  { label: "1–100 MB", value: "s" },
  { label: "100 MB–1 GB", value: "m" },
  { label: "> 1 GB", value: "l" },
];

export default function Search() {
  const [query, setQuery] = useState("");
  const [debouncedQuery, setDebouncedQuery] = useState("");
  const [activeType, setActiveType] = useState("all");
  const [dateFilter, setDateFilter] = useState("all");
  const [sizeFilter, setSizeFilter] = useState("all");
  const [activeTags, setActiveTags] = useState([]);
  const [showFilters, setShowFilters] = useState(true);

  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const timeout = setTimeout(() => setDebouncedQuery(query), 275);
    return () => clearTimeout(timeout);
  }, [query]);

  useEffect(() => {
    let cancelled = false;

    async function loadResults() {
      try {
        setLoading(true);
        setError(null);

        const data = await searchFiles({ name: debouncedQuery || undefined });

        if (!cancelled) {
          setResults(data);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err.message || "Failed to search files.");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    loadResults();

    return () => { cancelled = true; };
  }, [debouncedQuery]);

  const filteredResults = useMemo(() => {
    return results.filter(f => {
      if (activeType !== "all" && f.type !== activeType) return false;
      return true;
    });
  }, [results, activeType]);

  const toggleTag = (tag) => setActiveTags(t => t.includes(tag) ? t.filter(x => x !== tag) : [...t, tag]);

  return (
    <div className="p-6 lg:p-8 max-w-[1400px] mx-auto animate-fade-in">
      <PageHeader title="Search" subtitle="Find any file instantly with advanced filters." />

      {/* Search bar */}
      <div className="relative mb-4">
        <SearchIcon className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-muted-foreground" />
        <input
          autoFocus
          value={query}
          onChange={e => setQuery(e.target.value)}
          placeholder="Search by name, content, or tag…"
          className="w-full h-12 pl-12 pr-12 rounded-xl bg-card border border-border text-sm focus:outline-none focus:ring-2 focus:ring-violet-500/40 focus:border-violet-500/50 transition-all"
        />
        {query && (
          <button onClick={() => setQuery("")} className="absolute right-4 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground">
            <X className="w-[18px] h-[18px]" />
          </button>
        )}
      </div>

      {/* Type pills */}
      <div className="flex items-center gap-2 mb-4 overflow-x-auto pb-1">
        <button
          onClick={() => setShowFilters(s => !s)}
          className={cn("flex items-center gap-1.5 px-3 h-8 rounded-lg text-xs font-medium border transition-all shrink-0",
            showFilters ? "bg-primary text-primary-foreground border-primary" : "bg-card border-border hover:bg-muted")}
        >
          <Filter className="w-3.5 h-3.5" /> Filters
        </button>
        {typeFilters.map(t => (
          <button
            key={t}
            onClick={() => setActiveType(t)}
            className={cn("px-3 h-8 rounded-lg text-xs font-medium border capitalize transition-all shrink-0",
              activeType === t ? "bg-violet-600 text-white border-violet-600" : "bg-card border-border hover:bg-muted")}
          >
            {t}
          </button>
        ))}
      </div>

      <AnimatePresence>
        {showFilters && (
          <motion.div
            initial={{ height: 0, opacity: 0 }} animate={{ height: "auto", opacity: 1 }} exit={{ height: 0, opacity: 0 }}
            className="overflow-hidden mb-4"
          >
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 p-4 bg-card border border-border rounded-xl">
              <FilterGroup icon={Calendar} label="Date modified">
                <select value={dateFilter} onChange={e => setDateFilter(e.target.value)} className="w-full h-9 px-3 rounded-lg bg-background border border-border text-sm focus:outline-none focus:ring-2 focus:ring-violet-500/30">
                  {dateFilters.map(d => <option key={d.value} value={d.value}>{d.label}</option>)}
                </select>
              </FilterGroup>
              <FilterGroup icon={HardDrive} label="File size">
                <select value={sizeFilter} onChange={e => setSizeFilter(e.target.value)} className="w-full h-9 px-3 rounded-lg bg-background border border-border text-sm focus:outline-none focus:ring-2 focus:ring-violet-500/30">
                  {sizeFilters.map(s => <option key={s.value} value={s.value}>{s.label}</option>)}
                </select>
              </FilterGroup>
              <FilterGroup icon={Tag} label="Tags">
                <div className="flex flex-wrap gap-1.5">
                  {fileTags.slice(0, 6).map(tag => (
                    <button
                      key={tag}
                      onClick={() => toggleTag(tag)}
                      className={cn("px-2 py-0.5 rounded-md text-xs border transition-all",
                        activeTags.includes(tag) ? "bg-violet-600 text-white border-violet-600" : "bg-background border-border hover:bg-muted")}
                    >
                      {tag}
                    </button>
                  ))}
                </div>
              </FilterGroup>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Results */}
      <div className="bg-card border border-border rounded-xl overflow-hidden">
        <div className="flex items-center justify-between px-5 py-3.5 border-b border-border">
          <span className="text-sm font-medium">{filteredResults.length} results</span>
          <span className="text-xs text-muted-foreground">{query ? `for "${query}"` : "All files"}</span>
        </div>
        {error && (
          <div className="px-5 py-4 text-sm text-rose-500">{error}</div>
        )}
        {loading && !error && (
          <div className="px-5 py-8 text-sm text-muted-foreground text-center">Searching…</div>
        )}
        {!loading && !error && (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border text-left">
                  <th className="px-5 py-2.5 text-xs font-medium text-muted-foreground">Name</th>
                  <th className="px-5 py-2.5 text-xs font-medium text-muted-foreground hidden md:table-cell">Path</th>
                  <th className="px-5 py-2.5 text-xs font-medium text-muted-foreground hidden sm:table-cell">Size</th>
                  <th className="px-5 py-2.5 text-xs font-medium text-muted-foreground hidden lg:table-cell">Modified</th>
                  <th className="px-5 py-2.5"></th>
                </tr>
              </thead>
              <tbody>
                {filteredResults.map((file, i) => {
                  const meta = getFileTypeMeta(file.type);
                  const Icon = meta.icon;
                  return (
                    <motion.tr
                      key={file.id}
                      initial={{ opacity: 0 }} animate={{ opacity: 1 }} transition={{ delay: i * 0.03 }}
                      className="border-b border-border/50 last:border-0 hover:bg-muted/40 transition-colors group"
                    >
                      <td className="px-5 py-3">
                        <div className="flex items-center gap-3">
                          <div className={cn("w-9 h-9 rounded-lg flex items-center justify-center shrink-0", meta.bg)}>
                            <Icon className={cn("w-[18px] h-[18px]", meta.color)} />
                          </div>
                          <div className="min-w-0">
                            <div className="text-sm font-medium truncate">{file.name}</div>
                            <div className="text-xs text-muted-foreground sm:hidden">{file.size} · {file.modified}</div>
                          </div>
                        </div>
                      </td>
                      <td className="px-5 py-3 text-sm text-muted-foreground hidden md:table-cell truncate max-w-[200px]">{file.path}</td>
                      <td className="px-5 py-3 text-sm text-muted-foreground tabular-nums hidden sm:table-cell">{file.size}</td>
                      <td className="px-5 py-3 text-sm text-muted-foreground hidden lg:table-cell">{file.modified}</td>
                      <td className="px-5 py-3">
                        <div className="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                          <button className="w-7 h-7 rounded-md hover:bg-muted flex items-center justify-center text-muted-foreground hover:text-foreground">
                            <Star className="w-4 h-4" />
                          </button>
                          <button className="w-7 h-7 rounded-md hover:bg-muted flex items-center justify-center text-muted-foreground hover:text-foreground">
                            <Download className="w-4 h-4" />
                          </button>
                        </div>
                      </td>
                    </motion.tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

function FilterGroup({ icon: Icon, label, children }) {
  return (
    <div>
      <div className="flex items-center gap-1.5 text-xs font-medium text-muted-foreground mb-2">
        <Icon className="w-3.5 h-3.5" /> {label}
      </div>
      {children}
    </div>
  );
}