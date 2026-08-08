"use client";

import { useState } from "react";
import { Check } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useCart } from "@/lib/cart-context";

export function AddToCartButton({ productId }: { productId: number }) {
    const { addItem } = useCart();
    const [status, setStatus] = useState<"idle" | "adding" | "added">("idle");

    async function handleClick() {
        setStatus("adding");
        await addItem(productId, 1);
        setStatus("added");
        setTimeout(() => setStatus("idle"), 1500);
    }

    return (
        <Button onClick={handleClick} disabled={status === "adding"} size="lg">
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
