"use client";

import { createContext, useCallback, useContext, useEffect, useState } from "react";
import { extractError } from "@/lib/api-error";
import { apiFetch, refreshCsrfToken } from "@/lib/csrf";

export type User = {
    email: string;
};

type AuthResult = { ok: true } | { ok: false; error: string };

type AuthContextValue = {
    user: User | null;
    loading: boolean;
    login: (email: string, password: string) => Promise<AuthResult>;
    register: (email: string, password: string) => Promise<AuthResult>;
    logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
    const [user, setUser] = useState<User | null>(null);
    const [loading, setLoading] = useState(true);

    const refresh = useCallback(async () => {
        const res = await fetch("/api/account/me");
        setUser(res.ok ? await res.json() : null);
    }, []);

    useEffect(() => {
        refreshCsrfToken();
        fetch("/api/account/me")
            .then((res) => (res.ok ? res.json() : null))
            .then(setUser)
            .finally(() => setLoading(false));
    }, []);

    const login = useCallback(
        async (email: string, password: string): Promise<AuthResult> => {
            const res = await apiFetch("/api/login?useCookies=true", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ email, password }),
            });
            if (!res.ok) return { ok: false, error: await extractError(res) };
            await refreshCsrfToken(); // token is bound to identity - refresh now that it changed
            await refresh();
            return { ok: true };
        },
        [refresh]
    );

    const register = useCallback(
        async (email: string, password: string): Promise<AuthResult> => {
            const res = await apiFetch("/api/register", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ email, password }),
            });
            if (!res.ok) return { ok: false, error: await extractError(res) };
            return login(email, password);
        },
        [login]
    );

    const logout = useCallback(async () => {
        await apiFetch("/api/account/logout", { method: "POST" });
        await refreshCsrfToken();
        setUser(null);
    }, []);

    return (
        <AuthContext.Provider value={{ user, loading, login, register, logout }}>
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth() {
    const context = useContext(AuthContext);
    if (!context) throw new Error("useAuth must be used within an AuthProvider");
    return context;
}
