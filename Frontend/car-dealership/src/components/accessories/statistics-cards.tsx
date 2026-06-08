"use client";

import { useEffect, useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { fetchSuppliers, fetchAccessories, fetchOrders } from "@/lib/api/accessories";
import { HugeiconsIcon } from "@hugeicons/react";
import { UserGroup02Icon, ShoppingBag02Icon, SaleTag01Icon } from "@hugeicons/core-free-icons";

export function StatisticsCards() {
  const [stats, setStats] = useState({ suppliers: 0, accessories: 0, orders: 0 });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadStats() {
      try {
        const [suppliers, accessories, orders] = await Promise.all([
          fetchSuppliers().catch(() => []),
          fetchAccessories().catch(() => []),
          fetchOrders().catch(() => [])
        ]);
        
        setStats({
          suppliers: suppliers.length,
          accessories: accessories.length,
          orders: orders.filter(o => o.status === "ordered").length
        });
      } finally {
        setLoading(false);
      }
    }
    loadStats();
  }, []);

  return (
    <div className="grid gap-4 md:grid-cols-3">
      <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle className="text-sm font-medium">Total Suppliers</CardTitle>
          <HugeiconsIcon icon={UserGroup02Icon} size={16} className="text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold">{loading ? "..." : stats.suppliers}</div>
        </CardContent>
      </Card>
      
      <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle className="text-sm font-medium">Total Accessories</CardTitle>
          <HugeiconsIcon icon={ShoppingBag02Icon} size={16} className="text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold">{loading ? "..." : stats.accessories}</div>
        </CardContent>
      </Card>
      
      <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle className="text-sm font-medium">Active Orders</CardTitle>
          <HugeiconsIcon icon={SaleTag01Icon} size={16} className="text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold">{loading ? "..." : stats.orders}</div>
        </CardContent>
      </Card>
    </div>
  );
}
