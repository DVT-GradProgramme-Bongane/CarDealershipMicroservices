import { useState } from "react";

const EMPTY_FORM = {
  first_name: "",
  last_name: "",
  email: "",
  phone: "",
  id_number: "",
};

export default function AddClientModal({ onClose }: { onClose: () => void }) {
  const [form, setForm] = useState(EMPTY_FORM);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div className="bg-background rounded-xl border shadow-lg w-full max-w-md p-6 space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold">Add Client</h2>
          <button onClick={onClose}>&times;</button>
        </div>
        <div className="grid grid-cols-2 gap-3">
          {["first_name", "last_name"].map((f) => (
            <input
              key={f}
              name={f}
              placeholder={f}
              onChange={handleChange}
              className="h-9 w-full rounded-md border border-input bg-transparent px-3 text-sm"
            />
          ))}
        </div>
        {["email", "phone", "id_number"].map((f) => (
          <input
            key={f}
            name={f}
            placeholder={f}
            onChange={handleChange}
            className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 text-sm"
          />
        ))}
        <div className="flex justify-end gap-2 pt-2">
          <button
            onClick={onClose}
            className="border bg-background h-9 rounded-md px-4 text-sm"
          >
            Cancel
          </button>
          <button className="rounded-md text-sm font-medium bg-primary text-primary-foreground h-9 px-4">
            Save Client
          </button>
        </div>
      </div>
    </div>
  );
}
