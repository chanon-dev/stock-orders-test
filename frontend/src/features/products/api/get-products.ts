import { apiClient } from "@/lib/api-client";

export type Product = {
  id: string;
  name: string;
  price: number;
  imageUrl: string;
  stockQuantity: number;
};

export const getProducts = (): Promise<Product[]> => {
  return apiClient<Product[]>('/products');
};
