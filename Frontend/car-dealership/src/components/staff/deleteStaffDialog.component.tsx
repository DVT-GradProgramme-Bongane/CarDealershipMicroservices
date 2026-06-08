'use client'

import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogTitle,

} from "@/components/ui/dialog"


import { removeStaffMember } from "@/lib/api/staff";
import { Button } from "@/components/ui/button"

export default function DeleteStaffDialog({
    staffMemberId,
    onClose,
}: {
    staffMemberId: string;
    onClose: () => void;
}) {

    async function handleRemoveStaff() {
        await removeStaffMember(staffMemberId);
        onClose();
    }

    return (
        <Dialog open onOpenChange={onClose}>
            <DialogTitle>Are you sure you want to remove this staff member?</DialogTitle>


            <DialogContent>
                <DialogDescription>This action is irrevisible</DialogDescription>
                <DialogFooter className="sm:justify-start">
                    <DialogClose>
                        <Button type="button">Close</Button>
                    </DialogClose>

                    <Button type="button" onClick={() => handleRemoveStaff()}>Remove</Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    )
}