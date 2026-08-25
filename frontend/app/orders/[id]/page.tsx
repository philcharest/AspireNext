"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/lib/auth-context";
import { extractError } from "@/lib/api-error";
import { apiFetch } from "@/lib/csrf";

type OrderItem = {
    id: number;
    productId: number;
    productName: string;
    price: number;
    quantity: number;
    lineTotal: number;
};

type ReturnItem = {
    orderItemId: number;
    productName: string;
    quantity: number;
};

type OrderReturn = {
    id: number;
    requestedAt: string;
    status: "Requested" | "Approved" | "Rejected";
    reason: string;
    reviewNote: string | null;
    refundAmount: number | null;
    items: ReturnItem[];
};

type Order = {
    id: number;
    createdAt: string;
    status: string;
    items: OrderItem[];
    returns: OrderReturn[];
    total: number;
};

const currencyFormatter = new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
});

const RETURN_STATUS_BADGE: Record<string, { variant: "default" | "outline" | "destructive"; label: string }> = {
    Requested: { variant: "outline", label: "Requested" },
    Approved: { variant: "default", label: "Approved" },
    Rejected: { variant: "destructive", label: "Rejected" },
};

const PENDING_POLL_ATTEMPTS = 3;
const PENDING_POLL_INTERVAL_MS = 1500;

export default function OrderDetailPage() {
    const { id } = useParams<{ id: string }>();
    const { user, loading: authLoading } = useAuth();
    const router = useRouter();
    const [order, setOrder] = useState<Order | null>(null);
    const [notFound, setNotFound] = useState(false);
    const [returnQuantities, setReturnQuantities] = useState<Record<number, number>>({});
    const [returnReason, setReturnReason] = useState("");
    const [returnSubmitting, setReturnSubmitting] = useState(false);
    const [returnError, setReturnError] = useState<string | null>(null);

    useEffect(() => {
        if (!authLoading && !user) {
            router.push("/login");
        }
    }, [authLoading, user, router]);

    useEffect(() => {
        if (!user) return;

        let cancelled = false;

        async function poll(attempt: number) {
            const res = await fetch(`/api/orders/${id}`);
            if (cancelled) return;
            if (!res.ok) {
                setNotFound(true);
                return;
            }

            const data: Order = await res.json();
            setOrder(data);

            if (data.status === "PendingPayment" && attempt < PENDING_POLL_ATTEMPTS) {
                setTimeout(() => poll(attempt + 1), PENDING_POLL_INTERVAL_MS);
            }
        }

        poll(0);
        return () => {
            cancelled = true;
        };
    }, [user, id]);

    function remainingReturnable(item: OrderItem): number {
        if (!order) return 0;
        const alreadyRequested = order.returns
            .filter((r) => r.status !== "Rejected")
            .flatMap((r) => r.items)
            .filter((ri) => ri.orderItemId === item.id)
            .reduce((sum, ri) => sum + ri.quantity, 0);
        return item.quantity - alreadyRequested;
    }

    async function handleRequestReturn(e: React.FormEvent) {
        e.preventDefault();
        setReturnError(null);

        const items = Object.entries(returnQuantities)
            .map(([orderItemId, quantity]) => ({ orderItemId: Number(orderItemId), quantity }))
            .filter((i) => i.quantity > 0);

        if (items.length === 0) {
            setReturnError("Select at least one item and quantity to return.");
            return;
        }

        setReturnSubmitting(true);
        const res = await apiFetch(`/api/orders/${id}/returns`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ reason: returnReason, items }),
        });

        if (!res.ok) {
            setReturnError(await extractError(res));
            setReturnSubmitting(false);
            return;
        }

        const updated = await fetch(`/api/orders/${id}`);
        if (updated.ok) setOrder(await updated.json());
        setReturnQuantities({});
        setReturnReason("");
        setReturnSubmitting(false);
    }

    if (authLoading || !user) {
        return (
            <main className="mx-auto max-w-2xl px-6 py-16">
                <p className="text-muted-foreground">Loading...</p>
            </main>
        );
    }

    if (notFound) {
        return (
            <main className="mx-auto max-w-2xl px-6 py-16">
                <p className="text-muted-foreground">
                    Order not found.{" "}
                    <Link href="/orders" className="text-primary underline underline-offset-4">
                        View your orders
                    </Link>
                    .
                </p>
            </main>
        );
    }

    if (!order) {
        return (
            <main className="mx-auto max-w-2xl px-6 py-16">
                <p className="text-muted-foreground">Loading...</p>
            </main>
        );
    }

    if (order.status === "PendingPayment") {
        return (
            <main className="mx-auto max-w-2xl px-6 py-24">
                <h1 className="font-heading text-4xl font-medium tracking-tight text-foreground">
                    Processing your payment...
                </h1>
                <p className="mt-2 text-muted-foreground">
                    Order #{order.id} — this usually only takes a moment.
                </p>
            </main>
        );
    }

    if (order.status === "PaymentFailed" || order.status === "Cancelled") {
        return (
            <main className="mx-auto max-w-2xl px-6 py-24">
                <h1 className="font-heading text-4xl font-medium tracking-tight text-foreground">
                    Payment didn&apos;t go through
                </h1>
                <p className="mt-2 text-muted-foreground">
                    Order #{order.id} was not paid.{" "}
                    <Link href="/cart" className="text-primary underline underline-offset-4">
                        Return to your cart
                    </Link>{" "}
                    to try again.
                </p>
            </main>
        );
    }

    return (
        <main className="mx-auto max-w-2xl px-6 py-24">
            <p className="gallery-eyebrow">Order Confirmed</p>
            <h1 className="mt-3 font-heading text-4xl font-medium tracking-tight text-foreground">
                Thank you for your order!
            </h1>
            <p className="mt-2 text-muted-foreground">
                Order #{order.id} placed on {new Date(order.createdAt).toLocaleDateString()}
            </p>

            <ul className="mt-12 divide-y divide-border">
                {order.items.map((item) => (
                    <li key={item.id} className="flex items-center justify-between py-5">
                        <div>
                            <p className="font-heading text-base text-foreground">{item.productName}</p>
                            <p className="text-sm text-muted-foreground">
                                {currencyFormatter.format(item.price)} &times; {item.quantity}
                            </p>
                        </div>
                        <span className="font-heading text-foreground tabular-nums">
                            {currencyFormatter.format(item.lineTotal)}
                        </span>
                    </li>
                ))}
            </ul>

            <div className="mt-8 flex items-center justify-between border-t border-border pt-6">
                <span className="font-heading text-lg text-foreground">Total</span>
                <span className="font-heading text-lg text-foreground tabular-nums">
                    {currencyFormatter.format(order.total)}
                </span>
            </div>

            {order.returns.length > 0 && (
                <div className="mt-10 border-t border-border pt-8">
                    <h2 className="font-heading text-lg text-foreground">Return Requests</h2>
                    <ul className="mt-4 space-y-4">
                        {order.returns.map((ret) => {
                            const badge = RETURN_STATUS_BADGE[ret.status];
                            return (
                                <li key={ret.id} className="text-sm">
                                    <div className="flex items-center gap-2">
                                        {badge && <Badge variant={badge.variant}>{badge.label}</Badge>}
                                        <span className="text-muted-foreground">
                                            {new Date(ret.requestedAt).toLocaleDateString()}
                                        </span>
                                    </div>
                                    <p className="mt-1 text-muted-foreground">
                                        {ret.items.map((i) => `${i.quantity} × ${i.productName}`).join(", ")}
                                    </p>
                                    {ret.reviewNote && (
                                        <p className="mt-1 text-muted-foreground">Note: {ret.reviewNote}</p>
                                    )}
                                    {ret.refundAmount !== null && (
                                        <p className="mt-1 text-foreground">
                                            Refunded {currencyFormatter.format(ret.refundAmount)}
                                        </p>
                                    )}
                                </li>
                            );
                        })}
                    </ul>
                </div>
            )}

            {order.items.some((item) => remainingReturnable(item) > 0) && (
                <form onSubmit={handleRequestReturn} className="mt-10 border-t border-border pt-8">
                    <h2 className="font-heading text-lg text-foreground">Request a Return</h2>
                    <div className="mt-4 space-y-3">
                        {order.items.map((item) => {
                            const remaining = remainingReturnable(item);
                            if (remaining <= 0) return null;
                            return (
                                <div key={item.id} className="flex items-center justify-between gap-4">
                                    <span className="text-sm text-foreground">{item.productName}</span>
                                    <select
                                        value={returnQuantities[item.id] ?? 0}
                                        onChange={(e) =>
                                            setReturnQuantities((prev) => ({
                                                ...prev,
                                                [item.id]: Number(e.target.value),
                                            }))
                                        }
                                        className="rounded-md border border-border bg-background px-2 py-1 text-sm text-foreground"
                                    >
                                        {Array.from({ length: remaining + 1 }, (_, n) => n).map((n) => (
                                            <option key={n} value={n}>
                                                {n}
                                            </option>
                                        ))}
                                    </select>
                                </div>
                            );
                        })}
                    </div>

                    <textarea
                        value={returnReason}
                        onChange={(e) => setReturnReason(e.target.value)}
                        placeholder="Reason for return"
                        required
                        className="mt-4 w-full rounded-md border border-border bg-background p-3 text-sm text-foreground"
                        rows={3}
                    />

                    {returnError && <p className="mt-2 text-sm text-destructive">{returnError}</p>}

                    <Button type="submit" disabled={returnSubmitting} className="mt-4">
                        {returnSubmitting ? "Submitting..." : "Request Return"}
                    </Button>
                </form>
            )}

            <p className="mt-8 text-sm text-muted-foreground">
                <Link href="/orders" className="text-primary underline underline-offset-4">
                    View all orders
                </Link>
            </p>
        </main>
    );
}
