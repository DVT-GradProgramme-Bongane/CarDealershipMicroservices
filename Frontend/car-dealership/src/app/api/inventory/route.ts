import { NextResponse } from "next/server";

const INVENTORY_API = "http://localhost:3000/api/inventory/";

export async function GET() {
    const res = await fetch(`${INVENTORY_API}/inventory`);
    const data = await res.json();
    return NextResponse.json(data, { status: res.status });
}