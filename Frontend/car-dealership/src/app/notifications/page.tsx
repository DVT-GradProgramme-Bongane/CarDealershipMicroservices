export default function Page() {
  return (
    <>
      <h1>Notifications</h1>

      <div className="p-4 rounded-lg border">
        <h3>Event Activity Feed</h3>
        <p className="text-muted-foreground">Real-time events from all microservices</p>
        <div className="p-4 rounded-lg border cursor-pointer transition-colors bg-card hover:bg-accent/50">
          <div className="flex items-start gap-3">
            <div className="mt-1">
            </div>
            <div className="flex-1 space-y-2">
              <div className="flex items-start justify-between gap-2">
                <code>
                  event-name
                </code>
                <span className="text-xs text-muted-foreground whitespace-nowrap">
                  timestamp
                </span>
              </div>
              <span
                data-slot="badge"
                className="inline-flex items-center justify-center rounded-md border px-2 py-0.5 text-xs font-medium w-fit whitespace-nowrap shrink-0 [&amp;&gt;svg]:size-3 gap-1 [&amp;&gt;svg]:pointer-events-none focus-visible:border-ring focus-visible:ring-ring/50 focus-visible:ring-[3px] aria-invalid:ring-destructive/20 dark:aria-invalid:ring-destructive/40 aria-invalid:border-destructive transition-[color,box-shadow] overflow-hidden [a&amp;]:hover:bg-secondary/90 bg-blue-100 text-blue-800 border-blue-200"
              >
                service-name
              </span>
            </div>
          </div>
        </div>
      </div>

    </>
  );
}
