"use client";

import { useState } from "react";
import { AppSidebar } from "./app-sidebar";
import { AppHeader } from "./app-header";
import { cn } from "@/lib/utils";

export function DashboardLayout({ children }: { children: React.ReactNode }) {
  const [collapsed, setCollapsed] = useState(false);

  return (
    <div className="flex min-h-screen w-full flex-col bg-background">
      <AppSidebar 
        collapsed={collapsed} 
        onToggle={() => setCollapsed(!collapsed)} 
      />
      <div 
        className={cn(
          "flex flex-1 flex-col transition-all duration-300 ease-in-out",
          collapsed ? "md:pl-[68px]" : "md:pl-[260px]"
        )}
      >
        <AppHeader onToggleSidebar={() => setCollapsed(!collapsed)} />
        <main className="flex-1 p-4 md:p-6 lg:p-8">
          <div className="mx-auto max-w-7xl">
            {children}
          </div>
        </main>
      </div>
    </div>
  );
}
