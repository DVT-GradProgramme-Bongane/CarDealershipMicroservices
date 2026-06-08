import { StaffModel } from "../models/staff.model";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table"
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuTrigger,
    DropdownMenuItem,
    DropdownMenuSeparator
} from "@/components/ui/dropdown-menu"

import { MoreHorizontalIcon } from "lucide-react"
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";

interface StaffTableProps {
    staffData: StaffModel[],
    loading: boolean
}

export default function StaffTable({ staffData, loading }: StaffTableProps) {
    const ROW_COUNT = 6;

    return (
        <Table>
            <TableHeader>
                <TableRow>
                    <TableHead>Firstname</TableHead>
                    <TableHead>Lastname</TableHead>
                    <TableHead>Email</TableHead>
                    <TableHead>Phone</TableHead>
                    <TableHead>Role</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                </TableRow>
            </TableHeader>

            <TableBody>
                {loading ? (
                    Array.from({ length: ROW_COUNT }).map((_, rowIndex) => (
                        <TableRow key={rowIndex}>
                            <TableCell>
                                <div className="flex flex-col gap-1">
                                    <Skeleton className="h-4 w-28" /> {/* First Last */}
                                </div>
                            </TableCell>
                            <TableCell><Skeleton className="h-4 w-40" /></TableCell>
                            <TableCell><Skeleton className="h-4 w-28" /></TableCell>
                            <TableCell>
                                <Skeleton className="h-6 w-20 rounded-full" /> {/* badge shape */}
                            </TableCell>
                        </TableRow>
                    ))

                ) : (

                    staffData.map((staff, index) => {
                        return (
                            <TableRow key={staff.email || index}>
                                <TableCell className="font-medium">{staff.firstName}</TableCell>
                                <TableCell className="font-medium">{staff.lastName}</TableCell>
                                <TableCell className="font-medium">{staff.email}</TableCell>
                                <TableCell className="font-medium">{staff.phone}</TableCell>

                                <TableCell>{staff.role}</TableCell>
                                <TableCell className="text-right">
                                    <DropdownMenu>
                                        <DropdownMenuTrigger>
                                            <Button variant="ghost" size="icon" className="size-8">
                                                <MoreHorizontalIcon />
                                                <span className="sr-only">Open menu</span>
                                            </Button>
                                        </DropdownMenuTrigger>
                                        <DropdownMenuContent align="end">
                                            <DropdownMenuItem>Edit</DropdownMenuItem>
                                            <DropdownMenuItem>Duplicate</DropdownMenuItem>
                                            <DropdownMenuSeparator />
                                            <DropdownMenuItem variant="destructive">
                                                Delete
                                            </DropdownMenuItem>
                                        </DropdownMenuContent>
                                    </DropdownMenu>
                                </TableCell>
                            </TableRow>
                        );
                    })
                )}

            </TableBody>
        </Table>

    )
}