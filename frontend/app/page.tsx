import Link from "next/link";
import { AddToCartButton } from "@/components/add-to-cart-button";

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

export default async function Home({
    searchParams,
}: {
    searchParams: Promise<{ collection?: string }>;
}) {
    // Aspire injects the URL based on the name "server" used in AppHost.cs
    const serverUrl =
        process.env["services__server__http__0"] ||
        process.env["services__server__https__0"];

    let products: Product[] = [];
    let error: string | null = null;

    try {
        const response = await fetch(`${serverUrl}/api/products`, {
            cache: "no-store",
        });

        if (!response.ok) throw new Error(`Server responded with ${response.status}`);

        products = await response.json();
    } catch (e) {
        error = e instanceof Error ? e.message : String(e);
    }

    const { collection: selectedCollection } = await searchParams;
    const collections = [...new Set(products.map((p) => p.categoryName).filter((c) => c !== null))];
    const visibleProducts = selectedCollection
        ? products.filter((p) => p.categoryName === selectedCollection)
        : products;

    return (
        <div className="min-h-screen bg-background">
            <main className="mx-auto max-w-6xl px-6 py-24">
                <p className="gallery-eyebrow">The Collection</p>
                <h1 className="mt-3 font-heading text-5xl font-medium tracking-tight text-foreground">
                    Wall Art Canvases
                </h1>
                <p className="mt-4 max-w-xl text-muted-foreground">
                    Original canvas prints across Japandi, earth-tone, and abstract collections.
                </p>
                {collections.length > 0 && (
                    <div className="mt-8 flex flex-wrap gap-2">
                        <Link
                            href="/"
                            className={`rounded-full border px-4 py-1.5 text-sm transition-colors ${
                                !selectedCollection
                                    ? "border-foreground bg-foreground text-background"
                                    : "border-border text-foreground hover:bg-muted"
                            }`}
                        >
                            All
                        </Link>
                        {collections.map((collection) => (
                            <Link
                                key={collection}
                                href={`/?collection=${encodeURIComponent(collection!)}`}
                                className={`rounded-full border px-4 py-1.5 text-sm transition-colors ${
                                    selectedCollection === collection
                                        ? "border-foreground bg-foreground text-background"
                                        : "border-border text-foreground hover:bg-muted"
                                }`}
                            >
                                {collection}
                            </Link>
                        ))}
                    </div>
                )}

                <div className="mt-10 border-t border-border" />

                {error ? (
                    <p className="mt-8 text-destructive">
                        Could not load products: {error}
                    </p>
                ) : visibleProducts.length === 0 ? (
                    <p className="mt-8 text-muted-foreground">No products in this collection yet.</p>
                ) : (
                    <ul className="mt-16 grid grid-cols-1 gap-x-8 gap-y-14 sm:grid-cols-2 lg:grid-cols-3">
                        {visibleProducts.map((product) => (
                            <li key={product.id}>
                                <Link href={`/product/${product.id}`} className="block">
                                    {product.imageUrl && (
                                        <div className="gallery-frame aspect-4/5">
                                            {/* eslint-disable-next-line @next/next/no-img-element */}
                                            <img
                                                src={product.imageUrl}
                                                alt={product.name}
                                                className="size-full object-cover"
                                            />
                                        </div>
                                    )}
                                    <div className="mt-4 space-y-1">
                                        {product.categoryName && (
                                            <p className="gallery-eyebrow">{product.categoryName}</p>
                                        )}
                                        <h2 className="font-heading text-lg text-foreground">{product.name}</h2>
                                        {product.description && (
                                            <p className="text-sm text-muted-foreground">{product.description}</p>
                                        )}
                                    </div>
                                </Link>
                                <div className="mt-4 flex items-center justify-between gap-4">
                                    <span className="font-heading text-base text-foreground tabular-nums">
                                        {currencyFormatter.format(product.price)}
                                    </span>
                                    <AddToCartButton productId={product.id} />
                                </div>
                            </li>
                        ))}
                    </ul>
                )}
            </main>
        </div>
    );
}
