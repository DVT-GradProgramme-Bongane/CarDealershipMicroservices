"use client";

import { useEffect, useState } from "react";
import { fetchOrders, createOrder, AccessoryOrderDto, AccessoryItemDto, fetchAccessories } from "@/lib/api/accessories";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";

export function OrdersSection() {
  const [orders, setOrders] = useState<AccessoryOrderDto[]>([]);
  const [accessories, setAccessories] = useState<AccessoryItemDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  
  // Form State
  const [itemId, setItemId] = useState("");
  const [quantity, setQuantity] = useState("");
  const [submitError, setSubmitError] = useState("");

  const loadData = async () => {
    try {
      setLoading(true);
      const [ordData, accData] = await Promise.all([
        fetchOrders(),
        fetchAccessories()
      ]);
      setOrders(ordData);
      setAccessories(accData);
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
      await createOrder({ 
        itemId, 
        quantity: parseInt(quantity, 10) 
      });
      setIsDialogOpen(false);
      setItemId("");
      setQuantity("");
      loadData(); // refresh
    } catch (err: any) {
      setSubmitError(err.message);
    }
  };

  const getAccessoryName = (id: string) => accessories.find(a => a.id === id)?.name || id;

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <CardTitle>Orders</CardTitle>
        <Button onClick={() => setIsDialogOpen(true)}>Place Order</Button>
      </CardHeader>
      <CardContent>
        {error ? (
          <div className="text-destructive text-sm">{error}</div>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Accessory</TableHead>
                <TableHead>Quantity</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Created Date</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow><TableCell colSpan={4} className="text-center">Loading...</TableCell></TableRow>
              ) : orders.length === 0 ? (
                <TableRow><TableCell colSpan={4} className="text-center text-muted-foreground">No orders found.</TableCell></TableRow>
              ) : (
                orders.map((o) => (
                  <TableRow key={o.id}>
                    <TableCell className="font-medium">{getAccessoryName(o.itemId)}</TableCell>
                    <TableCell>{o.quantity}</TableCell>
                    <TableCell>
                      <Badge variant={o.status === "received" ? "secondary" : "default"}>
                        {o.status}
                      </Badge>
                    </TableCell>
                    <TableCell>{new Date(o.createdAt).toLocaleDateString()}</TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        )}
      </CardContent>

      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogHeader>
          <DialogTitle>Place Order</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          {submitError && <div className="text-destructive text-sm">{submitError}</div>}
          <div className="space-y-2">
            <label className="text-sm font-medium">Accessory</label>
            <Select required value={itemId} onChange={(e) => setItemId(e.target.value)}>
              <option value="" disabled>Select an accessory</option>
              {accessories.map(a => (
                <option key={a.id} value={a.id}>{a.name} (Stock: {a.stock})</option>
              ))}
            </Select>
          </div>
          <div className="space-y-2">
            <label className="text-sm font-medium">Quantity</label>
            <Input required type="number" min="1" value={quantity} onChange={(e) => setQuantity(e.target.value)} />
          </div>
          <div className="flex justify-end pt-4">
            <Button type="submit">Order</Button>
          </div>
        </form>
      </Dialog>
    </Card>
  );
}
