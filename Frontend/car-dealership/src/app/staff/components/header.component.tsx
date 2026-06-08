
export default function DealershipHeader({ title, description }: { title: string; description: string }) {    return (
        <div>
            <h1 className="">{title}</h1>

            <div id="header-description">{description}</div>
        </div>
    )
}