"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useAuth } from "@/lib/auth-context";

export default function LoginPage() {
    const { login } = useAuth();
    const router = useRouter();
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        setSubmitting(true);
        setError(null);

        const result = await login(email, password);
        if (result.ok) {
            router.push("/");
        } else {
            setError(result.error);
            setSubmitting(false);
        }
    }

    return (
        <main className="mx-auto max-w-sm px-6 py-16">
            <h1 className="font-heading text-3xl font-semibold tracking-tight text-foreground">
                Sign in
            </h1>

            <form onSubmit={handleSubmit} className="mt-8 flex flex-col gap-4">
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

                <Button type="submit" disabled={submitting} className="mt-2">
                    {submitting ? "Signing in..." : "Sign in"}
                </Button>
            </form>

            <p className="mt-6 text-sm text-muted-foreground">
                Don&apos;t have an account?{" "}
                <Link href="/register" className="text-primary underline underline-offset-4">
                    Register
                </Link>
                .
            </p>
        </main>
    );
}
