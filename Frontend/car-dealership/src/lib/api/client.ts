const API_BASE = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:3000/api';
const CLIENTS_API = `${API_BASE}/client`;

export interface Client {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  idNumber: string;
}

export interface CreateClientDto {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  idNumber: string;
}

export async function fetchClients(): Promise<Client[]> {
  const res = await fetch(`${CLIENTS_API}/clients`);
  if (!res.ok) throw new Error("Failed to fetch clients");
  return res.json();
}

export async function createClient(data: CreateClientDto): Promise<Client> {
  const res = await fetch(`${CLIENTS_API}/clients`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || "Failed to create client");
  }

  return res.json();
}