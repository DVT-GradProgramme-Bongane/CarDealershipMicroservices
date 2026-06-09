export interface CreateStaffMemberRequest{
    FirstName: string,
    LastName: string,
    Email: string,
    Phone: string,
    Role: string
}

export interface UpdateStaffMemberRequest{
        FirstName: string,
    LastName: string,
    Email: string,
    Phone: string,
    Role: string
}


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


const BASE_URL = 'http://localhost:3000/api/staff/staff'; // api gateway path
export async function getAllStaffMembers() {
    const response = await fetch(`${BASE_URL}/`)

    if(!response.ok){
        
        throw "Could not fetch staff members";
    }
    return response.json();
}

export async function getStaffMember(staffMemberId : string) {
    const response = await fetch(`${BASE_URL}/${staffMemberId}`);

    if(!response.ok){
        
        throw "Could not fetch staff members";
    }
    return response.json();
}

export async function addStaffMember(data: CreateStaffMemberRequest) {
    const response = await fetch(`${BASE_URL}/`, {
        method: 'POST',
        headers: {'content-type': 'application/json'},
        body: JSON.stringify(data)
    })

    if (!response.ok) {
        const error = response; // get error message to send to toats
        throw "could not add staff member";
    }

    return response.json();
}

export async function updateStaffMember(staffMemberId: string, data: UpdateStaffMemberRequest) {
        const response = await fetch(`${BASE_URL}/${staffMemberId}`, {
        method: 'PUT',
        headers: {'content-type': 'application/json'},
        body: JSON.stringify(data)
    });

    if(!response.ok) {
        const error = response; // get error message to send to toats
        throw "could not edit staff member";

    }
}

export async function removeStaffMember(staffMemberId : string) {
    const response = await fetch(`${BASE_URL}/${staffMemberId}`, {
        method: 'DELETE',
        headers: {'content-type': 'application/json'}
    })

    if (!response.ok) {
        const error = response; // get error message to send to toats
        throw "could not delete staff member";
    }


    return response.json();

}