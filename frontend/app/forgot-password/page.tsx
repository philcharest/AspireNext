"use client";

import { useState } from "react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useAuth } from "@/lib/auth-context";

export default function ForgotPasswordPage() {
    const { forgotPassword } = useAuth();
    const [email, setEmail] = useState("");
    const [submitting, setSubmitting] = useState(false);
    const [sent, setSent] = useState(false);

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        setSubmitting(true);
        await forgotPassword(email);
        // Always show the same message regardless of outcome - the endpoint itself never
        // reveals whether an account exists, so the UI shouldn't either.
        setSent(true);
    }

    return (
        <main className="mx-auto max-w-sm px-6 py-24">
            <p className="gallery-eyebrow text-center">Account Recovery</p>
            <h1 className="mt-3 text-center font-heading text-3xl font-medium tracking-tight text-foreground">
                Reset your password
            </h1>

            <div className="mt-8 rounded-2xl border border-border bg-card p-8 shadow-sm">
                {sent ? (
                    <p className="text-sm text-muted-foreground">
                        If an account exists for that email, we&apos;ve sent a link to reset your password.
                    </p>
                ) : (
                    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                        <div className="flex flex-col gap-2">
                            <Label htmlFor="email">Email</Label>
                            <Input
                                id="email"
                                type="email"
                                required
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                            />
                        </div>

                        <Button type="submit" disabled={submitting} size="lg" className="mt-2">
                            {submitting ? "Sending..." : "Send reset link"}
                        </Button>
                    </form>
                )}
            </div>

            <p className="mt-6 text-center text-sm text-muted-foreground">
                <Link href="/login" className="text-primary underline underline-offset-4">
                    Back to sign in
                </Link>
            </p>
        </main>
    );
}
