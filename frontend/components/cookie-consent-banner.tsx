"use client";

import { useState } from "react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { useConsent } from "@/lib/consent-context";

export function CookieConsentBanner() {
    const { consent, bannerOpen, acceptAll, rejectNonEssential, savePreferences } = useConsent();
    const [customizing, setCustomizing] = useState(false);
    const [analytics, setAnalytics] = useState(consent?.analytics ?? false);
    const [marketing, setMarketing] = useState(consent?.marketing ?? false);

    if (!bannerOpen) return null;

    return (
        <div className="fixed inset-x-0 bottom-0 z-50 border-t border-border bg-card p-6 shadow-2xl">
            <div className="mx-auto max-w-4xl">
                <p className="text-sm text-foreground">
                    We use strictly necessary cookies to run this site (your cart and sign-in session). With
                    your permission, we&apos;d also like to use analytics cookies to understand how the site is
                    used. See our{" "}
                    <Link href="/privacy-policy" className="underline underline-offset-4">
                        Privacy Policy
                    </Link>{" "}
                    for details.
                </p>

                {customizing && (
                    <div className="mt-4 space-y-3 border-t border-border pt-4">
                        <label className="flex items-center justify-between text-sm">
                            <span className="text-foreground">Necessary (always on)</span>
                            <input type="checkbox" checked disabled className="size-4" />
                        </label>
                        <label className="flex items-center justify-between text-sm">
                            <span className="text-foreground">Analytics (e.g. Google Analytics)</span>
                            <input
                                type="checkbox"
                                checked={analytics}
                                onChange={(e) => setAnalytics(e.target.checked)}
                                className="size-4"
                            />
                        </label>
                        <label className="flex items-center justify-between text-sm">
                            <span className="text-foreground">Marketing</span>
                            <input
                                type="checkbox"
                                checked={marketing}
                                onChange={(e) => setMarketing(e.target.checked)}
                                className="size-4"
                            />
                        </label>
                    </div>
                )}

                <div className="mt-4 flex flex-wrap items-center gap-3">
                    <Button onClick={acceptAll}>Accept All</Button>
                    <Button variant="outline" onClick={rejectNonEssential}>
                        Reject Non-Essential
                    </Button>
                    {customizing ? (
                        <Button variant="ghost" onClick={() => savePreferences({ analytics, marketing })}>
                            Save Preferences
                        </Button>
                    ) : (
                        <Button variant="ghost" onClick={() => setCustomizing(true)}>
                            Customize
                        </Button>
                    )}
                </div>
            </div>
        </div>
    );
}
