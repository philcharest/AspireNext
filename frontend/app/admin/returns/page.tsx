"use client";

import { useEffect, useState } from "react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { extractError } from "@/lib/api-error";
import { apiFetch } from "@/lib/csrf";

type ReturnItem = {
    orderItemId: number;
    productName: string;
    quantity: number;
};

type AdminReturn = {
    id: number;
    orderId: number;
    userEmail: string;
    requestedAt: string;
    status: "Requested" | "Approved" | "Rejected";
    reason: string;
    reviewNote: string | null;
    refundAmount: number | null;
    items: ReturnItem[];
};

const currencyFormatter = new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
});

const STATUS_BADGE: Record<string, { variant: "default" | "outline" | "destructive"; label: string }> = {
    Requested: { variant: "outline", label: "Requested" },
    Approved: { variant: "default", label: "Approved" },
    Rejected: { variant: "destructive", label: "Rejected" },
};

export default function AdminReturnsPage() {
    const [returns, setReturns] = useState<AdminReturn[] | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [processingId, setProcessingId] = useState<number | null>(null);

    function loadReturns() {
        return fetch("/api/admin/returns")
            .then((res) => (res.ok ? res.json() : []))
            .then(setReturns);
    }

    useEffect(() => {
        loadReturns();
    }, []);

    async function handleApprove(id: number) {
        setProcessingId(id);
        setError(null);
        const res = await apiFetch(`/api/admin/returns/${id}/approve`, { method: "POST" });
        if (!res.ok) {
            setError(await extractError(res));
        } else {
            await loadReturns();
        }
        setProcessingId(null);
    }

    async function handleReject(id: number) {
        setProcessingId(id);
        setError(null);
        const res = await apiFetch(`/api/admin/returns/${id}/reject`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ note: null }),
        });
        if (!res.ok) {
            setError(await extractError(res));
        } else {
            await loadReturns();
        }
        setProcessingId(null);
    }

    if (returns === null) {
        return <p className="text-muted-foreground">Loading...</p>;
    }

    if (returns.length === 0) {
        return <p className="text-muted-foreground">No return requests yet.</p>;
    }

    return (
        <div>
            {error && <p className="mb-6 text-sm text-destructive">{error}</p>}
            <ul className="divide-y divide-border">
                {returns.map((ret) => {
                    const badge = STATUS_BADGE[ret.status];
                    return (
                        <li key={ret.id} className="py-6">
                            <div className="flex items-center justify-between">
                                <div>
                                    <p className="font-heading text-base text-foreground">
                                        Return for Order #{ret.orderId}
                                    </p>
                                    <div className="mt-1 flex items-center gap-2 text-sm text-muted-foreground">
                                        <span>{ret.userEmail}</span>
                                        <span>&middot;</span>
                                        <span>{new Date(ret.requestedAt).toLocaleDateString()}</span>
                                        {badge && <Badge variant={badge.variant}>{badge.label}</Badge>}
                                    </div>
                                </div>
                                {ret.refundAmount !== null && (
                                    <span className="font-heading tabular-nums text-foreground">
                                        {currencyFormatter.format(ret.refundAmount)}
                                    </span>
                                )}
                            </div>

                            <ul className="mt-3 space-y-1 text-sm text-muted-foreground">
                                {ret.items.map((item) => (
                                    <li key={item.orderItemId}>
                                        {item.quantity} &times; {item.productName}
                                    </li>
                                ))}
                            </ul>

                            <p className="mt-2 text-sm text-muted-foreground">
                                <span className="text-foreground">Reason: </span>
                                {ret.reason}
                            </p>
                            {ret.reviewNote && (
                                <p className="mt-1 text-sm text-muted-foreground">
                                    <span className="text-foreground">Note: </span>
                                    {ret.reviewNote}
                                </p>
                            )}

                            {ret.status === "Requested" && (
                                <div className="mt-4 flex gap-2">
                                    <Button
                                        size="sm"
                                        disabled={processingId === ret.id}
                                        onClick={() => handleApprove(ret.id)}
                                    >
                                        {processingId === ret.id ? "Processing..." : "Approve & Refund"}
                                    </Button>
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        disabled={processingId === ret.id}
                                        onClick={() => handleReject(ret.id)}
                                    >
                                        Reject
                                    </Button>
                                </div>
                            )}
                        </li>
                    );
                })}
            </ul>
        </div>
    );
}
