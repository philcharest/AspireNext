"use client";

import { Suspense, useEffect, useState } from "react";
import { useSearchParams } from "next/navigation";
import Link from "next/link";

export default function ConfirmEmailPage() {
    return (
        <Suspense>
            <ConfirmEmailContent />
        </Suspense>
    );
}

function ConfirmEmailContent() {
    const searchParams = useSearchParams();
    const userId = searchParams.get("userId");
    const code = searchParams.get("code");
    const [status, setStatus] = useState<"pending" | "success" | "error">("pending");

    useEffect(() => {
        if (!userId || !code) {
            setStatus("error");
            return;
        }

        fetch(`/api/confirmEmail?userId=${encodeURIComponent(userId)}&code=${encodeURIComponent(code)}`)
            .then((res) => setStatus(res.ok ? "success" : "error"))
            .catch(() => setStatus("error"));
    }, [userId, code]);

    return (
        <main className="mx-auto max-w-sm px-6 py-24">
            <p className="gallery-eyebrow text-center">Account</p>
            <h1 className="mt-3 text-center font-heading text-3xl font-medium tracking-tight text-foreground">
                {status === "success" ? "Email confirmed" : status === "error" ? "Confirmation failed" : "Confirming..."}
            </h1>

            <div className="mt-8 rounded-2xl border border-border bg-card p-8 text-center shadow-sm">
                {status === "pending" && (
                    <p className="text-sm text-muted-foreground">One moment...</p>
                )}
                {status === "success" && (
                    <p className="text-sm text-muted-foreground">
                        Your email is confirmed.{" "}
                        <Link href="/login" className="text-primary underline underline-offset-4">
                            Sign in
                        </Link>
                        .
                    </p>
                )}
                {status === "error" && (
                    <p className="text-sm text-muted-foreground">
                        This confirmation link is invalid or has expired.{" "}
                        <Link href="/resend-confirmation" className="text-primary underline underline-offset-4">
                            Request a new one
                        </Link>
                        .
                    </p>
                )}
            </div>
        </main>
    );
}
