"use client";

import { createContext, useCallback, useContext, useEffect, useState } from "react";

export type Currency = "CAD" | "USD";

const STORAGE_KEY = "currency-preference";

type CurrencyContextValue = {
    currency: Currency;
    setCurrency: (currency: Currency) => void;
};

const CurrencyContext = createContext<CurrencyContextValue | null>(null);

function detectDefaultCurrency(): Currency {
    try {
        const locale = Intl.NumberFormat().resolvedOptions().locale;
        return locale.toUpperCase().endsWith("-US") ? "USD" : "CAD";
    } catch {
        return "CAD";
    }
}

export function CurrencyProvider({ children }: { children: React.ReactNode }) {
    const [currency, setCurrencyState] = useState<Currency>("CAD");

    // Runs once on mount, client-side only - localStorage/Intl locale aren't available during SSR.
    useEffect(() => {
        const stored = localStorage.getItem(STORAGE_KEY);
        if (stored === "CAD" || stored === "USD") {
            setCurrencyState(stored);
        } else {
            setCurrencyState(detectDefaultCurrency());
        }
    }, []);

    const setCurrency = useCallback((next: Currency) => {
        localStorage.setItem(STORAGE_KEY, next);
        setCurrencyState(next);
    }, []);

    return (
        <CurrencyContext.Provider value={{ currency, setCurrency }}>
            {children}
        </CurrencyContext.Provider>
    );
}

export function useCurrency() {
    const context = useContext(CurrencyContext);
    if (!context) throw new Error("useCurrency must be used within a CurrencyProvider");
    return context;
}
