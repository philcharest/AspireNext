"use client";

import { useEffect, useState } from "react";
import { Badge } from "@/components/ui/badge";

type AdminOrder = {
    id: number;
    createdAt: string;
    status: string;
    total: number;
    userEmail: string;
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

export default function AdminOrdersPage() {
    const [orders, setOrders] = useState<AdminOrder[] | null>(null);

    useEffect(() => {
        fetch("/api/admin/orders")
            .then((res) => (res.ok ? res.json() : []))
            .then(setOrders);
    }, []);

    if (orders === null) {
        return <p className="text-muted-foreground">Loading...</p>;
    }

    if (orders.length === 0) {
        return <p className="text-muted-foreground">No orders yet.</p>;
    }

    return (
        <ul className="divide-y divide-border">
            {orders.map((order) => {
                const badge = STATUS_BADGE[order.status];
                return (
                    <li key={order.id} className="flex items-center justify-between py-5">
                        <div>
                            <p className="font-heading text-base text-foreground">Order #{order.id}</p>
                            <div className="mt-1 flex items-center gap-2 text-sm text-muted-foreground">
                                <span>{order.userEmail}</span>
                                <span>&middot;</span>
                                <span>{new Date(order.createdAt).toLocaleDateString()}</span>
                                {badge && <Badge variant={badge.variant}>{badge.label}</Badge>}
                            </div>
                        </div>
                        <span className="font-heading tabular-nums text-foreground">
                            {currencyFormatter.format(order.total)}
                        </span>
                    </li>
                );
            })}
        </ul>
    );
}
