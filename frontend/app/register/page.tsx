"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useAuth } from "@/lib/auth-context";

export default function RegisterPage() {
    const { register } = useAuth();
    const router = useRouter();
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        setSubmitting(true);
        setError(null);

        const result = await register(email, password);
        if (result.ok) {
            router.push("/");
        } else {
            setError(result.error);
            setSubmitting(false);
        }
    }

    return (
        <main className="mx-auto max-w-sm px-6 py-24">
            <p className="gallery-eyebrow text-center">Join Us</p>
            <h1 className="mt-3 text-center font-heading text-3xl font-medium tracking-tight text-foreground">
                Create an account
            </h1>

            <div className="mt-8 rounded-2xl border border-border bg-card p-8 shadow-sm">
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
                    <div className="flex flex-col gap-2">
                        <Label htmlFor="password">Password</Label>
                        <Input
                            id="password"
                            type="password"
                            required
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                    </div>

                    {error && <p className="text-sm text-destructive">{error}</p>}

                    <Button type="submit" disabled={submitting} size="lg" className="mt-2">
                        {submitting ? "Creating account..." : "Create account"}
                    </Button>
                </form>
            </div>

            <p className="mt-6 text-center text-sm text-muted-foreground">
                Already have an account?{" "}
                <Link href="/login" className="text-primary underline underline-offset-4">
                    Sign in
                </Link>
                .
            </p>
        </main>
    );
}
