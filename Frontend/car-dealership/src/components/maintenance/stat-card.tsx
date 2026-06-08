export default function StatCard({ title, value, subtitle, icon }: { title: string; value: number; subtitle: string; icon: React.ReactNode }) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 flex flex-col">
      <div className="flex flex-row items-center justify-between px-6 pt-6 pb-2">
        <h4 className="text-sm font-medium text-gray-700">{title}</h4>
        <span className="text-gray-400">{icon}</span>
      </div>
      <div className="px-6 pb-6">
        <div className="text-2xl font-bold">{value}</div>
        <p className="text-xs text-gray-500 mt-1">{subtitle}</p>
      </div>
    </div>
  );
}