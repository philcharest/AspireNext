"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { useAuth } from "@/lib/auth-context";

export default function AdminLayout({ children }: { children: React.ReactNode }) {
    const { user, loading } = useAuth();
    const router = useRouter();

    useEffect(() => {
        if (!loading && !user?.isAdmin) {
            router.push("/");
        }
    }, [loading, user, router]);

    if (loading || !user?.isAdmin) {
        return (
            <main className="mx-auto max-w-5xl px-6 py-24">
                <p className="text-muted-foreground">Loading...</p>
            </main>
        );
    }

    return (
        <main className="mx-auto max-w-5xl px-6 py-24">
            <p className="gallery-eyebrow">Admin</p>
            <div className="mt-3 flex items-center gap-6">
                <Link href="/admin/orders" className="font-heading text-lg text-foreground">
                    Orders
                </Link>
                <Link href="/admin/returns" className="font-heading text-lg text-foreground">
                    Returns
                </Link>
            </div>
            <div className="mt-10 border-t border-border" />
            <div className="mt-10">{children}</div>
        </main>
    );
}
