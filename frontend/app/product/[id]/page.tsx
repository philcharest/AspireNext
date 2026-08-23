"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { ProductCartControls } from "@/components/product-cart-controls";

type Product = {
    id: number;
    name: string;
    description: string | null;
    imageUrl: string | null;
    price: number;
    categoryName: string | null;
};

const currencyFormatter = new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
});

export default function ProductDetailPage() {
    const { id } = useParams<{ id: string }>();
    const [product, setProduct] = useState<Product | null>(null);
    const [notFound, setNotFound] = useState(false);
    const [zoomOpen, setZoomOpen] = useState(false);

    useEffect(() => {
        fetch(`/api/products/${id}`).then((res) => {
            if (res.ok) return res.json().then(setProduct);
            setNotFound(true);
        });
    }, [id]);

    useEffect(() => {
        if (!zoomOpen) return;
        function handleKeyDown(e: KeyboardEvent) {
            if (e.key === "Escape") setZoomOpen(false);
        }
        window.addEventListener("keydown", handleKeyDown);
        return () => window.removeEventListener("keydown", handleKeyDown);
    }, [zoomOpen]);

    if (notFound) {
        return (
            <main className="mx-auto max-w-2xl px-6 py-24">
                <p className="text-muted-foreground">
                    Product not found.{" "}
                    <Link href="/" className="text-primary underline underline-offset-4">
                        Browse the collection
                    </Link>
                    .
                </p>
            </main>
        );
    }

    if (!product) {
        return (
            <main className="mx-auto max-w-5xl px-6 py-24">
                <p className="text-muted-foreground">Loading...</p>
            </main>
        );
    }

    return (
        <main className="mx-auto max-w-5xl px-6 py-24">
            <div className="grid grid-cols-1 gap-12 md:grid-cols-2">
                {product.imageUrl && (
                    <button
                        type="button"
                        onClick={() => setZoomOpen(true)}
                        className="gallery-frame block aspect-4/5 w-full cursor-zoom-in"
                        aria-label={`Zoom in on ${product.name}`}
                    >
                        {/* eslint-disable-next-line @next/next/no-img-element */}
                        <img
                            src={product.imageUrl}
                            alt={product.name}
                            className="size-full object-cover"
                        />
                    </button>
                )}

                <div>
                    {product.categoryName && <p className="gallery-eyebrow">{product.categoryName}</p>}
                    <h1 className="mt-3 font-heading text-4xl font-medium tracking-tight text-foreground">
                        {product.name}
                    </h1>
                    {product.description && (
                        <p className="mt-4 text-muted-foreground">{product.description}</p>
                    )}
                    <p className="mt-8 font-heading text-2xl text-foreground tabular-nums">
                        {currencyFormatter.format(product.price)}
                    </p>
                    <div className="mt-6">
                        <ProductCartControls productId={product.id} />
                    </div>
                </div>
            </div>

            {zoomOpen && product.imageUrl && (
                <div
                    role="dialog"
                    aria-modal="true"
                    aria-label={`${product.name}, enlarged`}
                    className="fixed inset-0 z-50 flex cursor-zoom-out items-center justify-center bg-black/80 p-6"
                    onClick={() => setZoomOpen(false)}
                >
                    {/* eslint-disable-next-line @next/next/no-img-element */}
                    <img
                        src={product.imageUrl}
                        alt={product.name}
                        className="max-h-full max-w-full rounded-sm object-contain shadow-2xl"
                    />
                </div>
            )}
        </main>
    );
}
