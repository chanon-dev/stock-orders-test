'use client';

import { useState, useCallback } from 'react';
import { CartItem } from '../components/CartPanel';
import * as cartApi from '../api/cart-api';

export function useCart(sessionId: string) {
  const [cartItems, setCartItems] = useState<CartItem[]>([]);
  const [cartTotal, setCartTotal] = useState(0);

  const fetchCart = useCallback(async () => {
    try {
      const data = await cartApi.getCart(sessionId);
      setCartItems(data.items ?? []);
      setCartTotal(data.total ?? 0);
    } catch (err) {
      console.error('Failed to fetch cart', err);
    }
  }, [sessionId]);

  const addToCart = useCallback(
    (productId: string) => cartApi.addToCart(sessionId, productId, 1),
    [sessionId],
  );

  const updateQuantity = useCallback(
    (cartItemId: string, quantity: number, items: CartItem[]) => {
      const item = items.find((i) => i.id === cartItemId);
      if (!item) return Promise.resolve();
      return cartApi.updateCartItem(sessionId, item.productId, quantity);
    },
    [sessionId],
  );

  const removeItem = useCallback(
    (cartItemId: string) => cartApi.removeCartItem(sessionId, cartItemId),
    [sessionId],
  );

  const clearCart = useCallback(
    () => cartApi.clearCart(sessionId),
    [sessionId],
  );

  const doCheckout = useCallback(
    () => cartApi.checkout(sessionId),
    [sessionId],
  );

  return { cartItems, cartTotal, fetchCart, addToCart, updateQuantity, removeItem, clearCart, doCheckout };
}
