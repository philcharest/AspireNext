"use client";

import { useEffect } from "react";
import { useConsent } from "@/lib/consent-context";
import { loadGoogleAnalytics } from "@/lib/analytics";

// Renders nothing - just watches consent state and loads Google Analytics only once the visitor
// has actually opted into analytics, and only if a measurement ID is configured.
export function AnalyticsLoader() {
    const { consent } = useConsent();

    useEffect(() => {
        if (consent?.analytics) {
            loadGoogleAnalytics();
        }
    }, [consent]);

    return null;
}
