// src/components/WeighingTable.jsx
import { useState, useMemo, lazy } from "react";
import { useDispatch, useSelector } from "react-redux";
import { completeWeighing, deactivateTransaction } from "../store/weighingSlice";
import { FileDown, FileSpreadsheet, Ban } from "lucide-react";

export default function WeighingTable() {
  const dispatch = useDispatch();
  const transactions = useSelector((state) => state.weighing.transactions);

  const [statusFilter, setStatusFilter] = useState("all");
  const [searchQuery, setSearchQuery] = useState("");
  const [w2Inputs, setW2Inputs] = useState({});

  const filteredTransactions = useMemo(() => {
    return transactions.filter((tx) => {
      const matchesStatus =
        statusFilter === "all" ||
        (statusFilter === "inqueue" && tx.w1 && !tx.w2 && !tx.deactivated) ||
        (statusFilter === "completed" && tx.w2) ||
        (statusFilter === "deactivated" && tx.deactivated);

      const matchesSearch =
        !searchQuery ||
        tx.plate?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        tx.orderId?.toLowerCase().includes(searchQuery.toLowerCase());

      return matchesStatus && matchesSearch;
    });
  }, [transactions, statusFilter, searchQuery]);

  const handleCompleteWeighing = (id) => {
    const w2 = parseFloat(w2Inputs[id]);
    if (!w2) {
      alert("Please enter Weight 2");
      return;
    }
    dispatch(completeWeighing({ id, w2 }));
    setW2Inputs((prev) => ({ ...prev, [id]: "" }));
  };

  const exportToExcel = async () => {
    const XLSX = await import("xlsx");
    const ws = XLSX.utils.json_to_sheet(
      filteredTransactions.map((tx) => ({
        Plate: tx.plate,
        OrderID: tx.orderId,
        Weight1: tx.w1,
        Weight2: tx.w2 ?? "",
        NetWeight: tx.w1 && tx.w2 ? tx.w1 - tx.w2 : "",
        TTAT: tx.ttat ? formatSeconds(tx.ttat) : "",
        Status: tx.deactivated
          ? "Deactivated"
          : tx.w2
          ? "Completed"
          : "In Queue",
        Date: new Date(tx.date).toLocaleString(),
      }))
    );
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Weighing Data");
    XLSX.writeFile(wb, "weighing_data.xlsx");
  };

  const exportToPDF = async () => {
    const jsPDF = (await import("jspdf")).default;
    const autoTable = (await import("jspdf-autotable")).default;
    const doc = new jsPDF();
    doc.text("Weighing Transactions Report", 14, 10);
    autoTable(doc, {
      head: [["Plate", "Order ID", "W1", "W2", "Net", "TTAT", "Status", "Date"]],
      body: filteredTransactions.map((tx) => [
        tx.plate,
        tx.orderId,
        tx.w1,
        tx.w2 ?? "",
        tx.w1 && tx.w2 ? tx.w1 - tx.w2 : "",
        tx.ttat ? formatSeconds(tx.ttat) : "",
        tx.deactivated ? "Deactivated" : tx.w2 ? "Completed" : "In Queue",
        new Date(tx.date).toLocaleString(),
      ]),
      startY: 20,
    });
    doc.save("weighing_report.pdf");
  };

  return (
    <div className="bg-white shadow rounded-lg p-4 border">
      {/* Filters & Export */}
      <div className="flex flex-wrap gap-3 mb-4 items-center">
        <input
          type="text"
          placeholder="Search by Plate or Order ID"
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="border rounded px-3 py-2"
        />
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="border rounded px-3 py-2"
        >
          <option value="all">All</option>
          <option value="inqueue">In Queue</option>
          <option value="completed">Completed</option>
          <option value="deactivated">Deactivated</option>
        </select>
        <button
          onClick={exportToExcel}
          className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 flex items-center gap-2"
        >
          <FileSpreadsheet size={16} /> Excel
        </button>
        <button
          onClick={exportToPDF}
          className="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600 flex items-center gap-2"
        >
          <FileDown size={16} /> PDF
        </button>
      </div>

      {/* Table */}
      <div className="overflow-x-auto">
        <table className="min-w-full bg-white border">
          <thead>
            <tr className="bg-gray-100 text-left">
              <th className="px-4 py-2 border">Plate</th>
              <th className="px-4 py-2 border">Order ID</th>
              <th className="px-4 py-2 border">Weight 1</th>
              <th className="px-4 py-2 border">Weight 2</th>
              <th className="px-4 py-2 border">Net</th>
              <th className="px-4 py-2 border">TTAT</th>
              <th className="px-4 py-2 border">Status</th>
              <th className="px-4 py-2 border">Date</th>
              <th className="px-4 py-2 border">Actions</th>
            </tr>
          </thead>
          <tbody>
            {filteredTransactions.map((tx) => (
              <tr
                key={tx.id}
                className={`hover:bg-gray-50 ${
                  tx.deactivated ? "bg-amber-50 text-gray-500" : ""
                }`}
              >
                <td className="px-4 py-2 border">{tx.plate}</td>
                <td className="px-4 py-2 border">{tx.orderId}</td>
                <td className="px-4 py-2 border">{tx.w1 ?? "-"}</td>
                <td className="px-4 py-2 border">
                  {tx.w2 ?? (
                    !tx.deactivated && (
                      <input
                        type="number"
                        placeholder="Enter W2"
                        value={w2Inputs[tx.id] || ""}
                        onChange={(e) =>
                          setW2Inputs((prev) => ({
                            ...prev,
                            [tx.id]: e.target.value,
                          }))
                        }
                        className="border rounded px-2 py-1 w-24"
                      />
                    )
                  )}
                </td>
                <td className="px-4 py-2 border">
                  {tx.w1 && tx.w2 ? tx.w1 - tx.w2 : "-"}
                </td>
                <td className="px-4 py-2 border">
                  {tx.ttat ? formatSeconds(tx.ttat) : "-"}
                </td>
                <td className="px-4 py-2 border">
                  {tx.deactivated
                    ? "Deactivated"
                    : tx.w2
                    ? "Completed"
                    : "In Queue"}
                </td>
                <td className="px-4 py-2 border">
                  {new Date(tx.date).toLocaleString()}
                </td>
                <td className="px-4 py-2 border space-x-2">
                  {!tx.w2 && !tx.deactivated && (
                    <>
                      <button
                        className="px-3 py-1 bg-blue-500 text-white rounded hover:bg-blue-600"
                        onClick={() => handleCompleteWeighing(tx.id)}
                      >
                        Complete
                      </button>
                      <button
                        className="px-3 py-1 bg-amber-500 text-white rounded hover:bg-amber-600 flex items-center gap-1"
                        onClick={() => dispatch(deactivateTransaction(tx.id))}
                      >
                        <Ban size={14} /> Deactivate
                      </button>
                    </>
                  )}
                </td>
              </tr>
            ))}
            {filteredTransactions.length === 0 && (
              <tr>
                <td
                  colSpan="9"
                  className="px-4 py-3 text-center text-gray-500"
                >
                  No transactions found.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function formatSeconds(sec) {
  const m = Math.floor(sec / 60).toString().padStart(2, "0");
  const s = (sec % 60).toString().padStart(2, "0");
  return `${m}:${s}`;
}
