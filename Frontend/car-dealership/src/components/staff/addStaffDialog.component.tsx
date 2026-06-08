'use client'

import { useState } from "react";
import { CreateStaffMemberRequest } from "../../app/staff/models/DTOs";
import { addStaffMember } from "@/lib/api/staff";

import {
  Dialog,
  DialogClose,
  DialogContent,

  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Field, FieldGroup } from "@/components/ui/field"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"

import { Button } from "@base-ui/react";
export default function AddStaffDialog({ onClose }: { onClose: () => void }) {

    const [formData, setFormData] = useState<CreateStaffMemberRequest>({
        FirstName: "",
        LastName: "",
        Email: "",
        Phone: "",
        Role: "salesperson",
    });

    function handleChange(field: keyof CreateStaffMemberRequest, value: string) {
        setFormData((prev) => ({ ...prev, [field]: value }));
    }

    async function handleSubmit() {
        try {
            await addStaffMember(formData); 
            onClose();
        } catch (e) {
           
        }
    }

    return (
        <Dialog>
            <DialogTitle>Add Staff Member</DialogTitle>
            <FieldGroup>
                <Field>
                    <Label>Firstname</Label>
                    <Input
                        value={formData.FirstName}
                        onChange={(e) => handleChange("FirstName", e.target.value)}
                        type="text" required
                    />
                </Field>
                <Field>
                    <Label>Lastname</Label>
                    <Input
                        value={formData.LastName}
                        onChange={(e) => handleChange("LastName", e.target.value)}
                        type="text" required
                    />
                </Field>
                <Field>
                    <Label>Email</Label>
                    <Input
                        value={formData.Email}
                        onChange={(e) => handleChange("Email", e.target.value)}
                        type="email" required
                    />
                </Field>
                <Field>
                    <Label>Phone</Label>
                    <Input
                        value={formData.Phone}
                        onChange={(e) => handleChange("Phone", e.target.value)}
                        type="tel"
                    />
                </Field>
                <Field>
                    <Label>Role</Label>
                    <Select
                        value={formData.Role}
                        onValueChange={(value) => handleChange("Role", value!)} 
                    >
                        <SelectTrigger><SelectValue /></SelectTrigger>
                        <SelectContent>
                            <SelectItem value="salesperson">Salesperson</SelectItem>
                            <SelectItem value="financemanager">Finance Manager</SelectItem>
                            <SelectItem value="mechanic">Mechanic</SelectItem>
                            <SelectItem value="manager">Manager</SelectItem>
                        </SelectContent>
                    </Select>
                </Field>

                <Field orientation="horizontal">
                    <Button type="button" onClick={onClose}>Cancel</Button>
                    <Button type="button" onClick={handleSubmit}>Submit</Button>
                </Field>
            </FieldGroup>
        </Dialog>
    );
}