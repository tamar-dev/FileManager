// Mock data for the Virtual Explorer feature.
// KEY CONCEPT: Folders here are VIRTUAL — they do not exist on disk.
// Files keep their original physical location; a file can live in many
// virtual folders at once. Moving a file inside the explorer never touches
// the real file system.


const img = (id) => `https://images.unsplash.com/photo-${id}?w=200`;


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