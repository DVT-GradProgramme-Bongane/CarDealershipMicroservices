'use client'


import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,

} from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { getStaffMember, StaffModel } from "@/lib/api/staff";
import { useEffect, useState } from "react";
import { Skeleton } from "@/components/ui/skeleton";
import { Badge } from "@/components/ui/badge";

import { MailIcon, PhoneIcon } from "lucide-react";
export default function ViewStaffDialog({ staffMemberId,
    onClose,
}: {
    staffMemberId: string;
    onClose: () => void;
}) {
    const [staffData, setStaffData] = useState<StaffModel | null>(null)
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        const fetchStaff = async () => {
            try {
                setLoading(true);
                const data = await getStaffMember(staffMemberId);
                setStaffData(data);
            } catch (error) {

            } finally {
                setLoading(false);
            }
        }

        fetchStaff();
    }, [])


    if (loading || !staffData) {
        return (
            <div className="flex flex-col gap-4 py-4">
                <div className="flex items-center gap-4">
                    <Skeleton className="size-14 rounded-full" />
                    <div className="flex flex-col gap-2">
                        <Skeleton className="h-4 w-36" />
                        <Skeleton className="h-3 w-20 rounded-full" />
                    </div>
                </div>
                <Skeleton className="h-4 w-full" />
                <Skeleton className="h-4 w-full" />
                <Skeleton className="h-4 w-48" />
            </div>
        )
    }

    return (
        <Dialog open onOpenChange={onClose}>
            <DialogContent>
                <DialogHeader>
                    <DialogHeader>Viewing Staff member details</DialogHeader>
                </DialogHeader>
                <div>
                     <div className="flex flex-col gap-6 py-4">

                        <div className="flex items-center gap-4">
                            <div className="flex size-14 items-center justify-center rounded-full bg-muted text-lg font-semibold uppercase">
                                {staffData.firstName[0]}{staffData.lastName[0]}
                            </div>
                            <div>
                                <p className="text-base font-semibold">
                                    {staffData.firstName} {staffData.lastName}
                                </p>
                                <Badge variant="secondary" className="mt-1 capitalize">
                                    {staffData.staffRole}
                                </Badge>
                            </div>
                        </div>

                        <div className="flex flex-col gap-3 text-sm">
                            <div className="flex items-center gap-3 text-muted-foreground">
                                <MailIcon className="size-4 shrink-0" />
                                <span>{staffData.email}</span>
                            </div>
                            <div className="flex items-center gap-3 text-muted-foreground">
                                <PhoneIcon className="size-4 shrink-0" />
                                <span>{staffData.phone ?? "No phone on record"}</span>
                            </div>
                        </div>
                    </div>

                </div>


                <DialogFooter className="sm:justify-start">
                    <DialogClose>
                        <Button type="button">Close</Button>
                    </DialogClose>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    )
}