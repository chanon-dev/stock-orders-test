export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:5001/api';

export async function apiClient<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  });

  if (!response.ok) {
    const error = await response.text();
    throw new Error(error || 'Something went wrong');
  }

  if (response.status === 204) return {} as T;
  return response.json();
}
