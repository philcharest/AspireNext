"use client";

import { createContext, useCallback, useContext, useEffect, useState } from "react";
import { extractError } from "@/lib/api-error";
import { useAuth } from "@/lib/auth-context";
import { apiFetch } from "@/lib/csrf";

export type CartItem = {
    productId: number;
    name: string;
    imageUrl: string | null;
    price: number;
    quantity: number;
    lineTotal: number;
};

export type Cart = {
    items: CartItem[];
    total: number;
};

const EMPTY_CART: Cart = { items: [], total: 0 };

type CheckoutResult = { ok: true; checkoutUrl: string } | { ok: false; error: string };

type CartContextValue = {
    cart: Cart;
    itemCount: number;
    loading: boolean;
    addItem: (productId: number, quantity?: number) => Promise<void>;
    updateQuantity: (productId: number, quantity: number) => Promise<void>;
    removeItem: (productId: number) => Promise<void>;
    checkout: () => Promise<CheckoutResult>;
};

const CartContext = createContext<CartContextValue | null>(null);

export function CartProvider({ children }: { children: React.ReactNode }) {
    const { user } = useAuth();
    const [cart, setCart] = useState<Cart>(EMPTY_CART);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        fetch("/api/cart")
            .then((res) => (res.ok ? res.json() : EMPTY_CART))
            .then(setCart)
            .finally(() => setLoading(false));
    }, []);

    // Fold any anonymous-session cart items into the signed-in user's cart. Cheap no-op
    // when there's nothing to merge (e.g. on every already-logged-in page load).
    useEffect(() => {
        if (!user) return;
        apiFetch("/api/cart/merge", { method: "POST" })
            .then((res) => (res.ok ? res.json() : null))
            .then((merged) => {
                if (merged) setCart(merged);
            });
    }, [user]);

    const addItem = useCallback(async (productId: number, quantity = 1) => {
        const res = await apiFetch("/api/cart/items", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ productId, quantity }),
        });
        if (res.ok) setCart(await res.json());
    }, []);

    const updateQuantity = useCallback(async (productId: number, quantity: number) => {
        const res = await apiFetch(`/api/cart/items/${productId}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ quantity }),
        });
        if (res.ok) setCart(await res.json());
    }, []);

    const removeItem = useCallback(async (productId: number) => {
        const res = await apiFetch(`/api/cart/items/${productId}`, { method: "DELETE" });
        if (res.ok) setCart(await res.json());
    }, []);

    const checkout = useCallback(async (): Promise<CheckoutResult> => {
        const res = await apiFetch("/api/checkout", { method: "POST" });
        if (!res.ok) return { ok: false, error: await extractError(res) };
        const { checkoutUrl } = await res.json();
        return { ok: true, checkoutUrl };
    }, []);

    const itemCount = cart.items.reduce((sum, item) => sum + item.quantity, 0);

    return (
        <CartContext.Provider value={{ cart, itemCount, loading, addItem, updateQuantity, removeItem, checkout }}>
            {children}
        </CartContext.Provider>
    );
}

export function useCart() {
    const context = useContext(CartContext);
    if (!context) throw new Error("useCart must be used within a CartProvider");
    return context;
}
