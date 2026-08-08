"use client";

import { Suspense, useState } from "react";
import { useSearchParams } from "next/navigation";
import Link from "next/link";
import { Minus, Plus, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useCart } from "@/lib/cart-context";
import { useAuth } from "@/lib/auth-context";

const currencyFormatter = new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
});

export default function CartPage() {
    return (
        <Suspense>
            <CartPageContent />
        </Suspense>
    );
}

function CartPageContent() {
    const { cart, loading, updateQuantity, removeItem, checkout } = useCart();
    const { user, loading: authLoading } = useAuth();
    const searchParams = useSearchParams();
    const canceled = searchParams.get("canceled") === "true";
    const [checkingOut, setCheckingOut] = useState(false);
    const [error, setError] = useState<string | null>(null);

    async function handleCheckout() {
        setCheckingOut(true);
        setError(null);

        const result = await checkout();
        if (result.ok) {
            window.location.href = result.checkoutUrl;
        } else {
            setError(result.error);
            setCheckingOut(false);
        }
    }

    return (
        <main className="mx-auto max-w-3xl px-6 py-24">
            <p className="gallery-eyebrow">Your Selections</p>
            <h1 className="mt-3 font-heading text-4xl font-medium tracking-tight text-foreground">
                Your Cart
            </h1>

            {canceled && (
                <p className="mt-6 rounded-md border border-border bg-muted px-4 py-3 text-sm text-muted-foreground">
                    Checkout canceled — your cart is still here.
                </p>
            )}

            {loading ? (
                <p className="mt-8 text-muted-foreground">Loading...</p>
            ) : cart.items.length === 0 ? (
                <p className="mt-8 text-muted-foreground">
                    Your cart is empty.{" "}
                    <Link href="/" className="text-primary underline underline-offset-4">
                        Browse the collection
                    </Link>
                    .
                </p>
            ) : (
                <>
                    <ul className="mt-12 divide-y divide-border">
                        {cart.items.map((item) => (
                            <li key={item.productId} className="flex items-center gap-5 py-6">
                                {item.imageUrl && (
                                    <div className="gallery-frame w-20 shrink-0">
                                        {/* eslint-disable-next-line @next/next/no-img-element */}
                                        <img
                                            src={item.imageUrl}
                                            alt={item.name}
                                            className="aspect-4/5 size-full object-cover"
                                        />
                                    </div>
                                )}
                                <div className="flex-1">
                                    <p className="font-heading text-base text-foreground">{item.name}</p>
                                    <p className="text-sm text-muted-foreground">
                                        {currencyFormatter.format(item.price)} each
                                    </p>
                                </div>
                                <div className="flex items-center gap-2">
                                    <Button
                                        variant="outline"
                                        size="icon-sm"
                                        onClick={() => updateQuantity(item.productId, item.quantity - 1)}
                                    >
                                        <Minus />
                                    </Button>
                                    <span className="w-6 text-center text-foreground">{item.quantity}</span>
                                    <Button
                                        variant="outline"
                                        size="icon-sm"
                                        onClick={() => updateQuantity(item.productId, item.quantity + 1)}
                                    >
                                        <Plus />
                                    </Button>
                                </div>
                                <span className="w-20 text-right font-heading text-foreground tabular-nums">
                                    {currencyFormatter.format(item.lineTotal)}
                                </span>
                                <Button
                                    variant="ghost"
                                    size="icon-sm"
                                    onClick={() => removeItem(item.productId)}
                                    aria-label={`Remove ${item.name}`}
                                >
                                    <X />
                                </Button>
                            </li>
                        ))}
                    </ul>

                    <div className="mt-8 flex items-center justify-between border-t border-border pt-6">
                        <span className="font-heading text-lg text-foreground">Total</span>
                        <span className="font-heading text-lg text-foreground tabular-nums">
                            {currencyFormatter.format(cart.total)}
                        </span>
                    </div>

                    {error && <p className="mt-4 text-sm text-destructive">{error}</p>}

                    <div className="mt-6 flex justify-end">
                        {!authLoading && (
                            user ? (
                                <Button onClick={handleCheckout} disabled={checkingOut} size="lg">
                                    {checkingOut ? "Placing order..." : "Checkout"}
                                </Button>
                            ) : (
                                <Button render={<Link href="/login" />} size="lg">Sign in to checkout</Button>
                            )
                        )}
                    </div>
                </>
            )}
        </main>
    );
}
