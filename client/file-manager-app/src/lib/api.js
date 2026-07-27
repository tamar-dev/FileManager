const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export async function getDashboard() {
  const response = await fetch(`${API_BASE_URL}/api/dashboard`)

  if (!response.ok) {
    throw new Error(`Dashboard request failed: ${response.status}`)
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