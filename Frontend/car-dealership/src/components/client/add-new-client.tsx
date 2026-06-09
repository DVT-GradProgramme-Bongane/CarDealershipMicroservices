"use client";

import { useState } from "react";
import { createClient, CreateClientDto } from "@/lib/api/client";

const EMPTY_FORM: CreateClientDto = {
  firstName: "",
  lastName: "",
  email: "",
  phone: "",
  idNumber: "",
};

export default function AddClientModal({
  onClose,
  onSuccess,
}: {
  onClose: () => void;
  onSuccess: () => void;
}) {
  const [form, setForm] = useState<CreateClientDto>(EMPTY_FORM);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));

  const handleSubmit = async () => {
    setError(null);
    setLoading(true);
    try {
      await createClient(form);
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
          <h2 className="text-lg font-semibold">Add Client</h2>
          <button onClick={onClose}>&times;</button>
        </div>
        <div className="grid grid-cols-2 gap-3">
          {["firstName", "lastName"].map((f) => (
            <input
              key={f}
              name={f}
              placeholder={f}
              onChange={handleChange}
              className="h-9 w-full rounded-md border border-input bg-transparent px-3 text-sm"
            />
          ))}
        </div>
        {["email", "phone"].map((f) => (
          <input
            key={f}
            name={f}
            placeholder={f}
            onChange={handleChange}
            className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 text-sm"
          />
        ))}
        {error && <p className="text-sm text-red-500">{error}</p>}
        <div className="flex justify-end gap-2 pt-2">
          <button
            onClick={onClose}
            className="border bg-background h-9 rounded-md px-4 text-sm"
          >
            Cancel
          </button>
          <button
            onClick={handleSubmit}
            disabled={loading}
            className="rounded-md text-sm font-medium bg-primary text-primary-foreground h-9 px-4 disabled:opacity-50"
          >
            {loading ? "Saving..." : "Save Client"}
          </button>
        </div>
      </div>
    </div>
  );
}
