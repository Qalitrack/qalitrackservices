import { useEffect, useState } from "react";
import { useSelector } from "react-redux";
import { getWeighingTransactions } from "../api/Weighing/Transactions";
import { Loader2 } from "lucide-react";
import toast from "react-hot-toast";

export default function TransactionsList() {
  const [backendTransactions, setBackendTransactions] = useState([]);
  const [loading, setLoading] = useState(false);
  const { transactions } = useSelector((state) => state.weighing);

  useEffect(() => {
    fetchTransactions();
  }, []);

  const fetchTransactions = async () => {
    try {
      setLoading(true);
      const res = await getWeighingTransactions();
      setBackendTransactions(res);
    } catch (error) {
      toast.error("Backend unavailable, showing local data");
    } finally {
      setLoading(false);
    }
  };

  const combined = [
    ...backendTransactions.map((tx) => ({ ...tx, source: "Backend" })),
    ...transactions.map((tx) => ({ ...tx, source: "Local" })),
  ];

  return (
    <div className="bg-white shadow-md rounded-lg p-4 border">
      <h2 className="text-xl font-bold text-amber-600 mb-4">📋 Weighing Transactions</h2>

      {loading && (
        <div className="flex items-center gap-2 text-gray-500">
          <Loader2 className="w-4 h-4 animate-spin" /> Loading transactions...
        </div>
      )}

      {!loading && combined.length === 0 && (
        <p className="text-gray-500 italic">No transactions found yet.</p>
      )}

      {!loading && combined.length > 0 && (
        <div className="overflow-x-auto">
          <table className="min-w-full border text-sm">
            <thead className="bg-gray-100">
              <tr>
                <th className="border px-2 py-1">#</th>
                <th className="border px-2 py-1">Vehicle</th>
                <th className="border px-2 py-1">Driver</th>
                <th className="border px-2 py-1">Product</th>
                <th className="border px-2 py-1">Operation</th>
                <th className="border px-2 py-1">W1</th>
                <th className="border px-2 py-1">W2</th>
                <th className="border px-2 py-1">Net</th>
                <th className="border px-2 py-1">Date</th>
                <th className="border px-2 py-1">Source</th>
              </tr>
            </thead>
            <tbody>
              {combined.map((tx, index) => (
                <tr key={tx.id || index} className="hover:bg-gray-50">
                  <td className="border px-2 py-1">{index + 1}</td>
                  <td className="border px-2 py-1">
                    {tx.vehicle?.registrationNumber || tx.vehicleId || "—"}
                  </td>
                  <td className="border px-2 py-1">
                    {tx.driver?.name || tx.driverId || "—"}
                  </td>
                  <td className="border px-2 py-1">
                    {tx.product?.name || tx.productId || "—"}
                  </td>
                  <td className="border px-2 py-1">{tx.operation}</td>
                  <td className="border px-2 py-1">{tx.w1}</td>
                  <td className="border px-2 py-1">{tx.w2 || "—"}</td>
                  <td className="border px-2 py-1 font-semibold text-amber-600">
                    {tx.netWeight ?? "—"}
                  </td>
                  <td className="border px-2 py-1">
                    {tx.date ? new Date(tx.date).toLocaleString() : "—"}
                  </td>
                  <td className="border px-2 py-1 text-xs text-gray-500">
                    {tx.source}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
