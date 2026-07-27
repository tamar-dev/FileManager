// Mock data for FileManager AI — purely for prototype demonstration

export const dashboardStats = {
  indexedFiles: 184293,
  indexedFolders: 12453,
  duplicateFiles: 3421,
  storageSaved: 47.8, // GB
  photos: 52840,
  videos: 1842,
  documents: 9123,
  indexStatus: "Indexing",
  indexProgress: 78,
};

export const fileTypeDistribution = [
  { name: "Images", value: 52840, color: "var(--chart-1)" },
  { name: "Videos", value: 1842, color: "var(--chart-2)" },
  { name: "Documents", value: 9123, color: "var(--chart-3)" },
  { name: "Audio", value: 3421, color: "var(--chart-4)" },
  { name: "Archives", value: 1280, color: "var(--chart-5)" },
  { name: "Other", value: 6787, color: "var(--muted-foreground)" },
];

export const storageUsage = [
  { label: "Photos", used: 142.6, total: 250 },
  { label: "Videos", used: 88.4, total: 250 },
  { label: "Documents", used: 12.3, total: 250 },
  { label: "Audio", used: 6.8, total: 250 },
  { label: "Archives", used: 24.1, total: 250 },
  { label: "Other", used: 9.7, total: 250 },
];

export const indexProgressData = [
  { day: "Mon", files: 4200 },
  { day: "Tue", files: 5800 },
  { day: "Wed", files: 3200 },
  { day: "Thu", files: 7100 },
  { day: "Fri", files: 6400 },
  { day: "Sat", files: 4900 },
  { day: "Sun", files: 8200 },
];

export const recentFiles = [
  { id: "f1", name: "Q3_Financial_Report.pdf", type: "pdf", size: "2.4 MB", modified: "2 min ago", path: "/Documents/Finance", starred: true },
  { id: "f2", name: "team_offsite_photos.zip", type: "archive", size: "412 MB", modified: "1 hour ago", path: "/Photos/Events", starred: false },
  { id: "f3", name: "product_demo_v3.mp4", type: "video", size: "1.2 GB", modified: "3 hours ago", path: "/Videos/Marketing", starred: true },
  { id: "f4", name: "logo_concepts.png", type: "image", size: "8.4 MB", modified: "5 hours ago", path: "/Design/Brand", starred: false },
  { id: "f5", name: "meeting_notes_july.docx", type: "document", size: "124 KB", modified: "Yesterday", path: "/Documents/Meetings", starred: false },
  { id: "f6", name: "invoice_2847.pdf", type: "pdf", size: "320 KB", modified: "Yesterday", path: "/Documents/Finance", starred: false },
  { id: "f7", name: "podcast_episode_12.mp3", type: "audio", size: "64 MB", modified: "2 days ago", path: "/Audio/Podcasts", starred: true },
  { id: "f8", name: "design_system.fig", type: "other", size: "48 MB", modified: "2 days ago", path: "/Design", starred: false },
  { id: "f9", name: "customer_survey_results.xlsx", type: "spreadsheet", size: "1.8 MB", modified: "3 days ago", path: "/Documents/Research", starred: false },
  { id: "f10", name: "sunset_balcony.jpg", type: "image", size: "6.2 MB", modified: "4 days ago", path: "/Photos/2025/July", starred: true },
  { id: "f11", name: "backup_july.tar.gz", type: "archive", size: "8.4 GB", modified: "5 days ago", path: "/Backups", starred: false },
  { id: "f12", name: "contract_final.docx", type: "document", size: "210 KB", modified: "1 week ago", path: "/Documents/Legal", starred: false },
];

export const searchResults = recentFiles;

export const duplicateGroups = [
  {
    id: "dg1",
    hash: "a3f9b2e1c4d8...",
    size: "8.4 MB",
    count: 3,
    type: "image",
    files: [
      { id: "d1a", name: "sunset_balcony.jpg", path: "/Photos/2025/July", modified: "Jul 12, 2025", selected: false },
      { id: "d1b", name: "IMG_4821.jpg", path: "/Photos/Imports/Camera", modified: "Jul 10, 2025", selected: true },
      { id: "d1c", name: "vacation_sunset.jpg", path: "/Backups/2025", modified: "Jul 8, 2025", selected: false },
    ],
  },
  {
    id: "dg2",
    hash: "7b2e9f1a3c5d...",
    size: "2.4 MB",
    count: 2,
    type: "pdf",
    files: [
      { id: "d2a", name: "Q3_Financial_Report.pdf", path: "/Documents/Finance", modified: "Jul 15, 2025", selected: true },
      { id: "d2b", name: "report_copy.pdf", path: "/Desktop", modified: "Jul 14, 2025", selected: false },
    ],
  },
  {
    id: "dg3",
    hash: "c4d8a3f9b2e1...",
    size: "1.2 GB",
    count: 2,
    type: "video",
    files: [
      { id: "d3a", name: "product_demo_v3.mp4", path: "/Videos/Marketing", modified: "Jul 16, 2025", selected: false },
      { id: "d3b", name: "demo_final_v3.mp4", path: "/Videos/Archive", modified: "Jul 15, 2025", selected: true },
    ],
  },
  {
    id: "dg4",
    hash: "e9f1a7b2c4d8...",
    size: "124 KB",
    count: 4,
    type: "document",
    files: [
      { id: "d4a", name: "meeting_notes_july.docx", path: "/Documents/Meetings", modified: "Jul 11, 2025", selected: false },
      { id: "d4b", name: "notes_july.docx", path: "/Desktop", modified: "Jul 11, 2025", selected: false },
      { id: "d4c", name: "july_notes_copy.docx", path: "/Documents/Backup", modified: "Jul 10, 2025", selected: true },
      { id: "d4d", name: "meeting_july.docx", path: "/OneDrive/Docs", modified: "Jul 9, 2025", selected: false },
    ],
  },
];

export const photos = [
  { id: "p1", name: "sunset_balcony.jpg", url: "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=400", date: "2025-07-12", size: "6.2 MB", dim: "4032×3024", album: "Summer 2025", favorite: true },
  { id: "p2", name: "mountain_view.jpg", url: "https://images.unsplash.com/photo-1464822759023-fed622ff2c3b?w=400", date: "2025-07-10", size: "5.1 MB", dim: "4928×3280", album: "Travel", favorite: false },
  { id: "p3", name: "city_lights.jpg", url: "https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=400", date: "2025-07-08", size: "4.8 MB", dim: "6000×4000", album: "Urban", favorite: true },
  { id: "p4", name: "forest_path.jpg", url: "https://images.unsplash.com/photo-1448375240586-882707db888b?w=400", date: "2025-07-05", size: "7.3 MB", dim: "5184×3456", album: "Nature", favorite: false },
  { id: "p5", name: "ocean_waves.jpg", url: "https://images.unsplash.com/photo-1505228395891-9a51e7e86bf6?w=400", date: "2025-07-03", size: "5.9 MB", dim: "4032×3024", album: "Summer 2025", favorite: false },
  { id: "p6", name: "desert_dunes.jpg", url: "https://images.unsplash.com/photo-1473580044384-7ba9967e16a0?w=400", date: "2025-06-28", size: "6.7 MB", dim: "6000×4000", album: "Travel", favorite: true },
  { id: "p7", name: "snowy_peaks.jpg", url: "https://images.unsplash.com/photo-1551524559-8af4e6624178?w=400", date: "2025-06-20", size: "8.1 MB", dim: "7360×4912", album: "Mountains", favorite: false },
  { id: "p8", name: "autumn_leaves.jpg", url: "https://images.unsplash.com/photo-1507371341162-763b5e419408?w=400", date: "2025-06-15", size: "4.2 MB", dim: "4928×3280", album: "Nature", favorite: false },
  { id: "p9", name: "lake_reflection.jpg", url: "https://images.unsplash.com/photo-1501785888041-af3ef285b470?w=400", date: "2025-06-10", size: "6.5 MB", dim: "5184×3456", album: "Travel", favorite: true },
  { id: "p10", name: "minimal_desk.jpg", url: "https://images.unsplash.com/photo-1499951360447-b19be8fe80f5?w=400", date: "2025-05-28", size: "3.8 MB", dim: "4032×3024", album: "Workspace", favorite: false },
  { id: "p11", name: "coffee_morning.jpg", url: "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=400", date: "2025-05-20", size: "4.4 MB", dim: "4928×3280", album: "Daily", favorite: false },
  { id: "p12", name: "architecture.jpg", url: "https://images.unsplash.com/photo-1486325212027-8081e485255e?w=400", date: "2025-05-15", size: "5.6 MB", dim: "6000×4000", album: "Urban", favorite: true },
];

export const photoAlbums = [
  { id: "a1", name: "Summer 2025", count: 142, cover: "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=300" },
  { id: "a2", name: "Travel", count: 1284, cover: "https://images.unsplash.com/photo-1464822759023-fed622ff2c3b?w=300" },
  { id: "a3", name: "Urban", count: 386, cover: "https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=300" },
  { id: "a4", name: "Nature", count: 921, cover: "https://images.unsplash.com/photo-1448375240586-882707db888b?w=300" },
  { id: "a5", name: "Mountains", count: 248, cover: "https://images.unsplash.com/photo-1551524559-8af4e6624178?w=300" },
  { id: "a6", name: "Workspace", count: 57, cover: "https://images.unsplash.com/photo-1499951360447-b19be8fe80f5?w=300" },
];

export const virtualFolders = [
  { id: "vf1", name: "Project Atlas", icon: "folder", rule: "Tagged: atlas", tags: ["atlas", "design"], count: 248, smart: true, color: "blue" },
  { id: "vf2", name: "Tax Documents 2025", icon: "folder", rule: "Type: PDF + Date: 2025", tags: ["finance", "tax"], count: 64, smart: true, color: "emerald" },
  { id: "vf3", name: "Vacation Media", icon: "image", rule: "Tag: vacation + Type: image/video", tags: ["vacation", "media"], count: 1842, smart: true, color: "amber" },
  { id: "vf4", name: "Large Files", icon: "file", rule: "Size: > 500MB", tags: ["large"], count: 37, smart: true, color: "rose" },
  { id: "vf5", name: "Design Assets", icon: "palette", rule: "Path contains: /Design", tags: ["design", "assets"], count: 1284, smart: false, color: "violet" },
  { id: "vf6", name: "Recently Edited", icon: "clock", rule: "Modified: last 7 days", tags: ["recent"], count: 421, smart: true, color: "cyan" },
];

export const indexStatusData = {
  currentScan: "/Users/jdoe/Documents/Finance/2025",
  filesScanned: 184293,
  totalFiles: 236000,
  queue: 51707,
  speed: 2840, // files/min
  eta: "18 min",
  progress: 78,
  recentChanges: [
    { id: "c1", action: "Added", file: "report_q3.pdf", path: "/Documents/Finance", time: "2 sec ago" },
    { id: "c2", action: "Modified", file: "logo.png", path: "/Design/Brand", time: "14 sec ago" },
    { id: "c3", action: "Added", file: "meeting_notes.docx", path: "/Documents/Meetings", time: "32 sec ago" },
    { id: "c4", action: "Deleted", file: "old_backup.zip", path: "/Backups", time: "1 min ago" },
    { id: "c5", action: "Moved", file: "demo.mp4", path: "/Videos → /Videos/Archive", time: "2 min ago" },
    { id: "c6", action: "Added", file: "invoice.pdf", path: "/Documents/Finance", time: "3 min ago" },
  ],
};

export const indexedLocations = [
  { id: "loc1", path: "/Users/jdoe/Documents", files: 9123, status: "Indexed", size: "12.3 GB" },
  { id: "loc2", path: "/Users/jdoe/Photos", files: 52840, status: "Indexed", size: "142.6 GB" },
  { id: "loc3", path: "/Users/jdoe/Videos", files: 1842, status: "Indexed", size: "88.4 GB" },
  { id: "loc4", path: "/Users/jdoe/Desktop", files: 247, status: "Indexing", size: "4.1 GB" },
  { id: "loc5", path: "/Users/jdoe/Downloads", files: 1284, status: "Queued", size: "8.8 GB" },
];

export const notifications = [
  { id: "n1", title: "Scan complete", body: "Indexed 2,840 new files in /Photos", time: "5 min ago", type: "success", read: false },
  { id: "n2", title: "Duplicates found", body: "3 duplicate groups detected (34 MB)", time: "12 min ago", type: "info", read: false },
  { id: "n3", title: "Storage warning", body: "Drive 'Documents' is 87% full", time: "1 hour ago", type: "warning", read: true },
  { id: "n4", title: "Backup ready", body: "Weekly backup available for download", time: "3 hours ago", type: "info", read: true },
];

export const fileTags = ["work", "personal", "finance", "design", "archive", "urgent", "vacation", "atlas", "draft", "final"];