"use client";

import { createContext, useCallback, useContext, useEffect, useState } from "react";

export type ConsentState = {
    necessary: true;
    analytics: boolean;
    marketing: boolean;
};

type StoredConsent = ConsentState & { decidedAt: string };

const STORAGE_KEY = "cookie-consent";

type ConsentContextValue = {
    consent: ConsentState | null;
    bannerOpen: boolean;
    acceptAll: () => void;
    rejectNonEssential: () => void;
    savePreferences: (prefs: { analytics: boolean; marketing: boolean }) => void;
    reopenBanner: () => void;
};

const ConsentContext = createContext<ConsentContextValue | null>(null);

function readStoredConsent(): StoredConsent | null {
    try {
        const raw = localStorage.getItem(STORAGE_KEY);
        return raw ? JSON.parse(raw) : null;
    } catch {
        return null;
    }
}

function writeStoredConsent(consent: ConsentState) {
    const stored: StoredConsent = { ...consent, decidedAt: new Date().toISOString() };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(stored));
}

export function ConsentProvider({ children }: { children: React.ReactNode }) {
    const [consent, setConsent] = useState<ConsentState | null>(null);
    const [bannerOpen, setBannerOpen] = useState(false);

    // Runs once on mount, client-side only - localStorage isn't available during SSR.
    useEffect(() => {
        const stored = readStoredConsent();
        if (stored) {
            setConsent({ necessary: true, analytics: stored.analytics, marketing: stored.marketing });
        } else {
            setBannerOpen(true);
        }
    }, []);

    const applyAndStore = useCallback((next: { analytics: boolean; marketing: boolean }) => {
        const full: ConsentState = { necessary: true, ...next };
        writeStoredConsent(full);
        setConsent(full);
        setBannerOpen(false);
    }, []);

    const acceptAll = useCallback(() => applyAndStore({ analytics: true, marketing: true }), [applyAndStore]);
    const rejectNonEssential = useCallback(() => applyAndStore({ analytics: false, marketing: false }), [applyAndStore]);
    const savePreferences = useCallback(
        (prefs: { analytics: boolean; marketing: boolean }) => applyAndStore(prefs),
        [applyAndStore]
    );
    const reopenBanner = useCallback(() => setBannerOpen(true), []);

    return (
        <ConsentContext.Provider
            value={{ consent, bannerOpen, acceptAll, rejectNonEssential, savePreferences, reopenBanner }}
        >
            {children}
        </ConsentContext.Provider>
    );
}

export function useConsent() {
    const context = useContext(ConsentContext);
    if (!context) throw new Error("useConsent must be used within a ConsentProvider");
    return context;
}
