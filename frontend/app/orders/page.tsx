"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Badge } from "@/components/ui/badge";
import { useAuth } from "@/lib/auth-context";

type Order = {
    id: number;
    createdAt: string;
    status: string;
    total: number;
};

const currencyFormatter = new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
});

const STATUS_BADGE: Record<string, { variant: "default" | "outline" | "destructive"; label: string }> = {
    Paid: { variant: "default", label: "Paid" },
    PendingPayment: { variant: "outline", label: "Pending" },
    PaymentFailed: { variant: "destructive", label: "Failed" },
    Cancelled: { variant: "destructive", label: "Cancelled" },
};

export default function OrdersPage() {
    const { user, loading: authLoading } = useAuth();
    const router = useRouter();
    const [orders, setOrders] = useState<Order[] | null>(null);

    useEffect(() => {
        if (!authLoading && !user) {
            router.push("/login");
        }
    }, [authLoading, user, router]);

    useEffect(() => {
        if (!user) return;
        fetch("/api/orders")
            .then((res) => (res.ok ? res.json() : []))
            .then(setOrders);
    }, [user]);

    if (authLoading || !user || orders === null) {
        return (
            <main className="mx-auto max-w-3xl px-6 py-16">
                <p className="text-muted-foreground">Loading...</p>
            </main>
        );
    }

    return (
        <main className="mx-auto max-w-3xl px-6 py-24">
            <p className="gallery-eyebrow">Order History</p>
            <h1 className="mt-3 font-heading text-4xl font-medium tracking-tight text-foreground">
                Your Orders
            </h1>

            {orders.length === 0 ? (
                <p className="mt-8 text-muted-foreground">
                    You haven&apos;t placed any orders yet.{" "}
                    <Link href="/" className="text-primary underline underline-offset-4">
                        Browse the collection
                    </Link>
                    .
                </p>
            ) : (
                <ul className="mt-12 divide-y divide-border">
                    {orders.map((order) => {
                        const badge = STATUS_BADGE[order.status];
                        return (
                            <li key={order.id} className="py-5">
                                <Link
                                    href={`/orders/${order.id}`}
                                    className="flex items-center justify-between text-foreground"
                                >
                                    <div>
                                        <p className="font-heading text-base">Order #{order.id}</p>
                                        <div className="mt-1 flex items-center gap-2 text-sm text-muted-foreground">
                                            <span>{new Date(order.createdAt).toLocaleDateString()}</span>
                                            {badge && <Badge variant={badge.variant}>{badge.label}</Badge>}
                                        </div>
                                    </div>
                                    <span className="font-heading tabular-nums">
                                        {currencyFormatter.format(order.total)}
                                    </span>
                                </Link>
                            </li>
                        );
                    })}
                </ul>
            )}
        </main>
    );
}
