'use client'

import { Button } from "../ui/button"
interface ButtonCTA {
    label: string,
    onclick: () => void,
}
export default function DealershipHeader({ title, description, buttonCTA }: { title: string; description: string , buttonCTA: ButtonCTA}) {    
    return (
        <div>
            <h1 className="">{title}</h1>

            <div id="header-description">{description}</div>

            <div>
                <Button onClick={buttonCTA.onclick}>
                    {buttonCTA.label}
                </Button>
            </div>
        </div>
    )
}