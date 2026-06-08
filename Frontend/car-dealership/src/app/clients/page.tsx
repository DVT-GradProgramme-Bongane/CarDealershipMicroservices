"use client";

import AddClientModal from "@/components/client/add-new-client";
import { Client, fetchClients } from "@/lib/api/client";
import { useState, useEffect } from "react";

export default function Page() {
  const [clients, setClients] = useState<Client[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);

  const fetchData = () => {
    fetchClients()
      .then((data) => setClients(data))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    fetchData();
  }, []);

  if (loading) return <p className="p-8">Loading clients...</p>;

  return (
    <main className="flex-1 overflow-auto">
      <div className="p-8 space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-semibold mb-2">Clients</h1>
            <p className="text-muted-foreground">
              Manage customer information and relationships
            </p>
          </div>
          <button
            className="inline-flex items-center justify-center gap-2 rounded-md text-sm font-medium bg-primary text-primary-foreground hover:bg-primary/90 h-9 px-4 py-2"
            onClick={() => setShowModal(true)}
          >
            + Add Client
          </button>
        </div>

        <div className="relative">
          <input
            className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 pl-10 text-sm shadow-sm placeholder:text-muted-foreground"
            placeholder="Search by name, email, or phone..."
          />
        </div>

        <div className="rounded-xl border bg-card">
          <table className="w-full caption-bottom text-sm">
            <thead className="[&_tr]:border-b">
              <tr className="border-b">
                <th className="h-10 px-2 text-left align-middle font-medium">
                  Name
                </th>
                <th className="h-10 px-2 text-left align-middle font-medium">
                  Email
                </th>
                <th className="h-10 px-2 text-left align-middle font-medium">
                  Phone
                </th>
                <th className="h-10 px-2 text-left align-middle font-medium">
                  ID Number
                </th>
                <th className="h-10 px-2 text-left align-middle font-medium">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {clients.map((client) => (
                <tr
                  key={client.id}
                  className="border-b hover:bg-muted/50 transition-colors"
                >
                  <td className="p-2 align-middle font-medium">
                    {client.firstName + " " + client.lastName}
                  </td>
                  <td className="p-2 align-middle">{client.email}</td>
                  <td className="p-2 align-middle">{client.phone}</td>
                  <td className="p-2 align-middle font-mono text-sm">
                    {client.id}
                  </td>
                  <td className="p-2 align-middle">
                    <button className="border bg-background hover:bg-accent hover:text-accent-foreground h-8 rounded-md px-3 text-sm">
                      View Details
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
      {showModal && (
        <AddClientModal
          onClose={() => setShowModal(false)}
          onSuccess={() => {
            setShowModal(false);
            fetchData();
          }}
        />
      )}
    </main>
  );
}
