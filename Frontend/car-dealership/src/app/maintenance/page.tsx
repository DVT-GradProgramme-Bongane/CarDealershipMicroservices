"use client";
import Badge, { statusConfig } from "@/components/maintenance/badge";
import ProgressBar from "@/components/maintenance/progress-bar";
import StatCard from "@/components/maintenance/stat-card";
import { Wrench, Clock, CircleCheck, Plus } from "lucide-react";

type Status = keyof typeof statusConfig;
interface Records {
    id: string; 
    vehicle: string; 
    vin: string; 
    mechanic: string;
    status: Status; 
    date: string;
}

const jobs: Records[] = [
  { id: "MNT-456", vehicle: "2023 Honda Accord",  vin: "1HGCM82633A123456", mechanic: "William Taylor", status: "completed",  date: "2026-06-05" },
  { id: "MNT-457", vehicle: "2024 Toyota Camry",   vin: "5FNRL5H40HB123789", mechanic: "Jennifer Lee",   status: "in-progress", date: "2026-06-08" },
  { id: "MNT-458", vehicle: "2023 Ford F-150",     vin: "2HGFC2F53MH987654", mechanic: "William Taylor", status: "scheduled",   date: "2026-06-10" },
  { id: "MNT-459", vehicle: "2024 Tesla Model 3",  vin: "5YJSA1E26MF123987", mechanic: "Jennifer Lee",   status: "in-progress", date: "2026-06-08" },
  { id: "MNT-460", vehicle: "2023 BMW 3 Series",   vin: "WBAPL7C55HN654321", mechanic: "William Taylor", status: "scheduled",   date: "2026-06-12" },
];

const MECHANICS = ["William Taylor", "Jennifer Lee"];
const TABLE_HEADERS = ["Job ID", "Vehicle", "VIN", "Assigned Mechanic", "Status", "Scheduled Date"];

export default function MaintenancePage() {
  const activeCount    = jobs.filter((j) => j.status === "in-progress").length;
  const scheduledCount = jobs.filter((j) => j.status === "scheduled").length;
  const completedCount = jobs.filter((j) => j.status === "completed").length;

  const mechanicProgress = MECHANICS.map((name) => {
    const assignedJobs = jobs.filter((j) => j.mechanic === name);
    const done = assignedJobs.filter((j) => j.status === "completed").length;
    return { name, done, total: assignedJobs.length, pct: assignedJobs.length ? Math.round((done / assignedJobs.length) * 100) : 0 };
  });

  return (
    <main className="flex-1 overflow-auto min-h-screen">
      <div className="p-8 space-y-6 max-w-6xl mx-auto">

        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-semibold mb-1">Maintenance</h1>
            <p className="text-gray-500 text-sm">Manage vehicle service and maintenance jobs</p>
          </div>
          <button className="inline-flex items-center gap-2 rounded-md bg-gray-900 text-white text-sm font-medium px-4 py-2 hover:bg-gray-700 transition-colors">
            <Plus size={16} /> Create Job
          </button>
        </div>

        {/* Stat Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <StatCard title="Active Jobs"      value={activeCount}    subtitle="Currently being worked on" icon={<Wrench size={16} />} />
          <StatCard title="Scheduled"        value={scheduledCount} subtitle="Upcoming appointments"     icon={<Clock size={16} />} />
          <StatCard title="Completed Today"  value={completedCount} subtitle="Finished jobs"             icon={<CircleCheck size={16} />} />
        </div>

        {/* Jobs Table */}
        <div className="bg-white rounded-xl border border-gray-200">
          <div className="px-6 pt-6 pb-3">
            <h4 className="text-base font-semibold leading-none">Active Jobs</h4>
            <p className="text-sm text-gray-500 mt-1">All maintenance and service jobs</p>
          </div>
          <div className="px-6 pb-6 overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-200">
                  {TABLE_HEADERS.map((h) => (
                    <th key={h} className="text-left font-medium text-gray-700 h-10 px-2 whitespace-nowrap">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {jobs.map((job) => (
                  <tr key={job.id} className="border-b border-gray-100 hover:bg-gray-50 transition-colors last:border-0">
                    <td className="p-2 font-mono text-sm whitespace-nowrap">{job.id}</td>
                    <td className="p-2 font-medium whitespace-nowrap">{job.vehicle}</td>
                    <td className="p-2 font-mono text-sm whitespace-nowrap">{job.vin}</td>
                    <td className="p-2 whitespace-nowrap">{job.mechanic}</td>
                    <td className="p-2 whitespace-nowrap"><Badge status={job.status} /></td>
                    <td className="p-2 whitespace-nowrap">{job.date}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Today's Progress */}
        <div className="bg-white rounded-xl border border-gray-200">
          <div className="px-6 pt-6 pb-3">
            <h4 className="text-base font-semibold leading-none">Today's Progress</h4>
            <p className="text-sm text-gray-500 mt-1">Overall maintenance completion status</p>
          </div>
          <div className="px-6 pb-6 space-y-4">
            {mechanicProgress.map(({ name, done, total, pct }) => (
              <div key={name} className="space-y-2">
                <div className="flex items-center justify-between text-sm">
                  <span>{name}</span>
                  <span className="text-gray-500">{done} of {total} jobs completed</span>
                </div>
                <ProgressBar value={pct} />
              </div>
            ))}
          </div>
        </div>

      </div>
    </main>
  );
}