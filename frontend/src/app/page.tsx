"use client";

import { useEffect, useState } from "react";
import { ProductCard } from "@/features/products/components/ProductCard";
import { CartPanel } from "@/features/cart/components/CartPanel";
import { useProducts } from "@/features/products/hooks/useProducts";
import { useCart } from "@/features/cart/hooks/useCart";
import { CheckoutResult } from "@/features/cart/api/cart-api";

const SESSION_ID = "user-123";

export default function Home() {
  const [loading, setLoading] = useState(true);
  const [checkoutResult, setCheckoutResult] = useState<CheckoutResult | null>(null);
  const [alertModal, setAlertModal] = useState<{ title: string; message: string } | null>(null);

  const { products, fetchProducts } = useProducts();
  const { cartItems, cartTotal, fetchCart, addToCart, updateQuantity, removeItem, clearCart, doCheckout } =
    useCart(SESSION_ID);

  const refresh = () => Promise.all([fetchProducts(), fetchCart()]);

  useEffect(() => {
    refresh().finally(() => setLoading(false));
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  const handleAddToCart = async (productId: string) => {
    try {
      await addToCart(productId);
      await refresh();
    } catch {
      setAlertModal({ title: "ไม่สามารถเพิ่มสินค้าได้", message: "สต็อกสินค้าไม่เพียงพอ" });
    }
  };

  const handleUpdateQuantity = async (cartItemId: string, quantity: number) => {
    try {
      await updateQuantity(cartItemId, quantity, cartItems);
      await refresh();
    } catch {
      setAlertModal({ title: "ไม่สามารถอัพเดทจำนวนสินค้าได้", message: "สต็อกสินค้าไม่เพียงพอ" });
    }
  };

  const handleRemoveItem = async (cartItemId: string) => {
    try {
      await removeItem(cartItemId);
      await refresh();
    } catch (err) {
      console.error(err);
    }
  };

  const handleClearCart = async () => {
    try {
      await clearCart();
      await refresh();
    } catch (err) {
      console.error(err);
    }
  };

  const handleCheckout = async () => {
    if (cartItems.length === 0) return;
    try {
      const result = await doCheckout();
      setCheckoutResult(result);
      await refresh();
    } catch {
      setAlertModal({ title: "ชำระเงินไม่สำเร็จ", message: "กรุณาลองใหม่อีกครั้ง" });
    }
  };

  return (
    <main className="min-h-screen bg-slate-50 p-8">
      <div className="max-w-7xl mx-auto">
        <header className="mb-8">
          <h1 className="text-4xl font-extrabold text-slate-900 tracking-tight">Stock Orders</h1>
        </header>

        <div className="flex gap-8 items-start">
          {/* Product Grid */}
          <div className="flex-1">
            <h2 className="text-xl font-semibold text-slate-700 mb-4">สินค้าทั้งหมด</h2>
            {loading ? (
              <div className="flex justify-center items-center h-64">
                <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-600"></div>
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
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

          {/* Cart Panel */}
          <div className="w-96 flex-shrink-0 sticky top-8">
            <h2 className="text-xl font-semibold text-slate-700 mb-4">
              🛒 ตะกร้าสินค้า ({cartItems.reduce((s, i) => s + i.quantity, 0)} ชิ้น)
            </h2>
            <CartPanel
              items={cartItems}
              total={cartTotal}
              onUpdateQuantity={handleUpdateQuantity}
              onRemoveItem={handleRemoveItem}
              onClearCart={handleClearCart}
              onCheckout={handleCheckout}
            />
          </div>
        </div>
      </div>

      {/* Alert Modal */}
      {alertModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-2xl shadow-xl p-8 max-w-sm w-full mx-4 text-center">
            <div className="text-5xl mb-4">⚠️</div>
            <h2 className="text-xl font-bold text-slate-900 mb-2">{alertModal.title}</h2>
            <p className="text-slate-500 mb-6">{alertModal.message}</p>
            <button
              onClick={() => setAlertModal(null)}
              className="w-full bg-slate-900 text-white py-3 rounded-xl font-semibold hover:bg-slate-700 transition-colors"
            >
              ปิด
            </button>
          </div>
        </div>
      )}

      {/* Checkout Result Modal */}
      {checkoutResult && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-2xl shadow-xl p-8 max-w-sm w-full mx-4 text-center">
            <div className="text-5xl mb-4">✅</div>
            <h2 className="text-2xl font-extrabold text-slate-900 mb-2">ชำระเงินสำเร็จ!</h2>
            <p className="text-slate-500 mb-6">
              จำนวน {checkoutResult.itemCount} รายการ
            </p>
            <div className="bg-slate-50 rounded-xl p-4 mb-6">
              <p className="text-sm text-slate-500">ยอดที่ชำระ</p>
              <p className="text-4xl font-extrabold text-blue-600">
                ${checkoutResult.total.toFixed(2)}
              </p>
            </div>
            <button
              onClick={() => setCheckoutResult(null)}
              className="w-full bg-blue-600 text-white py-3 rounded-xl font-semibold hover:bg-blue-700 transition-colors"
            >
              ปิด
            </button>
          </div>
        </div>
      )}
    </main>
  );
}
