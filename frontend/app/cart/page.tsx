"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
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
    const { cart, loading, updateQuantity, removeItem, checkout } = useCart();
    const { user, loading: authLoading } = useAuth();
    const router = useRouter();
    const [checkingOut, setCheckingOut] = useState(false);
    const [error, setError] = useState<string | null>(null);

    async function handleCheckout() {
        setCheckingOut(true);
        setError(null);

        const result = await checkout();
        if (result.ok) {
            router.push(`/orders/${result.orderId}`);
        } else {
            setError(result.error);
            setCheckingOut(false);
        }
    }

    return (
        <main className="mx-auto max-w-3xl px-6 py-16">
            <h1 className="font-heading text-3xl font-semibold tracking-tight text-foreground">
                Your Cart
            </h1>

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
                    <ul className="mt-10 divide-y divide-border">
                        {cart.items.map((item) => (
                            <li key={item.productId} className="flex items-center gap-4 py-4">
                                {item.imageUrl && (
                                    // eslint-disable-next-line @next/next/no-img-element
                                    <img
                                        src={item.imageUrl}
                                        alt={item.name}
                                        className="size-20 rounded-md object-cover"
                                    />
                                )}
                                <div className="flex-1">
                                    <p className="font-medium text-foreground">{item.name}</p>
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
                                <span className="w-20 text-right font-medium text-foreground">
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
                        <span className="text-lg font-semibold text-foreground">Total</span>
                        <span className="text-lg font-semibold text-foreground">
                            {currencyFormatter.format(cart.total)}
                        </span>
                    </div>

                    {error && <p className="mt-4 text-sm text-destructive">{error}</p>}

                    <div className="mt-6 flex justify-end">
                        {!authLoading && (
                            user ? (
                                <Button onClick={handleCheckout} disabled={checkingOut}>
                                    {checkingOut ? "Placing order..." : "Checkout"}
                                </Button>
                            ) : (
                                <Button render={<Link href="/login" />}>Sign in to checkout</Button>
                            )
                        )}
                    </div>
                </>
            )}
        </main>
    );
}
