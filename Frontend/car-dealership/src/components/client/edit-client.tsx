"use client";

import { useState } from "react";
import { Client, deleteClient, updateClient } from "@/lib/api/client";

interface EditClientModalProps {
  client: Client;
  onClose: () => void;
  onSuccess: () => void;
}

export default function EditClientModal({
  client,
  onClose,
  onSuccess,
}: EditClientModalProps) {
  const [form, setForm] = useState({
    firstName: client.firstName,
    lastName: client.lastName,
    email: client.email,
    phone: client.phone,
    idNumber: client.idNumber,
  });
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));

  const handleUpdate = async () => {
    setError(null);
    setLoading(true);
    try {
      await updateClient(client.id, form);
      onSuccess();
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    setLoading(true);
    try {
      await deleteClient(client.id);
      onSuccess();
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div className="bg-background rounded-xl border shadow-lg w-full max-w-md p-6 space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold">Edit Client</h2>
          <button onClick={onClose}>&times;</button>
        </div>
        <div className="grid grid-cols-2 gap-3">
          {["firstName", "lastName"].map((f) => (
            <input
              key={f}
              name={f}
              value={form[f as keyof typeof form]}
              placeholder={f}
              onChange={handleChange}
              className="h-9 w-full rounded-md border border-input bg-transparent px-3 text-sm"
            />
          ))}
        </div>
        {["email", "phone", "idNumber"].map((f) => (
          <input
            key={f}
            name={f}
            value={form[f as keyof typeof form]}
            placeholder={f}
            onChange={handleChange}
            className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 text-sm"
          />
        ))}
        {error && <p className="text-sm text-red-500">{error}</p>}
        <div className="flex justify-between pt-2">
          <button
            onClick={handleDelete}
            disabled={loading}
            className="inline-flex items-center justify-center whitespace-nowrap text-sm font-medium transition-all disabled:pointer-events-none disabled:opacity-50 border bg-background hover:bg-accent dark:bg-input/30 dark:border-input dark:hover:bg-input/50 h-8 rounded-md gap-1.5 px-3 text-red-600 hover:text-red-700"
          >
            Delete
          </button>
          <div className="flex gap-2">
            <button
              onClick={onClose}
              className="border bg-background h-9 rounded-md px-4 text-sm"
            >
              Cancel
            </button>
            <button
              onClick={handleUpdate}
              disabled={loading}
              className="rounded-md text-sm font-medium bg-primary text-primary-foreground h-9 px-4 "
            >
              {loading ? "Saving..." : "Save Changes"}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
