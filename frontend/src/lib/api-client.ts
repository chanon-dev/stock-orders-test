export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5295/api';

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

  // Handle empty responses (204 No Content, or 200 with empty body from Ok())
  if (response.status === 204) return {} as T;

  const contentLength = response.headers.get('content-length');
  if (contentLength === '0') return {} as T;

  const text = await response.text();
  if (!text) return {} as T;

  return JSON.parse(text) as T;
}
