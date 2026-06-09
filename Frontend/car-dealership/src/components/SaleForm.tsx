"use client";

import { useState, useActionState } from "react";
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import DropdownItem from "@/app/models/DropdownItem";

interface SaleFormProps {
    title: string;
    description: string;
    vehicles: DropdownItem[];
    clients: DropdownItem[];
    salespeople: DropdownItem[];
    onSave: (data: { vehicleId: string; clientId: string; salespersonId: string; salePrice: string }) => Promise<void>;
}

export default function SaleForm({
    title,
    description,
    vehicles,
    clients,
    salespeople,
    onSave
}: SaleFormProps) {
    const [formData, setFormData] = useState({
        vehicleId: "",
        clientId: "",
        salespersonId: "",
        salePrice: "32900",
    });

    const [errorMessage, formAction, isPending] = useActionState(
        async (_previousState: string | null, _formDataPayload: FormData) => {
            try {
                await onSave(formData);
                return null;
            } catch (err: unknown) {
                return (err as { message?: string })?.message || "An unexpected error occurred.";
            }
        },
        null
    );

    return (
        <div className="max-w-2xl mx-auto mt-8">
            <Card className="bg-white border-slate-200 shadow-sm rounded-2xl">
                <CardHeader className="space-y-1">
                    <CardTitle className="text-xl font-bold text-slate-900">{title}</CardTitle>
                    <CardDescription className="text-slate-500 text-sm">{description}</CardDescription>
                </CardHeader>

                <CardContent>
                    <form action={formAction} className="space-y-5">

                        {/* Vehicle Field */}
                        <div className="space-y-2">
                            <label className="text-sm font-semibold text-slate-900">Vehicle</label>
                            <Select
                                value={formData.vehicleId}
                                onValueChange={(val: string | null) => setFormData({ ...formData, vehicleId: val ?? "" })}
                            >
                                <SelectTrigger className="w-full bg-slate-50 border-slate-200 text-slate-700 h-11 rounded-lg">
                                    <SelectValue placeholder="Select a vehicle" />
                                </SelectTrigger>
                                <SelectContent>
                                    {vehicles.map(v => (
                                        <SelectItem key={v.id} value={v.label}>{v.label}</SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                        </div>

                        {/* Client Field */}
                        <div className="space-y-2">
                            <label className="text-sm font-semibold text-slate-900">Client</label>
                            <Select
                                value={formData.clientId}
                                onValueChange={(val: string | null) => setFormData({ ...formData, clientId: val ?? "" })}
                            >
                                <SelectTrigger className="w-full bg-slate-50 border-slate-200 text-slate-700 h-11 rounded-lg">
                                    <SelectValue placeholder="Select a client" />
                                </SelectTrigger>
                                <SelectContent>
                                    {clients.map(c => (
                                        <SelectItem key={c.id} value={c.label}>{c.label}</SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                        </div>

                        {/* Salesperson Field */}
                        <div className="space-y-2">
                            <label className="text-sm font-semibold text-slate-900">Salesperson</label>
                            <Select
                                value={formData.salespersonId}
                                onValueChange={(val: string | null) => setFormData({ ...formData, salespersonId: val ?? "" })}
                            >
                                <SelectTrigger className="w-full bg-slate-50 border-slate-200 text-slate-700 h-11 rounded-lg">
                                    <SelectValue placeholder="Select a salesperson" />
                                </SelectTrigger>
                                <SelectContent>
                                    {salespeople.map(s => (
                                        <SelectItem key={s.id} value={s.label}>{s.label}</SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                        </div>

                        {errorMessage && (
                            <p className="text-sm text-red-500 font-medium">{errorMessage}</p>
                        )}

                        <Button
                            type="submit"
                            disabled={isPending || !formData.vehicleId || !formData.clientId || !formData.salespersonId}
                            className="w-full h-12 bg-slate-500 hover:bg-slate-600 text-white font-medium rounded-xl transition-colors mt-6 shadow-none"
                        >
                            {isPending ? "Processing..." : "Create Sale"}
                        </Button>

                    </form>
                </CardContent>
            </Card>
        </div>
    );
}