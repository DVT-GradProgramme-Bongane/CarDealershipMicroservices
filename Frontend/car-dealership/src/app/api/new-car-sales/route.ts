import { NextResponse } from "next/server";

const NEW_SALES_API = "http://localhost:3000/api/new-car-sales/";

export async function POST(request: Request) {
    const body = await request.json();

    const res = await fetch(`${NEW_SALES_API}/new-sales`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
    });

    const data = await res.json().catch(() => null);
    return NextResponse.json(data, { status: res.status });
}