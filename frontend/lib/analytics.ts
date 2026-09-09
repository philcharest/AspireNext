declare global {
    interface Window {
        dataLayer: unknown[];
    }
}

// Unset until you have a real GA4 measurement ID - nothing loads, even with analytics consent,
// until this is configured. Set NEXT_PUBLIC_GA_MEASUREMENT_ID in the frontend's environment.
const GA_MEASUREMENT_ID = process.env.NEXT_PUBLIC_GA_MEASUREMENT_ID;

let loaded = false;

export function isAnalyticsConfigured() {
    return Boolean(GA_MEASUREMENT_ID);
}

export function loadGoogleAnalytics() {
    if (loaded || !GA_MEASUREMENT_ID) return;
    loaded = true;

    const script = document.createElement("script");
    script.async = true;
    script.src = `https://www.googletagmanager.com/gtag/js?id=${GA_MEASUREMENT_ID}`;
    document.head.appendChild(script);

    window.dataLayer = window.dataLayer || [];
    function gtag(...args: unknown[]) {
        window.dataLayer.push(args);
    }
    gtag("js", new Date());
    gtag("config", GA_MEASUREMENT_ID);
}
