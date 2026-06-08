import { CreateStaffMemberRequest, UpdateStaffMemberRequest } from "./DTOs";


const BASE_URL = 'htt://localhost:300/api/staff'; // api gateway path
export async function getAllStaffMembers() {
    const response = await fetch(`${BASE_URL}/`)

    if(!response.ok){
        
        throw "Could not fetch staff members";
    }
    return response.json();
}

export async function getStaffMember(staffMemberId : number) {
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

export async function updateStaffMember(staffMemberId: number, data: UpdateStaffMemberRequest) {
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

export async function removeStaffMember(staffMemberId : number) {
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