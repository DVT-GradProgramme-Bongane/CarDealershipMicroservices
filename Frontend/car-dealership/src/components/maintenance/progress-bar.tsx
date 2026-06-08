export default function ProgressBar({ value }: { value: number }) {
  return (
    <div className="relative h-2 w-full overflow-hidden rounded-full bg-blue-100">
      <div
        className="h-full bg-blue-600 transition-all rounded-full"
        style={{ width: `${value}%` }}
      />
    </div>
  );
}