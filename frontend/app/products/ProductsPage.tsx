'use client';

import { ProductList } from './ProductList';
import { useProducts } from './useProducts';
import { Navigation } from '../components/Navigation';

export function ProductsPage() {
  const { products, loading, error } = useProducts();

  return (
    <>
      <Navigation />
      <ProductList 
        products={products} 
        loading={loading} 
        error={error} 
      />
    </>
  );
}
