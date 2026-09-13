const API_BASE_URL = import.meta.env.VITE_API_BASE_URL as string;

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly errors?: Record<string, string[]>,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

interface RequestOptions {
  method?: "GET" | "POST" | "PUT" | "DELETE";
  body?: unknown;
  token?: string | null;
}

export async function apiRequest<TResponse>(
  path: string,
  { method = "GET", body, token }: RequestOptions = {},
): Promise<TResponse> {
  const headers: Record<string, string> = {};
  const isFormData = body instanceof FormData;

  if (body !== undefined && !isFormData) {
    headers["Content-Type"] = "application/json";
  }

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers,
    body: body === undefined ? undefined : isFormData ? body : JSON.stringify(body),
  });

  if (response.status === 204) {
    return undefined as TResponse;
  }

  // ASP.NET Core's error responses (400/401/403/404/409, ...) are served as
  // "application/problem+json", not "application/json" - match on "json" generally so
  // both success bodies and RFC 7807 problem bodies get parsed instead of silently
  // falling back to the generic HTTP status text below.
  const isJson = response.headers.get("content-type")?.includes("json");
  const data = isJson ? await response.json() : undefined;

  if (!response.ok) {
    // RFC 7807: `title` is a generic, problem-type-level summary (e.g. "Access denied.");
    // `detail` is the specific, human-readable explanation for this occurrence. Prefer
    // `detail` so users see the actually useful message instead of the generic category.
    const message = data?.detail ?? data?.title ?? response.statusText ?? "Request failed";
    throw new ApiError(message, response.status, data?.errors);
  }

  return data as TResponse;
}

/// Fetches a binary response (e.g. a file download) with the auth header attached, since a
/// plain <a href> link can't carry an Authorization header itself.
export async function apiFetchBlob(
  path: string,
  token: string,
): Promise<{ blob: Blob; fileName: string }> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: { Authorization: `Bearer ${token}` },
  });

  if (!response.ok) {
    throw new ApiError("Could not download the file.", response.status);
  }

  const disposition = response.headers.get("content-disposition") ?? "";
  const match = /filename="?([^";]+)"?/i.exec(disposition);
  const fileName = match?.[1] ?? "download";

  return { blob: await response.blob(), fileName };
}
