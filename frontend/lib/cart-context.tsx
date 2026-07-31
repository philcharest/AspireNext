"use client";

import { createContext, useCallback, useContext, useEffect, useState } from "react";

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

type CartContextValue = {
    cart: Cart;
    itemCount: number;
    loading: boolean;
    addItem: (productId: number, quantity?: number) => Promise<void>;
    updateQuantity: (productId: number, quantity: number) => Promise<void>;
    removeItem: (productId: number) => Promise<void>;
};

const CartContext = createContext<CartContextValue | null>(null);

export function CartProvider({ children }: { children: React.ReactNode }) {
    const [cart, setCart] = useState<Cart>(EMPTY_CART);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        fetch("/api/cart")
            .then((res) => (res.ok ? res.json() : EMPTY_CART))
            .then(setCart)
            .finally(() => setLoading(false));
    }, []);

    const addItem = useCallback(async (productId: number, quantity = 1) => {
        const res = await fetch("/api/cart/items", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ productId, quantity }),
        });
        if (res.ok) setCart(await res.json());
    }, []);

    const updateQuantity = useCallback(async (productId: number, quantity: number) => {
        const res = await fetch(`/api/cart/items/${productId}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ quantity }),
        });
        if (res.ok) setCart(await res.json());
    }, []);

    const removeItem = useCallback(async (productId: number) => {
        const res = await fetch(`/api/cart/items/${productId}`, { method: "DELETE" });
        if (res.ok) setCart(await res.json());
    }, []);

    const itemCount = cart.items.reduce((sum, item) => sum + item.quantity, 0);

    return (
        <CartContext.Provider value={{ cart, itemCount, loading, addItem, updateQuantity, removeItem }}>
            {children}
        </CartContext.Provider>
    );
}

export function useCart() {
    const context = useContext(CartContext);
    if (!context) throw new Error("useCart must be used within a CartProvider");
    return context;
}
