'use client'

import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogTitle,

} from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { getStaffMember, updateStaffMember } from"@/lib/api/staff";
import { useEffect, useState } from "react";
import { StaffModel } from "@/lib/api/staff";
import { Skeleton } from "@/components/ui/skeleton";
import { UpdateStaffMemberRequest } from "@/lib/api/staff";
import { Field, FieldGroup } from "@/components/ui/field"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { LoaderIcon } from "lucide-react";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select"
export default function EditStaffDialog({ staffMemberId,
    onClose,
}: {
    staffMemberId: string;
    onClose: () => void;
}) {
    const [staffData, setStaffData] = useState<StaffModel | null>(null)
    const [loading, setLoading] = useState(false);
    const [saving, setSaving] = useState(false);
    const [formData, setFormData] = useState<UpdateStaffMemberRequest | null>(null);

    useEffect(() => {
        const fetchStaff = async () => {
            try {
                setLoading(true);
                const data = await getStaffMember(staffMemberId);
                setStaffData(data);
                setFormData({
                    FirstName: data.firstName,
                    LastName: data.lastName,
                    Email: data.email,
                    Phone: data.phone,
                    Role: data.role,
                })
            } catch (error) {

            } finally {
                setLoading(false);
            }
        }

        fetchStaff();
    }, [])

    const isDirty = staffData && formData && (
        formData.FirstName !== staffData.firstName ||
        formData.LastName !== staffData.lastName ||
        formData.Email !== staffData.email ||
        formData.Phone !== staffData.phone ||
        formData.Role !== staffData.staffRole
    );

    function handleChange(field: keyof UpdateStaffMemberRequest, value: string) {
        setFormData((prev) => prev ? { ...prev, [field]: value } : prev);
    }

    async function handleSave() {
        if (!formData || !isDirty) return;
        try {
            setSaving(true);
            await updateStaffMember(staffMemberId, formData);
            onClose();
        } catch {

        } finally {
            setSaving(false);
        }
    }
    if (loading || !formData) {
        return (
            <div className="flex flex-col gap-4 py-4">
                <div className="flex gap-4">
                    <Skeleton className="h-10 w-full" />
                    <Skeleton className="h-10 w-full" />
                </div>
                <Skeleton className="h-10 w-full" />
                <Skeleton className="h-10 w-full" />
                <Skeleton className="h-10 w-full" />
            </div>
        )
    }

    return (
        <Dialog open onOpenChange={onClose}>
            <DialogTitle>Edit Staff Member Details</DialogTitle>


            <DialogContent>
                <div className="flex flex-col gap-4 py-4">
                    <div className="flex gap-4">
                        <Field className="flex-1">
                            <Label>First Name</Label>
                            <Input
                                value={formData.FirstName}
                                onChange={(e) => handleChange("FirstName", e.target.value)}
                            />
                        </Field>
                        <Field className="flex-1">
                            <Label>Last Name</Label>
                            <Input
                                value={formData.LastName}
                                onChange={(e) => handleChange("LastName", e.target.value)}
                            />
                        </Field>
                    </div>

                    <Field>
                        <Label>Email</Label>
                        <Input
                            type="email"
                            value={formData.Email}
                            onChange={(e) => handleChange("Email", e.target.value)}
                        />
                    </Field>

                    <Field>
                        <Label>Phone</Label>
                        <Input
                            type="tel"
                            value={formData.Phone ?? ""}
                            onChange={(e) => handleChange("Phone", e.target.value)}
                        />
                    </Field>

                    <Field>
                        <Label>Role</Label>
                        <Select
                            value={formData.Role}
                            onValueChange={(value) => handleChange("Role", value!)}
                        >
                            <SelectTrigger>
                                <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="salesperson">Salesperson</SelectItem>
                                <SelectItem value="financemanager">Finance Manager</SelectItem>
                                <SelectItem value="mechanic">Mechanic</SelectItem>
                                <SelectItem value="manager">Manager</SelectItem>
                            </SelectContent>
                        </Select>
                    </Field>
                </div>
                <DialogFooter>
                    <Button type="button" variant="outline" onClick={onClose} disabled={saving}>
                        Cancel
                    </Button>
                    <Button
                        type="button"
                        onClick={handleSave}
                        disabled={!isDirty || saving}   // ✅ blocked until a real change is made
                    >
                        {saving ? (
                            <>
                                <LoaderIcon className="mr-2 size-4 animate-spin" />
                                Saving...
                            </>
                        ) : "Save Changes"}
                    </Button>
                </DialogFooter>           
            </DialogContent>
        </Dialog>
    )
}