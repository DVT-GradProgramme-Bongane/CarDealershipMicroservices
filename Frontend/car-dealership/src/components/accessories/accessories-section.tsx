"use client";

import { useEffect, useState } from "react";
import { fetchAccessories, createAccessory, AccessoryItemDto, SupplierDto, fetchSuppliers } from "@/lib/api/accessories";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";

export function AccessoriesSection() {
  const [accessories, setAccessories] = useState<AccessoryItemDto[]>([]);
  const [suppliers, setSuppliers] = useState<SupplierDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [isDialogOpen, setIsDialogOpen] = useState(false);

  // Form State
  const [name, setName] = useState("");
  const [supplierId, setSupplierId] = useState("");
  const [price, setPrice] = useState("");
  const [stock, setStock] = useState("");
  const [submitError, setSubmitError] = useState("");

  const loadData = async () => {
    try {
      setLoading(true);
      const [accData, supData] = await Promise.all([
        fetchAccessories(),
        fetchSuppliers()
      ]);
      setAccessories(accData);
      setSuppliers(supData);
      setError("");
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitError("");
    try {
      await createAccessory({
        name,
        supplierId,
        price: parseFloat(price),
        stock: parseInt(stock, 10)
      });
      setIsDialogOpen(false);
      setName("");
      setSupplierId("");
      setPrice("");
      setStock("");
      loadData(); // refresh
    } catch (err: any) {
      setSubmitError(err.message);
    }
  };

  const getSupplierName = (id: string) => suppliers.find(s => s.id === id)?.name || id;

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <CardTitle>Accessories</CardTitle>
        <Button onClick={() => setIsDialogOpen(true)}>Add Accessory</Button>
      </CardHeader>
      <CardContent>
        {error ? (
          <div className="text-destructive text-sm">{error}</div>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Accessory Name</TableHead>
                <TableHead>Supplier</TableHead>
                <TableHead>Price</TableHead>
                <TableHead>Stock</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow><TableCell colSpan={4} className="text-center">Loading...</TableCell></TableRow>
              ) : accessories.length === 0 ? (
                <TableRow><TableCell colSpan={4} className="text-center text-muted-foreground">No accessories found.</TableCell></TableRow>
              ) : (
                accessories.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell className="font-medium">{item.name}</TableCell>
                    <TableCell>{getSupplierName(item.supplierId)}</TableCell>
                    <TableCell>${item.price.toFixed(2)}</TableCell>
                    <TableCell>
                      {item.stock < 5 ? (
                        <Badge variant="destructive">{item.stock} (Low Stock)</Badge>
                      ) : (
                        <Badge variant="outline">{item.stock}</Badge>
                      )}
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        )}
      </CardContent>

      {/* <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogHeader>
          <DialogTitle>Add Accessory</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          {submitError && <div className="text-destructive text-sm">{submitError}</div>}
          <div className="space-y-2">
            <label className="text-sm font-medium">Name</label>
            <Input required value={name} onChange={(e) => setName(e.target.value)} />
          </div>
          <div className="space-y-2">
            <label className="text-sm font-medium">Supplier</label>
            <Select required value={supplierId} onChange={(e) => setSupplierId(e.target.value)}>
              <option value="" disabled>Select a supplier</option>
              {suppliers.map(s => (
                <option key={s.id} value={s.id}>{s.name}</option>
              ))}
            </Select>
          </div>
          <div className="space-y-2">
            <label className="text-sm font-medium">Price</label>
            <Input required type="number" step="0.01" min="0" value={price} onChange={(e) => setPrice(e.target.value)} />
          </div>
          <div className="space-y-2">
            <label className="text-sm font-medium">Stock</label>
            <Input required type="number" min="0" value={stock} onChange={(e) => setStock(e.target.value)} />
          </div>
          <div className="flex justify-end pt-4">
            <Button type="submit">Create</Button>
          </div>
        </form>
      </Dialog> */}
    </Card>
  );
}
