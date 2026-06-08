"use client";

import SaleForm from "@/components/SaleForm";
import DropdownItem from "../models/DropdownItem";

export default function NewSalesPage() {
    const newVehicles: DropdownItem[] = [
        { id: "a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1", label: "2024 Ford Mustang (Ruby Red)" },
        { id: "a2a2a2a2-a2a2-a2a2-a2a2-a2a2a2a2a2a2", label: "2026 BMW M3 (Alpine White)" },
        { id: "a3a3a3a3-a3a3-a3a3-a3a3-a3a3a3a3a3a3", label: "2025 Toyota Hilux (Charcoal Grey)" },
    ];

    const clients: DropdownItem[] = [
        { id: "b1b1b1b1-b1b1-b1b1-b1b1-b1b1b1b1b1b1", label: "Oarabile" },
        { id: "b2b2b2b2-b2b2-b2b2-b2b2-b2b2b2b2b2b2", label: "Lebo" },
        { id: "b3b3b3b3-b3b3-b3b3-b3b3-b3b3b3b3b3b3", label: "Carina" },
    ];

    const staff: DropdownItem[] = [
        { id: "c1c1c1c1-c1c1-c1c1-c1c1-c1c1c1c1c1c1", label: "Sashen" },
        { id: "c2c2c2c2-c2c2-c2c2-c2c2-c2c2c2c2c2c2", label: "Joseph" },
    ];

    const handleSaveNewSale = async (data: { vehicleId: string; clientId: string; salespersonId: string; salePrice: string }) => {
        const simulatedPayload = {
            carId: data.vehicleId,
            clientId: data.clientId,
            staffId: data.salespersonId,
            salePrice: parseFloat(data.salePrice),
            status: "pending"
        };

        console.log("Mock Mode - Payload prepared for backend mapping:", simulatedPayload);
    };

    return (
        <SaleForm
            title="Create New Sale"
            description="Complete the form to initiate a new vehicle sale"
            vehicles={newVehicles}
            clients={clients}
            salespeople={staff}
            onSave={handleSaveNewSale}
        />
    );
}