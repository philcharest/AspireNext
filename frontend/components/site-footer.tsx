"use client";

import Link from "next/link";
import { useConsent } from "@/lib/consent-context";

export function SiteFooter() {
    const { reopenBanner } = useConsent();

    return (
        <footer className="border-t border-border">
            <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-4 px-6 py-8 text-sm text-muted-foreground">
                <span>&copy; {new Date().getFullYear()} Wall Art Canvases</span>
                <div className="flex items-center gap-4">
                    <Link href="/privacy-policy" className="hover:text-foreground">
                        Privacy Policy
                    </Link>
                    <button type="button" onClick={reopenBanner} className="hover:text-foreground">
                        Cookie Preferences
                    </button>
                </div>
            </div>
        </footer>
    );
}
