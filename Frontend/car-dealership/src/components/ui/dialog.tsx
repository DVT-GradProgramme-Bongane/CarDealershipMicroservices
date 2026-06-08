import * as React from "react"
import { cn } from "@/lib/utils"

export interface DialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  children: React.ReactNode;
}

export function Dialog({ open, onOpenChange, children }: DialogProps) {
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm" onClick={() => onOpenChange(false)}>
      <div className="bg-background rounded-lg shadow-lg w-full max-w-md p-6 relative border" onClick={(e) => e.stopPropagation()}>
        <button className="absolute top-4 right-4 text-muted-foreground hover:text-foreground" onClick={() => onOpenChange(false)}>✕</button>
        {children}
      </div>
    </div>
  );
}

export function DialogHeader({ children }: { children: React.ReactNode }) {
  return <div className="mb-4">{children}</div>;
}

export function DialogTitle({ children }: { children: React.ReactNode }) {
  return <h2 className="text-lg font-semibold">{children}</h2>;
}
