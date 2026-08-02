let csrfToken: string | null = null;

export async function refreshCsrfToken(): Promise<void> {
    const res = await fetch("/api/antiforgery/token");
    csrfToken = res.ok ? (await res.json()).token : null;
}

/**
 * fetch() wrapper that attaches the CSRF header for unsafe methods (POST/PUT/DELETE/PATCH).
 * Safe methods (GET/HEAD, the default) pass through untouched - the server doesn't require
 * a token for those either. Lazily fetches a token on first use if one isn't already held.
 */
export async function apiFetch(input: string, init: RequestInit = {}): Promise<Response> {
    const method = (init.method ?? "GET").toUpperCase();
    if (method === "GET" || method === "HEAD") {
        return fetch(input, init);
    }

    if (csrfToken === null) {
        await refreshCsrfToken();
    }

    const headers = new Headers(init.headers);
    if (csrfToken) headers.set("X-CSRF-TOKEN", csrfToken);
    return fetch(input, { ...init, headers });
}
