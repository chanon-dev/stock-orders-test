import { Product } from "../api/get-products";

type ProductCardProps = {
  product: Product;
  onAddToCart: (productId: string) => void;
};

export const ProductCard = ({ product, onAddToCart }: ProductCardProps) => {
  return (
    <div className="bg-white rounded-2xl shadow-sm border border-slate-100 overflow-hidden hover:shadow-md transition-shadow">
      <div className="h-48 bg-slate-100 flex items-center justify-center p-4">
        {/* Placeholder for Product Image */}
        <div className="text-4xl">📦</div>
      </div>
      <div className="p-5">
        <h3 className="font-semibold text-lg text-slate-900 mb-1">{product.name}</h3>
        <p className="text-blue-600 font-bold text-xl mb-4">${product.price.toFixed(2)}</p>
        
        <div className="flex items-center justify-between mt-auto">
          <span className={`text-sm ${product.stockQuantity > 0 ? 'text-green-600' : 'text-red-500'}`}>
            {product.stockQuantity > 0 ? `${product.stockQuantity} in stock` : 'Out of stock'}
          </span>
          
          <button
            onClick={() => onAddToCart(product.id)}
            disabled={product.stockQuantity === 0}
            className="px-4 py-2 bg-slate-900 text-white rounded-lg hover:bg-slate-800 disabled:bg-slate-300 disabled:cursor-not-allowed transition-colors"
          >
            Add to Cart
          </button>
        </div>
      </div>
    </div>
  );
};
