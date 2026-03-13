"use client";

export type CartItem = {
  id: string;
  productId: string;
  productName: string;
  quantity: number;
  price: number;
  subTotal: number;
};

type CartPanelProps = {
  items: CartItem[];
  total: number;
  onUpdateQuantity: (cartItemId: string, quantity: number) => void;
  onRemoveItem: (cartItemId: string) => void;
  onClearCart: () => void;
  onCheckout: () => void;
};

export const CartPanel = ({
  items,
  total,
  onUpdateQuantity,
  onRemoveItem,
  onClearCart,
  onCheckout,
}: CartPanelProps) => {
  if (items.length === 0) {
    return (
      <div className="bg-white rounded-2xl shadow-sm border border-slate-100 p-8 text-center text-slate-400">
        ตะกร้าสินค้าว่างเปล่า
      </div>
    );
  }

  return (
    <div className="bg-white rounded-2xl shadow-sm border border-slate-100 overflow-hidden">
      <div className="p-5 border-b border-slate-100 flex justify-between items-center">
        <h2 className="font-bold text-lg text-slate-900">ตะกร้าสินค้า</h2>
        <button
          onClick={onClearCart}
          className="text-sm text-red-500 hover:text-red-700 transition-colors"
        >
          ล้างตะกร้า
        </button>
      </div>

      <div className="divide-y divide-slate-100">
        {items.map((item) => (
          <div key={item.id} className="p-4 flex items-center gap-4">
            <div className="flex-1 min-w-0">
              <p className="font-medium text-slate-900 truncate">{item.productName}</p>
              <p className="text-sm text-slate-500">${item.price.toFixed(2)} / ชิ้น</p>
            </div>

            <div className="flex items-center gap-2">
              <button
                onClick={() => onUpdateQuantity(item.id, item.quantity - 1)}
                className="w-8 h-8 flex items-center justify-center rounded-lg bg-slate-100 hover:bg-slate-200 font-bold text-slate-700 transition-colors"
              >
                −
              </button>
              <span className="w-8 text-center font-semibold text-slate-900">{item.quantity}</span>
              <button
                onClick={() => onUpdateQuantity(item.id, item.quantity + 1)}
                className="w-8 h-8 flex items-center justify-center rounded-lg bg-slate-100 hover:bg-slate-200 font-bold text-slate-700 transition-colors"
              >
                +
              </button>
            </div>

            <p className="w-20 text-right font-semibold text-slate-800">
              ${item.subTotal.toFixed(2)}
            </p>

            <button
              onClick={() => onRemoveItem(item.id)}
              className="text-slate-400 hover:text-red-500 transition-colors text-lg leading-none"
            >
              ✕
            </button>
          </div>
        ))}
      </div>

      <div className="p-5 border-t border-slate-100 flex items-center justify-between">
        <div>
          <p className="text-sm text-slate-500">ยอดรวม</p>
          <p className="text-2xl font-extrabold text-slate-900">${total.toFixed(2)}</p>
        </div>
        <button
          onClick={onCheckout}
          className="bg-blue-600 text-white px-8 py-3 rounded-xl font-semibold hover:bg-blue-700 transition-colors"
        >
          ชำระเงิน
        </button>
      </div>
    </div>
  );
};
