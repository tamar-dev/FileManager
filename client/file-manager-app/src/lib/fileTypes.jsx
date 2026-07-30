import { FileText, Image as ImageIcon, FileVideo, FileAudio, Archive, FileSpreadsheet, File as FileIcon } from "lucide-react";

export function getFileTypeMeta(type) {
  const map = {
    pdf: { icon: FileText, color: "text-rose-500", bg: "bg-rose-500/10", label: "PDF" },
    document: { icon: FileText, color: "text-blue-500", bg: "bg-blue-500/10", label: "Document" },
    image: { icon: ImageIcon, color: "text-violet-500", bg: "bg-violet-500/10", label: "Image" },
    video: { icon: FileVideo, color: "text-amber-500", bg: "bg-amber-500/10", label: "Video" },
    audio: { icon: FileAudio, color: "text-emerald-500", bg: "bg-emerald-500/10", label: "Audio" },
    archive: { icon: Archive, color: "text-orange-500", bg: "bg-orange-500/10", label: "Archive" },
    spreadsheet: { icon: FileSpreadsheet, color: "text-green-500", bg: "bg-green-500/10", label: "Spreadsheet" },
    other: { icon: FileIcon, color: "text-slate-500", bg: "bg-slate-500/10", label: "Other" },
  };
  return map[type] || map.other;
}