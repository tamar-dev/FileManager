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