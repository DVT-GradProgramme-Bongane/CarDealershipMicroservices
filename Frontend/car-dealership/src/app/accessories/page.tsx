import { StatisticsCards } from "@/components/accessories/statistics-cards";
import { SuppliersSection } from "@/components/accessories/suppliers-section";
import { AccessoriesSection } from "@/components/accessories/accessories-section";
import { OrdersSection } from "@/components/accessories/orders-section";

export default function AccessoriesPage() {
  return (
    <div className="flex flex-col gap-8">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Accessories & Suppliers</h1>
        <p className="text-muted-foreground mt-2">
          Manage your dealership's accessories, suppliers, and track orders.
        </p>
      </div>

      <StatisticsCards />

      <div className="grid gap-8 grid-cols-1 xl:grid-cols-2">
        <div className="space-y-8">
          <SuppliersSection />
        </div>
        <div className="space-y-8">
          <AccessoriesSection />
          <OrdersSection />
        </div>
      </div>
    </div>
  );
}