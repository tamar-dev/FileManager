// Mock data for the Virtual Explorer feature.
// KEY CONCEPT: Folders here are VIRTUAL — they do not exist on disk.
// Files keep their original physical location; a file can live in many
// virtual folders at once. Moving a file inside the explorer never touches
// the real file system.

export const virtualTree = {
  id: "root",
  name: "Virtual Library",
  children: [
    {
      id: "vf-atlas",
      name: "Project Atlas",
      children: [
        { id: "vf-atlas-design", name: "Design Assets", children: [] },
        { id: "vf-atlas-research", name: "Research", children: [] },
        { id: "vf-atlas-presentations", name: "Presentations", children: [] },
      ],
    },
    {
      id: "vf-tax",
      name: "Tax Documents 2025",
      children: [
        { id: "vf-tax-invoices", name: "Invoices", children: [] },
        { id: "vf-tax-receipts", name: "Receipts", children: [] },
      ],
    },
    {
      id: "vf-vacation",
      name: "Vacation Media",
      children: [
        { id: "vf-vacation-summer", name: "Summer 2025", children: [] },
        { id: "vf-vacation-mountains", name: "Mountains", children: [] },
      ],
    },
    {
      id: "vf-clients",
      name: "Client Work",
      children: [
        { id: "vf-clients-acme", name: "Acme Corp", children: [] },
        { id: "vf-clients-globex", name: "Globex", children: [] },
      ],
    },
    {
      id: "vf-personal",
      name: "Personal",
      children: [
        { id: "vf-personal-photos", name: "Family Photos", children: [] },
        { id: "vf-personal-docs", name: "Important Docs", children: [] },
      ],
    },
    { id: "vf-archive", name: "Archive", children: [] },
  ],
};

const img = (id) => `https://images.unsplash.com/photo-${id}?w=200`;

export const virtualFiles = [
  {
    id: "vx1",
    name: "atlas_hero_concept.png",
    type: "image",
    size: "8.4 MB",
    sizeBytes: 8806020,
    physicalPath: "/Users/jdoe/Design/Brand/atlas",
    created: "2025-06-12",
    modified: "2025-07-21 14:32",
    hash: "a3f9b2e1c4d8f7a0",
    tags: ["atlas", "design", "hero"],
    thumbnail: img("1506905925346-21bda4d32df4"),
    folders: ["vf-atlas", "vf-atlas-design", "vf-personal-photos"],
  },
  {
    id: "vx2",
    name: "atlas_research_brief.pdf",
    type: "pdf",
    size: "2.1 MB",
    sizeBytes: 2202000,
    physicalPath: "/Users/jdoe/Documents/Research",
    created: "2025-05-30",
    modified: "2025-07-18 09:14",
    hash: "7b2e9f1a3c5d9012",
    tags: ["atlas", "research"],
    thumbnail: null,
    folders: ["vf-atlas", "vf-atlas-research"],
  },
  {
    id: "vx3",
    name: "atlas_pitch_deck.pptx",
    type: "document",
    size: "18.7 MB",
    sizeBytes: 19608576,
    physicalPath: "/Users/jdoe/Documents/Presentations",
    created: "2025-06-20",
    modified: "2025-07-22 11:05",
    hash: "c4d8a3f9b2e1ff04",
    tags: ["atlas", "pitch", "final"],
    thumbnail: null,
    folders: ["vf-atlas", "vf-atlas-presentations", "vf-clients-acme"],
  },
  {
    id: "vx4",
    name: "logo_variants.fig",
    type: "other",
    size: "48 MB",
    sizeBytes: 50331648,
    physicalPath: "/Users/jdoe/Design/Brand",
    created: "2025-06-01",
    modified: "2025-07-19 16:40",
    hash: "e9f1a7b2c4d8a009",
    tags: ["atlas", "design", "logo"],
    thumbnail: null,
    folders: ["vf-atlas", "vf-atlas-design"],
  },
  {
    id: "vx5",
    name: "invoice_2847.pdf",
    type: "pdf",
    size: "320 KB",
    sizeBytes: 327680,
    physicalPath: "/Users/jdoe/Documents/Finance/2025",
    created: "2025-07-02",
    modified: "2025-07-02 10:00",
    hash: "11aa22bb33cc44dd",
    tags: ["finance", "tax", "invoice"],
    thumbnail: null,
    folders: ["vf-tax", "vf-tax-invoices"],
  },
  {
    id: "vx6",
    name: "receipt_office_supplies.pdf",
    type: "pdf",
    size: "184 KB",
    sizeBytes: 188416,
    physicalPath: "/Users/jdoe/Downloads",
    created: "2025-07-05",
    modified: "2025-07-05 13:22",
    hash: "22bb33cc44dd55ee",
    tags: ["finance", "tax", "receipt"],
    thumbnail: null,
    folders: ["vf-tax", "vf-tax-receipts"],
  },
  {
    id: "vx7",
    name: "sunset_balcony.jpg",
    type: "image",
    size: "6.2 MB",
    sizeBytes: 6501171,
    physicalPath: "/Users/jdoe/Photos/2025/July",
    created: "2025-07-12",
    modified: "2025-07-12 18:30",
    hash: "a3f9b2e1c4d8f7a0",
    tags: ["vacation", "summer", "favorite"],
    thumbnail: img("1506905925346-21bda4d32df4"),
    folders: ["vf-vacation", "vf-vacation-summer", "vf-personal-photos"],
  },
  {
    id: "vx8",
    name: "mountain_view.jpg",
    type: "image",
    size: "5.1 MB",
    sizeBytes: 5347737,
    physicalPath: "/Users/jdoe/Photos/2025/June",
    created: "2025-06-28",
    modified: "2025-06-28 07:45",
    hash: "33cc44dd55ee66ff",
    tags: ["vacation", "mountains"],
    thumbnail: img("1464822759023-fed622ff2c3b"),
    folders: ["vf-vacation", "vf-vacation-mountains", "vf-personal-photos"],
  },
  {
    id: "vx9",
    name: "hiking_trail.mp4",
    type: "video",
    size: "1.2 GB",
    sizeBytes: 1288490188,
    physicalPath: "/Users/jdoe/Videos/Vacation",
    created: "2025-06-29",
    modified: "2025-07-01 20:10",
    hash: "44dd55ee66ff7700",
    tags: ["vacation", "mountains", "video"],
    thumbnail: img("1551524559-8af4e6624178"),
    folders: ["vf-vacation", "vf-vacation-mountains"],
  },
  {
    id: "vx10",
    name: "acme_contract_final.docx",
    type: "document",
    size: "210 KB",
    sizeBytes: 215040,
    physicalPath: "/Users/jdoe/Documents/Legal",
    created: "2025-06-15",
    modified: "2025-07-10 15:00",
    hash: "55ee66ff77008811",
    tags: ["client", "acme", "contract", "final"],
    thumbnail: null,
    folders: ["vf-clients", "vf-clients-acme", "vf-personal-docs"],
  },
  {
    id: "vx11",
    name: "acme_kickoff_notes.docx",
    type: "document",
    size: "124 KB",
    sizeBytes: 126976,
    physicalPath: "/Users/jdoe/Documents/Meetings",
    created: "2025-06-10",
    modified: "2025-07-11 09:30",
    hash: "66ff770088119922",
    tags: ["client", "acme", "meeting"],
    thumbnail: null,
    folders: ["vf-clients", "vf-clients-acme"],
  },
  {
    id: "vx12",
    name: "globex_proposal_v2.pdf",
    type: "pdf",
    size: "3.4 MB",
    sizeBytes: 3565158,
    physicalPath: "/Users/jdoe/Documents/Clients/Globex",
    created: "2025-07-01",
    modified: "2025-07-20 12:18",
    hash: "770088119922aa33",
    tags: ["client", "globex", "proposal", "draft"],
    thumbnail: null,
    folders: ["vf-clients", "vf-clients-globex"],
  },
  {
    id: "vx13",
    name: "family_reunion_2025.jpg",
    type: "image",
    size: "7.8 MB",
    sizeBytes: 8178892,
    physicalPath: "/Users/jdoe/Photos/Family",
    created: "2025-07-04",
    modified: "2025-07-04 16:00",
    hash: "88119922aa33bb44",
    tags: ["personal", "family", "favorite"],
    thumbnail: img("1501785888041-af3ef285b470"),
    folders: ["vf-personal", "vf-personal-photos"],
  },
  {
    id: "vx14",
    name: "passport_scan.pdf",
    type: "pdf",
    size: "920 KB",
    sizeBytes: 943718,
    physicalPath: "/Users/jdoe/Documents/Personal",
    created: "2025-01-12",
    modified: "2025-01-12 11:00",
    hash: "9922aa33bb44cc55",
    tags: ["personal", "important", "id"],
    thumbnail: null,
    folders: ["vf-personal", "vf-personal-docs"],
  },
  {
    id: "vx15",
    name: "old_backup_2024.tar.gz",
    type: "archive",
    size: "8.4 GB",
    sizeBytes: 9019431322,
    physicalPath: "/Users/jdoe/Backups",
    created: "2024-12-20",
    modified: "2024-12-20 03:00",
    hash: "aa33bb44cc55dd66",
    tags: ["archive", "backup"],
    thumbnail: null,
    folders: ["vf-archive"],
  },
  {
    id: "vx16",
    name: "podcast_episode_12.mp3",
    type: "audio",
    size: "64 MB",
    sizeBytes: 67108864,
    physicalPath: "/Users/jdoe/Audio/Podcasts",
    created: "2025-07-08",
    modified: "2025-07-08 14:00",
    hash: "bb44cc55dd66ee77",
    tags: ["personal", "audio"],
    thumbnail: null,
    folders: ["vf-personal", "vf-personal-docs"],
  },
  {
    id: "vx17",
    name: "city_lights.jpg",
    type: "image",
    size: "4.8 MB",
    sizeBytes: 5033164,
    physicalPath: "/Users/jdoe/Photos/Urban",
    created: "2025-07-08",
    modified: "2025-07-08 21:15",
    hash: "cc55dd66ee77ff88",
    tags: ["vacation", "urban", "favorite"],
    thumbnail: img("1502602898657-3e91760cbb34"),
    folders: ["vf-vacation", "vf-vacation-summer", "vf-personal-photos"],
  },
  {
    id: "vx18",
    name: "acme_brand_guidelines.pdf",
    type: "pdf",
    size: "12.3 MB",
    sizeBytes: 12897484,
    physicalPath: "/Users/jdoe/Design/Clients/Acme",
    created: "2025-06-18",
    modified: "2025-07-15 10:45",
    hash: "dd66ee77ff880099",
    tags: ["client", "acme", "design"],
    thumbnail: null,
    folders: ["vf-clients", "vf-clients-acme", "vf-atlas", "vf-atlas-design"],
  },
];

// Helper: flatten the tree into a map of id -> node and a list of ancestor ids
export function buildTreeIndex(node, parentPath = [], index = {}) {
  const path = [...parentPath, node.id];
  index[node.id] = { node, path };
  (node.children || []).forEach((c) => buildTreeIndex(c, path, index));
  return index;
}

export function findNode(node, id) {
  if (node.id === id) return node;
  for (const child of node.children || []) {
    const found = findNode(child, id);
    if (found) return found;
  }
  return null;
}

export function getFolderNameMap(node, map = {}) {
  map[node.id] = node.name;
  (node.children || []).forEach((c) => getFolderNameMap(c, map));
  return map;
}

export function getBreadcrumb(treeIndex, folderId) {
  if (!treeIndex[folderId]) return [];
  return treeIndex[folderId].path.map((id) => treeIndex[id].node);
}