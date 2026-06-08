"use client";

import { useState, useEffect } from 'react';

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

type NotificationLog = {
  id: string;
  event_type: string;
  payload: string;
  created_at: string;
};


export default function Page() {
  const [logs, setLogs] = useState<NotificationLog[]>([]);
  const [selectedNotification, setSelectedNotification] = useState<NotificationLog | null>(null);

  useEffect(() => {
    const fetchLogs = async () => {
      try {
        const response = await fetch("http://localhost:5009/notifications");
        if (response.ok) {
          const data: NotificationLog[] = await response.json();
          setLogs(data.reverse()); 
        }
      } catch (err) {
        console.error("Failed to fetch notifications", err);
      }
    };
    fetchLogs();
    const interval = setInterval(fetchLogs, 3000);
    return () => clearInterval(interval);
  }, []);

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
          {logs.length === 0 && (
            <p className="text-sm text-muted-foreground">
              Loading events...
            </p>
          )}
          {logs.map((log) => (
            <div
              key={log.id}
              onClick={() => setSelectedNotification(log)}
              className={`p-4 rounded-lg border cursor-pointer transition-colors ${
                selectedNotification?.id === log.id
                  ? "bg-accent border-accent-foreground/20"
                  : "bg-card hover:bg-accent/50"
              }`}
            >
              <div className="flex items-start gap-3">
                <div className="flex-1 space-y-2">
                  <div className="flex items-start justify-between gap-2">
                    <code>{log.event_type}</code>
                    <span className="text-xs text-muted-foreground whitespace-nowrap">
                      {new Date(log.created_at).toLocaleString()}
                    </span>
                  </div>
                  <span className="inline-flex items-center justify-center rounded-md border px-2 py-0.5 text-xs font-medium bg-blue-100 text-blue-800 border-blue-200">
                    {log.event_type.split("."[0])}
                  </span>
                </div>
              </div>
            </div>
          ))}
        </CardContent>
      </Card>

      <Card className="mt-8">
        <CardHeader>
          <CardTitle>Event Payload</CardTitle>
          <CardDescription>Detailed event data in JSON format</CardDescription>
        </CardHeader>
        <CardContent>
          {!selectedNotification ? (
            <p className="text-sm text-muted-foreground">
              Select an event above to view its payload.
            </p>
          ) : (
            <>
              <div className="flex items-start justify-between gap-2">
                <div className="flex flex-col">
                  <span className="text-xs text-muted-foreground">Event name</span>
                  <code>{selectedNotification.event_type}</code>
                </div>
                <div className="flex flex-col">
                  <span className="text-xs text-muted-foreground">Timestamp</span>
                  <code>{new Date(selectedNotification.created_at).toLocaleString()}</code>
                </div>
              </div>

              <div className="flex flex-col mt-5">
                <span className="text-xs text-muted-foreground">Service source</span>
                <span className="inline-flex items-center justify-center rounded-md border px-2 py-0.5 text-xs font-medium w-fit bg-blue-100 text-blue-800 border-blue-200">
                  {selectedNotification.payload.split(".")[0]}
                </span>
              </div>

              <div className="flex flex-col mt-5">
                <span className="text-xs text-muted-foreground">Payload Data</span>
                <div className="bg-muted p-4 rounded-lg mt-1">
                  <pre className="text-xs overflow-x-auto">
                    {JSON.stringify(selectedNotification.payload, null, 2)}
                  </pre>
                </div>
              </div>
            </>
          )}
        </CardContent>
      </Card>
    </>
  );
}
