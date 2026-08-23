"use client";

import { useState } from "react";
import { Check, Minus, Plus, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useCart } from "@/lib/cart-context";

export function ProductCartControls({ productId }: { productId: number }) {
    const { cart, addItem, updateQuantity, removeItem } = useCart();
    const [status, setStatus] = useState<"idle" | "adding" | "added">("idle");

    const item = cart.items.find((i) => i.productId === productId);

    async function handleAdd() {
        setStatus("adding");
        await addItem(productId, 1);
        setStatus("added");
        setTimeout(() => setStatus("idle"), 1500);
    }

    if (!item) {
        return (
            <Button onClick={handleAdd} disabled={status === "adding"} size="lg">
                {status === "added" ? (
                    <>
                        <Check /> Added
                    </>
                ) : (
                    "Add to Cart"
                )}
            </Button>
        );
    }

    return (
        <div className="flex items-center gap-4">
            <div className="flex items-center gap-2">
                <Button
                    variant="outline"
                    size="icon-sm"
                    onClick={() => updateQuantity(productId, item.quantity - 1)}
                    aria-label="Decrease quantity"
                >
                    <Minus />
                </Button>
                <span className="w-6 text-center text-foreground">{item.quantity}</span>
                <Button
                    variant="outline"
                    size="icon-sm"
                    onClick={() => updateQuantity(productId, item.quantity + 1)}
                    aria-label="Increase quantity"
                >
                    <Plus />
                </Button>
            </div>
            <Button variant="ghost" size="sm" onClick={() => removeItem(productId)}>
                <X /> Remove
            </Button>
        </div>
    );
}
