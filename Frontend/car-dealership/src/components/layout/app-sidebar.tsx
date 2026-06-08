"use client";

import { usePathname } from "next/navigation";
import Link from "next/link";
import { HugeiconsIcon } from "@hugeicons/react";
import {
  DashboardSquare01Icon,
  Car01Icon,
  UserGroup02Icon,
  UserMultiple02Icon,
  SaleTag01Icon,
  MoneyBag02Icon,
  Wrench01Icon,
  ShoppingBag02Icon,
  Notification03Icon,
  SidebarLeft01Icon,
  Settings01Icon,
  Logout03Icon,
} from "@hugeicons/core-free-icons";
import { cn } from "@/lib/utils";

interface NavItem {
  label: string;
  href: string;
  icon: typeof DashboardSquare01Icon;
}

const mainNavItems: NavItem[] = [
  { label: "Dashboard", href: "/", icon: DashboardSquare01Icon },
  { label: "Inventory", href: "/inventory", icon: Car01Icon },
  { label: "Clients", href: "/clients", icon: UserGroup02Icon },
  { label: "Staff", href: "/staff", icon: UserMultiple02Icon },
  { label: "New Sales", href: "/new-sales", icon: SaleTag01Icon },
  { label: "Used Sales", href: "/used-sales", icon: SaleTag01Icon },
  { label: "Financing", href: "/financing", icon: MoneyBag02Icon },
  { label: "Maintenance", href: "/maintenance", icon: Wrench01Icon },
  { label: "Accessories", href: "/accessories", icon: ShoppingBag02Icon },
  { label: "Notifications", href: "/notifications", icon: Notification03Icon },
];

interface AppSidebarProps {
  collapsed: boolean;
  onToggle: () => void;
}

export function AppSidebar({ collapsed, onToggle }: AppSidebarProps) {
  const pathname = usePathname();

  return (
    <aside
      data-slot="sidebar"
      className={cn(
        "fixed inset-y-0 left-0 z-30 flex flex-col border-r border-sidebar-border bg-sidebar transition-all duration-300 ease-in-out",
        collapsed ? "w-[68px]" : "w-[260px]"
      )}
    >
      {/* Logo & Brand */}
      <div className="flex h-14 items-center gap-3 border-b border-sidebar-border px-4">
        <div className="flex size-8 shrink-0 items-center justify-center rounded-lg bg-primary">
          <HugeiconsIcon
            icon={Car01Icon}
            size={18}
            className="text-primary-foreground"
          />
        </div>
        {!collapsed && (
          <div className="flex flex-col overflow-hidden">
            <span className="truncate text-sm font-semibold text-sidebar-foreground">
              Dealership
            </span>
            <span className="truncate text-[11px] text-muted-foreground">
              Microservices Dashboard
            </span>
          </div>
        )}
      </div>

      {/* Navigation */}
      <nav className="flex-1 overflow-y-auto px-3 py-4">
        <div className={cn("mb-3 px-2", collapsed && "sr-only")}>
          <span className="text-[11px] font-medium uppercase tracking-wider text-muted-foreground">
            Navigation
          </span>
        </div>
        <ul className="flex flex-col gap-0.5">
          {mainNavItems.map((item) => {
            const isActive =
              item.href === "/"
                ? pathname === "/"
                : pathname.startsWith(item.href);

            return (
              <li key={item.href}>
                <Link
                  href={item.href}
                  title={collapsed ? item.label : undefined}
                  className={cn(
                    "group relative flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-all duration-150",
                    collapsed && "justify-center px-0",
                    isActive
                      ? "bg-sidebar-accent text-sidebar-accent-foreground"
                      : "text-sidebar-foreground/70 hover:bg-sidebar-accent/50 hover:text-sidebar-foreground"
                  )}
                >
                  {/* Active indicator pill */}
                  {isActive && (
                    <span className="absolute left-0 top-1/2 h-5 w-[3px] -translate-y-1/2 rounded-r-full bg-sidebar-primary" />
                  )}
                  <HugeiconsIcon
                    icon={item.icon}
                    size={18}
                    className={cn(
                      "shrink-0 transition-colors",
                      isActive
                        ? "text-sidebar-primary"
                        : "text-sidebar-foreground/50 group-hover:text-sidebar-foreground/70"
                    )}
                  />
                  {!collapsed && (
                    <span className="truncate">{item.label}</span>
                  )}

                  {/* Tooltip for collapsed state */}
                  {collapsed && (
                    <span className="pointer-events-none absolute left-full ml-2 rounded-md bg-popover px-2.5 py-1.5 text-xs font-medium text-popover-foreground opacity-0 shadow-md ring-1 ring-border transition-opacity group-hover:pointer-events-auto group-hover:opacity-100">
                      {item.label}
                    </span>
                  )}
                </Link>
              </li>
            );
          })}
        </ul>
      </nav>

      {/* Bottom section */}
      <div className="border-t border-sidebar-border p-3">
        {/* Settings link */}
        <Link
          href="/settings"
          className={cn(
            "group flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-sidebar-foreground/70 transition-all duration-150 hover:bg-sidebar-accent/50 hover:text-sidebar-foreground",
            collapsed && "justify-center px-0"
          )}
        >
          <HugeiconsIcon
            icon={Settings01Icon}
            size={18}
            className="shrink-0 text-sidebar-foreground/50 transition-colors group-hover:text-sidebar-foreground/70"
          />
          {!collapsed && <span className="truncate">Settings</span>}
        </Link>

        {/* Collapse toggle */}
        <button
          onClick={onToggle}
          className={cn(
            "group mt-1 flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-sidebar-foreground/70 transition-all duration-150 hover:bg-sidebar-accent/50 hover:text-sidebar-foreground",
            collapsed && "justify-center px-0"
          )}
          title={collapsed ? "Expand sidebar" : "Collapse sidebar"}
        >
          <HugeiconsIcon
            icon={SidebarLeft01Icon}
            size={18}
            className={cn(
              "shrink-0 transition-all",
              collapsed && "rotate-180",
              "text-sidebar-foreground/50 group-hover:text-sidebar-foreground/70"
            )}
          />
          {!collapsed && <span className="truncate">Collapse</span>}
        </button>
      </div>
    </aside>
  );
}
