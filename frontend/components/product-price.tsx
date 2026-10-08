"use client";

import { useCurrency } from "@/lib/currency-context";
import { formatPrice } from "@/lib/currency";

export function ProductPrice({
    price,
    priceUsd,
    className,
}: {
    price: number;
    priceUsd: number | null;
    className?: string;
}) {
    const { currency } = useCurrency();
    const amount = currency === "USD" ? priceUsd ?? price : price;

    return <span className={className}>{formatPrice(amount, currency)}</span>;
}
