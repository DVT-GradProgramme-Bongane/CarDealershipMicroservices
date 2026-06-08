import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableFooter,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

export interface SalesTransaction {
  id: string;
  carId: string;
  clientId: string;
  staffId: string;
  salePrice: number;
  tradeInId: string | null;
  status: string;
  createdAt: Date;
}

export async function SalesTable() {
  const response = await fetch(`http://localhost:5005/used-sales`);

  if (!response.ok) {
    throw new Error("Failed to fetch sales");
  }

  const sales: SalesTransaction[] = await response.json();

  return (
    <Table className="border border-accent p-4">
      <TableCaption>A list of your recent sales</TableCaption>
      <TableHeader>
        <TableRow>
          <TableHead className="w-[100px]">CarId</TableHead>
          <TableHead>ClientId</TableHead>
          <TableHead>StaffId</TableHead>
          <TableHead className="">SalePrice</TableHead>
          <TableHead className="">Status</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {sales.map((sales) => (
          <TableRow key={sales.id}>
            <TableCell className="font-medium">{sales.carId}</TableCell>
            <TableCell>{sales.clientId}</TableCell>
            <TableCell>{sales.staffId}</TableCell>
            <TableCell className="">{sales.salePrice}</TableCell>
            <TableCell>{sales.status}</TableCell>
          </TableRow>
        ))}
      </TableBody>
      <TableFooter>
        {/* Hardcoded sales total */}
        <TableRow>
          <TableCell colSpan={3}>Total Sales</TableCell>
          <TableCell className="text-right">$2,500.00</TableCell>
        </TableRow>
      </TableFooter>
    </Table>
  );
}
