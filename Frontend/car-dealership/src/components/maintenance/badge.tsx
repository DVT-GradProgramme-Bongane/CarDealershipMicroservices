export const statusConfig = {
  completed: {
    label: "completed",
    className: "bg-green-100 text-green-800 border border-green-200",
  },
  "in-progress": {
    label: "in-progress",
    className: "bg-yellow-100 text-yellow-800 border border-yellow-200",
  },
  scheduled: {
    label: "scheduled",
    className: "bg-blue-100 text-blue-800 border border-blue-200",
  },
};
export default function Badge({ status }: { status: keyof typeof statusConfig }) {
  const config = statusConfig[status] || {};
  return (
    <span
      className={`inline-flex items-center justify-center rounded-md px-2 py-0.5 text-xs font-medium whitespace-nowrap ${config.className}`}
    >
      {config.label}
    </span>
  );
}