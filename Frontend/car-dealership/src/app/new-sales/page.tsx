"use client";

import { useEffect, useState } from "react";
import SaleForm from "@/components/SaleForm";
import DropdownItem from "../models/DropdownItem";

const INVENTORY_API = "http://localhost:3000/api/inventory";
const CLIENTS_API = "http://localhost:5003";
const STAFF_API = "http://localhost:5002";
const NEW_SALES_API = "http://localhost:5004";

export default function NewSalesPage() {
    const [vehicles, setVehicles] = useState<DropdownItem[]>([]);
    const [clients, setClients] = useState<DropdownItem[]>([]);
    const [salespeople, setSalespeople] = useState<DropdownItem[]>([]);
    const [loadError, setLoadError] = useState<string | null>(null);

    useEffect(() => {
        async function loadDropdowns() {
            try {
                const [inventoryRes, clientsRes, staffRes] = await Promise.all([
                    fetch(`${INVENTORY_API}/inventory`),
                    fetch(`${CLIENTS_API}/clients`),
                    fetch(`${STAFF_API}/staff`),
                ]);

                if (!inventoryRes.ok) throw new Error(`Inventory service error: ${inventoryRes.status}`);
                if (!clientsRes.ok) throw new Error(`Clients service error: ${clientsRes.status}`);
                if (!staffRes.ok) throw new Error(`Staff service error: ${staffRes.status}`);

                const [inventoryData, clientsData, staffData] = await Promise.all([
                    inventoryRes.json(),
                    clientsRes.json(),
                    staffRes.json(),
                ]);

                // Only show new cars that are available
                setVehicles(
                    inventoryData
                        .filter((car: any) => car.type === "new" && car.status === "available")
                        .map((car: any): DropdownItem => ({
                            id: car.id,
                            label: `${car.year} ${car.make} ${car.model} (${car.color})`,
                        }))
                );

                setClients(
                    clientsData.map((client: any): DropdownItem => ({
                        id: client.id,
                        label: `${client.firstName} ${client.lastName}`,
                    }))
                );

                // Only show staff with the salesperson role
                setSalespeople(
                    staffData
                        .filter((member: any) => member.role === "salesperson")
                        .map((member: any): DropdownItem => ({
                            id: member.id,
                            label: `${member.firstName} ${member.lastName}`,
                        }))
                );
            } catch (err: unknown) {
                setLoadError((err as { message?: string })?.message ?? "Failed to load form data.");
            }
        }

        loadDropdowns();
    }, []);

    const handleSaveNewSale = async (data: {
        vehicleId: string;
        clientId: string;
        salespersonId: string;
        salePrice: string;
    }) => {
        const payload = {
            carId: data.vehicleId,
            clientId: data.clientId,
            staffId: data.salespersonId,
            salePrice: parseFloat(data.salePrice),
            status: "pending",
        };

        const response = await fetch(`${NEW_SALES_API}/new-sales`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload),
        });

        if (!response.ok) {
            let message = `Request failed with status ${response.status}`;
            try {
                const errorBody = await response.json();
                message = errorBody?.message ?? errorBody?.title ?? message;
            } catch {
                // ignore parse errors — use the default message above
            }
            throw new Error(message);
        }
    };

    if (loadError) {
        return (
            <div className="max-w-2xl mx-auto mt-8">
                <p className="text-sm text-red-500 font-medium">{loadError}</p>
            </div>
        );
    }

    return (
        <SaleForm
            title="Create New Sale"
            description="Complete the form to initiate a new vehicle sale"
            vehicles={vehicles}
            clients={clients}
            salespeople={salespeople}
            onSave={handleSaveNewSale}
        />
    );
}