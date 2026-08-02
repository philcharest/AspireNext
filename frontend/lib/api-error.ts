export async function extractError(res: Response): Promise<string> {
    try {
        const body = await res.json();
        if (body.errors) {
            return Object.values(body.errors as Record<string, string[]>).flat().join(" ");
        }
        if (body.detail) return body.detail;
        if (body.title) return body.title;
    } catch {
        // response had no JSON body
    }
    return "Something went wrong.";
}
