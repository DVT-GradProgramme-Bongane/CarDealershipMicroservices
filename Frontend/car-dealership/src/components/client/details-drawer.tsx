"use client";

import { Client } from "@/lib/api/client";
import { Mail, Phone, User } from "lucide-react";
import { useState } from "react";
import EditClientModal from "./edit-client";

interface ClientDetailsSheetProps {
  client: Client;
  onClose: () => void;
  onSuccess: () => void;
}

export default function ClientDetailsSheet({
  client,
  onClose,
  onSuccess,
}: ClientDetailsSheetProps) {
  const initials = `${client.firstName[0]}${client.lastName[0]}`.toUpperCase();
  const [showEdit, setShowEdit] = useState(false);
  return (
    <>
      <div className="fixed inset-0 z-40 bg-black/50" onClick={onClose} />

      <div className="fixed inset-y-0 right-0 z-50 h-full w-3/4 max-w-sm border-l bg-background shadow-lg flex flex-col gap-4">
        <div className="flex flex-col gap-1.5 p-4">
          <h2 className="font-semibold text-foreground">Customer Details</h2>
          <p className="text-sm text-muted-foreground">
            Complete information for {client.firstName} {client.lastName}
          </p>
        </div>

        <div className="space-y-6 mt-6 px-4">
          <div className="flex items-center gap-3">
            <div className="w-16 h-16 rounded-full bg-primary text-primary-foreground flex items-center justify-center text-2xl font-semibold">
              {initials}
            </div>
            <div>
              <h3 className="text-xl font-semibold">
                {client.firstName} {client.lastName}
              </h3>
              <p className="text-sm text-muted-foreground font-mono">
                {client.idNumber}
              </p>
            </div>
          </div>

          <div className="space-y-4">
            <div className="flex items-center gap-3 p-3 bg-muted rounded-lg">
              <Mail className="w-5 h-5 text-muted-foreground" />
              <div>
                <p className="text-xs text-muted-foreground">Email</p>
                <p className="text-sm">{client.email}</p>
              </div>
            </div>
            <div className="flex items-center gap-3 p-3 bg-muted rounded-lg">
              <Phone className="w-5 h-5 text-muted-foreground" />
              <div>
                <p className="text-xs text-muted-foreground">Phone</p>
                <p className="text-sm">{client.phone}</p>
              </div>
            </div>
            <div className="flex items-center gap-3 p-3 bg-muted rounded-lg">
              <User className="w-5 h-5 text-muted-foreground" />
              <div>
                <p className="text-xs text-muted-foreground">Customer ID</p>
                <p className="text-sm font-mono">{client.idNumber}</p>
              </div>
            </div>
          </div>

          <div className="pt-4 space-y-2">
            <button
              onClick={() => setShowEdit(true)}
              className="inline-flex items-center justify-center w-full rounded-md text-sm font-medium bg-primary text-primary-foreground hover:bg-primary/90 h-9 px-4"
            >
              Edit Customer
            </button>
          </div>
        </div>

        <button
          onClick={onClose}
          className="absolute top-4 right-4 opacity-70 hover:opacity-100 transition-opacity"
        >
          ✕
        </button>

        {showEdit && (
          <EditClientModal
            client={client}
            onClose={() => setShowEdit(false)}
            onSuccess={onSuccess}
          />
        )}
        
      </div>
    </>
  );
}
