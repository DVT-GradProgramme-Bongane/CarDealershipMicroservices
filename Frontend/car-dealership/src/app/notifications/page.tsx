import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

export default function Page() {
  return (
    <>
      <div>
        <h1 className="text-2xl font-semibold mb-2">Notifications</h1>
        <p className="text-muted-foreground">
          Monitor system events and RabbitMQ message queue
        </p>
      </div>
      <Card className="mt-8">
        <CardHeader>
          <CardTitle>Event Activity Feed</CardTitle>
          <CardDescription>
            Real-time events from all microservicesn
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="p-4 rounded-lg border cursor-pointer transition-colors bg-card hover:bg-accent/50">
            <div className="flex items-start gap-3">
              <div className="flex-1 space-y-2">
                <div className="flex items-start justify-between gap-2">
                  <code>event-name</code>
                  <span className="text-xs text-muted-foreground whitespace-nowrap">
                    2026-06-08 13:45:10
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
        </CardContent>
      </Card>

      <Card className="mt-8">
        <CardHeader>
          <CardTitle>Event Payload</CardTitle>
          <CardDescription>Detailed event data in JSON format</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-start justify-between gap-2">
            <div className="flex flex-col">
              <span className="text-xs text-muted-foreground whitespace-nowrap">
                Event name
              </span>
              <code>event name</code>
            </div>

            <div className="flex flex-col">
              <span className="text-xs text-muted-foreground whitespace-nowrap">
                Timestamp
              </span>
              <code>2026-06-08 13:45:10</code>
            </div>
          </div>

          <div className="flex flex-col mt-5">
            <span className="text-xs text-muted-foreground whitespace-nowrap">
              Service source
            </span>
            <span
              data-slot="badge"
              className="inline-flex items-center justify-center rounded-md border px-2 py-0.5 text-xs font-medium w-fit whitespace-nowrap shrink-0 [&amp;&gt;svg]:size-3 gap-1 [&amp;&gt;svg]:pointer-events-none focus-visible:border-ring focus-visible:ring-ring/50 focus-visible:ring-[3px] aria-invalid:ring-destructive/20 dark:aria-invalid:ring-destructive/40 aria-invalid:border-destructive transition-[color,box-shadow] overflow-hidden [a&amp;]:hover:bg-secondary/90 bg-blue-100 text-blue-800 border-blue-200"
            >
              service-name
            </span>
          </div>

          <div className="flex flex-col mt-5">
            <span className="text-xs text-muted-foreground whitespace-nowrap">
              Payload Data
            </span>
            <div className="bg-muted p-4 rounded-lg">
              <pre className="text-xs overflow-x-auto">
                {`{
  "jobId": "MNT-456", 
  "vehicleId": "V-123", 
  "mechanicId": "M-05",
  "completedTasks": [ 
    "Oil change", 
    "Tire rotation", 
    "Brake inspection" 
  ], 
  "duration": 120 "
}`}
              </pre>
            </div>
          </div>
        </CardContent>
      </Card>
    </>
  );
}
