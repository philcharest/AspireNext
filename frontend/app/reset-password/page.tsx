"use client";

import { Suspense, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useAuth } from "@/lib/auth-context";

export default function ResetPasswordPage() {
    return (
        <Suspense>
            <ResetPasswordContent />
        </Suspense>
    );
}

function ResetPasswordContent() {
    const { resetPassword } = useAuth();
    const router = useRouter();
    const searchParams = useSearchParams();
    const email = searchParams.get("email") ?? "";
    const code = searchParams.get("code") ?? "";
    const [newPassword, setNewPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        setError(null);

        if (newPassword !== confirmPassword) {
            setError("Passwords do not match.");
            return;
        }

        setSubmitting(true);
        const result = await resetPassword(email, code, newPassword);
        if (result.ok) {
            router.push("/login");
        } else {
            setError(result.error);
            setSubmitting(false);
        }
    }

    if (!email || !code) {
        return (
            <main className="mx-auto max-w-sm px-6 py-24">
                <h1 className="font-heading text-3xl font-medium tracking-tight text-foreground">
                    Invalid reset link
                </h1>
                <p className="mt-2 text-muted-foreground">
                    This password reset link is missing or incomplete.{" "}
                    <Link href="/forgot-password" className="text-primary underline underline-offset-4">
                        Request a new one
                    </Link>
                    .
                </p>
            </main>
        );
    }

    return (
        <main className="mx-auto max-w-sm px-6 py-24">
            <p className="gallery-eyebrow text-center">Account Recovery</p>
            <h1 className="mt-3 text-center font-heading text-3xl font-medium tracking-tight text-foreground">
                Choose a new password
            </h1>

            <div className="mt-8 rounded-2xl border border-border bg-card p-8 shadow-sm">
                <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                    <div className="flex flex-col gap-2">
                        <Label htmlFor="newPassword">New password</Label>
                        <Input
                            id="newPassword"
                            type="password"
                            required
                            value={newPassword}
                            onChange={(e) => setNewPassword(e.target.value)}
                        />
                    </div>
                    <div className="flex flex-col gap-2">
                        <Label htmlFor="confirmPassword">Confirm new password</Label>
                        <Input
                            id="confirmPassword"
                            type="password"
                            required
                            value={confirmPassword}
                            onChange={(e) => setConfirmPassword(e.target.value)}
                        />
                    </div>

                    {error && <p className="text-sm text-destructive">{error}</p>}

                    <Button type="submit" disabled={submitting} size="lg" className="mt-2">
                        {submitting ? "Saving..." : "Save new password"}
                    </Button>
                </form>
            </div>
        </main>
    );
}
