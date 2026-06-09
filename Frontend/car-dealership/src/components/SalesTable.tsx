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
  createdAt: string;
}

type SalesTableProps = {
  sales: SalesTransaction[];
};

export function SalesTable({ sales }: SalesTableProps) {
  const totalSales = sales.reduce((sum, sale) => sum + sale.salePrice, 0);

  return (
    <Table className="border border-accent p-4">
      <TableCaption>A list of your recent sales</TableCaption>
      <TableHeader>
        <TableRow>
          <TableHead>CarId</TableHead>
          <TableHead>ClientId</TableHead>
          <TableHead>StaffId</TableHead>
          <TableHead>Sale Price</TableHead>
          <TableHead>Status</TableHead>
        </TableRow>
      </TableHeader>

      <TableBody>
        {sales.map((sale) => (
          <TableRow key={sale.id}>
            <TableCell>{sale.carId}</TableCell>
            <TableCell>{sale.clientId}</TableCell>
            <TableCell>{sale.staffId}</TableCell>
            <TableCell>{sale.salePrice}</TableCell>
            <TableCell>{sale.status}</TableCell>
          </TableRow>
        ))}
      </TableBody>

      <TableFooter>
        <TableRow>
          <TableCell colSpan={3}>Total Sales</TableCell>
          <TableCell>{totalSales}</TableCell>
          <TableCell />
        </TableRow>
      </TableFooter>
    </Table>
  );
}
