const API_BASE = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:3000/api';
const ACCESSORIES_API = `${API_BASE}/accessories-suppliers`;

export interface SupplierDto {
  id: string;
  name: string;
  contact: string;
  email: string;
}

export interface AccessoryItemDto {
  id: string;
  supplierId: string;
  name: string;
  price: number;
  stock: number;
}

export interface AccessoryOrderDto {
  id: string;
  itemId: string;
  quantity: number;
  status: string;
  createdAt: string;
}

export async function fetchSuppliers(): Promise<SupplierDto[]> {
  const res = await fetch(`${ACCESSORIES_API}/suppliers`);
  if (!res.ok) throw new Error('Failed to fetch suppliers');
  return res.json();
}

export async function createSupplier(data: { name: string; contact: string; email: string }): Promise<SupplierDto> {
  const res = await fetch(`${ACCESSORIES_API}/suppliers`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.error || 'Failed to create supplier');
  }
  return res.json();
}

export async function fetchAccessories(): Promise<AccessoryItemDto[]> {
  const res = await fetch(`${ACCESSORIES_API}/accessories`);
  if (!res.ok) throw new Error('Failed to fetch accessories');
  return res.json();
}

export async function createAccessory(data: { supplierId: string; name: string; price: number; stock: number }): Promise<AccessoryItemDto> {
  const res = await fetch(`${ACCESSORIES_API}/accessories`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.error || 'Failed to create accessory');
  }
  return res.json();
}

export async function fetchOrders(): Promise<AccessoryOrderDto[]> {
  const res = await fetch(`${ACCESSORIES_API}/accessories/orders`);
  if (!res.ok) throw new Error('Failed to fetch orders');
  return res.json();
}

export async function createOrder(data: { itemId: string; quantity: number }): Promise<AccessoryOrderDto> {
  const res = await fetch(`${ACCESSORIES_API}/accessories/order`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.error || 'Failed to create order');
  }
  return res.json();
}
