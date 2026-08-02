"use client";

import Link from "next/link";
import { ShoppingCart } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { useCart } from "@/lib/cart-context";
import { useAuth } from "@/lib/auth-context";

export function SiteHeader() {
    const { itemCount } = useCart();
    const { user, loading, logout } = useAuth();

    return (
        <header className="border-b border-border">
            <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
                <Link href="/" className="font-heading text-lg font-semibold text-foreground">
                    Wall Art Canvases
                </Link>
                <div className="flex items-center gap-6">
                    {!loading && (
                        user ? (
                            <div className="flex items-center gap-3">
                                <Link href="/orders" className="text-sm text-foreground">
                                    Orders
                                </Link>
                                <span className="text-sm text-muted-foreground">{user.email}</span>
                                <Button variant="outline" size="sm" onClick={() => logout()}>
                                    Sign out
                                </Button>
                            </div>
                        ) : (
                            <Link href="/login" className="text-sm text-foreground">
                                Sign in
                            </Link>
                        )
                    )}
                    <Link href="/cart" className="relative flex items-center gap-2 text-foreground">
                        <ShoppingCart className="size-5" />
                        {itemCount > 0 && (
                            <Badge className="absolute -right-2 -top-2">{itemCount}</Badge>
                        )}
                    </Link>
                </div>
            </div>
        </header>
    );
}
