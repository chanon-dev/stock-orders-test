"use client";

import { useEffect, useState } from "react";
import { getProducts, Product } from "@/features/products/api/get-products";
import { ProductCard } from "@/features/products/components/ProductCard";

export default function Home() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getProducts()
      .then(setProducts)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const handleAddToCart = async (productId: string) => {
    // SessionId logic - just using a static one for demo
    const sessionId = "user-123";
    
    try {
      const res = await fetch('http://localhost:5001/api/cart/add', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ productId, quantity: 1, sessionId })
      });
      
      if (res.ok) {
        alert('Added to cart!');
        // Refresh products to see updated stock
        getProducts().then(setProducts);
      } else {
        alert('Failed to add to cart (Check stock)');
      }
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <main className="min-h-screen bg-slate-50 p-8">
      <div className="max-w-7xl mx-auto">
        <header className="mb-12 flex justify-between items-center">
          <div>
            <h1 className="text-4xl font-extrabold text-slate-900 tracking-tight">Stock Pro</h1>
            <p className="text-slate-500 mt-2 text-lg">Modern Inventory & Shopping System</p>
          </div>
          <div className="bg-white p-4 rounded-2xl shadow-sm border border-slate-100 flex items-center gap-4">
             <span className="font-semibold text-slate-700">🛒 Cart: {0} items</span>
             <button className="bg-blue-600 text-white px-6 py-2 rounded-xl font-medium hover:bg-blue-700 transition-all">Check out</button>
          </div>
        </header>

        {loading ? (
          <div className="flex justify-center items-center h-64">
            <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-600"></div>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-8">
            {products.map((product) => (
              <ProductCard 
                key={product.id} 
                product={product} 
                onAddToCart={handleAddToCart}
              />
            ))}
          </div>
        )}
      </div>
    </main>
  );
}
