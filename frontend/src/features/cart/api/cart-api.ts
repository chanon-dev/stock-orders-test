import { apiClient } from '@/lib/api-client';
import { CartItem } from '../components/CartPanel';

export type CartDto = { items: CartItem[]; total: number };
export type CheckoutResult = { total: number; itemCount: number };

export const getCart = (sessionId: string) =>
  apiClient<CartDto>(`/cart/${sessionId}`);

export const addToCart = (sessionId: string, productId: string, quantity: number) =>
  apiClient(`/cart/${sessionId}/items`, {
    method: 'POST',
    body: JSON.stringify({ productId, quantity }),
  });

export const updateCartItem = (sessionId: string, productId: string, newQuantity: number) =>
  apiClient(`/cart/${sessionId}/items/${productId}`, {
    method: 'PUT',
    body: JSON.stringify({ newQuantity }),
  });

export const removeCartItem = (sessionId: string, cartItemId: string) =>
  apiClient(`/cart/${sessionId}/items/${cartItemId}`, { method: 'DELETE' });

export const clearCart = (sessionId: string) =>
  apiClient(`/cart/${sessionId}/items`, { method: 'DELETE' });

export const checkout = (sessionId: string) =>
  apiClient<CheckoutResult>(`/cart/${sessionId}/checkout`, { method: 'POST' });
