
export type Role = "salesperson" | "mechanic" | "manager" | "financemanager";

export interface StaffModel {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    phone: string;
    role: Role;
}

export interface StaffRolesFilter {
    all: boolean;
    salesperson: boolean;
    financemanager: boolean;
    mechanic: boolean;
    manager: boolean;
}