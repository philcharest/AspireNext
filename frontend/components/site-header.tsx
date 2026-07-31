"use client";

import Link from "next/link";
import { ShoppingCart } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { useCart } from "@/lib/cart-context";

export function SiteHeader() {
    const { itemCount } = useCart();

    return (
        <header className="border-b border-border">
            <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
                <Link href="/" className="font-heading text-lg font-semibold text-foreground">
                    Wall Art Canvases
                </Link>
                <Link href="/cart" className="relative flex items-center gap-2 text-foreground">
                    <ShoppingCart className="size-5" />
                    {itemCount > 0 && (
                        <Badge className="absolute -right-2 -top-2">{itemCount}</Badge>
                    )}
                </Link>
            </div>
        </header>
    );
}
