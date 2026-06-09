'use client'

import { Button } from "../ui/button"
interface ButtonCTA {
    label: string,
    onclick: () => void,
}
export default function DealershipHeader({ title, description, buttonCTA }: { title: string; description: string, buttonCTA: ButtonCTA }) {
    return (
        <div className="flex items-center justify-between px-6 py-4 border-b">
            <div>
                <h1 className="text-2xl font-semibold tracking-tight">{title}</h1>
                <p className="text-sm text-muted-foreground mt-1">{description}</p>
            </div>

            {buttonCTA && (
                <Button onClick={buttonCTA.onclick}>
                    {buttonCTA.label}
                </Button>
            )}
        </div>
    )
}