"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { ProductCartControls } from "@/components/product-cart-controls";
import { ProductPrice } from "@/components/product-price";

type ProductSize = {
    id: number;
    label: string;
    price: number;
    priceUsd: number;
};

type Product = {
    id: number;
    name: string;
    description: string | null;
    imageUrl: string | null;
    categoryName: string | null;
    sizes: ProductSize[];
};

export default function ProductDetailPage() {
    const { id } = useParams<{ id: string }>();
    const [product, setProduct] = useState<Product | null>(null);
    const [sizeId, setSizeId] = useState<number | null>(null);
    const [notFound, setNotFound] = useState(false);
    const [zoomOpen, setZoomOpen] = useState(false);

    useEffect(() => {
        fetch(`/api/products/${id}`).then((res) => {
            if (!res.ok) {
                setNotFound(true);
                return;
            }
            return res.json().then((loaded: Product) => {
                setProduct(loaded);
                setSizeId(loaded.sizes[0]?.id ?? null);
            });
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

    const selectedSize = product.sizes.find((s) => s.id === sizeId) ?? null;

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
                    {product.sizes.length > 0 && (
                        <fieldset className="mt-8">
                            <legend className="gallery-eyebrow">Canvas size</legend>
                            <div className="mt-3 flex flex-wrap gap-2">
                                {product.sizes.map((size) => (
                                    <button
                                        key={size.id}
                                        type="button"
                                        onClick={() => setSizeId(size.id)}
                                        aria-pressed={size.id === sizeId}
                                        className={`rounded-full border px-4 py-1.5 text-sm tabular-nums transition-colors ${
                                            size.id === sizeId
                                                ? "border-foreground bg-foreground text-background"
                                                : "border-border text-foreground hover:bg-muted"
                                        }`}
                                    >
                                        {size.label}
                                    </button>
                                ))}
                            </div>
                        </fieldset>
                    )}
                    {selectedSize && (
                        <>
                            <ProductPrice
                                price={selectedSize.price}
                                priceUsd={selectedSize.priceUsd}
                                className="mt-6 block font-heading text-2xl text-foreground tabular-nums"
                            />
                            <div className="mt-6">
                                <ProductCartControls productId={product.id} sizeId={selectedSize.id} />
                            </div>
                        </>
                    )}
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
