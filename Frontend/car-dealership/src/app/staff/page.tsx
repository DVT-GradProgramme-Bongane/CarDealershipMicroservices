"use client";

import DealershipHeader from "../../components/staff/header.component";
import StaffTableHeader from "../../components/staff/staff-table-header.components";
import StaffTable from "../../components/staff/staff-table.component";
import { useState, useMemo, useEffect } from "react";
import { StaffModel, StaffRolesFilter,  getAllStaffMembers } from "@/lib/api/staff";
import AddStaffDialog from "../../components/staff/addStaffDialog.component";


export default function Page() {
    const [staffMembers, setStaffMembers] = useState<StaffModel[]>([])
    const [staffNameFilter, setStaffNameFilter] = useState("");
    const [staffModel, openAddStaffModel] = useState(false);
    const [loading, setLoadng] = useState(false);
    const [staffRolesFilter, setStaffRolesFilter] = useState({
        all: true,
        salesperson: false,
        financemanager: false,
        mechanic: false,
        manager: false
    });


    useEffect(() => {
        const fetchStaff = async () => {
            try {
                setLoadng(true);
                const data = await getAllStaffMembers();
                setStaffMembers(data);
            } catch (error) {
                // toost error
            } finally {
                setLoadng(false);
            }
        }

        fetchStaff();
    }, []);

    

    const filterStaff = useMemo(() => {
        const activeRoles = Object.entries(staffRolesFilter)
            .filter(([key, on]) => key !== "all" && on)
            .map(([key]) => key);

        return staffMembers.filter((member) => {
            const matchesName =
                `${member.firstName} ${member.lastName}`
                    .toLowerCase()
                    .includes(staffNameFilter.toLowerCase());

            const matchesRole =
                staffRolesFilter.all || activeRoles.length === 0
                    ? true
                    : activeRoles.includes(member.staffRole);

            return matchesName && matchesRole;
        });
    }, [staffMembers, setStaffNameFilter, staffRolesFilter])


    function toggleRoleFilter(role: keyof StaffRolesFilter) {
        if (role === "all") {
            // Reset everything, select All
            setStaffRolesFilter({ all: true, salesperson: false, financemanager: false, mechanic: false, manager: false });
            return;
        }
        setStaffRolesFilter((prev) => {
            const updated = { ...prev, [role]: !prev[role], all: false };
            // If nothing is checked, fall back to "All"
            const anyActive = (["salesperson", "financemanager", "mechanic", "manager"] as const)
                .some((r) => updated[r]);
            return anyActive ? updated : { ...updated, all: true };
        });
    }



    return (
        <>
            <DealershipHeader
                title="Staff"
                description="Manage dealership employees and roles"
                buttonCTA={{
                    label: "Add Staff Member",
                    onclick: () => openAddStaffModel(true)
                }}
            />

            {staffModel && (
                <AddStaffDialog onClose={() => openAddStaffModel(false)} />
            )}


            <StaffTableHeader
                nameFilter={staffNameFilter}
                rolesFilter={staffRolesFilter}
                onNameFilterChange={setStaffNameFilter}
                onRoleToggle={toggleRoleFilter}
            />

            <StaffTable staffData={filterStaff} loading={loading}/>
        </>
    )
}