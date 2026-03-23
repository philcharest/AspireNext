import Link from 'next/link';
import styles from './products.module.css';

interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
}

interface ProductListProps {
  products: Product[];
  loading: boolean;
  error: string | null;
}

export function ProductList({ products, loading, error }: ProductListProps) {
  return (
    <div className={styles.container}>
      <div className={styles.wrapper}>
        <div className={styles.header}>
          <h1 className={styles.title}>Products</h1>
          <Link href="/" className={styles.backButton}>
            Back Home
          </Link>
        </div>

        {loading && (
          <div className={styles.centerContent}>
            <p className={styles.loadingText}>Loading products...</p>
          </div>
        )}

        {error && (
          <div className={styles.errorBox}>
            <p>
              <strong>Error:</strong> {error}
            </p>
          </div>
        )}

        {!loading && !error && products.length === 0 && (
          <div className={styles.centerContent}>
            <p className={styles.emptyText}>No products available at the moment.</p>
          </div>
        )}

        {!loading && !error && products.length > 0 && (
          <div className={styles.grid}>
            {products.map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

function ProductCard({ product }: { product: Product }) {
  return (
    <div className={styles.card}>
      <h2 className={styles.cardTitle}>{product.name}</h2>
      <p className={styles.cardDescription}>{product.description}</p>
      <p className={styles.cardPrice}>${product.price.toFixed(2)}</p>
    </div>
  );
}
