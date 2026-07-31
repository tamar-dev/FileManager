const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export async function getDashboard() {
  const response = await fetch(`${API_BASE_URL}/api/dashboard`)

  if (!response.ok) {
    throw new Error(`Dashboard request failed: ${response.status}`)
  }

  return response.json()
}

export async function getDuplicates() {
  const response = await fetch(`${API_BASE_URL}/api/duplicates`)

  if (!response.ok) {
    throw new Error(`Duplicates request failed: ${response.status}`)
  }

  return response.json()
}

export async function getIndexedLocations() {
  const response = await fetch(`${API_BASE_URL}/api/indexed-locations`)

  if (!response.ok) {
    throw new Error(`Indexed locations request failed: ${response.status}`)
  }

  return response.json()
}

export async function addIndexedLocation({ path, watchEnabled = true }) {
  const response = await fetch(`${API_BASE_URL}/api/indexed-locations`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ path, watchEnabled }),
  })

  if (!response.ok) {
    const data = await response.json().catch(() => null)
    throw new Error(data?.error || `Add indexed location failed: ${response.status}`)
  }

  return response.json()
}

export async function removeIndexedLocation(id) {
  const response = await fetch(`${API_BASE_URL}/api/indexed-locations/${id}`, {
    method: "DELETE",
  })

  if (!response.ok && response.status !== 404) {
    throw new Error(`Remove indexed location failed: ${response.status}`)
  }

  return true
}

export async function startIndexing(path) {
  const response = await fetch(`${API_BASE_URL}/api/index`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ path }),
  })

  if (response.status === 409) {
    throw new Error("Indexing is already running.")
  }

  if (!response.ok) {
    const data = await response.json().catch(() => null)
    throw new Error(data?.error || `Start indexing failed: ${response.status}`)
  }

  return true
}

export async function getIndexStatus() {
  const response = await fetch(`${API_BASE_URL}/api/index/status`)

  if (!response.ok) {
    throw new Error(`Index status request failed: ${response.status}`)
  }

  return response.json()
}

export async function searchFiles(params = {}) {
  const searchParams = new URLSearchParams()

  if (params.name) searchParams.set("name", params.name)
  if (params.extension) searchParams.set("extension", params.extension)
  if (params.path) searchParams.set("path", params.path)
  if (params.modifiedAfter) searchParams.set("modifiedAfter", params.modifiedAfter)
  if (params.modifiedBefore) searchParams.set("modifiedBefore", params.modifiedBefore)

  const query = searchParams.toString()
  const url = `${API_BASE_URL}/api/files/search${query ? `?${query}` : ""}`

  const response = await fetch(url)

  if (!response.ok) {
    throw new Error(`Search request failed: ${response.status}`)
  }

  return response.json()
}

export async function getVirtualFolders() {
  const response = await fetch(`${API_BASE_URL}/api/virtual-folders`)

  if (!response.ok) {
    throw new Error(`Virtual folders request failed: ${response.status}`)
  }

  return response.json()
}

export async function createVirtualFolder({ name, parentId = null }) {
  const response = await fetch(`${API_BASE_URL}/api/virtual-folders`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ name, parentId }),
  })

  if (!response.ok) {
    const data = await response.json().catch(() => null)
    throw new Error(data?.error || `Create virtual folder failed: ${response.status}`)
  }

  return response.json()
}

export async function updateVirtualFolder(id, data) {
  const response = await fetch(`${API_BASE_URL}/api/virtual-folders/${id}`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  })

  if (!response.ok) {
    const body = await response.json().catch(() => null)
    throw new Error(body?.error || `Update virtual folder failed: ${response.status}`)
  }

  return response.json()
}

export async function deleteVirtualFolder(id) {
  const response = await fetch(`${API_BASE_URL}/api/virtual-folders/${id}`, {
    method: "DELETE",
  })

  if (!response.ok) {
    const data = await response.json().catch(() => null)
    throw new Error(data?.error || `Delete virtual folder failed: ${response.status}`)
  }

  return true
}

export async function getVirtualFolderFiles(folderId) {
  const response = await fetch(`${API_BASE_URL}/api/virtual-folders/${folderId}/files`)

  if (!response.ok) {
    throw new Error(`Virtual folder files request failed: ${response.status}`)
  }

  return response.json()
}

export async function addFileToVirtualFolder(folderId, fileId) {
  const response = await fetch(`${API_BASE_URL}/api/virtual-folders/${folderId}/files`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ fileId }),
  })

  if (!response.ok) {
    const data = await response.json().catch(() => null)
    throw new Error(data?.error || `Add file to virtual folder failed: ${response.status}`)
  }

  return true
}

export async function removeFileFromVirtualFolder(folderId, fileId) {
  const response = await fetch(
    `${API_BASE_URL}/api/virtual-folders/${folderId}/files/${fileId}`,
    {
      method: "DELETE",
    }
  )

  if (!response.ok) {
    const data = await response.json().catch(() => null)
    throw new Error(data?.error || `Remove file from virtual folder failed: ${response.status}`)
  }

  return true
}

export async function getFileVirtualFolders(fileId) {
  const response = await fetch(`${API_BASE_URL}/api/files/${fileId}/virtual-folders`)

  if (!response.ok) {
    throw new Error(`File virtual folders request failed: ${response.status}`)
  }

  return response.json()
}

