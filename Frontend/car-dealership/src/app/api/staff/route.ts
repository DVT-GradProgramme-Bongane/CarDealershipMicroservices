import { NextResponse } from "next/server";

const STAFF_API = "http://localhost:3000/api";

export async function GET() {
    const res = await fetch(`${STAFF_API}/staff/`);
    const data = await res.json();
    return NextResponse.json(data, { status: res.status });
}