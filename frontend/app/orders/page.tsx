"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
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
        <main className="mx-auto max-w-3xl px-6 py-16">
            <h1 className="font-heading text-3xl font-semibold tracking-tight text-foreground">
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
                <ul className="mt-10 divide-y divide-border">
                    {orders.map((order) => (
                        <li key={order.id} className="py-4">
                            <Link
                                href={`/orders/${order.id}`}
                                className="flex items-center justify-between text-foreground"
                            >
                                <div>
                                    <p className="font-medium">Order #{order.id}</p>
                                    <p className="text-sm text-muted-foreground">
                                        {new Date(order.createdAt).toLocaleDateString()} &middot; {order.status}
                                    </p>
                                </div>
                                <span className="font-semibold">{currencyFormatter.format(order.total)}</span>
                            </Link>
                        </li>
                    ))}
                </ul>
            )}
        </main>
    );
}
