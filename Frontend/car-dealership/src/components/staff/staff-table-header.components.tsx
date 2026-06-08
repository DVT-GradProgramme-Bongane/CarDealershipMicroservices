'use client'

import {
    InputGroup,
    InputGroupAddon,
    InputGroupButton,
    InputGroupInput,
    InputGroupText,
    InputGroupTextarea,
} from "@/components/ui/input-group";
import { Button } from "@/components/ui/button"
import {
    DropdownMenu,
    DropdownMenuCheckboxItem,
    DropdownMenuContent,
    DropdownMenuGroup,
    DropdownMenuLabel,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { SearchIcon } from "lucide-react";
import { StaffRolesFilter } from "../../app/staff/models/staff.model";

interface StaffTableHeaderProps {
    nameFilter: string;
    rolesFilter: StaffRolesFilter;
    onNameFilterChange: (value: string) => void;
    onRoleToggle: (role: keyof StaffRolesFilter) => void;
}

export default function StaffTableHeader({
    nameFilter,
    rolesFilter,
    onNameFilterChange,
    onRoleToggle,
}: StaffTableHeaderProps) {

    return (
        <div>
            <InputGroup>
                <InputGroupInput value={nameFilter} onChange={(event) => onNameFilterChange(event.target.value)} placeholder="Search for staff member..." />
                <InputGroupAddon>
                    <SearchIcon />
                </InputGroupAddon>
            </InputGroup>

            <DropdownMenu>
                <DropdownMenuTrigger >
                    <Button variant="outline">Roles filter</Button>
                </DropdownMenuTrigger>

                <DropdownMenuContent className="w-48">
                    <DropdownMenuGroup>
                        <DropdownMenu>
                            <DropdownMenuTrigger>
                                <Button variant="outline">Roles filter</Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent className="w-48">
                                <DropdownMenuGroup>
                                    {(["all", "salesperson", "financemanager", "manager", "mechanic"] as const).map((role) => (
                                        <DropdownMenuCheckboxItem
                                            key={role}
                                            checked={rolesFilter[role]}
                                            onCheckedChange={() => onRoleToggle(role)}
                                        >
                                            {{ all: "All", salesperson: "Salesperson", financemanager: "Finance Manager", manager: "Manager", mechanic: "Mechanic" }[role]}
                                        </DropdownMenuCheckboxItem>
                                    ))}
                                </DropdownMenuGroup>
                            </DropdownMenuContent>
                        </DropdownMenu>

                    </DropdownMenuGroup>
                </DropdownMenuContent>
            </DropdownMenu>

        </div>
    )
}