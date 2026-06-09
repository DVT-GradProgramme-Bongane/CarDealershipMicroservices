import { NextResponse } from "next/server";

const CLIENTS_API = "http://localhost:3000/api/client";

export async function GET() {
    const res = await fetch(`${CLIENTS_API}/clients`);
    const data = await res.json();
    return NextResponse.json(data, { status: res.status });
}