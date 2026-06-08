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

interface StaffTableProps {
    staffData : StaffModel[]
}

export default function StaffTable({ staffData }: StaffTableProps) {

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
            {staffData.map((staff, index) => {
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
            })}

            </TableBody>
        </Table>

    )
}