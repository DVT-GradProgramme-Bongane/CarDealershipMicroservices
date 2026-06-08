const API_BASE = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:3000/api';
const CLIENTS_API = `${API_BASE}/client`;

export interface Client {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  id: string;
}

export async function fetchClients(): Promise<Client[]> {
  const res = await fetch(`${CLIENTS_API}/clients`);
  if (!res.ok) throw new Error("Failed to fetch clients");
  return res.json();
}