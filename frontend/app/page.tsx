import { Badge } from "@/components/ui/badge";
import {
    Card,
    CardDescription,
    CardFooter,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
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

export default async function Home() {
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

    return (
        <div className="min-h-screen bg-background">
            <main className="mx-auto max-w-6xl px-6 py-16">
                <h1 className="font-heading text-3xl font-semibold tracking-tight text-foreground">
                    Wall Art Canvases
                </h1>
                <p className="mt-2 text-muted-foreground">
                    Original canvas prints across Japandi, earth-tone, and abstract collections.
                </p>

                {error ? (
                    <p className="mt-8 text-destructive">
                        Could not load products: {error}
                    </p>
                ) : products.length === 0 ? (
                    <p className="mt-8 text-muted-foreground">No products yet.</p>
                ) : (
                    <ul className="mt-10 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
                        {products.map((product) => (
                            <li key={product.id}>
                                <Card className="h-full">
                                    {product.imageUrl && (
                                        // eslint-disable-next-line @next/next/no-img-element
                                        <img
                                            src={product.imageUrl}
                                            alt={product.name}
                                            className="aspect-4/5 w-full object-cover"
                                        />
                                    )}
                                    <CardHeader>
                                        {product.categoryName && (
                                            <Badge variant="secondary">{product.categoryName}</Badge>
                                        )}
                                        <CardTitle>{product.name}</CardTitle>
                                        {product.description && (
                                            <CardDescription>{product.description}</CardDescription>
                                        )}
                                    </CardHeader>
                                    <CardFooter className="mt-auto flex-col items-stretch gap-3">
                                        <span className="text-base font-semibold text-foreground">
                                            {currencyFormatter.format(product.price)}
                                        </span>
                                        <AddToCartButton productId={product.id} />
                                    </CardFooter>
                                </Card>
                            </li>
                        ))}
                    </ul>
                )}
            </main>
        </div>
    );
}
