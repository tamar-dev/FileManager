import React, { useState, useMemo } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { Image as ImageIcon, Star, Calendar, Maximize2, X, Download, Heart, Folder } from "lucide-react";
import PageHeader from "@/components/PageHeader";
import { photos, photoAlbums } from "@/lib/mockData";
import { cn } from "@/lib/utils";

export default function Photos() {
  const [selected, setSelected] = useState(null);
  const [view, setView] = useState("all"); // all | favorites | albums
  const [timeline, setTimeline] = useState("month"); // year | month

  const grouped = useMemo(() => {
    let list = photos;
    if (view === "favorites") list = photos.filter(p => p.favorite);
    const groups = {};
    list.forEach(p => {
      const d = new Date(p.date);
      const key = timeline === "year" ? d.getFullYear().toString() : d.toLocaleDateString("en", { year: "numeric", month: "long" });
      if (!groups[key]) groups[key] = [];
      groups[key].push(p);
    });
    return Object.entries(groups).sort((a, b) => b[0].localeCompare(a[0]));
  }, [view, timeline]);

  return (
    <div className="p-6 lg:p-8 max-w-[1400px] mx-auto animate-fade-in">
      <PageHeader
        title="Photos"
        subtitle="Your entire photo library, beautifully organized."
        actions={
          <div className="flex items-center gap-1 p-1 bg-muted rounded-lg">
            {["all", "favorites", "albums"].map(v => (
              <button key={v} onClick={() => setView(v)}
                className={cn("px-3 h-7 rounded-md text-xs font-medium capitalize transition-all", view === v ? "bg-background shadow-sm text-foreground" : "text-muted-foreground hover:text-foreground")}>
                {v}
              </button>
            ))}
          </div>
        }
      />

      {view === "albums" ? (
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
          {photoAlbums.map((album, i) => (
            <motion.div key={album.id} initial={{ opacity: 0, scale: 0.95 }} animate={{ opacity: 1, scale: 1 }} transition={{ delay: i * 0.05 }}
              className="group cursor-pointer rounded-xl overflow-hidden bg-card border border-border hover:shadow-lg hover:shadow-black/5 transition-all">
              <div className="aspect-square overflow-hidden">
                <img src={album.cover} alt={album.name} className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300" />
              </div>
              <div className="p-3">
                <div className="text-sm font-medium truncate flex items-center gap-1.5"><Folder className="w-3.5 h-3.5 text-muted-foreground" /> {album.name}</div>
                <div className="text-xs text-muted-foreground">{album.count} photos</div>
              </div>
            </motion.div>
          ))}
        </div>
      ) : (
        <>
          {/* Timeline toggle */}
          <div className="flex items-center gap-1 p-1 bg-muted rounded-lg mb-6 w-fit">
            {["month", "year"].map(t => (
              <button key={t} onClick={() => setTimeline(t)}
                className={cn("px-3 h-7 rounded-md text-xs font-medium capitalize transition-all", timeline === t ? "bg-background shadow-sm text-foreground" : "text-muted-foreground hover:text-foreground")}>
                By {t}
              </button>
            ))}
          </div>

          {grouped.map(([period, items]) => (
            <div key={period} className="mb-8">
              <div className="flex items-center gap-2 mb-3">
                <Calendar className="w-4 h-4 text-muted-foreground" />
                <h3 className="text-sm font-semibold">{period}</h3>
                <span className="text-xs text-muted-foreground">{items.length} photos</span>
                <div className="flex-1 h-px bg-border ml-2" />
              </div>
              <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-3">
                {items.map((photo, i) => (
                  <motion.div
                    key={photo.id}
                    initial={{ opacity: 0, scale: 0.95 }} animate={{ opacity: 1, scale: 1 }} transition={{ delay: i * 0.03 }}
                    onClick={() => setSelected(photo)}
                    className="group relative aspect-square rounded-lg overflow-hidden bg-muted cursor-pointer"
                  >
                    <img src={photo.url} alt={photo.name} className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-300" />
                    <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />
                    {photo.favorite && (
                      <div className="absolute top-2 right-2 w-7 h-7 rounded-full bg-black/40 backdrop-blur-sm flex items-center justify-center">
                        <Heart className="w-3.5 h-3.5 fill-rose-400 text-rose-400" />
                      </div>
                    )}
                    <div className="absolute bottom-0 left-0 right-0 p-2 opacity-0 group-hover:opacity-100 transition-opacity">
                      <div className="text-xs text-white font-medium truncate">{photo.name}</div>
                    </div>
                  </motion.div>
                ))}
              </div>
            </div>
          ))}
        </>
      )}

      {/* Photo detail modal */}
      <AnimatePresence>
        {selected && (
          <motion.div
            initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
            onClick={() => setSelected(null)}
            className="fixed inset-0 z-50 bg-black/80 backdrop-blur-sm flex items-center justify-center p-4 lg:p-8"
          >
            <motion.div
              initial={{ scale: 0.95, opacity: 0 }} animate={{ scale: 1, opacity: 1 }} exit={{ scale: 0.95, opacity: 0 }}
              onClick={e => e.stopPropagation()}
              className="bg-card border border-border rounded-2xl overflow-hidden max-w-5xl w-full max-h-[90vh] flex flex-col lg:flex-row"
            >
              <div className="flex-1 bg-black flex items-center justify-center min-h-[300px]">
                <img src={selected.url} alt={selected.name} className="max-h-[90vh] w-full object-contain" />
              </div>
              <div className="w-full lg:w-80 p-5 border-t lg:border-t-0 lg:border-l border-border shrink-0">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-sm font-semibold truncate">{selected.name}</h3>
                  <button onClick={() => setSelected(null)} className="w-7 h-7 rounded-md hover:bg-muted flex items-center justify-center text-muted-foreground">
                    <X className="w-4 h-4" />
                  </button>
                </div>
                <div className="space-y-3">
                  <Meta label="Date" value={new Date(selected.date).toLocaleDateString("en", { year: "numeric", month: "long", day: "numeric" })} />
                  <Meta label="Size" value={selected.size} />
                  <Meta label="Dimensions" value={selected.dim} />
                  <Meta label="Album" value={selected.album} />
                </div>
                <div className="flex items-center gap-2 mt-5">
                  <button className="flex-1 flex items-center justify-center gap-1.5 h-9 rounded-lg bg-primary text-primary-foreground text-sm font-medium hover:bg-primary/90 transition-colors">
                    <Download className="w-4 h-4" /> Download
                  </button>
                  <button className={cn("w-9 h-9 rounded-lg flex items-center justify-center border transition-colors",
                    selected.favorite ? "bg-rose-500/10 border-rose-500/20 text-rose-500" : "border-border hover:bg-muted")}>
                    <Star className={cn("w-4 h-4", selected.favorite && "fill-current")} />
                  </button>
                </div>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}

function Meta({ label, value }) {
  return (
    <div className="flex items-center justify-between">
      <span className="text-xs text-muted-foreground">{label}</span>
      <span className="text-xs font-medium">{value}</span>
    </div>
  );
}