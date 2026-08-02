"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { useAuth } from "@/lib/auth-context";

type OrderItem = {
    productId: number;
    productName: string;
    price: number;
    quantity: number;
    lineTotal: number;
};

type Order = {
    id: number;
    createdAt: string;
    status: string;
    items: OrderItem[];
    total: number;
};

const currencyFormatter = new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
});

export default function OrderDetailPage() {
    const { id } = useParams<{ id: string }>();
    const { user, loading: authLoading } = useAuth();
    const router = useRouter();
    const [order, setOrder] = useState<Order | null>(null);
    const [notFound, setNotFound] = useState(false);

    useEffect(() => {
        if (!authLoading && !user) {
            router.push("/login");
        }
    }, [authLoading, user, router]);

    useEffect(() => {
        if (!user) return;
        fetch(`/api/orders/${id}`).then((res) => {
            if (res.ok) return res.json().then(setOrder);
            setNotFound(true);
        });
    }, [user, id]);

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

    return (
        <main className="mx-auto max-w-2xl px-6 py-16">
            <h1 className="font-heading text-3xl font-semibold tracking-tight text-foreground">
                Thank you for your order!
            </h1>
            <p className="mt-2 text-muted-foreground">
                Order #{order.id} placed on {new Date(order.createdAt).toLocaleDateString()}
            </p>

            <ul className="mt-10 divide-y divide-border">
                {order.items.map((item) => (
                    <li key={item.productId} className="flex items-center justify-between py-4">
                        <div>
                            <p className="font-medium text-foreground">{item.productName}</p>
                            <p className="text-sm text-muted-foreground">
                                {currencyFormatter.format(item.price)} &times; {item.quantity}
                            </p>
                        </div>
                        <span className="font-semibold text-foreground">
                            {currencyFormatter.format(item.lineTotal)}
                        </span>
                    </li>
                ))}
            </ul>

            <div className="mt-8 flex items-center justify-between border-t border-border pt-6">
                <span className="text-lg font-semibold text-foreground">Total</span>
                <span className="text-lg font-semibold text-foreground">
                    {currencyFormatter.format(order.total)}
                </span>
            </div>

            <p className="mt-8 text-sm text-muted-foreground">
                <Link href="/orders" className="text-primary underline underline-offset-4">
                    View all orders
                </Link>
            </p>
        </main>
    );
}
