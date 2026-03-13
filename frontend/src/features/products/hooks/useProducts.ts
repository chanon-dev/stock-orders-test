'use client';

import { useState, useCallback } from 'react';
import { Product, getProducts } from '../api/get-products';

export function useProducts() {
  const [products, setProducts] = useState<Product[]>([]);

  const fetchProducts = useCallback(async () => {
    const data = await getProducts();
    setProducts([...data].sort((a, b) => a.name.localeCompare(b.name)));
  }, []);

  return { products, fetchProducts };
}
